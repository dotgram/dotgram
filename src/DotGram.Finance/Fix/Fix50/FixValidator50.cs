using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix50;

// Written by generate.cs from the FIX 5.0 SP2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The check of every FIX 5.0 SP2 message type, of every component it reuses, and of every entry of
/// every repeating group; the fields' own are in FixValidator50.Fields.cs.
/// </summary>
/// <remarks>
/// Straight-line checks written against the fields of the class they are about, and nothing else:
/// a field the repository marks required, a component it marks required and the carrier left empty,
/// every field handed to its own slot, and every count held to the entries that follow it. Every
/// slot is typed on what it checks and named for it — a message type, a component, and a group
/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run
/// time replaces the slots it describes by name and leaves the rest.
/// </remarks>
partial class FixValidator50
{
	/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>
	public static readonly FixValidator50 Default = new();

	/// <summary>The translation of a message whose class cannot have the name the dictionaries give it.</summary>
	private protected override string MessageClass(string name)
	{
		return name switch
		{
			"SecurityStatus" => "SecurityStatusMessage",
			_                => name,
		};
	}

	/// <summary>The translation of a field whose name a message or a component has first.</summary>
	private protected override string FieldSlotName(string name)
	{
		return name switch
		{
			"RateSource" => "RateSourceField",
			_            => name,
		};
	}

	/// <summary>The tag of a field this version names otherwise than FixTag does.</summary>
	private protected override int FieldTag(string name)
	{
		return name switch
		{
			"HaltReasonInt" => 327,
			_               => 0,
		};
	}

	/// <summary>Holds a FIX 5.0 SP2 AdjustedPositionReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AdjustedPositionReport> AdjustedPositionReport { get; set; } = ValidateAdjustedPositionReport;

	/// <summary>Holds a FIX 5.0 SP2 Advertisement to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Advertisement> Advertisement { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 5.0 SP2 AllocationInstruction to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction> AllocationInstruction { get; set; } = ValidateAllocationInstruction;

	/// <summary>Holds a FIX 5.0 SP2 AllocationInstructionAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAck> AllocationInstructionAck { get; set; } = ValidateAllocationInstructionAck;

	/// <summary>Holds a FIX 5.0 SP2 AllocationInstructionAlert to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert> AllocationInstructionAlert { get; set; } = ValidateAllocationInstructionAlert;

	/// <summary>Holds a FIX 5.0 SP2 AllocationReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport> AllocationReport { get; set; } = ValidateAllocationReport;

	/// <summary>Holds a FIX 5.0 SP2 AllocationReportAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReportAck> AllocationReportAck { get; set; } = ValidateAllocationReportAck;

	/// <summary>Holds a FIX 5.0 SP2 ApplicationMessageReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageReport> ApplicationMessageReport { get; set; } = ValidateApplicationMessageReport;

	/// <summary>Holds a FIX 5.0 SP2 ApplicationMessageRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageRequest> ApplicationMessageRequest { get; set; } = ValidateApplicationMessageRequest;

	/// <summary>Holds a FIX 5.0 SP2 ApplicationMessageRequestAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageRequestAck> ApplicationMessageRequestAck { get; set; } = ValidateApplicationMessageRequestAck;

	/// <summary>Holds a FIX 5.0 SP2 AssignmentReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AssignmentReport> AssignmentReport { get; set; } = ValidateAssignmentReport;

	/// <summary>Holds a FIX 5.0 SP2 BidRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BidRequest> BidRequest { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 5.0 SP2 BidResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BidResponse> BidResponse { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 5.0 SP2 BusinessMessageReject to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BusinessMessageReject> BusinessMessageReject { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 5.0 SP2 CollateralAssignment to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment> CollateralAssignment { get; set; } = ValidateCollateralAssignment;

	/// <summary>Holds a FIX 5.0 SP2 CollateralInquiry to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry> CollateralInquiry { get; set; } = ValidateCollateralInquiry;

	/// <summary>Holds a FIX 5.0 SP2 CollateralInquiryAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck> CollateralInquiryAck { get; set; } = ValidateCollateralInquiryAck;

	/// <summary>Holds a FIX 5.0 SP2 CollateralReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport> CollateralReport { get; set; } = ValidateCollateralReport;

	/// <summary>Holds a FIX 5.0 SP2 CollateralRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest> CollateralRequest { get; set; } = ValidateCollateralRequest;

	/// <summary>Holds a FIX 5.0 SP2 CollateralResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse> CollateralResponse { get; set; } = ValidateCollateralResponse;

	/// <summary>Holds a FIX 5.0 SP2 Confirmation to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation> Confirmation { get; set; } = ValidateConfirmation;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmationAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ConfirmationAck> ConfirmationAck { get; set; } = ValidateConfirmationAck;

	/// <summary>Holds a FIX 5.0 SP2 ConfirmationRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ConfirmationRequest> ConfirmationRequest { get; set; } = ValidateConfirmationRequest;

	/// <summary>Holds a FIX 5.0 SP2 ContraryIntentionReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ContraryIntentionReport> ContraryIntentionReport { get; set; } = ValidateContraryIntentionReport;

	/// <summary>Holds a FIX 5.0 SP2 CrossOrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest> CrossOrderCancelReplaceRequest { get; set; } = ValidateCrossOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 5.0 SP2 CrossOrderCancelRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelRequest> CrossOrderCancelRequest { get; set; } = ValidateCrossOrderCancelRequest;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityList to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList> DerivativeSecurityList { get; set; } = ValidateDerivativeSecurityList;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityListRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListRequest> DerivativeSecurityListRequest { get; set; } = ValidateDerivativeSecurityListRequest;

	/// <summary>Holds a FIX 5.0 SP2 DerivativeSecurityListUpdateReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport> DerivativeSecurityListUpdateReport { get; set; } = ValidateDerivativeSecurityListUpdateReport;

	/// <summary>Holds a FIX 5.0 SP2 DontKnowTrade to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DontKnowTrade> DontKnowTrade { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 5.0 SP2 Email to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email> Email { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 5.0 SP2 ExecutionAcknowledgement to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionAcknowledgement> ExecutionAcknowledgement { get; set; } = ValidateExecutionAcknowledgement;

	/// <summary>Holds a FIX 5.0 SP2 ExecutionReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport> ExecutionReport { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 5.0 SP2 Heartbeat to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Heartbeat> Heartbeat { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 5.0 SP2 IOI to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.IOI> IOI { get; set; } = ValidateIOI;

	/// <summary>Holds a FIX 5.0 SP2 ListCancelRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListCancelRequest> ListCancelRequest { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 5.0 SP2 ListExecute to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListExecute> ListExecute { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 5.0 SP2 ListStatus to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStatus> ListStatus { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 5.0 SP2 ListStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStatusRequest> ListStatusRequest { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 ListStrikePrice to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStrikePrice> ListStrikePrice { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 5.0 SP2 Logon to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Logon> Logon { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 5.0 SP2 Logout to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Logout> Logout { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 5.0 SP2 MarketDataIncrementalRefresh to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh> MarketDataIncrementalRefresh { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 5.0 SP2 MarketDataRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest> MarketDataRequest { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 5.0 SP2 MarketDataRequestReject to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequestReject> MarketDataRequestReject { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 5.0 SP2 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh> MarketDataSnapshotFullRefresh { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 5.0 SP2 MarketDefinition to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition> MarketDefinition { get; set; } = ValidateMarketDefinition;

	/// <summary>Holds a FIX 5.0 SP2 MarketDefinitionRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionRequest> MarketDefinitionRequest { get; set; } = ValidateMarketDefinitionRequest;

	/// <summary>Holds a FIX 5.0 SP2 MarketDefinitionUpdateReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport> MarketDefinitionUpdateReport { get; set; } = ValidateMarketDefinitionUpdateReport;

	/// <summary>Holds a FIX 5.0 SP2 MassQuote to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuote> MassQuote { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 5.0 SP2 MassQuoteAcknowledgement to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuoteAcknowledgement> MassQuoteAcknowledgement { get; set; } = ValidateMassQuoteAcknowledgement;

	/// <summary>Holds a FIX 5.0 SP2 MultilegOrderCancelReplace to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace> MultilegOrderCancelReplace { get; set; } = ValidateMultilegOrderCancelReplace;

	/// <summary>Holds a FIX 5.0 SP2 NetworkCounterpartySystemStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NetworkCounterpartySystemStatusRequest> NetworkCounterpartySystemStatusRequest { get; set; } = ValidateNetworkCounterpartySystemStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 NetworkCounterpartySystemStatusResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NetworkCounterpartySystemStatusResponse> NetworkCounterpartySystemStatusResponse { get; set; } = ValidateNetworkCounterpartySystemStatusResponse;

	/// <summary>Holds a FIX 5.0 SP2 NewOrderCross to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross> NewOrderCross { get; set; } = ValidateNewOrderCross;

	/// <summary>Holds a FIX 5.0 SP2 NewOrderList to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList> NewOrderList { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 5.0 SP2 NewOrderMultileg to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg> NewOrderMultileg { get; set; } = ValidateNewOrderMultileg;

	/// <summary>Holds a FIX 5.0 SP2 NewOrderSingle to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderSingle> NewOrderSingle { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 5.0 SP2 News to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News> News { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 5.0 SP2 OrderCancelReject to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReject> OrderCancelReject { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 5.0 SP2 OrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReplaceRequest> OrderCancelReplaceRequest { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 5.0 SP2 OrderCancelRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelRequest> OrderCancelRequest { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 5.0 SP2 OrderMassActionReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassActionReport> OrderMassActionReport { get; set; } = ValidateOrderMassActionReport;

	/// <summary>Holds a FIX 5.0 SP2 OrderMassActionRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassActionRequest> OrderMassActionRequest { get; set; } = ValidateOrderMassActionRequest;

	/// <summary>Holds a FIX 5.0 SP2 OrderMassCancelReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassCancelReport> OrderMassCancelReport { get; set; } = ValidateOrderMassCancelReport;

	/// <summary>Holds a FIX 5.0 SP2 OrderMassCancelRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassCancelRequest> OrderMassCancelRequest { get; set; } = ValidateOrderMassCancelRequest;

	/// <summary>Holds a FIX 5.0 SP2 OrderMassStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassStatusRequest> OrderMassStatusRequest { get; set; } = ValidateOrderMassStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 OrderStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderStatusRequest> OrderStatusRequest { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 PositionMaintenanceReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceReport> PositionMaintenanceReport { get; set; } = ValidatePositionMaintenanceReport;

	/// <summary>Holds a FIX 5.0 SP2 PositionMaintenanceRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceRequest> PositionMaintenanceRequest { get; set; } = ValidatePositionMaintenanceRequest;

	/// <summary>Holds a FIX 5.0 SP2 PositionReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionReport> PositionReport { get; set; } = ValidatePositionReport;

	/// <summary>Holds a FIX 5.0 SP2 Quote to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Quote> Quote { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 5.0 SP2 QuoteCancel to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteCancel> QuoteCancel { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequest> QuoteRequest { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 5.0 SP2 QuoteRequestReject to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequestReject> QuoteRequestReject { get; set; } = ValidateQuoteRequestReject;

	/// <summary>Holds a FIX 5.0 SP2 QuoteResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteResponse> QuoteResponse { get; set; } = ValidateQuoteResponse;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatusReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusReport> QuoteStatusReport { get; set; } = ValidateQuoteStatusReport;

	/// <summary>Holds a FIX 5.0 SP2 QuoteStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusRequest> QuoteStatusRequest { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 RFQRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RFQRequest> RFQRequest { get; set; } = ValidateRFQRequest;

	/// <summary>Holds a FIX 5.0 SP2 RegistrationInstructions to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RegistrationInstructions> RegistrationInstructions { get; set; } = ValidateRegistrationInstructions;

	/// <summary>Holds a FIX 5.0 SP2 RegistrationInstructionsResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RegistrationInstructionsResponse> RegistrationInstructionsResponse { get; set; } = ValidateRegistrationInstructionsResponse;

	/// <summary>Holds a FIX 5.0 SP2 Reject to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Reject> Reject { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 5.0 SP2 RequestForPositions to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositions> RequestForPositions { get; set; } = ValidateRequestForPositions;

	/// <summary>Holds a FIX 5.0 SP2 RequestForPositionsAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositionsAck> RequestForPositionsAck { get; set; } = ValidateRequestForPositionsAck;

	/// <summary>Holds a FIX 5.0 SP2 ResendRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ResendRequest> ResendRequest { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 5.0 SP2 SecurityDefinition to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition> SecurityDefinition { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 5.0 SP2 SecurityDefinitionRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionRequest> SecurityDefinitionRequest { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 5.0 SP2 SecurityDefinitionUpdateReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport> SecurityDefinitionUpdateReport { get; set; } = ValidateSecurityDefinitionUpdateReport;

	/// <summary>Holds a FIX 5.0 SP2 SecurityList to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList> SecurityList { get; set; } = ValidateSecurityList;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListRequest> SecurityListRequest { get; set; } = ValidateSecurityListRequest;

	/// <summary>Holds a FIX 5.0 SP2 SecurityListUpdateReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport> SecurityListUpdateReport { get; set; } = ValidateSecurityListUpdateReport;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatusMessage to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusMessage> SecurityStatusMessage { get; set; } = ValidateSecurityStatusMessage;

	/// <summary>Holds a FIX 5.0 SP2 SecurityStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusRequest> SecurityStatusRequest { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTypeRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityTypeRequest> SecurityTypeRequest { get; set; } = ValidateSecurityTypeRequest;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTypes to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityTypes> SecurityTypes { get; set; } = ValidateSecurityTypes;

	/// <summary>Holds a FIX 5.0 SP2 SequenceReset to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SequenceReset> SequenceReset { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 5.0 SP2 SettlementInstructionRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementInstructionRequest> SettlementInstructionRequest { get; set; } = ValidateSettlementInstructionRequest;

	/// <summary>Holds a FIX 5.0 SP2 SettlementInstructions to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementInstructions> SettlementInstructions { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 5.0 SP2 SettlementObligationReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementObligationReport> SettlementObligationReport { get; set; } = ValidateSettlementObligationReport;

	/// <summary>Holds a FIX 5.0 SP2 StreamAssignmentReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentReport> StreamAssignmentReport { get; set; } = ValidateStreamAssignmentReport;

	/// <summary>Holds a FIX 5.0 SP2 StreamAssignmentReportACK to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentReportACK> StreamAssignmentReportACK { get; set; } = ValidateStreamAssignmentReportACK;

	/// <summary>Holds a FIX 5.0 SP2 StreamAssignmentRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentRequest> StreamAssignmentRequest { get; set; } = ValidateStreamAssignmentRequest;

	/// <summary>Holds a FIX 5.0 SP2 TestRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TestRequest> TestRequest { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 5.0 SP2 TradeCaptureReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport> TradeCaptureReport { get; set; } = ValidateTradeCaptureReport;

	/// <summary>Holds a FIX 5.0 SP2 TradeCaptureReportAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck> TradeCaptureReportAck { get; set; } = ValidateTradeCaptureReportAck;

	/// <summary>Holds a FIX 5.0 SP2 TradeCaptureReportRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequest> TradeCaptureReportRequest { get; set; } = ValidateTradeCaptureReportRequest;

	/// <summary>Holds a FIX 5.0 SP2 TradeCaptureReportRequestAck to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequestAck> TradeCaptureReportRequestAck { get; set; } = ValidateTradeCaptureReportRequestAck;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionList to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList> TradingSessionList { get; set; } = ValidateTradingSessionList;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionListRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListRequest> TradingSessionListRequest { get; set; } = ValidateTradingSessionListRequest;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionListUpdateReport to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport> TradingSessionListUpdateReport { get; set; } = ValidateTradingSessionListUpdateReport;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionStatus to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionStatus> TradingSessionStatus { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 5.0 SP2 TradingSessionStatusRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionStatusRequest> TradingSessionStatusRequest { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds a FIX 5.0 SP2 UserNotification to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.UserNotification> UserNotification { get; set; } = ValidateUserNotification;

	/// <summary>Holds a FIX 5.0 SP2 UserRequest to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.UserRequest> UserRequest { get; set; } = ValidateUserRequest;

	/// <summary>Holds a FIX 5.0 SP2 UserResponse to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.UserResponse> UserResponse { get; set; } = ValidateUserResponse;

	/// <summary>Holds a FIX 5.0 SP2 XMLnonFIX to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.XMLnonFIX> XMLnonFIX { get; set; } = ValidateXMLnonFIX;

	/// <summary>Holds a FIX 5.0 SP2 ApplicationSequenceControl to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IApplicationSequenceControl> ApplicationSequenceControl { get; set; } = ValidateApplicationSequenceControl;

	/// <summary>Holds a FIX 5.0 SP2 CommissionData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ICommissionData> CommissionData { get; set; } = ValidateCommissionData;

	/// <summary>Holds a FIX 5.0 SP2 DiscretionInstructions to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IDiscretionInstructions> DiscretionInstructions { get; set; } = ValidateDiscretionInstructions;

	/// <summary>Holds a FIX 5.0 SP2 DisplayInstruction to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IDisplayInstruction> DisplayInstruction { get; set; } = ValidateDisplayInstruction;

	/// <summary>Holds a FIX 5.0 SP2 ExpirationQty to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IExpirationQty> ExpirationQty { get; set; } = ValidateExpirationQty;

	/// <summary>Holds a FIX 5.0 SP2 FinancingDetails to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IFinancingDetails> FinancingDetails { get; set; } = ValidateFinancingDetails;

	/// <summary>Holds a FIX 5.0 SP2 Instrument to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IInstrument> Instrument { get; set; } = ValidateInstrument;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentExtension to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentExtension> InstrumentExtension { get; set; } = ValidateInstrumentExtension;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentLeg to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentLeg> InstrumentLeg { get; set; } = ValidateInstrumentLeg;

	/// <summary>Holds a FIX 5.0 SP2 InstrumentParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentParties> InstrumentParties { get; set; } = ValidateInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 LegBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ILegBenchmarkCurveData> LegBenchmarkCurveData { get; set; } = ValidateLegBenchmarkCurveData;

	/// <summary>Holds a FIX 5.0 SP2 LegStipulations to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ILegStipulations> LegStipulations { get; set; } = ValidateLegStipulations;

	/// <summary>Holds a FIX 5.0 SP2 NestedParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, INestedParties> NestedParties { get; set; } = ValidateNestedParties;

	/// <summary>Holds a FIX 5.0 SP2 NestedParties2 to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, INestedParties2> NestedParties2 { get; set; } = ValidateNestedParties2;

	/// <summary>Holds a FIX 5.0 SP2 NestedParties3 to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, INestedParties3> NestedParties3 { get; set; } = ValidateNestedParties3;

	/// <summary>Holds a FIX 5.0 SP2 NestedParties4 to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, INestedParties4> NestedParties4 { get; set; } = ValidateNestedParties4;

	/// <summary>Holds a FIX 5.0 SP2 OrderQtyData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IOrderQtyData> OrderQtyData { get; set; } = ValidateOrderQtyData;

	/// <summary>Holds a FIX 5.0 SP2 Parties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IParties> Parties { get; set; } = ValidateParties;

	/// <summary>Holds a FIX 5.0 SP2 PegInstructions to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IPegInstructions> PegInstructions { get; set; } = ValidatePegInstructions;

	/// <summary>Holds a FIX 5.0 SP2 PositionAmountData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IPositionAmountData> PositionAmountData { get; set; } = ValidatePositionAmountData;

	/// <summary>Holds a FIX 5.0 SP2 PositionQty to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IPositionQty> PositionQty { get; set; } = ValidatePositionQty;

	/// <summary>Holds a FIX 5.0 SP2 RateSource to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IRateSource> RateSource { get; set; } = ValidateRateSource;

	/// <summary>Holds a FIX 5.0 SP2 RootParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IRootParties> RootParties { get; set; } = ValidateRootParties;

	/// <summary>Holds a FIX 5.0 SP2 SecurityTradingRules to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules> SecurityTradingRules { get; set; } = ValidateSecurityTradingRules;

	/// <summary>Holds a FIX 5.0 SP2 SettlInstructionsData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ISettlInstructionsData> SettlInstructionsData { get; set; } = ValidateSettlInstructionsData;

	/// <summary>Holds a FIX 5.0 SP2 SettlParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ISettlParties> SettlParties { get; set; } = ValidateSettlParties;

	/// <summary>Holds a FIX 5.0 SP2 SideTrdRegTS to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ISideTrdRegTS> SideTrdRegTS { get; set; } = ValidateSideTrdRegTS;

	/// <summary>Holds a FIX 5.0 SP2 SpreadOrBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ISpreadOrBenchmarkCurveData> SpreadOrBenchmarkCurveData { get; set; } = ValidateSpreadOrBenchmarkCurveData;

	/// <summary>Holds a FIX 5.0 SP2 Stipulations to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IStipulations> Stipulations { get; set; } = ValidateStipulations;

	/// <summary>Holds a FIX 5.0 SP2 TargetParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ITargetParties> TargetParties { get; set; } = ValidateTargetParties;

	/// <summary>Holds a FIX 5.0 SP2 TrdRegTimestamps to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ITrdRegTimestamps> TrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps;

	/// <summary>Holds a FIX 5.0 SP2 TriggeringInstruction to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, ITriggeringInstruction> TriggeringInstruction { get; set; } = ValidateTriggeringInstruction;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingAmount to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingAmount> UnderlyingAmount { get; set; } = ValidateUnderlyingAmount;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingInstrument to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingInstrument> UnderlyingInstrument { get; set; } = ValidateUnderlyingInstrument;

	/// <summary>Holds a FIX 5.0 SP2 UnderlyingStipulations to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingStipulations> UnderlyingStipulations { get; set; } = ValidateUnderlyingStipulations;

	/// <summary>Holds a FIX 5.0 SP2 UndlyInstrumentParties to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IUndlyInstrumentParties> UndlyInstrumentParties { get; set; } = ValidateUndlyInstrumentParties;

	/// <summary>Holds a FIX 5.0 SP2 YieldData to the schema, wherever it is carried.</summary>
	public Action<Fix50Context, FixCheck, IYieldData> YieldData { get; set; } = ValidateYieldData;

	/// <summary>Holds one entry of IExpirationQty.NoExpirationGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IExpirationQty.NoExpirationGroup> ExpirationQty_NoExpiration { get; set; } = ValidateExpirationQty_NoExpiration;

	/// <summary>Holds one entry of IInstrument.NoSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrument.NoSecurityAltIDGroup> Instrument_NoSecurityAltID { get; set; } = ValidateInstrument_NoSecurityAltID;

	/// <summary>Holds one entry of IInstrument.NoEventsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrument.NoEventsGroup> Instrument_NoEvents { get; set; } = ValidateInstrument_NoEvents;

	/// <summary>Holds one entry of IInstrument.NoComplexEventsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrument.NoComplexEventsGroup> Instrument_NoComplexEvents { get; set; } = ValidateInstrument_NoComplexEvents;

	/// <summary>Holds one entry of IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup> Instrument_NoComplexEvents_NoComplexEventDates { get; set; } = ValidateInstrument_NoComplexEvents_NoComplexEventDates;

	/// <summary>Holds one entry of IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup.NoComplexEventTimesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup.NoComplexEventTimesGroup> Instrument_NoComplexEvents_NoComplexEventDates_NoComplexEventTimes { get; set; } = ValidateInstrument_NoComplexEvents_NoComplexEventDates_NoComplexEventTimes;

	/// <summary>Holds one entry of IInstrumentExtension.NoInstrAttribGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentExtension.NoInstrAttribGroup> InstrumentExtension_NoInstrAttrib { get; set; } = ValidateInstrumentExtension_NoInstrAttrib;

	/// <summary>Holds one entry of IInstrumentLeg.NoLegSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentLeg.NoLegSecurityAltIDGroup> InstrumentLeg_NoLegSecurityAltID { get; set; } = ValidateInstrumentLeg_NoLegSecurityAltID;

	/// <summary>Holds one entry of IInstrumentParties.NoInstrumentPartiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentParties.NoInstrumentPartiesGroup> InstrumentParties_NoInstrumentParties { get; set; } = ValidateInstrumentParties_NoInstrumentParties;

	/// <summary>Holds one entry of IInstrumentParties.NoInstrumentPartiesGroup.NoInstrumentPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IInstrumentParties.NoInstrumentPartiesGroup.NoInstrumentPartySubIDsGroup> InstrumentParties_NoInstrumentParties_NoInstrumentPartySubIDs { get; set; } = ValidateInstrumentParties_NoInstrumentParties_NoInstrumentPartySubIDs;

	/// <summary>Holds one entry of ILegStipulations.NoLegStipulationsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ILegStipulations.NoLegStipulationsGroup> LegStipulations_NoLegStipulations { get; set; } = ValidateLegStipulations_NoLegStipulations;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties.NoNestedPartyIDsGroup> NestedParties_NoNestedPartyIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup> NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties2.NoNested2PartyIDsGroup> NestedParties2_NoNested2PartyIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup> NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties3.NoNested3PartyIDsGroup> NestedParties3_NoNested3PartyIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup> NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs;

	/// <summary>Holds one entry of INestedParties4.NoNested4PartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties4.NoNested4PartyIDsGroup> NestedParties4_NoNested4PartyIDs { get; set; } = ValidateNestedParties4_NoNested4PartyIDs;

	/// <summary>Holds one entry of INestedParties4.NoNested4PartyIDsGroup.NoNested4PartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, INestedParties4.NoNested4PartyIDsGroup.NoNested4PartySubIDsGroup> NestedParties4_NoNested4PartyIDs_NoNested4PartySubIDs { get; set; } = ValidateNestedParties4_NoNested4PartyIDs_NoNested4PartySubIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IParties.NoPartyIDsGroup> Parties_NoPartyIDs { get; set; } = ValidateParties_NoPartyIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup.NoPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IParties.NoPartyIDsGroup.NoPartySubIDsGroup> Parties_NoPartyIDs_NoPartySubIDs { get; set; } = ValidateParties_NoPartyIDs_NoPartySubIDs;

	/// <summary>Holds one entry of IPositionAmountData.NoPosAmtGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IPositionAmountData.NoPosAmtGroup> PositionAmountData_NoPosAmt { get; set; } = ValidatePositionAmountData_NoPosAmt;

	/// <summary>Holds one entry of IPositionQty.NoPositionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IPositionQty.NoPositionsGroup> PositionQty_NoPositions { get; set; } = ValidatePositionQty_NoPositions;

	/// <summary>Holds one entry of IRateSource.NoRateSourcesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IRateSource.NoRateSourcesGroup> RateSource_NoRateSources { get; set; } = ValidateRateSource_NoRateSources;

	/// <summary>Holds one entry of IRootParties.NoRootPartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IRootParties.NoRootPartyIDsGroup> RootParties_NoRootPartyIDs { get; set; } = ValidateRootParties_NoRootPartyIDs;

	/// <summary>Holds one entry of IRootParties.NoRootPartyIDsGroup.NoRootPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IRootParties.NoRootPartyIDsGroup.NoRootPartySubIDsGroup> RootParties_NoRootPartyIDs_NoRootPartySubIDs { get; set; } = ValidateRootParties_NoRootPartyIDs_NoRootPartySubIDs;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTickRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTickRulesGroup> SecurityTradingRules_NoTickRules { get; set; } = ValidateSecurityTradingRules_NoTickRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoLotTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoLotTypeRulesGroup> SecurityTradingRules_NoLotTypeRules { get; set; } = ValidateSecurityTradingRules_NoLotTypeRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup> SecurityTradingRules_NoTradingSessionRules { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup.NoOrdTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup.NoOrdTypeRulesGroup> SecurityTradingRules_NoTradingSessionRules_NoOrdTypeRules { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules_NoOrdTypeRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup.NoTimeInForceRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup.NoTimeInForceRulesGroup> SecurityTradingRules_NoTradingSessionRules_NoTimeInForceRules { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules_NoTimeInForceRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup.NoExecInstRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup.NoExecInstRulesGroup> SecurityTradingRules_NoTradingSessionRules_NoExecInstRules { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules_NoExecInstRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup.NoMatchRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup.NoMatchRulesGroup> SecurityTradingRules_NoTradingSessionRules_NoMatchRules { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules_NoMatchRules;

	/// <summary>Holds one entry of ISecurityTradingRules.NoTradingSessionRulesGroup.NoMDFeedTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoTradingSessionRulesGroup.NoMDFeedTypesGroup> SecurityTradingRules_NoTradingSessionRules_NoMDFeedTypes { get; set; } = ValidateSecurityTradingRules_NoTradingSessionRules_NoMDFeedTypes;

	/// <summary>Holds one entry of ISecurityTradingRules.NoNestedInstrAttribGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISecurityTradingRules.NoNestedInstrAttribGroup> SecurityTradingRules_NoNestedInstrAttrib { get; set; } = ValidateSecurityTradingRules_NoNestedInstrAttrib;

	/// <summary>Holds one entry of ISettlInstructionsData.NoDlvyInstGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISettlInstructionsData.NoDlvyInstGroup> SettlInstructionsData_NoDlvyInst { get; set; } = ValidateSettlInstructionsData_NoDlvyInst;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISettlParties.NoSettlPartyIDsGroup> SettlParties_NoSettlPartyIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup> SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs;

	/// <summary>Holds one entry of ISideTrdRegTS.NoSideTrdRegTSGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ISideTrdRegTS.NoSideTrdRegTSGroup> SideTrdRegTS_NoSideTrdRegTS { get; set; } = ValidateSideTrdRegTS_NoSideTrdRegTS;

	/// <summary>Holds one entry of IStipulations.NoStipulationsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IStipulations.NoStipulationsGroup> Stipulations_NoStipulations { get; set; } = ValidateStipulations_NoStipulations;

	/// <summary>Holds one entry of ITargetParties.NoTargetPartyIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ITargetParties.NoTargetPartyIDsGroup> TargetParties_NoTargetPartyIDs { get; set; } = ValidateTargetParties_NoTargetPartyIDs;

	/// <summary>Holds one entry of ITrdRegTimestamps.NoTrdRegTimestampsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, ITrdRegTimestamps.NoTrdRegTimestampsGroup> TrdRegTimestamps_NoTrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps_NoTrdRegTimestamps;

	/// <summary>Holds one entry of IUnderlyingAmount.NoUnderlyingAmountsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingAmount.NoUnderlyingAmountsGroup> UnderlyingAmount_NoUnderlyingAmounts { get; set; } = ValidateUnderlyingAmount_NoUnderlyingAmounts;

	/// <summary>Holds one entry of IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup> UnderlyingInstrument_NoUnderlyingSecurityAltID { get; set; } = ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID;

	/// <summary>Holds one entry of IUnderlyingStipulations.NoUnderlyingStipsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IUnderlyingStipulations.NoUnderlyingStipsGroup> UnderlyingStipulations_NoUnderlyingStips { get; set; } = ValidateUnderlyingStipulations_NoUnderlyingStips;

	/// <summary>Holds one entry of IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup> UndlyInstrumentParties_NoUndlyInstrumentParties { get; set; } = ValidateUndlyInstrumentParties_NoUndlyInstrumentParties;

	/// <summary>Holds one entry of IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup.NoUndlyInstrumentPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup.NoUndlyInstrumentPartySubIDsGroup> UndlyInstrumentParties_NoUndlyInstrumentParties_NoUndlyInstrumentPartySubIDs { get; set; } = ValidateUndlyInstrumentParties_NoUndlyInstrumentParties_NoUndlyInstrumentPartySubIDs;

	/// <summary>Holds one entry of FixMessage.AdjustedPositionReport.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AdjustedPositionReport.NoRelatedSymGroup> AdjustedPositionReport_NoRelatedSym { get; set; } = ValidateAdjustedPositionReport_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Advertisement.NoLegsGroup> Advertisement_NoLegs { get; set; } = ValidateAdvertisement_NoLegs;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Advertisement.NoUnderlyingsGroup> Advertisement_NoUnderlyings { get; set; } = ValidateAdvertisement_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoOrdersGroup> AllocationInstruction_NoOrders { get; set; } = ValidateAllocationInstruction_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoExecsGroup> AllocationInstruction_NoExecs { get; set; } = ValidateAllocationInstruction_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoUnderlyingsGroup> AllocationInstruction_NoUnderlyings { get; set; } = ValidateAllocationInstruction_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoLegsGroup> AllocationInstruction_NoLegs { get; set; } = ValidateAllocationInstruction_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup> AllocationInstruction_NoAllocs { get; set; } = ValidateAllocationInstruction_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup> AllocationInstruction_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationInstruction_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup> AllocationInstruction_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationInstruction_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAck.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAck.NoAllocsGroup> AllocationInstructionAck_NoAllocs { get; set; } = ValidateAllocationInstructionAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoOrdersGroup> AllocationInstructionAlert_NoOrders { get; set; } = ValidateAllocationInstructionAlert_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoExecsGroup> AllocationInstructionAlert_NoExecs { get; set; } = ValidateAllocationInstructionAlert_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoUnderlyingsGroup> AllocationInstructionAlert_NoUnderlyings { get; set; } = ValidateAllocationInstructionAlert_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoLegsGroup> AllocationInstructionAlert_NoLegs { get; set; } = ValidateAllocationInstructionAlert_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoAllocsGroup> AllocationInstructionAlert_NoAllocs { get; set; } = ValidateAllocationInstructionAlert_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoMiscFeesGroup> AllocationInstructionAlert_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationInstructionAlert_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoClearingInstructionsGroup> AllocationInstructionAlert_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationInstructionAlert_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoOrdersGroup> AllocationReport_NoOrders { get; set; } = ValidateAllocationReport_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoExecsGroup> AllocationReport_NoExecs { get; set; } = ValidateAllocationReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoUnderlyingsGroup> AllocationReport_NoUnderlyings { get; set; } = ValidateAllocationReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoLegsGroup> AllocationReport_NoLegs { get; set; } = ValidateAllocationReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup> AllocationReport_NoAllocs { get; set; } = ValidateAllocationReport_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup> AllocationReport_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationReport_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup> AllocationReport_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationReport_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationReportAck.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AllocationReportAck.NoAllocsGroup> AllocationReportAck_NoAllocs { get; set; } = ValidateAllocationReportAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.ApplicationMessageReport.NoApplIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageReport.NoApplIDsGroup> ApplicationMessageReport_NoApplIDs { get; set; } = ValidateApplicationMessageReport_NoApplIDs;

	/// <summary>Holds one entry of FixMessage.ApplicationMessageRequest.NoApplIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageRequest.NoApplIDsGroup> ApplicationMessageRequest_NoApplIDs { get; set; } = ValidateApplicationMessageRequest_NoApplIDs;

	/// <summary>Holds one entry of FixMessage.ApplicationMessageRequestAck.NoApplIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ApplicationMessageRequestAck.NoApplIDsGroup> ApplicationMessageRequestAck_NoApplIDs { get; set; } = ValidateApplicationMessageRequestAck_NoApplIDs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AssignmentReport.NoLegsGroup> AssignmentReport_NoLegs { get; set; } = ValidateAssignmentReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.AssignmentReport.NoUnderlyingsGroup> AssignmentReport_NoUnderlyings { get; set; } = ValidateAssignmentReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidDescriptorsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BidRequest.NoBidDescriptorsGroup> BidRequest_NoBidDescriptors { get; set; } = ValidateBidRequest_NoBidDescriptors;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BidRequest.NoBidComponentsGroup> BidRequest_NoBidComponents { get; set; } = ValidateBidRequest_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.BidResponse.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.BidResponse.NoBidComponentsGroup> BidResponse_NoBidComponents { get; set; } = ValidateBidResponse_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment.NoExecsGroup> CollateralAssignment_NoExecs { get; set; } = ValidateCollateralAssignment_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment.NoTradesGroup> CollateralAssignment_NoTrades { get; set; } = ValidateCollateralAssignment_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment.NoLegsGroup> CollateralAssignment_NoLegs { get; set; } = ValidateCollateralAssignment_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment.NoUnderlyingsGroup> CollateralAssignment_NoUnderlyings { get; set; } = ValidateCollateralAssignment_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralAssignment.NoMiscFeesGroup> CollateralAssignment_NoMiscFees { get; set; } = ValidateCollateralAssignment_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup> CollateralInquiry_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiry_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry.NoExecsGroup> CollateralInquiry_NoExecs { get; set; } = ValidateCollateralInquiry_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry.NoTradesGroup> CollateralInquiry_NoTrades { get; set; } = ValidateCollateralInquiry_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry.NoLegsGroup> CollateralInquiry_NoLegs { get; set; } = ValidateCollateralInquiry_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiry.NoUnderlyingsGroup> CollateralInquiry_NoUnderlyings { get; set; } = ValidateCollateralInquiry_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup> CollateralInquiryAck_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiryAck_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck.NoExecsGroup> CollateralInquiryAck_NoExecs { get; set; } = ValidateCollateralInquiryAck_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck.NoTradesGroup> CollateralInquiryAck_NoTrades { get; set; } = ValidateCollateralInquiryAck_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck.NoLegsGroup> CollateralInquiryAck_NoLegs { get; set; } = ValidateCollateralInquiryAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup> CollateralInquiryAck_NoUnderlyings { get; set; } = ValidateCollateralInquiryAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport.NoExecsGroup> CollateralReport_NoExecs { get; set; } = ValidateCollateralReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport.NoTradesGroup> CollateralReport_NoTrades { get; set; } = ValidateCollateralReport_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport.NoLegsGroup> CollateralReport_NoLegs { get; set; } = ValidateCollateralReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport.NoUnderlyingsGroup> CollateralReport_NoUnderlyings { get; set; } = ValidateCollateralReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralReport.NoMiscFeesGroup> CollateralReport_NoMiscFees { get; set; } = ValidateCollateralReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest.NoExecsGroup> CollateralRequest_NoExecs { get; set; } = ValidateCollateralRequest_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest.NoTradesGroup> CollateralRequest_NoTrades { get; set; } = ValidateCollateralRequest_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest.NoLegsGroup> CollateralRequest_NoLegs { get; set; } = ValidateCollateralRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest.NoUnderlyingsGroup> CollateralRequest_NoUnderlyings { get; set; } = ValidateCollateralRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralRequest.NoMiscFeesGroup> CollateralRequest_NoMiscFees { get; set; } = ValidateCollateralRequest_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoExecsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse.NoExecsGroup> CollateralResponse_NoExecs { get; set; } = ValidateCollateralResponse_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoTradesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse.NoTradesGroup> CollateralResponse_NoTrades { get; set; } = ValidateCollateralResponse_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse.NoLegsGroup> CollateralResponse_NoLegs { get; set; } = ValidateCollateralResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse.NoUnderlyingsGroup> CollateralResponse_NoUnderlyings { get; set; } = ValidateCollateralResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CollateralResponse.NoMiscFeesGroup> CollateralResponse_NoMiscFees { get; set; } = ValidateCollateralResponse_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation.NoOrdersGroup> Confirmation_NoOrders { get; set; } = ValidateConfirmation_NoOrders;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation.NoUnderlyingsGroup> Confirmation_NoUnderlyings { get; set; } = ValidateConfirmation_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation.NoLegsGroup> Confirmation_NoLegs { get; set; } = ValidateConfirmation_NoLegs;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoCapacitiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation.NoCapacitiesGroup> Confirmation_NoCapacities { get; set; } = ValidateConfirmation_NoCapacities;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Confirmation.NoMiscFeesGroup> Confirmation_NoMiscFees { get; set; } = ValidateConfirmation_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.ConfirmationRequest.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ConfirmationRequest.NoOrdersGroup> ConfirmationRequest_NoOrders { get; set; } = ValidateConfirmationRequest_NoOrders;

	/// <summary>Holds one entry of FixMessage.ContraryIntentionReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ContraryIntentionReport.NoUnderlyingsGroup> ContraryIntentionReport_NoUnderlyings { get; set; } = ValidateContraryIntentionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup> CrossOrderCancelReplaceRequest_NoSides { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup> CrossOrderCancelReplaceRequest_NoSides_NoAllocs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup> CrossOrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup> CrossOrderCancelReplaceRequest_NoLegs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup> CrossOrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoStrategyParametersGroup> CrossOrderCancelReplaceRequest_NoStrategyParameters { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoSidesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoSidesGroup> CrossOrderCancelRequest_NoSides { get; set; } = ValidateCrossOrderCancelRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup> CrossOrderCancelRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoLegsGroup> CrossOrderCancelRequest_NoLegs { get; set; } = ValidateCrossOrderCancelRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoDerivativeSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoDerivativeSecurityAltIDGroup> DerivativeSecurityList_NoDerivativeSecurityAltID { get; set; } = ValidateDerivativeSecurityList_NoDerivativeSecurityAltID;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoDerivativeEventsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoDerivativeEventsGroup> DerivativeSecurityList_NoDerivativeEvents { get; set; } = ValidateDerivativeSecurityList_NoDerivativeEvents;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup> DerivativeSecurityList_NoDerivativeInstrumentParties { get; set; } = ValidateDerivativeSecurityList_NoDerivativeInstrumentParties;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup> DerivativeSecurityList_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs { get; set; } = ValidateDerivativeSecurityList_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoDerivativeInstrAttribGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoDerivativeInstrAttribGroup> DerivativeSecurityList_NoDerivativeInstrAttrib { get; set; } = ValidateDerivativeSecurityList_NoDerivativeInstrAttrib;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup> DerivativeSecurityList_NoMarketSegments { get; set; } = ValidateDerivativeSecurityList_NoMarketSegments;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup> DerivativeSecurityList_NoMarketSegments_NoStrikeRules { get; set; } = ValidateDerivativeSecurityList_NoMarketSegments_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> DerivativeSecurityList_NoMarketSegments_NoStrikeRules_NoMaturityRules { get; set; } = ValidateDerivativeSecurityList_NoMarketSegments_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoRelatedSymGroup> DerivativeSecurityList_NoRelatedSym { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup> DerivativeSecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListRequest.NoDerivativeSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListRequest.NoDerivativeSecurityAltIDGroup> DerivativeSecurityListRequest_NoDerivativeSecurityAltID { get; set; } = ValidateDerivativeSecurityListRequest_NoDerivativeSecurityAltID;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListRequest.NoDerivativeEventsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListRequest.NoDerivativeEventsGroup> DerivativeSecurityListRequest_NoDerivativeEvents { get; set; } = ValidateDerivativeSecurityListRequest_NoDerivativeEvents;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup> DerivativeSecurityListRequest_NoDerivativeInstrumentParties { get; set; } = ValidateDerivativeSecurityListRequest_NoDerivativeInstrumentParties;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup> DerivativeSecurityListRequest_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs { get; set; } = ValidateDerivativeSecurityListRequest_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeSecurityAltIDGroup> DerivativeSecurityListUpdateReport_NoDerivativeSecurityAltID { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoDerivativeSecurityAltID;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeEventsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeEventsGroup> DerivativeSecurityListUpdateReport_NoDerivativeEvents { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoDerivativeEvents;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup> DerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup> DerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrAttribGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrAttribGroup> DerivativeSecurityListUpdateReport_NoDerivativeInstrAttrib { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrAttrib;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup> DerivativeSecurityListUpdateReport_NoMarketSegments { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoMarketSegments;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup> DerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> DerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup> DerivativeSecurityListUpdateReport_NoRelatedSym { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup> DerivativeSecurityListUpdateReport_NoRelatedSym_NoLegs { get; set; } = ValidateDerivativeSecurityListUpdateReport_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DontKnowTrade.NoUnderlyingsGroup> DontKnowTrade_NoUnderlyings { get; set; } = ValidateDontKnowTrade_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.DontKnowTrade.NoLegsGroup> DontKnowTrade_NoLegs { get; set; } = ValidateDontKnowTrade_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email.NoRoutingIDsGroup> Email_NoRoutingIDs { get; set; } = ValidateEmail_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.Email.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email.NoRelatedSymGroup> Email_NoRelatedSym { get; set; } = ValidateEmail_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Email.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email.NoUnderlyingsGroup> Email_NoUnderlyings { get; set; } = ValidateEmail_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Email.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email.NoLegsGroup> Email_NoLegs { get; set; } = ValidateEmail_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.NoLinesOfTextGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Email.NoLinesOfTextGroup> Email_NoLinesOfText { get; set; } = ValidateEmail_NoLinesOfText;

	/// <summary>Holds one entry of FixMessage.ExecutionAcknowledgement.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionAcknowledgement.NoUnderlyingsGroup> ExecutionAcknowledgement_NoUnderlyings { get; set; } = ValidateExecutionAcknowledgement_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.ExecutionAcknowledgement.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionAcknowledgement.NoLegsGroup> ExecutionAcknowledgement_NoLegs { get; set; } = ValidateExecutionAcknowledgement_NoLegs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContraBrokersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoContraBrokersGroup> ExecutionReport_NoContraBrokers { get; set; } = ValidateExecutionReport_NoContraBrokers;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoAllocsGroup> ExecutionReport_NoAllocs { get; set; } = ValidateExecutionReport_NoAllocs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoUnderlyingsGroup> ExecutionReport_NoUnderlyings { get; set; } = ValidateExecutionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoStrategyParametersGroup> ExecutionReport_NoStrategyParameters { get; set; } = ValidateExecutionReport_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoFillsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoFillsGroup> ExecutionReport_NoFills { get; set; } = ValidateExecutionReport_NoFills;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContAmtsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoContAmtsGroup> ExecutionReport_NoContAmts { get; set; } = ValidateExecutionReport_NoContAmts;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoLegsGroup> ExecutionReport_NoLegs { get; set; } = ValidateExecutionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoLegsGroup.NoLegAllocsGroup> ExecutionReport_NoLegs_NoLegAllocs { get; set; } = ValidateExecutionReport_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ExecutionReport.NoMiscFeesGroup> ExecutionReport_NoMiscFees { get; set; } = ValidateExecutionReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.IOI.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.IOI.NoUnderlyingsGroup> IOI_NoUnderlyings { get; set; } = ValidateIOI_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.IOI.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.IOI.NoLegsGroup> IOI_NoLegs { get; set; } = ValidateIOI_NoLegs;

	/// <summary>Holds one entry of FixMessage.IOI.NoIOIQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.IOI.NoIOIQualifiersGroup> IOI_NoIOIQualifiers { get; set; } = ValidateIOI_NoIOIQualifiers;

	/// <summary>Holds one entry of FixMessage.IOI.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.IOI.NoRoutingIDsGroup> IOI_NoRoutingIDs { get; set; } = ValidateIOI_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.ListStatus.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStatus.NoOrdersGroup> ListStatus_NoOrders { get; set; } = ValidateListStatus_NoOrders;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStrikePrice.NoStrikesGroup> ListStrikePrice_NoStrikes { get; set; } = ValidateListStrikePrice_NoStrikes;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.ListStrikePrice.NoStrikesGroup.NoUnderlyingsGroup> ListStrikePrice_NoStrikes_NoUnderlyings { get; set; } = ValidateListStrikePrice_NoStrikes_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Logon.NoMsgTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Logon.NoMsgTypesGroup> Logon_NoMsgTypes { get; set; } = ValidateLogon_NoMsgTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup> MarketDataIncrementalRefresh_NoMDEntries { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup> MarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup> MarketDataIncrementalRefresh_NoMDEntries_NoLegs { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoOfSecSizesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoOfSecSizesGroup> MarketDataIncrementalRefresh_NoMDEntries_NoOfSecSizes { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoOfSecSizes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoStatsIndicatorsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoStatsIndicatorsGroup> MarketDataIncrementalRefresh_NoMDEntries_NoStatsIndicators { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoStatsIndicators;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoRoutingIDsGroup> MarketDataIncrementalRefresh_NoRoutingIDs { get; set; } = ValidateMarketDataIncrementalRefresh_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoMDEntryTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest.NoMDEntryTypesGroup> MarketDataRequest_NoMDEntryTypes { get; set; } = ValidateMarketDataRequest_NoMDEntryTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup> MarketDataRequest_NoRelatedSym { get; set; } = ValidateMarketDataRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup> MarketDataRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup> MarketDataRequest_NoRelatedSym_NoLegs { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequest.NoTradingSessionsGroup> MarketDataRequest_NoTradingSessions { get; set; } = ValidateMarketDataRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MarketDataRequestReject.NoAltMDSourceGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup> MarketDataRequestReject_NoAltMDSource { get; set; } = ValidateMarketDataRequestReject_NoAltMDSource;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup> MarketDataSnapshotFullRefresh_NoUnderlyings { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup> MarketDataSnapshotFullRefresh_NoLegs { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup> MarketDataSnapshotFullRefresh_NoMDEntries { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup.NoOfSecSizesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup.NoOfSecSizesGroup> MarketDataSnapshotFullRefresh_NoMDEntries_NoOfSecSizes { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries_NoOfSecSizes;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoRoutingIDsGroup> MarketDataSnapshotFullRefresh_NoRoutingIDs { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.MarketDefinition.NoTickRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition.NoTickRulesGroup> MarketDefinition_NoTickRules { get; set; } = ValidateMarketDefinition_NoTickRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinition.NoLotTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition.NoLotTypeRulesGroup> MarketDefinition_NoLotTypeRules { get; set; } = ValidateMarketDefinition_NoLotTypeRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinition.NoOrdTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition.NoOrdTypeRulesGroup> MarketDefinition_NoOrdTypeRules { get; set; } = ValidateMarketDefinition_NoOrdTypeRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinition.NoTimeInForceRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition.NoTimeInForceRulesGroup> MarketDefinition_NoTimeInForceRules { get; set; } = ValidateMarketDefinition_NoTimeInForceRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinition.NoExecInstRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinition.NoExecInstRulesGroup> MarketDefinition_NoExecInstRules { get; set; } = ValidateMarketDefinition_NoExecInstRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinitionUpdateReport.NoTickRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport.NoTickRulesGroup> MarketDefinitionUpdateReport_NoTickRules { get; set; } = ValidateMarketDefinitionUpdateReport_NoTickRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinitionUpdateReport.NoLotTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport.NoLotTypeRulesGroup> MarketDefinitionUpdateReport_NoLotTypeRules { get; set; } = ValidateMarketDefinitionUpdateReport_NoLotTypeRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinitionUpdateReport.NoOrdTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport.NoOrdTypeRulesGroup> MarketDefinitionUpdateReport_NoOrdTypeRules { get; set; } = ValidateMarketDefinitionUpdateReport_NoOrdTypeRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinitionUpdateReport.NoTimeInForceRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport.NoTimeInForceRulesGroup> MarketDefinitionUpdateReport_NoTimeInForceRules { get; set; } = ValidateMarketDefinitionUpdateReport_NoTimeInForceRules;

	/// <summary>Holds one entry of FixMessage.MarketDefinitionUpdateReport.NoExecInstRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MarketDefinitionUpdateReport.NoExecInstRulesGroup> MarketDefinitionUpdateReport_NoExecInstRules { get; set; } = ValidateMarketDefinitionUpdateReport_NoExecInstRules;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup> MassQuote_NoQuoteSets { get; set; } = ValidateMassQuote_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup> MassQuote_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup> MassQuote_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup> MassQuoteAcknowledgement_NoQuoteSets { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoAllocsGroup> MultilegOrderCancelReplace_NoAllocs { get; set; } = ValidateMultilegOrderCancelReplace_NoAllocs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoTradingSessionsGroup> MultilegOrderCancelReplace_NoTradingSessions { get; set; } = ValidateMultilegOrderCancelReplace_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoUnderlyingsGroup> MultilegOrderCancelReplace_NoUnderlyings { get; set; } = ValidateMultilegOrderCancelReplace_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoLegsGroup> MultilegOrderCancelReplace_NoLegs { get; set; } = ValidateMultilegOrderCancelReplace_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoLegsGroup.NoLegAllocsGroup> MultilegOrderCancelReplace_NoLegs_NoLegAllocs { get; set; } = ValidateMultilegOrderCancelReplace_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplace.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.MultilegOrderCancelReplace.NoStrategyParametersGroup> MultilegOrderCancelReplace_NoStrategyParameters { get; set; } = ValidateMultilegOrderCancelReplace_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.NetworkCounterpartySystemStatusRequest.NoCompIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NetworkCounterpartySystemStatusRequest.NoCompIDsGroup> NetworkCounterpartySystemStatusRequest_NoCompIDs { get; set; } = ValidateNetworkCounterpartySystemStatusRequest_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NetworkCounterpartySystemStatusResponse.NoCompIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NetworkCounterpartySystemStatusResponse.NoCompIDsGroup> NetworkCounterpartySystemStatusResponse_NoCompIDs { get; set; } = ValidateNetworkCounterpartySystemStatusResponse_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoSidesGroup> NewOrderCross_NoSides { get; set; } = ValidateNewOrderCross_NoSides;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup> NewOrderCross_NoSides_NoAllocs { get; set; } = ValidateNewOrderCross_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoUnderlyingsGroup> NewOrderCross_NoUnderlyings { get; set; } = ValidateNewOrderCross_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoLegsGroup> NewOrderCross_NoLegs { get; set; } = ValidateNewOrderCross_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoTradingSessionsGroup> NewOrderCross_NoTradingSessions { get; set; } = ValidateNewOrderCross_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderCross.NoStrategyParametersGroup> NewOrderCross_NoStrategyParameters { get; set; } = ValidateNewOrderCross_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup> NewOrderList_NoOrders { get; set; } = ValidateNewOrderList_NoOrders;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup> NewOrderList_NoOrders_NoAllocs { get; set; } = ValidateNewOrderList_NoOrders_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup> NewOrderList_NoOrders_NoTradingSessions { get; set; } = ValidateNewOrderList_NoOrders_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup> NewOrderList_NoOrders_NoUnderlyings { get; set; } = ValidateNewOrderList_NoOrders_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoStrategyParametersGroup> NewOrderList_NoOrders_NoStrategyParameters { get; set; } = ValidateNewOrderList_NoOrders_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoAllocsGroup> NewOrderMultileg_NoAllocs { get; set; } = ValidateNewOrderMultileg_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoTradingSessionsGroup> NewOrderMultileg_NoTradingSessions { get; set; } = ValidateNewOrderMultileg_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoUnderlyingsGroup> NewOrderMultileg_NoUnderlyings { get; set; } = ValidateNewOrderMultileg_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoLegsGroup> NewOrderMultileg_NoLegs { get; set; } = ValidateNewOrderMultileg_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup> NewOrderMultileg_NoLegs_NoLegAllocs { get; set; } = ValidateNewOrderMultileg_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderMultileg.NoStrategyParametersGroup> NewOrderMultileg_NoStrategyParameters { get; set; } = ValidateNewOrderMultileg_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderSingle.NoAllocsGroup> NewOrderSingle_NoAllocs { get; set; } = ValidateNewOrderSingle_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderSingle.NoTradingSessionsGroup> NewOrderSingle_NoTradingSessions { get; set; } = ValidateNewOrderSingle_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderSingle.NoUnderlyingsGroup> NewOrderSingle_NoUnderlyings { get; set; } = ValidateNewOrderSingle_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.NewOrderSingle.NoStrategyParametersGroup> NewOrderSingle_NoStrategyParameters { get; set; } = ValidateNewOrderSingle_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.News.NoNewsRefIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoNewsRefIDsGroup> News_NoNewsRefIDs { get; set; } = ValidateNews_NoNewsRefIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoRoutingIDsGroup> News_NoRoutingIDs { get; set; } = ValidateNews_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoRelatedSymGroup> News_NoRelatedSym { get; set; } = ValidateNews_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.News.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoLegsGroup> News_NoLegs { get; set; } = ValidateNews_NoLegs;

	/// <summary>Holds one entry of FixMessage.News.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoUnderlyingsGroup> News_NoUnderlyings { get; set; } = ValidateNews_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.News.NoLinesOfTextGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.News.NoLinesOfTextGroup> News_NoLinesOfText { get; set; } = ValidateNews_NoLinesOfText;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup> OrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup> OrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup> OrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoStrategyParametersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoStrategyParametersGroup> OrderCancelReplaceRequest_NoStrategyParameters { get; set; } = ValidateOrderCancelReplaceRequest_NoStrategyParameters;

	/// <summary>Holds one entry of FixMessage.OrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderCancelRequest.NoUnderlyingsGroup> OrderCancelRequest_NoUnderlyings { get; set; } = ValidateOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderMassActionReport.NoAffectedOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassActionReport.NoAffectedOrdersGroup> OrderMassActionReport_NoAffectedOrders { get; set; } = ValidateOrderMassActionReport_NoAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderMassActionReport.NoNotAffectedOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassActionReport.NoNotAffectedOrdersGroup> OrderMassActionReport_NoNotAffectedOrders { get; set; } = ValidateOrderMassActionReport_NoNotAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup> OrderMassCancelReport_NoAffectedOrders { get; set; } = ValidateOrderMassCancelReport_NoAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderMassCancelReport.NoNotAffectedOrdersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderMassCancelReport.NoNotAffectedOrdersGroup> OrderMassCancelReport_NoNotAffectedOrders { get; set; } = ValidateOrderMassCancelReport_NoNotAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.OrderStatusRequest.NoUnderlyingsGroup> OrderStatusRequest_NoUnderlyings { get; set; } = ValidateOrderStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceReport.NoLegsGroup> PositionMaintenanceReport_NoLegs { get; set; } = ValidatePositionMaintenanceReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup> PositionMaintenanceReport_NoUnderlyings { get; set; } = ValidatePositionMaintenanceReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup> PositionMaintenanceReport_NoTradingSessions { get; set; } = ValidatePositionMaintenanceReport_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoLegsGroup> PositionMaintenanceRequest_NoLegs { get; set; } = ValidatePositionMaintenanceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup> PositionMaintenanceRequest_NoUnderlyings { get; set; } = ValidatePositionMaintenanceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup> PositionMaintenanceRequest_NoTradingSessions { get; set; } = ValidatePositionMaintenanceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionReport.NoLegsGroup> PositionReport_NoLegs { get; set; } = ValidatePositionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.PositionReport.NoUnderlyingsGroup> PositionReport_NoUnderlyings { get; set; } = ValidatePositionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Quote.NoQuoteQualifiersGroup> Quote_NoQuoteQualifiers { get; set; } = ValidateQuote_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.Quote.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Quote.NoUnderlyingsGroup> Quote_NoUnderlyings { get; set; } = ValidateQuote_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.Quote.NoLegsGroup> Quote_NoLegs { get; set; } = ValidateQuote_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup> QuoteCancel_NoQuoteEntries { get; set; } = ValidateQuoteCancel_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup> QuoteCancel_NoQuoteEntries_NoUnderlyings { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup> QuoteCancel_NoQuoteEntries_NoLegs { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup> QuoteRequest_NoRelatedSym { get; set; } = ValidateQuoteRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup> QuoteRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup> QuoteRequest_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup> QuoteRequest_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup> QuoteRequestReject_NoRelatedSym { get; set; } = ValidateQuoteRequestReject_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup> QuoteRequestReject_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup> QuoteRequestReject_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup> QuoteRequestReject_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteResponse.NoQuoteQualifiersGroup> QuoteResponse_NoQuoteQualifiers { get; set; } = ValidateQuoteResponse_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteResponse.NoUnderlyingsGroup> QuoteResponse_NoUnderlyings { get; set; } = ValidateQuoteResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteResponse.NoLegsGroup> QuoteResponse_NoLegs { get; set; } = ValidateQuoteResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusReport.NoUnderlyingsGroup> QuoteStatusReport_NoUnderlyings { get; set; } = ValidateQuoteStatusReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusReport.NoLegsGroup> QuoteStatusReport_NoLegs { get; set; } = ValidateQuoteStatusReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup> QuoteStatusReport_NoQuoteQualifiers { get; set; } = ValidateQuoteStatusReport_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup> QuoteStatusRequest_NoUnderlyings { get; set; } = ValidateQuoteStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.QuoteStatusRequest.NoLegsGroup> QuoteStatusRequest_NoLegs { get; set; } = ValidateQuoteStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup> RFQRequest_NoRelatedSym { get; set; } = ValidateRFQRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup> RFQRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateRFQRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup> RFQRequest_NoRelatedSym_NoLegs { get; set; } = ValidateRFQRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoRegistDtlsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RegistrationInstructions.NoRegistDtlsGroup> RegistrationInstructions_NoRegistDtls { get; set; } = ValidateRegistrationInstructions_NoRegistDtls;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoDistribInstsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RegistrationInstructions.NoDistribInstsGroup> RegistrationInstructions_NoDistribInsts { get; set; } = ValidateRegistrationInstructions_NoDistribInsts;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositions.NoLegsGroup> RequestForPositions_NoLegs { get; set; } = ValidateRequestForPositions_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositions.NoUnderlyingsGroup> RequestForPositions_NoUnderlyings { get; set; } = ValidateRequestForPositions_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositions.NoTradingSessionsGroup> RequestForPositions_NoTradingSessions { get; set; } = ValidateRequestForPositions_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositionsAck.NoLegsGroup> RequestForPositionsAck_NoLegs { get; set; } = ValidateRequestForPositionsAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup> RequestForPositionsAck_NoUnderlyings { get; set; } = ValidateRequestForPositionsAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition.NoUnderlyingsGroup> SecurityDefinition_NoUnderlyings { get; set; } = ValidateSecurityDefinition_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition.NoLegsGroup> SecurityDefinition_NoLegs { get; set; } = ValidateSecurityDefinition_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoMarketSegmentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition.NoMarketSegmentsGroup> SecurityDefinition_NoMarketSegments { get; set; } = ValidateSecurityDefinition_NoMarketSegments;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup> SecurityDefinition_NoMarketSegments_NoStrikeRules { get; set; } = ValidateSecurityDefinition_NoMarketSegments_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> SecurityDefinition_NoMarketSegments_NoStrikeRules_NoMaturityRules { get; set; } = ValidateSecurityDefinition_NoMarketSegments_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup> SecurityDefinitionRequest_NoUnderlyings { get; set; } = ValidateSecurityDefinitionRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionRequest.NoLegsGroup> SecurityDefinitionRequest_NoLegs { get; set; } = ValidateSecurityDefinitionRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionUpdateReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport.NoUnderlyingsGroup> SecurityDefinitionUpdateReport_NoUnderlyings { get; set; } = ValidateSecurityDefinitionUpdateReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionUpdateReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport.NoLegsGroup> SecurityDefinitionUpdateReport_NoLegs { get; set; } = ValidateSecurityDefinitionUpdateReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup> SecurityDefinitionUpdateReport_NoMarketSegments { get; set; } = ValidateSecurityDefinitionUpdateReport_NoMarketSegments;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup> SecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules { get; set; } = ValidateSecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> SecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules { get; set; } = ValidateSecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup> SecurityList_NoRelatedSym { get; set; } = ValidateSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup> SecurityList_NoRelatedSym_NoStrikeRules { get; set; } = ValidateSecurityList_NoRelatedSym_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> SecurityList_NoRelatedSym_NoStrikeRules_NoMaturityRules { get; set; } = ValidateSecurityList_NoRelatedSym_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup> SecurityList_NoRelatedSym_NoUnderlyings { get; set; } = ValidateSecurityList_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup> SecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListRequest.NoUnderlyingsGroup> SecurityListRequest_NoUnderlyings { get; set; } = ValidateSecurityListRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListRequest.NoLegsGroup> SecurityListRequest_NoLegs { get; set; } = ValidateSecurityListRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityListUpdateReport.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup> SecurityListUpdateReport_NoRelatedSym { get; set; } = ValidateSecurityListUpdateReport_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup> SecurityListUpdateReport_NoRelatedSym_NoStrikeRules { get; set; } = ValidateSecurityListUpdateReport_NoRelatedSym_NoStrikeRules;

	/// <summary>Holds one entry of FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup> SecurityListUpdateReport_NoRelatedSym_NoStrikeRules_NoMaturityRules { get; set; } = ValidateSecurityListUpdateReport_NoRelatedSym_NoStrikeRules_NoMaturityRules;

	/// <summary>Holds one entry of FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoUnderlyingsGroup> SecurityListUpdateReport_NoRelatedSym_NoUnderlyings { get; set; } = ValidateSecurityListUpdateReport_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup> SecurityListUpdateReport_NoRelatedSym_NoLegs { get; set; } = ValidateSecurityListUpdateReport_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatusMessage.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusMessage.NoUnderlyingsGroup> SecurityStatusMessage_NoUnderlyings { get; set; } = ValidateSecurityStatusMessage_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatusMessage.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusMessage.NoLegsGroup> SecurityStatusMessage_NoLegs { get; set; } = ValidateSecurityStatusMessage_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup> SecurityStatusRequest_NoUnderlyings { get; set; } = ValidateSecurityStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityStatusRequest.NoLegsGroup> SecurityStatusRequest_NoLegs { get; set; } = ValidateSecurityStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityTypes.NoSecurityTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SecurityTypes.NoSecurityTypesGroup> SecurityTypes_NoSecurityTypes { get; set; } = ValidateSecurityTypes_NoSecurityTypes;

	/// <summary>Holds one entry of FixMessage.SettlementInstructions.NoSettlInstGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementInstructions.NoSettlInstGroup> SettlementInstructions_NoSettlInst { get; set; } = ValidateSettlementInstructions_NoSettlInst;

	/// <summary>Holds one entry of FixMessage.SettlementObligationReport.NoSettlObligGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementObligationReport.NoSettlObligGroup> SettlementObligationReport_NoSettlOblig { get; set; } = ValidateSettlementObligationReport_NoSettlOblig;

	/// <summary>Holds one entry of FixMessage.SettlementObligationReport.NoSettlObligGroup.NoSettlDetailsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.SettlementObligationReport.NoSettlObligGroup.NoSettlDetailsGroup> SettlementObligationReport_NoSettlOblig_NoSettlDetails { get; set; } = ValidateSettlementObligationReport_NoSettlOblig_NoSettlDetails;

	/// <summary>Holds one entry of FixMessage.StreamAssignmentReport.NoAsgnReqsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentReport.NoAsgnReqsGroup> StreamAssignmentReport_NoAsgnReqs { get; set; } = ValidateStreamAssignmentReport_NoAsgnReqs;

	/// <summary>Holds one entry of FixMessage.StreamAssignmentReport.NoAsgnReqsGroup.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentReport.NoAsgnReqsGroup.NoRelatedSymGroup> StreamAssignmentReport_NoAsgnReqs_NoRelatedSym { get; set; } = ValidateStreamAssignmentReport_NoAsgnReqs_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup> StreamAssignmentRequest_NoAsgnReqs { get; set; } = ValidateStreamAssignmentRequest_NoAsgnReqs;

	/// <summary>Holds one entry of FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup.NoRelatedSymGroup> StreamAssignmentRequest_NoAsgnReqs_NoRelatedSym { get; set; } = ValidateStreamAssignmentRequest_NoAsgnReqs_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoUnderlyingsGroup> TradeCaptureReport_NoUnderlyings { get; set; } = ValidateTradeCaptureReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoLegsGroup> TradeCaptureReport_NoLegs { get; set; } = ValidateTradeCaptureReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup> TradeCaptureReport_NoLegs_NoOfLegUnderlyings { get; set; } = ValidateTradeCaptureReport_NoLegs_NoOfLegUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup> TradeCaptureReport_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID { get; set; } = ValidateTradeCaptureReport_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup> TradeCaptureReport_NoSides { get; set; } = ValidateTradeCaptureReport_NoSides;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup> TradeCaptureReport_NoSides_NoClearingInstructions { get; set; } = ValidateTradeCaptureReport_NoSides_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup> TradeCaptureReport_NoSides_NoContAmts { get; set; } = ValidateTradeCaptureReport_NoSides_NoContAmts;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup> TradeCaptureReport_NoSides_NoMiscFees { get; set; } = ValidateTradeCaptureReport_NoSides_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup> TradeCaptureReport_NoSides_NoAllocs { get; set; } = ValidateTradeCaptureReport_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoSettlDetailsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoSettlDetailsGroup> TradeCaptureReport_NoSides_NoSettlDetails { get; set; } = ValidateTradeCaptureReport_NoSides_NoSettlDetails;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoTrdRepIndicatorsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReport.NoTrdRepIndicatorsGroup> TradeCaptureReport_NoTrdRepIndicators { get; set; } = ValidateTradeCaptureReport_NoTrdRepIndicators;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoUnderlyingsGroup> TradeCaptureReportAck_NoUnderlyings { get; set; } = ValidateTradeCaptureReportAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoTrdRepIndicatorsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoTrdRepIndicatorsGroup> TradeCaptureReportAck_NoTrdRepIndicators { get; set; } = ValidateTradeCaptureReportAck_NoTrdRepIndicators;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoLegsGroup> TradeCaptureReportAck_NoLegs { get; set; } = ValidateTradeCaptureReportAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup> TradeCaptureReportAck_NoLegs_NoOfLegUnderlyings { get; set; } = ValidateTradeCaptureReportAck_NoLegs_NoOfLegUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup> TradeCaptureReportAck_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID { get; set; } = ValidateTradeCaptureReportAck_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup> TradeCaptureReportAck_NoSides { get; set; } = ValidateTradeCaptureReportAck_NoSides;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoClearingInstructionsGroup> TradeCaptureReportAck_NoSides_NoClearingInstructions { get; set; } = ValidateTradeCaptureReportAck_NoSides_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup.NoContAmtsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoContAmtsGroup> TradeCaptureReportAck_NoSides_NoContAmts { get; set; } = ValidateTradeCaptureReportAck_NoSides_NoContAmts;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoMiscFeesGroup> TradeCaptureReportAck_NoSides_NoMiscFees { get; set; } = ValidateTradeCaptureReportAck_NoSides_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup.NoSettlDetailsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoSettlDetailsGroup> TradeCaptureReportAck_NoSides_NoSettlDetails { get; set; } = ValidateTradeCaptureReportAck_NoSides_NoSettlDetails;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoAllocsGroup> TradeCaptureReportAck_NoSides_NoAllocs { get; set; } = ValidateTradeCaptureReportAck_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup> TradeCaptureReportRequest_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoLegsGroup> TradeCaptureReportRequest_NoLegs { get; set; } = ValidateTradeCaptureReportRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoDatesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoDatesGroup> TradeCaptureReportRequest_NoDates { get; set; } = ValidateTradeCaptureReportRequest_NoDates;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup> TradeCaptureReportRequestAck_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequestAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoLegsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup> TradeCaptureReportRequestAck_NoLegs { get; set; } = ValidateTradeCaptureReportRequestAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup> TradingSessionList_NoTradingSessions { get; set; } = ValidateTradingSessionList_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup.NoOrdTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoOrdTypeRulesGroup> TradingSessionList_NoTradingSessions_NoOrdTypeRules { get; set; } = ValidateTradingSessionList_NoTradingSessions_NoOrdTypeRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup.NoTimeInForceRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoTimeInForceRulesGroup> TradingSessionList_NoTradingSessions_NoTimeInForceRules { get; set; } = ValidateTradingSessionList_NoTradingSessions_NoTimeInForceRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup.NoExecInstRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoExecInstRulesGroup> TradingSessionList_NoTradingSessions_NoExecInstRules { get; set; } = ValidateTradingSessionList_NoTradingSessions_NoExecInstRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMatchRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMatchRulesGroup> TradingSessionList_NoTradingSessions_NoMatchRules { get; set; } = ValidateTradingSessionList_NoTradingSessions_NoMatchRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMDFeedTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMDFeedTypesGroup> TradingSessionList_NoTradingSessions_NoMDFeedTypes { get; set; } = ValidateTradingSessionList_NoTradingSessions_NoMDFeedTypes;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup> TradingSessionListUpdateReport_NoTradingSessions { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoOrdTypeRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoOrdTypeRulesGroup> TradingSessionListUpdateReport_NoTradingSessions_NoOrdTypeRules { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions_NoOrdTypeRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoTimeInForceRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoTimeInForceRulesGroup> TradingSessionListUpdateReport_NoTradingSessions_NoTimeInForceRules { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions_NoTimeInForceRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoExecInstRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoExecInstRulesGroup> TradingSessionListUpdateReport_NoTradingSessions_NoExecInstRules { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions_NoExecInstRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMatchRulesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMatchRulesGroup> TradingSessionListUpdateReport_NoTradingSessions_NoMatchRules { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions_NoMatchRules;

	/// <summary>Holds one entry of FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMDFeedTypesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMDFeedTypesGroup> TradingSessionListUpdateReport_NoTradingSessions_NoMDFeedTypes { get; set; } = ValidateTradingSessionListUpdateReport_NoTradingSessions_NoMDFeedTypes;

	/// <summary>Holds one entry of FixMessage.UserNotification.NoUsernamesGroup to the schema.</summary>
	public Action<Fix50Context, FixCheck, FixMessage.UserNotification.NoUsernamesGroup> UserNotification_NoUsernames { get; set; } = ValidateUserNotification_NoUsernames;

	static void ValidateAdjustedPositionReport(Fix50Context context, FixCheck check, FixMessage.AdjustedPositionReport message)
	{
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosReqType is not null) context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.PosMaintRptRefID is not null) context.Validators.PosMaintRptRefID(context, check, message.PosMaintRptRefID);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.AdjustedPositionReport_NoRelatedSym);
		if (message.SettlPrice is not null) context.Validators.SettlPrice(context, check, message.SettlPrice);
		if (message.PriorSettlPrice is not null) context.Validators.PriorSettlPrice(context, check, message.PriorSettlPrice);
	}

	static void ValidateAdvertisement(Fix50Context context, FixCheck check, FixMessage.Advertisement message)
	{
		if (message.AdvId is null) Missing(check, FixTag.AdvId);
		else context.Validators.AdvId(context, check, message.AdvId);
		if (message.AdvTransType is null) Missing(check, FixTag.AdvTransType);
		else context.Validators.AdvTransType(context, check, message.AdvTransType);
		if (message.AdvRefID is not null) context.Validators.AdvRefID(context, check, message.AdvRefID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.Advertisement_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.Advertisement_NoUnderlyings);
		if (message.AdvSide is null) Missing(check, FixTag.AdvSide);
		else context.Validators.AdvSide(context, check, message.AdvSide);
		if (message.Quantity is null) Missing(check, FixTag.Quantity);
		else context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
	}

	static void ValidateAllocationInstruction(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction message)
	{
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (message.AllocTransType is null) Missing(check, FixTag.AllocTransType);
		else context.Validators.AllocTransType(context, check, message.AllocTransType);
		if (message.AllocType is null) Missing(check, FixTag.AllocType);
		else context.Validators.AllocType(context, check, message.AllocType);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, check, message.RefAllocID);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, check, message.AllocCancReplaceReason);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, check, message.AllocIntermedReqType);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, check, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, check, message.AllocLinkType);
		if (message.BookingRefID is not null) context.Validators.BookingRefID(context, check, message.BookingRefID);
		if (message.AllocNoOrdersType is not null) context.Validators.AllocNoOrdersType(context, check, message.AllocNoOrdersType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.AllocationInstruction_NoOrders);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.AllocationInstruction_NoExecs);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.ReversalIndicator is not null) context.Validators.ReversalIndicator(context, check, message.ReversalIndicator);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.AllocationInstruction_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.AllocationInstruction_NoLegs);
		if (message.Quantity is null) Missing(check, FixTag.Quantity);
		else context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, check, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, check, message.AvgPxPrecision);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.Concession is not null) context.Validators.Concession(context, check, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.AutoAcceptIndicator is not null) context.Validators.AutoAcceptIndicator(context, check, message.AutoAcceptIndicator);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.TotalAccruedInterestAmt is not null) context.Validators.TotalAccruedInterestAmt(context, check, message.TotalAccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, check, message.LegalConfirm);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, check, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationInstruction_NoAllocs);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, message.TradeInputSource);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (message.RndPx is not null) context.Validators.RndPx(context, check, message.RndPx);
		if (!Empty((IRateSource)message)) context.Validators.RateSource(context, check, message);
	}

	static void ValidateAllocationInstructionAck(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAck message)
	{
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AllocStatus is null) Missing(check, FixTag.AllocStatus);
		else context.Validators.AllocStatus(context, check, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, check, message.AllocRejCode);
		if (message.AllocType is not null) context.Validators.AllocType(context, check, message.AllocType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, check, message.AllocIntermedReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.Product is not null) context.Validators.Product(context, check, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationInstructionAck_NoAllocs);
	}

	static void ValidateAllocationInstructionAlert(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert message)
	{
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (message.AllocTransType is null) Missing(check, FixTag.AllocTransType);
		else context.Validators.AllocTransType(context, check, message.AllocTransType);
		if (message.AllocType is null) Missing(check, FixTag.AllocType);
		else context.Validators.AllocType(context, check, message.AllocType);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, check, message.RefAllocID);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, check, message.AllocCancReplaceReason);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, check, message.AllocIntermedReqType);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, check, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, check, message.AllocLinkType);
		if (message.BookingRefID is not null) context.Validators.BookingRefID(context, check, message.BookingRefID);
		if (message.AllocNoOrdersType is not null) context.Validators.AllocNoOrdersType(context, check, message.AllocNoOrdersType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.AllocationInstructionAlert_NoOrders);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.AllocationInstructionAlert_NoExecs);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.ReversalIndicator is not null) context.Validators.ReversalIndicator(context, check, message.ReversalIndicator);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.AllocationInstructionAlert_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.AllocationInstructionAlert_NoLegs);
		if (message.Quantity is null) Missing(check, FixTag.Quantity);
		else context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, check, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, check, message.AvgPxPrecision);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.Concession is not null) context.Validators.Concession(context, check, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.AutoAcceptIndicator is not null) context.Validators.AutoAcceptIndicator(context, check, message.AutoAcceptIndicator);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.TotalAccruedInterestAmt is not null) context.Validators.TotalAccruedInterestAmt(context, check, message.TotalAccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, check, message.LegalConfirm);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, check, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationInstructionAlert_NoAllocs);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, message.TradeInputSource);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (message.RndPx is not null) context.Validators.RndPx(context, check, message.RndPx);
	}

	static void ValidateAllocationReport(Fix50Context context, FixCheck check, FixMessage.AllocationReport message)
	{
		if (message.AllocReportID is null) Missing(check, FixTag.AllocReportID);
		else context.Validators.AllocReportID(context, check, message.AllocReportID);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.AllocTransType is null) Missing(check, FixTag.AllocTransType);
		else context.Validators.AllocTransType(context, check, message.AllocTransType);
		if (message.AllocReportRefID is not null) context.Validators.AllocReportRefID(context, check, message.AllocReportRefID);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, check, message.AllocCancReplaceReason);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.AllocReportType is null) Missing(check, FixTag.AllocReportType);
		else context.Validators.AllocReportType(context, check, message.AllocReportType);
		if (message.AllocStatus is null) Missing(check, FixTag.AllocStatus);
		else context.Validators.AllocStatus(context, check, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, check, message.AllocRejCode);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, check, message.RefAllocID);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, check, message.AllocIntermedReqType);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, check, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, check, message.AllocLinkType);
		if (message.BookingRefID is not null) context.Validators.BookingRefID(context, check, message.BookingRefID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, message.TradeInputSource);
		if (message.RndPx is not null) context.Validators.RndPx(context, check, message.RndPx);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (message.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, check, message.TradeInputDevice);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (message.AllocNoOrdersType is not null) context.Validators.AllocNoOrdersType(context, check, message.AllocNoOrdersType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.AllocationReport_NoOrders);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.AllocationReport_NoExecs);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.ReversalIndicator is not null) context.Validators.ReversalIndicator(context, check, message.ReversalIndicator);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.AllocationReport_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.AllocationReport_NoLegs);
		if (message.Quantity is null) Missing(check, FixTag.Quantity);
		else context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AvgPx is null) Missing(check, FixTag.AvgPx);
		else context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, check, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, check, message.AvgPxPrecision);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.Concession is not null) context.Validators.Concession(context, check, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.AutoAcceptIndicator is not null) context.Validators.AutoAcceptIndicator(context, check, message.AutoAcceptIndicator);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.TotalAccruedInterestAmt is not null) context.Validators.TotalAccruedInterestAmt(context, check, message.TotalAccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, check, message.LegalConfirm);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, check, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationReport_NoAllocs);
		if (!Empty((IRateSource)message)) context.Validators.RateSource(context, check, message);
	}

	static void ValidateAllocationReportAck(Fix50Context context, FixCheck check, FixMessage.AllocationReportAck message)
	{
		if (message.AllocReportID is null) Missing(check, FixTag.AllocReportID);
		else context.Validators.AllocReportID(context, check, message.AllocReportID);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.AllocTransType is not null) context.Validators.AllocTransType(context, check, message.AllocTransType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AllocStatus is not null) context.Validators.AllocStatus(context, check, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, check, message.AllocRejCode);
		if (message.AllocReportType is not null) context.Validators.AllocReportType(context, check, message.AllocReportType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, check, message.AllocIntermedReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.Product is not null) context.Validators.Product(context, check, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationReportAck_NoAllocs);
	}

	static void ValidateApplicationMessageReport(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageReport message)
	{
		if (message.ApplReportID is null) Missing(check, FixTag.ApplReportID);
		else context.Validators.ApplReportID(context, check, message.ApplReportID);
		if (message.ApplReqID is not null) context.Validators.ApplReqID(context, check, message.ApplReqID);
		if (message.ApplReportType is null) Missing(check, FixTag.ApplReportType);
		else context.Validators.ApplReportType(context, check, message.ApplReportType);
		if (message.NoApplIDs is not null) context.Validators.NoApplIDs(context, check, message.NoApplIDs);
		Counted(check, message.NoApplIDs, message.NoApplIDsGroups);
		Each(context, check, FixTag.NoApplIDs, message.NoApplIDsGroups, context.Validators.ApplicationMessageReport_NoApplIDs);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateApplicationMessageRequest(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageRequest message)
	{
		if (message.ApplReqID is null) Missing(check, FixTag.ApplReqID);
		else context.Validators.ApplReqID(context, check, message.ApplReqID);
		if (message.ApplReqType is null) Missing(check, FixTag.ApplReqType);
		else context.Validators.ApplReqType(context, check, message.ApplReqType);
		if (message.NoApplIDs is not null) context.Validators.NoApplIDs(context, check, message.NoApplIDs);
		Counted(check, message.NoApplIDs, message.NoApplIDsGroups);
		Each(context, check, FixTag.NoApplIDs, message.NoApplIDsGroups, context.Validators.ApplicationMessageRequest_NoApplIDs);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateApplicationMessageRequestAck(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageRequestAck message)
	{
		if (message.ApplResponseID is null) Missing(check, FixTag.ApplResponseID);
		else context.Validators.ApplResponseID(context, check, message.ApplResponseID);
		if (message.ApplReqID is not null) context.Validators.ApplReqID(context, check, message.ApplReqID);
		if (message.ApplReqType is not null) context.Validators.ApplReqType(context, check, message.ApplReqType);
		if (message.ApplResponseType is not null) context.Validators.ApplResponseType(context, check, message.ApplResponseType);
		if (message.ApplTotalMessageCount is not null) context.Validators.ApplTotalMessageCount(context, check, message.ApplTotalMessageCount);
		if (message.NoApplIDs is not null) context.Validators.NoApplIDs(context, check, message.NoApplIDs);
		Counted(check, message.NoApplIDs, message.NoApplIDsGroups);
		Each(context, check, FixTag.NoApplIDs, message.NoApplIDsGroups, context.Validators.ApplicationMessageRequestAck_NoApplIDs);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateAssignmentReport(Fix50Context context, FixCheck check, FixMessage.AssignmentReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.AsgnRptID is null) Missing(check, FixTag.AsgnRptID);
		else context.Validators.AsgnRptID(context, check, message.AsgnRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.TotNumAssignmentReports is not null) context.Validators.TotNumAssignmentReports(context, check, message.TotNumAssignmentReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, check, message.LastRptRequested);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.AssignmentReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.AssignmentReport_NoUnderlyings);
		if (!Empty((IPositionQty)message)) context.Validators.PositionQty(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.SettlPrice is not null) context.Validators.SettlPrice(context, check, message.SettlPrice);
		if (message.SettlPriceType is not null) context.Validators.SettlPriceType(context, check, message.SettlPriceType);
		if (message.UnderlyingSettlPrice is not null) context.Validators.UnderlyingSettlPrice(context, check, message.UnderlyingSettlPrice);
		if (message.PriorSettlPrice is not null) context.Validators.PriorSettlPrice(context, check, message.PriorSettlPrice);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.AssignmentMethod is not null) context.Validators.AssignmentMethod(context, check, message.AssignmentMethod);
		if (message.AssignmentUnit is not null) context.Validators.AssignmentUnit(context, check, message.AssignmentUnit);
		if (message.OpenInterest is not null) context.Validators.OpenInterest(context, check, message.OpenInterest);
		if (message.ExerciseMethod is not null) context.Validators.ExerciseMethod(context, check, message.ExerciseMethod);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateBidRequest(Fix50Context context, FixCheck check, FixMessage.BidRequest message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is null) Missing(check, FixTag.ClientBidID);
		else context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.BidRequestTransType is null) Missing(check, FixTag.BidRequestTransType);
		else context.Validators.BidRequestTransType(context, check, message.BidRequestTransType);
		if (message.ListName is not null) context.Validators.ListName(context, check, message.ListName);
		if (message.TotNoRelatedSym is null) Missing(check, FixTag.TotNoRelatedSym);
		else context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.BidType is null) Missing(check, FixTag.BidType);
		else context.Validators.BidType(context, check, message.BidType);
		if (message.NumTickets is not null) context.Validators.NumTickets(context, check, message.NumTickets);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SideValue1 is not null) context.Validators.SideValue1(context, check, message.SideValue1);
		if (message.SideValue2 is not null) context.Validators.SideValue2(context, check, message.SideValue2);
		if (message.NoBidDescriptors is not null) context.Validators.NoBidDescriptors(context, check, message.NoBidDescriptors);
		Counted(check, message.NoBidDescriptors, message.NoBidDescriptorsGroups);
		Each(context, check, FixTag.NoBidDescriptors, message.NoBidDescriptorsGroups, context.Validators.BidRequest_NoBidDescriptors);
		if (message.NoBidComponents is not null) context.Validators.NoBidComponents(context, check, message.NoBidComponents);
		Counted(check, message.NoBidComponents, message.NoBidComponentsGroups);
		Each(context, check, FixTag.NoBidComponents, message.NoBidComponentsGroups, context.Validators.BidRequest_NoBidComponents);
		if (message.LiquidityIndType is not null) context.Validators.LiquidityIndType(context, check, message.LiquidityIndType);
		if (message.WtAverageLiquidity is not null) context.Validators.WtAverageLiquidity(context, check, message.WtAverageLiquidity);
		if (message.ExchangeForPhysical is not null) context.Validators.ExchangeForPhysical(context, check, message.ExchangeForPhysical);
		if (message.OutMainCntryUIndex is not null) context.Validators.OutMainCntryUIndex(context, check, message.OutMainCntryUIndex);
		if (message.CrossPercent is not null) context.Validators.CrossPercent(context, check, message.CrossPercent);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, check, message.ProgRptReqs);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, check, message.ProgPeriodInterval);
		if (message.IncTaxInd is not null) context.Validators.IncTaxInd(context, check, message.IncTaxInd);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.NumBidders is not null) context.Validators.NumBidders(context, check, message.NumBidders);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.BidTradeType is null) Missing(check, FixTag.BidTradeType);
		else context.Validators.BidTradeType(context, check, message.BidTradeType);
		if (message.BasisPxType is null) Missing(check, FixTag.BasisPxType);
		else context.Validators.BasisPxType(context, check, message.BasisPxType);
		if (message.StrikeTime is not null) context.Validators.StrikeTime(context, check, message.StrikeTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateBidResponse(Fix50Context context, FixCheck check, FixMessage.BidResponse message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.NoBidComponents is null) Missing(check, FixTag.NoBidComponents);
		else context.Validators.NoBidComponents(context, check, message.NoBidComponents);
		Counted(check, message.NoBidComponents, message.NoBidComponentsGroups);
		Each(context, check, FixTag.NoBidComponents, message.NoBidComponentsGroups, context.Validators.BidResponse_NoBidComponents);
	}

	static void ValidateBusinessMessageReject(Fix50Context context, FixCheck check, FixMessage.BusinessMessageReject message)
	{
		if (message.RefSeqNum is not null) context.Validators.RefSeqNum(context, check, message.RefSeqNum);
		if (message.RefMsgType is null) Missing(check, FixTag.RefMsgType);
		else context.Validators.RefMsgType(context, check, message.RefMsgType);
		if (message.RefApplVerID is not null) context.Validators.RefApplVerID(context, check, message.RefApplVerID);
		if (message.RefApplExtID is not null) context.Validators.RefApplExtID(context, check, message.RefApplExtID);
		if (message.RefCstmApplVerID is not null) context.Validators.RefCstmApplVerID(context, check, message.RefCstmApplVerID);
		if (message.BusinessRejectRefID is not null) context.Validators.BusinessRejectRefID(context, check, message.BusinessRejectRefID);
		if (message.BusinessRejectReason is null) Missing(check, FixTag.BusinessRejectReason);
		else context.Validators.BusinessRejectReason(context, check, message.BusinessRejectReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralAssignment(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment message)
	{
		if (message.CollAsgnID is null) Missing(check, FixTag.CollAsgnID);
		else context.Validators.CollAsgnID(context, check, message.CollAsgnID);
		if (message.CollReqID is not null) context.Validators.CollReqID(context, check, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(check, FixTag.CollAsgnReason);
		else context.Validators.CollAsgnReason(context, check, message.CollAsgnReason);
		if (message.CollAsgnTransType is null) Missing(check, FixTag.CollAsgnTransType);
		else context.Validators.CollAsgnTransType(context, check, message.CollAsgnTransType);
		if (message.CollAsgnRefID is not null) context.Validators.CollAsgnRefID(context, check, message.CollAsgnRefID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralAssignment_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralAssignment_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralAssignment_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralAssignment_NoUnderlyings);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, check, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, check, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, check, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.CollateralAssignment_NoMiscFees);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralInquiry(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry message)
	{
		if (message.CollInquiryID is null) Missing(check, FixTag.CollInquiryID);
		else context.Validators.CollInquiryID(context, check, message.CollInquiryID);
		if (message.NoCollInquiryQualifier is not null) context.Validators.NoCollInquiryQualifier(context, check, message.NoCollInquiryQualifier);
		Counted(check, message.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups);
		Each(context, check, FixTag.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups, context.Validators.CollateralInquiry_NoCollInquiryQualifier);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralInquiry_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralInquiry_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralInquiry_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralInquiry_NoUnderlyings);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, check, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, check, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, check, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralInquiryAck(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck message)
	{
		if (message.CollInquiryID is null) Missing(check, FixTag.CollInquiryID);
		else context.Validators.CollInquiryID(context, check, message.CollInquiryID);
		if (message.CollInquiryStatus is null) Missing(check, FixTag.CollInquiryStatus);
		else context.Validators.CollInquiryStatus(context, check, message.CollInquiryStatus);
		if (message.CollInquiryResult is not null) context.Validators.CollInquiryResult(context, check, message.CollInquiryResult);
		if (message.NoCollInquiryQualifier is not null) context.Validators.NoCollInquiryQualifier(context, check, message.NoCollInquiryQualifier);
		Counted(check, message.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups);
		Each(context, check, FixTag.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups, context.Validators.CollateralInquiryAck_NoCollInquiryQualifier);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, check, message.TotNumReports);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralInquiryAck_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralInquiryAck_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralInquiryAck_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralInquiryAck_NoUnderlyings);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralReport(Fix50Context context, FixCheck check, FixMessage.CollateralReport message)
	{
		if (message.CollRptID is null) Missing(check, FixTag.CollRptID);
		else context.Validators.CollRptID(context, check, message.CollRptID);
		if (message.CollInquiryID is not null) context.Validators.CollInquiryID(context, check, message.CollInquiryID);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.CollApplType is not null) context.Validators.CollApplType(context, check, message.CollApplType);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.CollStatus is null) Missing(check, FixTag.CollStatus);
		else context.Validators.CollStatus(context, check, message.CollStatus);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, check, message.TotNumReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, check, message.LastRptRequested);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralReport_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralReport_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralReport_NoUnderlyings);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, check, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, check, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, check, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.CollateralReport_NoMiscFees);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralRequest(Fix50Context context, FixCheck check, FixMessage.CollateralRequest message)
	{
		if (message.CollReqID is null) Missing(check, FixTag.CollReqID);
		else context.Validators.CollReqID(context, check, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(check, FixTag.CollAsgnReason);
		else context.Validators.CollAsgnReason(context, check, message.CollAsgnReason);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralRequest_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralRequest_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralRequest_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralRequest_NoUnderlyings);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, check, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, check, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, check, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.CollateralRequest_NoMiscFees);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralResponse(Fix50Context context, FixCheck check, FixMessage.CollateralResponse message)
	{
		if (message.CollRespID is null) Missing(check, FixTag.CollRespID);
		else context.Validators.CollRespID(context, check, message.CollRespID);
		if (message.CollAsgnID is not null) context.Validators.CollAsgnID(context, check, message.CollAsgnID);
		if (message.CollReqID is not null) context.Validators.CollReqID(context, check, message.CollReqID);
		if (message.CollAsgnReason is not null) context.Validators.CollAsgnReason(context, check, message.CollAsgnReason);
		if (message.CollAsgnTransType is not null) context.Validators.CollAsgnTransType(context, check, message.CollAsgnTransType);
		if (message.CollAsgnRespType is null) Missing(check, FixTag.CollAsgnRespType);
		else context.Validators.CollAsgnRespType(context, check, message.CollAsgnRespType);
		if (message.CollAsgnRejectReason is not null) context.Validators.CollAsgnRejectReason(context, check, message.CollAsgnRejectReason);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.CollApplType is not null) context.Validators.CollApplType(context, check, message.CollApplType);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.CollateralResponse_NoExecs);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, check, message.NoTrades);
		Counted(check, message.NoTrades, message.NoTradesGroups);
		Each(context, check, FixTag.NoTrades, message.NoTradesGroups, context.Validators.CollateralResponse_NoTrades);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, check, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CollateralResponse_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CollateralResponse_NoUnderlyings);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, check, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, check, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, check, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.CollateralResponse_NoMiscFees);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateConfirmation(Fix50Context context, FixCheck check, FixMessage.Confirmation message)
	{
		if (message.ConfirmID is null) Missing(check, FixTag.ConfirmID);
		else context.Validators.ConfirmID(context, check, message.ConfirmID);
		if (message.ConfirmRefID is not null) context.Validators.ConfirmRefID(context, check, message.ConfirmRefID);
		if (message.ConfirmReqID is not null) context.Validators.ConfirmReqID(context, check, message.ConfirmReqID);
		if (message.ConfirmTransType is null) Missing(check, FixTag.ConfirmTransType);
		else context.Validators.ConfirmTransType(context, check, message.ConfirmTransType);
		if (message.ConfirmType is null) Missing(check, FixTag.ConfirmType);
		else context.Validators.ConfirmType(context, check, message.ConfirmType);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, check, message.CopyMsgIndicator);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, check, message.LegalConfirm);
		if (message.ConfirmStatus is null) Missing(check, FixTag.ConfirmStatus);
		else context.Validators.ConfirmStatus(context, check, message.ConfirmStatus);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.Confirmation_NoOrders);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, message.IndividualAllocID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.Confirmation_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.Confirmation_NoLegs);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.AllocQty is null) Missing(check, FixTag.AllocQty);
		else context.Validators.AllocQty(context, check, message.AllocQty);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.NoCapacities is null) Missing(check, FixTag.NoCapacities);
		else context.Validators.NoCapacities(context, check, message.NoCapacities);
		Counted(check, message.NoCapacities, message.NoCapacitiesGroups);
		Each(context, check, FixTag.NoCapacities, message.NoCapacitiesGroups, context.Validators.Confirmation_NoCapacities);
		if (message.AllocAccount is null) Missing(check, FixTag.AllocAccount);
		else context.Validators.AllocAccount(context, check, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, message.AllocAcctIDSource);
		if (message.AllocAccountType is not null) context.Validators.AllocAccountType(context, check, message.AllocAccountType);
		if (message.AvgPx is null) Missing(check, FixTag.AvgPx);
		else context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, check, message.AvgPxPrecision);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, check, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (message.ReportedPx is not null) context.Validators.ReportedPx(context, check, message.ReportedPx);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.GrossTradeAmt is null) Missing(check, FixTag.GrossTradeAmt);
		else context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.ExDate is not null) context.Validators.ExDate(context, check, message.ExDate);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (message.Concession is not null) context.Validators.Concession(context, check, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, message.TotalTakedown);
		if (message.NetMoney is null) Missing(check, FixTag.NetMoney);
		else context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.MaturityNetMoney is not null) context.Validators.MaturityNetMoney(context, check, message.MaturityNetMoney);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, check, message);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.SharedCommission is not null) context.Validators.SharedCommission(context, check, message.SharedCommission);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.Confirmation_NoMiscFees);
	}

	static void ValidateConfirmationAck(Fix50Context context, FixCheck check, FixMessage.ConfirmationAck message)
	{
		if (message.ConfirmID is null) Missing(check, FixTag.ConfirmID);
		else context.Validators.ConfirmID(context, check, message.ConfirmID);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AffirmStatus is null) Missing(check, FixTag.AffirmStatus);
		else context.Validators.AffirmStatus(context, check, message.AffirmStatus);
		if (message.ConfirmRejReason is not null) context.Validators.ConfirmRejReason(context, check, message.ConfirmRejReason);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateConfirmationRequest(Fix50Context context, FixCheck check, FixMessage.ConfirmationRequest message)
	{
		if (message.ConfirmReqID is null) Missing(check, FixTag.ConfirmReqID);
		else context.Validators.ConfirmReqID(context, check, message.ConfirmReqID);
		if (message.ConfirmType is null) Missing(check, FixTag.ConfirmType);
		else context.Validators.ConfirmType(context, check, message.ConfirmType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.ConfirmationRequest_NoOrders);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, message.IndividualAllocID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AllocAccount is not null) context.Validators.AllocAccount(context, check, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, message.AllocAcctIDSource);
		if (message.AllocAccountType is not null) context.Validators.AllocAccountType(context, check, message.AllocAccountType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateContraryIntentionReport(Fix50Context context, FixCheck check, FixMessage.ContraryIntentionReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.ContIntRptID is null) Missing(check, FixTag.ContIntRptID);
		else context.Validators.ContIntRptID(context, check, message.ContIntRptID);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.LateIndicator is not null) context.Validators.LateIndicator(context, check, message.LateIndicator);
		if (message.InputSource is not null) context.Validators.InputSource(context, check, message.InputSource);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (Empty((IExpirationQty)message)) Absent(check, FixTag.NoExpiration);
		else context.Validators.ExpirationQty(context, check, message);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.ContraryIntentionReport_NoUnderlyings);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCrossOrderCancelReplaceRequest(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.OrigCrossID is null) Missing(check, FixTag.OrigCrossID);
		else context.Validators.OrigCrossID(context, check, message.OrigCrossID);
		if (message.HostCrossID is not null) context.Validators.HostCrossID(context, check, message.HostCrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoSides is null) Missing(check, FixTag.NoSides);
		else context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.CrossOrderCancelReplaceRequest_NoSides);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CrossOrderCancelReplaceRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CrossOrderCancelReplaceRequest_NoLegs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.CrossOrderCancelReplaceRequest_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, message.TransBkdTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.CrossOrderCancelReplaceRequest_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
	}

	static void ValidateCrossOrderCancelRequest(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.OrigCrossID is null) Missing(check, FixTag.OrigCrossID);
		else context.Validators.OrigCrossID(context, check, message.OrigCrossID);
		if (message.HostCrossID is not null) context.Validators.HostCrossID(context, check, message.HostCrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoSides is null) Missing(check, FixTag.NoSides);
		else context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.CrossOrderCancelRequest_NoSides);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.CrossOrderCancelRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.CrossOrderCancelRequest_NoLegs);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
	}

	static void ValidateDerivativeSecurityList(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReportID is not null) context.Validators.SecurityReportID(context, check, message.SecurityReportID);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.DerivativeSymbol is not null) context.Validators.DerivativeSymbol(context, check, message.DerivativeSymbol);
		if (message.DerivativeSymbolSfx is not null) context.Validators.DerivativeSymbolSfx(context, check, message.DerivativeSymbolSfx);
		if (message.DerivativeSecurityID is not null) context.Validators.DerivativeSecurityID(context, check, message.DerivativeSecurityID);
		if (message.DerivativeSecurityIDSource is not null) context.Validators.DerivativeSecurityIDSource(context, check, message.DerivativeSecurityIDSource);
		if (message.NoDerivativeSecurityAltID is not null) context.Validators.NoDerivativeSecurityAltID(context, check, message.NoDerivativeSecurityAltID);
		Counted(check, message.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups);
		Each(context, check, FixTag.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups, context.Validators.DerivativeSecurityList_NoDerivativeSecurityAltID);
		if (message.DerivativeProduct is not null) context.Validators.DerivativeProduct(context, check, message.DerivativeProduct);
		if (message.DerivativeProductComplex is not null) context.Validators.DerivativeProductComplex(context, check, message.DerivativeProductComplex);
		if (message.DerivFlexProductEligibilityIndicator is not null) context.Validators.DerivFlexProductEligibilityIndicator(context, check, message.DerivFlexProductEligibilityIndicator);
		if (message.DerivativeSecurityGroup is not null) context.Validators.DerivativeSecurityGroup(context, check, message.DerivativeSecurityGroup);
		if (message.DerivativeCFICode is not null) context.Validators.DerivativeCFICode(context, check, message.DerivativeCFICode);
		if (message.DerivativeSecurityType is not null) context.Validators.DerivativeSecurityType(context, check, message.DerivativeSecurityType);
		if (message.DerivativeSecuritySubType is not null) context.Validators.DerivativeSecuritySubType(context, check, message.DerivativeSecuritySubType);
		if (message.DerivativeMaturityMonthYear is not null) context.Validators.DerivativeMaturityMonthYear(context, check, message.DerivativeMaturityMonthYear);
		if (message.DerivativeMaturityDate is not null) context.Validators.DerivativeMaturityDate(context, check, message.DerivativeMaturityDate);
		if (message.DerivativeMaturityTime is not null) context.Validators.DerivativeMaturityTime(context, check, message.DerivativeMaturityTime);
		if (message.DerivativeSettleOnOpenFlag is not null) context.Validators.DerivativeSettleOnOpenFlag(context, check, message.DerivativeSettleOnOpenFlag);
		if (message.DerivativeInstrmtAssignmentMethod is not null) context.Validators.DerivativeInstrmtAssignmentMethod(context, check, message.DerivativeInstrmtAssignmentMethod);
		if (message.DerivativeSecurityStatus is not null) context.Validators.DerivativeSecurityStatus(context, check, message.DerivativeSecurityStatus);
		if (message.DerivativeIssueDate is not null) context.Validators.DerivativeIssueDate(context, check, message.DerivativeIssueDate);
		if (message.DerivativeInstrRegistry is not null) context.Validators.DerivativeInstrRegistry(context, check, message.DerivativeInstrRegistry);
		if (message.DerivativeCountryOfIssue is not null) context.Validators.DerivativeCountryOfIssue(context, check, message.DerivativeCountryOfIssue);
		if (message.DerivativeStateOrProvinceOfIssue is not null) context.Validators.DerivativeStateOrProvinceOfIssue(context, check, message.DerivativeStateOrProvinceOfIssue);
		if (message.DerivativeLocaleOfIssue is not null) context.Validators.DerivativeLocaleOfIssue(context, check, message.DerivativeLocaleOfIssue);
		if (message.DerivativeStrikePrice is not null) context.Validators.DerivativeStrikePrice(context, check, message.DerivativeStrikePrice);
		if (message.DerivativeStrikeCurrency is not null) context.Validators.DerivativeStrikeCurrency(context, check, message.DerivativeStrikeCurrency);
		if (message.DerivativeStrikeMultiplier is not null) context.Validators.DerivativeStrikeMultiplier(context, check, message.DerivativeStrikeMultiplier);
		if (message.DerivativeStrikeValue is not null) context.Validators.DerivativeStrikeValue(context, check, message.DerivativeStrikeValue);
		if (message.DerivativeOptAttribute is not null) context.Validators.DerivativeOptAttribute(context, check, message.DerivativeOptAttribute);
		if (message.DerivativeContractMultiplier is not null) context.Validators.DerivativeContractMultiplier(context, check, message.DerivativeContractMultiplier);
		if (message.DerivativeContractMultiplierUnit is not null) context.Validators.DerivativeContractMultiplierUnit(context, check, message.DerivativeContractMultiplierUnit);
		if (message.DerivativeFlowScheduleType is not null) context.Validators.DerivativeFlowScheduleType(context, check, message.DerivativeFlowScheduleType);
		if (message.DerivativeMinPriceIncrement is not null) context.Validators.DerivativeMinPriceIncrement(context, check, message.DerivativeMinPriceIncrement);
		if (message.DerivativeMinPriceIncrementAmount is not null) context.Validators.DerivativeMinPriceIncrementAmount(context, check, message.DerivativeMinPriceIncrementAmount);
		if (message.DerivativeUnitOfMeasure is not null) context.Validators.DerivativeUnitOfMeasure(context, check, message.DerivativeUnitOfMeasure);
		if (message.DerivativeUnitOfMeasureQty is not null) context.Validators.DerivativeUnitOfMeasureQty(context, check, message.DerivativeUnitOfMeasureQty);
		if (message.DerivativePriceUnitOfMeasure is not null) context.Validators.DerivativePriceUnitOfMeasure(context, check, message.DerivativePriceUnitOfMeasure);
		if (message.DerivativePriceUnitOfMeasureQty is not null) context.Validators.DerivativePriceUnitOfMeasureQty(context, check, message.DerivativePriceUnitOfMeasureQty);
		if (message.DerivativeSettlMethod is not null) context.Validators.DerivativeSettlMethod(context, check, message.DerivativeSettlMethod);
		if (message.DerivativePriceQuoteMethod is not null) context.Validators.DerivativePriceQuoteMethod(context, check, message.DerivativePriceQuoteMethod);
		if (message.DerivativeValuationMethod is not null) context.Validators.DerivativeValuationMethod(context, check, message.DerivativeValuationMethod);
		if (message.DerivativeListMethod is not null) context.Validators.DerivativeListMethod(context, check, message.DerivativeListMethod);
		if (message.DerivativeCapPrice is not null) context.Validators.DerivativeCapPrice(context, check, message.DerivativeCapPrice);
		if (message.DerivativeFloorPrice is not null) context.Validators.DerivativeFloorPrice(context, check, message.DerivativeFloorPrice);
		if (message.DerivativePutOrCall is not null) context.Validators.DerivativePutOrCall(context, check, message.DerivativePutOrCall);
		if (message.DerivativeExerciseStyle is not null) context.Validators.DerivativeExerciseStyle(context, check, message.DerivativeExerciseStyle);
		if (message.DerivativeOptPayAmount is not null) context.Validators.DerivativeOptPayAmount(context, check, message.DerivativeOptPayAmount);
		if (message.DerivativeTimeUnit is not null) context.Validators.DerivativeTimeUnit(context, check, message.DerivativeTimeUnit);
		if (message.DerivativeSecurityExchange is not null) context.Validators.DerivativeSecurityExchange(context, check, message.DerivativeSecurityExchange);
		if (message.DerivativePositionLimit is not null) context.Validators.DerivativePositionLimit(context, check, message.DerivativePositionLimit);
		if (message.DerivativeNTPositionLimit is not null) context.Validators.DerivativeNTPositionLimit(context, check, message.DerivativeNTPositionLimit);
		if (message.DerivativeIssuer is not null) context.Validators.DerivativeIssuer(context, check, message.DerivativeIssuer);
		if (message.DerivativeEncodedIssuerLen is not null) context.Validators.DerivativeEncodedIssuerLen(context, check, message.DerivativeEncodedIssuerLen);
		if (message.DerivativeEncodedIssuer is not null) context.Validators.DerivativeEncodedIssuer(context, check, message.DerivativeEncodedIssuer);
		if (message.DerivativeSecurityDesc is not null) context.Validators.DerivativeSecurityDesc(context, check, message.DerivativeSecurityDesc);
		if (message.DerivativeEncodedSecurityDescLen is not null) context.Validators.DerivativeEncodedSecurityDescLen(context, check, message.DerivativeEncodedSecurityDescLen);
		if (message.DerivativeEncodedSecurityDesc is not null) context.Validators.DerivativeEncodedSecurityDesc(context, check, message.DerivativeEncodedSecurityDesc);
		if (message.DerivativeSecurityXMLLen is not null) context.Validators.DerivativeSecurityXMLLen(context, check, message.DerivativeSecurityXMLLen);
		if (message.DerivativeSecurityXML is not null) context.Validators.DerivativeSecurityXML(context, check, message.DerivativeSecurityXML);
		if (message.DerivativeSecurityXMLSchema is not null) context.Validators.DerivativeSecurityXMLSchema(context, check, message.DerivativeSecurityXMLSchema);
		if (message.DerivativeContractSettlMonth is not null) context.Validators.DerivativeContractSettlMonth(context, check, message.DerivativeContractSettlMonth);
		if (message.NoDerivativeEvents is not null) context.Validators.NoDerivativeEvents(context, check, message.NoDerivativeEvents);
		Counted(check, message.NoDerivativeEvents, message.NoDerivativeEventsGroups);
		Each(context, check, FixTag.NoDerivativeEvents, message.NoDerivativeEventsGroups, context.Validators.DerivativeSecurityList_NoDerivativeEvents);
		if (message.NoDerivativeInstrumentParties is not null) context.Validators.NoDerivativeInstrumentParties(context, check, message.NoDerivativeInstrumentParties);
		Counted(check, message.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups, context.Validators.DerivativeSecurityList_NoDerivativeInstrumentParties);
		if (message.NoDerivativeInstrAttrib is not null) context.Validators.NoDerivativeInstrAttrib(context, check, message.NoDerivativeInstrAttrib);
		Counted(check, message.NoDerivativeInstrAttrib, message.NoDerivativeInstrAttribGroups);
		Each(context, check, FixTag.NoDerivativeInstrAttrib, message.NoDerivativeInstrAttribGroups, context.Validators.DerivativeSecurityList_NoDerivativeInstrAttrib);
		if (message.NoMarketSegments is not null) context.Validators.NoMarketSegments(context, check, message.NoMarketSegments);
		Counted(check, message.NoMarketSegments, message.NoMarketSegmentsGroups);
		Each(context, check, FixTag.NoMarketSegments, message.NoMarketSegmentsGroups, context.Validators.DerivativeSecurityList_NoMarketSegments);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.DerivativeSecurityList_NoRelatedSym);
	}

	static void ValidateDerivativeSecurityListRequest(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(check, FixTag.SecurityListRequestType);
		else context.Validators.SecurityListRequestType(context, check, message.SecurityListRequestType);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.DerivativeSymbol is not null) context.Validators.DerivativeSymbol(context, check, message.DerivativeSymbol);
		if (message.DerivativeSymbolSfx is not null) context.Validators.DerivativeSymbolSfx(context, check, message.DerivativeSymbolSfx);
		if (message.DerivativeSecurityID is not null) context.Validators.DerivativeSecurityID(context, check, message.DerivativeSecurityID);
		if (message.DerivativeSecurityIDSource is not null) context.Validators.DerivativeSecurityIDSource(context, check, message.DerivativeSecurityIDSource);
		if (message.NoDerivativeSecurityAltID is not null) context.Validators.NoDerivativeSecurityAltID(context, check, message.NoDerivativeSecurityAltID);
		Counted(check, message.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups);
		Each(context, check, FixTag.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups, context.Validators.DerivativeSecurityListRequest_NoDerivativeSecurityAltID);
		if (message.DerivativeProduct is not null) context.Validators.DerivativeProduct(context, check, message.DerivativeProduct);
		if (message.DerivativeProductComplex is not null) context.Validators.DerivativeProductComplex(context, check, message.DerivativeProductComplex);
		if (message.DerivFlexProductEligibilityIndicator is not null) context.Validators.DerivFlexProductEligibilityIndicator(context, check, message.DerivFlexProductEligibilityIndicator);
		if (message.DerivativeSecurityGroup is not null) context.Validators.DerivativeSecurityGroup(context, check, message.DerivativeSecurityGroup);
		if (message.DerivativeCFICode is not null) context.Validators.DerivativeCFICode(context, check, message.DerivativeCFICode);
		if (message.DerivativeSecurityType is not null) context.Validators.DerivativeSecurityType(context, check, message.DerivativeSecurityType);
		if (message.DerivativeSecuritySubType is not null) context.Validators.DerivativeSecuritySubType(context, check, message.DerivativeSecuritySubType);
		if (message.DerivativeMaturityMonthYear is not null) context.Validators.DerivativeMaturityMonthYear(context, check, message.DerivativeMaturityMonthYear);
		if (message.DerivativeMaturityDate is not null) context.Validators.DerivativeMaturityDate(context, check, message.DerivativeMaturityDate);
		if (message.DerivativeMaturityTime is not null) context.Validators.DerivativeMaturityTime(context, check, message.DerivativeMaturityTime);
		if (message.DerivativeSettleOnOpenFlag is not null) context.Validators.DerivativeSettleOnOpenFlag(context, check, message.DerivativeSettleOnOpenFlag);
		if (message.DerivativeInstrmtAssignmentMethod is not null) context.Validators.DerivativeInstrmtAssignmentMethod(context, check, message.DerivativeInstrmtAssignmentMethod);
		if (message.DerivativeSecurityStatus is not null) context.Validators.DerivativeSecurityStatus(context, check, message.DerivativeSecurityStatus);
		if (message.DerivativeIssueDate is not null) context.Validators.DerivativeIssueDate(context, check, message.DerivativeIssueDate);
		if (message.DerivativeInstrRegistry is not null) context.Validators.DerivativeInstrRegistry(context, check, message.DerivativeInstrRegistry);
		if (message.DerivativeCountryOfIssue is not null) context.Validators.DerivativeCountryOfIssue(context, check, message.DerivativeCountryOfIssue);
		if (message.DerivativeStateOrProvinceOfIssue is not null) context.Validators.DerivativeStateOrProvinceOfIssue(context, check, message.DerivativeStateOrProvinceOfIssue);
		if (message.DerivativeLocaleOfIssue is not null) context.Validators.DerivativeLocaleOfIssue(context, check, message.DerivativeLocaleOfIssue);
		if (message.DerivativeStrikePrice is not null) context.Validators.DerivativeStrikePrice(context, check, message.DerivativeStrikePrice);
		if (message.DerivativeStrikeCurrency is not null) context.Validators.DerivativeStrikeCurrency(context, check, message.DerivativeStrikeCurrency);
		if (message.DerivativeStrikeMultiplier is not null) context.Validators.DerivativeStrikeMultiplier(context, check, message.DerivativeStrikeMultiplier);
		if (message.DerivativeStrikeValue is not null) context.Validators.DerivativeStrikeValue(context, check, message.DerivativeStrikeValue);
		if (message.DerivativeOptAttribute is not null) context.Validators.DerivativeOptAttribute(context, check, message.DerivativeOptAttribute);
		if (message.DerivativeContractMultiplier is not null) context.Validators.DerivativeContractMultiplier(context, check, message.DerivativeContractMultiplier);
		if (message.DerivativeContractMultiplierUnit is not null) context.Validators.DerivativeContractMultiplierUnit(context, check, message.DerivativeContractMultiplierUnit);
		if (message.DerivativeFlowScheduleType is not null) context.Validators.DerivativeFlowScheduleType(context, check, message.DerivativeFlowScheduleType);
		if (message.DerivativeMinPriceIncrement is not null) context.Validators.DerivativeMinPriceIncrement(context, check, message.DerivativeMinPriceIncrement);
		if (message.DerivativeMinPriceIncrementAmount is not null) context.Validators.DerivativeMinPriceIncrementAmount(context, check, message.DerivativeMinPriceIncrementAmount);
		if (message.DerivativeUnitOfMeasure is not null) context.Validators.DerivativeUnitOfMeasure(context, check, message.DerivativeUnitOfMeasure);
		if (message.DerivativeUnitOfMeasureQty is not null) context.Validators.DerivativeUnitOfMeasureQty(context, check, message.DerivativeUnitOfMeasureQty);
		if (message.DerivativePriceUnitOfMeasure is not null) context.Validators.DerivativePriceUnitOfMeasure(context, check, message.DerivativePriceUnitOfMeasure);
		if (message.DerivativePriceUnitOfMeasureQty is not null) context.Validators.DerivativePriceUnitOfMeasureQty(context, check, message.DerivativePriceUnitOfMeasureQty);
		if (message.DerivativeSettlMethod is not null) context.Validators.DerivativeSettlMethod(context, check, message.DerivativeSettlMethod);
		if (message.DerivativePriceQuoteMethod is not null) context.Validators.DerivativePriceQuoteMethod(context, check, message.DerivativePriceQuoteMethod);
		if (message.DerivativeValuationMethod is not null) context.Validators.DerivativeValuationMethod(context, check, message.DerivativeValuationMethod);
		if (message.DerivativeListMethod is not null) context.Validators.DerivativeListMethod(context, check, message.DerivativeListMethod);
		if (message.DerivativeCapPrice is not null) context.Validators.DerivativeCapPrice(context, check, message.DerivativeCapPrice);
		if (message.DerivativeFloorPrice is not null) context.Validators.DerivativeFloorPrice(context, check, message.DerivativeFloorPrice);
		if (message.DerivativePutOrCall is not null) context.Validators.DerivativePutOrCall(context, check, message.DerivativePutOrCall);
		if (message.DerivativeExerciseStyle is not null) context.Validators.DerivativeExerciseStyle(context, check, message.DerivativeExerciseStyle);
		if (message.DerivativeOptPayAmount is not null) context.Validators.DerivativeOptPayAmount(context, check, message.DerivativeOptPayAmount);
		if (message.DerivativeTimeUnit is not null) context.Validators.DerivativeTimeUnit(context, check, message.DerivativeTimeUnit);
		if (message.DerivativeSecurityExchange is not null) context.Validators.DerivativeSecurityExchange(context, check, message.DerivativeSecurityExchange);
		if (message.DerivativePositionLimit is not null) context.Validators.DerivativePositionLimit(context, check, message.DerivativePositionLimit);
		if (message.DerivativeNTPositionLimit is not null) context.Validators.DerivativeNTPositionLimit(context, check, message.DerivativeNTPositionLimit);
		if (message.DerivativeIssuer is not null) context.Validators.DerivativeIssuer(context, check, message.DerivativeIssuer);
		if (message.DerivativeEncodedIssuerLen is not null) context.Validators.DerivativeEncodedIssuerLen(context, check, message.DerivativeEncodedIssuerLen);
		if (message.DerivativeEncodedIssuer is not null) context.Validators.DerivativeEncodedIssuer(context, check, message.DerivativeEncodedIssuer);
		if (message.DerivativeSecurityDesc is not null) context.Validators.DerivativeSecurityDesc(context, check, message.DerivativeSecurityDesc);
		if (message.DerivativeEncodedSecurityDescLen is not null) context.Validators.DerivativeEncodedSecurityDescLen(context, check, message.DerivativeEncodedSecurityDescLen);
		if (message.DerivativeEncodedSecurityDesc is not null) context.Validators.DerivativeEncodedSecurityDesc(context, check, message.DerivativeEncodedSecurityDesc);
		if (message.DerivativeSecurityXMLLen is not null) context.Validators.DerivativeSecurityXMLLen(context, check, message.DerivativeSecurityXMLLen);
		if (message.DerivativeSecurityXML is not null) context.Validators.DerivativeSecurityXML(context, check, message.DerivativeSecurityXML);
		if (message.DerivativeSecurityXMLSchema is not null) context.Validators.DerivativeSecurityXMLSchema(context, check, message.DerivativeSecurityXMLSchema);
		if (message.DerivativeContractSettlMonth is not null) context.Validators.DerivativeContractSettlMonth(context, check, message.DerivativeContractSettlMonth);
		if (message.NoDerivativeEvents is not null) context.Validators.NoDerivativeEvents(context, check, message.NoDerivativeEvents);
		Counted(check, message.NoDerivativeEvents, message.NoDerivativeEventsGroups);
		Each(context, check, FixTag.NoDerivativeEvents, message.NoDerivativeEventsGroups, context.Validators.DerivativeSecurityListRequest_NoDerivativeEvents);
		if (message.NoDerivativeInstrumentParties is not null) context.Validators.NoDerivativeInstrumentParties(context, check, message.NoDerivativeInstrumentParties);
		Counted(check, message.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups, context.Validators.DerivativeSecurityListRequest_NoDerivativeInstrumentParties);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, message.SecuritySubType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateDerivativeSecurityListUpdateReport(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (message.SecurityUpdateAction is not null) context.Validators.SecurityUpdateAction(context, check, message.SecurityUpdateAction);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.DerivativeSymbol is not null) context.Validators.DerivativeSymbol(context, check, message.DerivativeSymbol);
		if (message.DerivativeSymbolSfx is not null) context.Validators.DerivativeSymbolSfx(context, check, message.DerivativeSymbolSfx);
		if (message.DerivativeSecurityID is not null) context.Validators.DerivativeSecurityID(context, check, message.DerivativeSecurityID);
		if (message.DerivativeSecurityIDSource is not null) context.Validators.DerivativeSecurityIDSource(context, check, message.DerivativeSecurityIDSource);
		if (message.NoDerivativeSecurityAltID is not null) context.Validators.NoDerivativeSecurityAltID(context, check, message.NoDerivativeSecurityAltID);
		Counted(check, message.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups);
		Each(context, check, FixTag.NoDerivativeSecurityAltID, message.NoDerivativeSecurityAltIDGroups, context.Validators.DerivativeSecurityListUpdateReport_NoDerivativeSecurityAltID);
		if (message.DerivativeProduct is not null) context.Validators.DerivativeProduct(context, check, message.DerivativeProduct);
		if (message.DerivativeProductComplex is not null) context.Validators.DerivativeProductComplex(context, check, message.DerivativeProductComplex);
		if (message.DerivFlexProductEligibilityIndicator is not null) context.Validators.DerivFlexProductEligibilityIndicator(context, check, message.DerivFlexProductEligibilityIndicator);
		if (message.DerivativeSecurityGroup is not null) context.Validators.DerivativeSecurityGroup(context, check, message.DerivativeSecurityGroup);
		if (message.DerivativeCFICode is not null) context.Validators.DerivativeCFICode(context, check, message.DerivativeCFICode);
		if (message.DerivativeSecurityType is not null) context.Validators.DerivativeSecurityType(context, check, message.DerivativeSecurityType);
		if (message.DerivativeSecuritySubType is not null) context.Validators.DerivativeSecuritySubType(context, check, message.DerivativeSecuritySubType);
		if (message.DerivativeMaturityMonthYear is not null) context.Validators.DerivativeMaturityMonthYear(context, check, message.DerivativeMaturityMonthYear);
		if (message.DerivativeMaturityDate is not null) context.Validators.DerivativeMaturityDate(context, check, message.DerivativeMaturityDate);
		if (message.DerivativeMaturityTime is not null) context.Validators.DerivativeMaturityTime(context, check, message.DerivativeMaturityTime);
		if (message.DerivativeSettleOnOpenFlag is not null) context.Validators.DerivativeSettleOnOpenFlag(context, check, message.DerivativeSettleOnOpenFlag);
		if (message.DerivativeInstrmtAssignmentMethod is not null) context.Validators.DerivativeInstrmtAssignmentMethod(context, check, message.DerivativeInstrmtAssignmentMethod);
		if (message.DerivativeSecurityStatus is not null) context.Validators.DerivativeSecurityStatus(context, check, message.DerivativeSecurityStatus);
		if (message.DerivativeIssueDate is not null) context.Validators.DerivativeIssueDate(context, check, message.DerivativeIssueDate);
		if (message.DerivativeInstrRegistry is not null) context.Validators.DerivativeInstrRegistry(context, check, message.DerivativeInstrRegistry);
		if (message.DerivativeCountryOfIssue is not null) context.Validators.DerivativeCountryOfIssue(context, check, message.DerivativeCountryOfIssue);
		if (message.DerivativeStateOrProvinceOfIssue is not null) context.Validators.DerivativeStateOrProvinceOfIssue(context, check, message.DerivativeStateOrProvinceOfIssue);
		if (message.DerivativeLocaleOfIssue is not null) context.Validators.DerivativeLocaleOfIssue(context, check, message.DerivativeLocaleOfIssue);
		if (message.DerivativeStrikePrice is not null) context.Validators.DerivativeStrikePrice(context, check, message.DerivativeStrikePrice);
		if (message.DerivativeStrikeCurrency is not null) context.Validators.DerivativeStrikeCurrency(context, check, message.DerivativeStrikeCurrency);
		if (message.DerivativeStrikeMultiplier is not null) context.Validators.DerivativeStrikeMultiplier(context, check, message.DerivativeStrikeMultiplier);
		if (message.DerivativeStrikeValue is not null) context.Validators.DerivativeStrikeValue(context, check, message.DerivativeStrikeValue);
		if (message.DerivativeOptAttribute is not null) context.Validators.DerivativeOptAttribute(context, check, message.DerivativeOptAttribute);
		if (message.DerivativeContractMultiplier is not null) context.Validators.DerivativeContractMultiplier(context, check, message.DerivativeContractMultiplier);
		if (message.DerivativeContractMultiplierUnit is not null) context.Validators.DerivativeContractMultiplierUnit(context, check, message.DerivativeContractMultiplierUnit);
		if (message.DerivativeFlowScheduleType is not null) context.Validators.DerivativeFlowScheduleType(context, check, message.DerivativeFlowScheduleType);
		if (message.DerivativeMinPriceIncrement is not null) context.Validators.DerivativeMinPriceIncrement(context, check, message.DerivativeMinPriceIncrement);
		if (message.DerivativeMinPriceIncrementAmount is not null) context.Validators.DerivativeMinPriceIncrementAmount(context, check, message.DerivativeMinPriceIncrementAmount);
		if (message.DerivativeUnitOfMeasure is not null) context.Validators.DerivativeUnitOfMeasure(context, check, message.DerivativeUnitOfMeasure);
		if (message.DerivativeUnitOfMeasureQty is not null) context.Validators.DerivativeUnitOfMeasureQty(context, check, message.DerivativeUnitOfMeasureQty);
		if (message.DerivativePriceUnitOfMeasure is not null) context.Validators.DerivativePriceUnitOfMeasure(context, check, message.DerivativePriceUnitOfMeasure);
		if (message.DerivativePriceUnitOfMeasureQty is not null) context.Validators.DerivativePriceUnitOfMeasureQty(context, check, message.DerivativePriceUnitOfMeasureQty);
		if (message.DerivativeSettlMethod is not null) context.Validators.DerivativeSettlMethod(context, check, message.DerivativeSettlMethod);
		if (message.DerivativePriceQuoteMethod is not null) context.Validators.DerivativePriceQuoteMethod(context, check, message.DerivativePriceQuoteMethod);
		if (message.DerivativeValuationMethod is not null) context.Validators.DerivativeValuationMethod(context, check, message.DerivativeValuationMethod);
		if (message.DerivativeListMethod is not null) context.Validators.DerivativeListMethod(context, check, message.DerivativeListMethod);
		if (message.DerivativeCapPrice is not null) context.Validators.DerivativeCapPrice(context, check, message.DerivativeCapPrice);
		if (message.DerivativeFloorPrice is not null) context.Validators.DerivativeFloorPrice(context, check, message.DerivativeFloorPrice);
		if (message.DerivativePutOrCall is not null) context.Validators.DerivativePutOrCall(context, check, message.DerivativePutOrCall);
		if (message.DerivativeExerciseStyle is not null) context.Validators.DerivativeExerciseStyle(context, check, message.DerivativeExerciseStyle);
		if (message.DerivativeOptPayAmount is not null) context.Validators.DerivativeOptPayAmount(context, check, message.DerivativeOptPayAmount);
		if (message.DerivativeTimeUnit is not null) context.Validators.DerivativeTimeUnit(context, check, message.DerivativeTimeUnit);
		if (message.DerivativeSecurityExchange is not null) context.Validators.DerivativeSecurityExchange(context, check, message.DerivativeSecurityExchange);
		if (message.DerivativePositionLimit is not null) context.Validators.DerivativePositionLimit(context, check, message.DerivativePositionLimit);
		if (message.DerivativeNTPositionLimit is not null) context.Validators.DerivativeNTPositionLimit(context, check, message.DerivativeNTPositionLimit);
		if (message.DerivativeIssuer is not null) context.Validators.DerivativeIssuer(context, check, message.DerivativeIssuer);
		if (message.DerivativeEncodedIssuerLen is not null) context.Validators.DerivativeEncodedIssuerLen(context, check, message.DerivativeEncodedIssuerLen);
		if (message.DerivativeEncodedIssuer is not null) context.Validators.DerivativeEncodedIssuer(context, check, message.DerivativeEncodedIssuer);
		if (message.DerivativeSecurityDesc is not null) context.Validators.DerivativeSecurityDesc(context, check, message.DerivativeSecurityDesc);
		if (message.DerivativeEncodedSecurityDescLen is not null) context.Validators.DerivativeEncodedSecurityDescLen(context, check, message.DerivativeEncodedSecurityDescLen);
		if (message.DerivativeEncodedSecurityDesc is not null) context.Validators.DerivativeEncodedSecurityDesc(context, check, message.DerivativeEncodedSecurityDesc);
		if (message.DerivativeSecurityXMLLen is not null) context.Validators.DerivativeSecurityXMLLen(context, check, message.DerivativeSecurityXMLLen);
		if (message.DerivativeSecurityXML is not null) context.Validators.DerivativeSecurityXML(context, check, message.DerivativeSecurityXML);
		if (message.DerivativeSecurityXMLSchema is not null) context.Validators.DerivativeSecurityXMLSchema(context, check, message.DerivativeSecurityXMLSchema);
		if (message.DerivativeContractSettlMonth is not null) context.Validators.DerivativeContractSettlMonth(context, check, message.DerivativeContractSettlMonth);
		if (message.NoDerivativeEvents is not null) context.Validators.NoDerivativeEvents(context, check, message.NoDerivativeEvents);
		Counted(check, message.NoDerivativeEvents, message.NoDerivativeEventsGroups);
		Each(context, check, FixTag.NoDerivativeEvents, message.NoDerivativeEventsGroups, context.Validators.DerivativeSecurityListUpdateReport_NoDerivativeEvents);
		if (message.NoDerivativeInstrumentParties is not null) context.Validators.NoDerivativeInstrumentParties(context, check, message.NoDerivativeInstrumentParties);
		Counted(check, message.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentParties, message.NoDerivativeInstrumentPartiesGroups, context.Validators.DerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties);
		if (message.NoDerivativeInstrAttrib is not null) context.Validators.NoDerivativeInstrAttrib(context, check, message.NoDerivativeInstrAttrib);
		Counted(check, message.NoDerivativeInstrAttrib, message.NoDerivativeInstrAttribGroups);
		Each(context, check, FixTag.NoDerivativeInstrAttrib, message.NoDerivativeInstrAttribGroups, context.Validators.DerivativeSecurityListUpdateReport_NoDerivativeInstrAttrib);
		if (message.NoMarketSegments is not null) context.Validators.NoMarketSegments(context, check, message.NoMarketSegments);
		Counted(check, message.NoMarketSegments, message.NoMarketSegmentsGroups);
		Each(context, check, FixTag.NoMarketSegments, message.NoMarketSegmentsGroups, context.Validators.DerivativeSecurityListUpdateReport_NoMarketSegments);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.DerivativeSecurityListUpdateReport_NoRelatedSym);
	}

	static void ValidateDontKnowTrade(Fix50Context context, FixCheck check, FixMessage.DontKnowTrade message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.ExecID is null) Missing(check, FixTag.ExecID);
		else context.Validators.ExecID(context, check, message.ExecID);
		if (message.DKReason is null) Missing(check, FixTag.DKReason);
		else context.Validators.DKReason(context, check, message.DKReason);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.DontKnowTrade_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.DontKnowTrade_NoLegs);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.LastQty is not null) context.Validators.LastQty(context, check, message.LastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateEmail(Fix50Context context, FixCheck check, FixMessage.Email message)
	{
		if (message.EmailThreadID is null) Missing(check, FixTag.EmailThreadID);
		else context.Validators.EmailThreadID(context, check, message.EmailThreadID);
		if (message.EmailType is null) Missing(check, FixTag.EmailType);
		else context.Validators.EmailType(context, check, message.EmailType);
		if (message.OrigTime is not null) context.Validators.OrigTime(context, check, message.OrigTime);
		if (message.Subject is null) Missing(check, FixTag.Subject);
		else context.Validators.Subject(context, check, message.Subject);
		if (message.EncodedSubjectLen is not null) context.Validators.EncodedSubjectLen(context, check, message.EncodedSubjectLen);
		if (message.EncodedSubject is not null) context.Validators.EncodedSubject(context, check, message.EncodedSubject);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.Email_NoRoutingIDs);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.Email_NoRelatedSym);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.Email_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.Email_NoLegs);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.NoLinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.NoLinesOfText(context, check, message.NoLinesOfText);
		Counted(check, message.NoLinesOfText, message.NoLinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.NoLinesOfTextGroups, context.Validators.Email_NoLinesOfText);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateExecutionAcknowledgement(Fix50Context context, FixCheck check, FixMessage.ExecutionAcknowledgement message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ExecAckStatus is null) Missing(check, FixTag.ExecAckStatus);
		else context.Validators.ExecAckStatus(context, check, message.ExecAckStatus);
		if (message.ExecID is null) Missing(check, FixTag.ExecID);
		else context.Validators.ExecID(context, check, message.ExecID);
		if (message.DKReason is not null) context.Validators.DKReason(context, check, message.DKReason);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.ExecutionAcknowledgement_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.ExecutionAcknowledgement_NoLegs);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.LastQty is not null) context.Validators.LastQty(context, check, message.LastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, check, message.LastParPx);
		if (message.CumQty is not null) context.Validators.CumQty(context, check, message.CumQty);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateExecutionReport(Fix50Context context, FixCheck check, FixMessage.ExecutionReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, message.SecondaryExecID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.OrdStatusReqID is not null) context.Validators.OrdStatusReqID(context, check, message.OrdStatusReqID);
		if (message.MassStatusReqID is not null) context.Validators.MassStatusReqID(context, check, message.MassStatusReqID);
		if (message.HostCrossID is not null) context.Validators.HostCrossID(context, check, message.HostCrossID);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, check, message.TotNumReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, check, message.LastRptRequested);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.NoContraBrokers is not null) context.Validators.NoContraBrokers(context, check, message.NoContraBrokers);
		Counted(check, message.NoContraBrokers, message.NoContraBrokersGroups);
		Each(context, check, FixTag.NoContraBrokers, message.NoContraBrokersGroups, context.Validators.ExecutionReport_NoContraBrokers);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.CrossID is not null) context.Validators.CrossID(context, check, message.CrossID);
		if (message.OrigCrossID is not null) context.Validators.OrigCrossID(context, check, message.OrigCrossID);
		if (message.CrossType is not null) context.Validators.CrossType(context, check, message.CrossType);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, check, message.TrdMatchID);
		if (message.ExecID is null) Missing(check, FixTag.ExecID);
		else context.Validators.ExecID(context, check, message.ExecID);
		if (message.ExecRefID is not null) context.Validators.ExecRefID(context, check, message.ExecRefID);
		if (message.ExecType is null) Missing(check, FixTag.ExecType);
		else context.Validators.ExecType(context, check, message.ExecType);
		if (message.OrdStatus is null) Missing(check, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, check, message.OrdStatus);
		if (message.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, check, message.WorkingIndicator);
		if (message.OrdRejReason is not null) context.Validators.OrdRejReason(context, check, message.OrdRejReason);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, check, message.ExecRestatementReason);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, check, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.ExecutionReport_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.OrderCategory is not null) context.Validators.OrderCategory(context, check, message.OrderCategory);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.ExecutionReport_NoUnderlyings);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.LotType is not null) context.Validators.LotType(context, check, message.LotType);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.PeggedPrice is not null) context.Validators.PeggedPrice(context, check, message.PeggedPrice);
		if (message.PeggedRefPrice is not null) context.Validators.PeggedRefPrice(context, check, message.PeggedRefPrice);
		if (message.DiscretionPrice is not null) context.Validators.DiscretionPrice(context, check, message.DiscretionPrice);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.ExecutionReport_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.TargetStrategyPerformance is not null) context.Validators.TargetStrategyPerformance(context, check, message.TargetStrategyPerformance);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.AggressorIndicator is not null) context.Validators.AggressorIndicator(context, check, message.AggressorIndicator);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.LastQty is not null) context.Validators.LastQty(context, check, message.LastQty);
		if (message.CalculatedCcyLastQty is not null) context.Validators.CalculatedCcyLastQty(context, check, message.CalculatedCcyLastQty);
		if (message.LastSwapPoints is not null) context.Validators.LastSwapPoints(context, check, message.LastSwapPoints);
		if (message.UnderlyingLastQty is not null) context.Validators.UnderlyingLastQty(context, check, message.UnderlyingLastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.UnderlyingLastPx is not null) context.Validators.UnderlyingLastPx(context, check, message.UnderlyingLastPx);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, check, message.LastParPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, check, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, check, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TimeBracket is not null) context.Validators.TimeBracket(context, check, message.TimeBracket);
		if (message.LastCapacity is not null) context.Validators.LastCapacity(context, check, message.LastCapacity);
		if (message.LeavesQty is null) Missing(check, FixTag.LeavesQty);
		else context.Validators.LeavesQty(context, check, message.LeavesQty);
		if (message.CumQty is null) Missing(check, FixTag.CumQty);
		else context.Validators.CumQty(context, check, message.CumQty);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.DayOrderQty is not null) context.Validators.DayOrderQty(context, check, message.DayOrderQty);
		if (message.DayCumQty is not null) context.Validators.DayCumQty(context, check, message.DayCumQty);
		if (message.DayAvgPx is not null) context.Validators.DayAvgPx(context, check, message.DayAvgPx);
		if (message.TotNoFills is not null) context.Validators.TotNoFills(context, check, message.TotNoFills);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoFills is not null) context.Validators.NoFills(context, check, message.NoFills);
		Counted(check, message.NoFills, message.NoFillsGroups);
		Each(context, check, FixTag.NoFills, message.NoFillsGroups, context.Validators.ExecutionReport_NoFills);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.ReportToExch is not null) context.Validators.ReportToExch(context, check, message.ReportToExch);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.ExDate is not null) context.Validators.ExDate(context, check, message.ExDate);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, message.AccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, check, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, check, message.EndCash);
		if (message.TradedFlatSwitch is not null) context.Validators.TradedFlatSwitch(context, check, message.TradedFlatSwitch);
		if (message.BasisFeatureDate is not null) context.Validators.BasisFeatureDate(context, check, message.BasisFeatureDate);
		if (message.BasisFeaturePrice is not null) context.Validators.BasisFeaturePrice(context, check, message.BasisFeaturePrice);
		if (message.Concession is not null) context.Validators.Concession(context, check, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (!Empty((IRateSource)message)) context.Validators.RateSource(context, check, message);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.LastForwardPoints2 is not null) context.Validators.LastForwardPoints2(context, check, message.LastForwardPoints2);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, message.TransBkdTime);
		if (message.ExecValuationPoint is not null) context.Validators.ExecValuationPoint(context, check, message.ExecValuationPoint);
		if (message.ExecPriceType is not null) context.Validators.ExecPriceType(context, check, message.ExecPriceType);
		if (message.ExecPriceAdjustment is not null) context.Validators.ExecPriceAdjustment(context, check, message.ExecPriceAdjustment);
		if (message.PriorityIndicator is not null) context.Validators.PriorityIndicator(context, check, message.PriorityIndicator);
		if (message.PriceImprovement is not null) context.Validators.PriceImprovement(context, check, message.PriceImprovement);
		if (message.LastLiquidityInd is not null) context.Validators.LastLiquidityInd(context, check, message.LastLiquidityInd);
		if (message.NoContAmts is not null) context.Validators.NoContAmts(context, check, message.NoContAmts);
		Counted(check, message.NoContAmts, message.NoContAmtsGroups);
		Each(context, check, FixTag.NoContAmts, message.NoContAmtsGroups, context.Validators.ExecutionReport_NoContAmts);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.ExecutionReport_NoLegs);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, check, message.CopyMsgIndicator);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, message.NoMiscFees);
		Counted(check, message.NoMiscFees, message.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, message.NoMiscFeesGroups, context.Validators.ExecutionReport_NoMiscFees);
		if (message.DividendYield is not null) context.Validators.DividendYield(context, check, message.DividendYield);
		if (message.ManualOrderIndicator is not null) context.Validators.ManualOrderIndicator(context, check, message.ManualOrderIndicator);
		if (message.CustDirectedOrder is not null) context.Validators.CustDirectedOrder(context, check, message.CustDirectedOrder);
		if (message.ReceivedDeptID is not null) context.Validators.ReceivedDeptID(context, check, message.ReceivedDeptID);
		if (message.CustOrderHandlingInst is not null) context.Validators.CustOrderHandlingInst(context, check, message.CustOrderHandlingInst);
		if (message.OrderHandlingInstSource is not null) context.Validators.OrderHandlingInstSource(context, check, message.OrderHandlingInstSource);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.Volatility is not null) context.Validators.Volatility(context, check, message.Volatility);
		if (message.TimeToExpiration is not null) context.Validators.TimeToExpiration(context, check, message.TimeToExpiration);
		if (message.RiskFreeRate is not null) context.Validators.RiskFreeRate(context, check, message.RiskFreeRate);
		if (message.PriceDelta is not null) context.Validators.PriceDelta(context, check, message.PriceDelta);
	}

	static void ValidateHeartbeat(Fix50Context context, FixCheck check, FixMessage.Heartbeat message)
	{
		if (message.TestReqID is not null) context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateIOI(Fix50Context context, FixCheck check, FixMessage.IOI message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.IOIID is null) Missing(check, FixTag.IOIID);
		else context.Validators.IOIID(context, check, message.IOIID);
		if (message.IOITransType is null) Missing(check, FixTag.IOITransType);
		else context.Validators.IOITransType(context, check, message.IOITransType);
		if (message.IOIRefID is not null) context.Validators.IOIRefID(context, check, message.IOIRefID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.IOI_NoUnderlyings);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.IOIQty is null) Missing(check, FixTag.IOIQty);
		else context.Validators.IOIQty(context, check, message.IOIQty);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.IOI_NoLegs);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.IOIQltyInd is not null) context.Validators.IOIQltyInd(context, check, message.IOIQltyInd);
		if (message.IOINaturalFlag is not null) context.Validators.IOINaturalFlag(context, check, message.IOINaturalFlag);
		if (message.NoIOIQualifiers is not null) context.Validators.NoIOIQualifiers(context, check, message.NoIOIQualifiers);
		Counted(check, message.NoIOIQualifiers, message.NoIOIQualifiersGroups);
		Each(context, check, FixTag.NoIOIQualifiers, message.NoIOIQualifiersGroups, context.Validators.IOI_NoIOIQualifiers);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.IOI_NoRoutingIDs);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
	}

	static void ValidateListCancelRequest(Fix50Context context, FixCheck check, FixMessage.ListCancelRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListExecute(Fix50Context context, FixCheck check, FixMessage.ListExecute message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListStatus(Fix50Context context, FixCheck check, FixMessage.ListStatus message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.ListStatusType is null) Missing(check, FixTag.ListStatusType);
		else context.Validators.ListStatusType(context, check, message.ListStatusType);
		if (message.NoRpts is null) Missing(check, FixTag.NoRpts);
		else context.Validators.NoRpts(context, check, message.NoRpts);
		if (message.ListOrderStatus is null) Missing(check, FixTag.ListOrderStatus);
		else context.Validators.ListOrderStatus(context, check, message.ListOrderStatus);
		if (message.ContingencyType is not null) context.Validators.ContingencyType(context, check, message.ContingencyType);
		if (message.ListRejectReason is not null) context.Validators.ListRejectReason(context, check, message.ListRejectReason);
		if (message.RptSeq is null) Missing(check, FixTag.RptSeq);
		else context.Validators.RptSeq(context, check, message.RptSeq);
		if (message.ListStatusText is not null) context.Validators.ListStatusText(context, check, message.ListStatusText);
		if (message.EncodedListStatusTextLen is not null) context.Validators.EncodedListStatusTextLen(context, check, message.EncodedListStatusTextLen);
		if (message.EncodedListStatusText is not null) context.Validators.EncodedListStatusText(context, check, message.EncodedListStatusText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TotNoOrders is null) Missing(check, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, check, message.TotNoOrders);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoOrders is null) Missing(check, FixTag.NoOrders);
		else context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.ListStatus_NoOrders);
	}

	static void ValidateListStatusRequest(Fix50Context context, FixCheck check, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListStrikePrice(Fix50Context context, FixCheck check, FixMessage.ListStrikePrice message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.TotNoStrikes is null) Missing(check, FixTag.TotNoStrikes);
		else context.Validators.TotNoStrikes(context, check, message.TotNoStrikes);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoStrikes is null) Missing(check, FixTag.NoStrikes);
		else context.Validators.NoStrikes(context, check, message.NoStrikes);
		Counted(check, message.NoStrikes, message.NoStrikesGroups);
		Each(context, check, FixTag.NoStrikes, message.NoStrikesGroups, context.Validators.ListStrikePrice_NoStrikes);
	}

	static void ValidateLogon(Fix50Context context, FixCheck check, FixMessage.Logon message)
	{
		if (message.EncryptMethod is null) Missing(check, FixTag.EncryptMethod);
		else context.Validators.EncryptMethod(context, check, message.EncryptMethod);
		if (message.HeartBtInt is null) Missing(check, FixTag.HeartBtInt);
		else context.Validators.HeartBtInt(context, check, message.HeartBtInt);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
		if (message.ResetSeqNumFlag is not null) context.Validators.ResetSeqNumFlag(context, check, message.ResetSeqNumFlag);
		if (message.NextExpectedMsgSeqNum is not null) context.Validators.NextExpectedMsgSeqNum(context, check, message.NextExpectedMsgSeqNum);
		if (message.MaxMessageSize is not null) context.Validators.MaxMessageSize(context, check, message.MaxMessageSize);
		if (message.NoMsgTypes is not null) context.Validators.NoMsgTypes(context, check, message.NoMsgTypes);
		Counted(check, message.NoMsgTypes, message.NoMsgTypesGroups);
		Each(context, check, FixTag.NoMsgTypes, message.NoMsgTypesGroups, context.Validators.Logon_NoMsgTypes);
		if (message.TestMessageIndicator is not null) context.Validators.TestMessageIndicator(context, check, message.TestMessageIndicator);
		if (message.Username is not null) context.Validators.Username(context, check, message.Username);
		if (message.Password is not null) context.Validators.Password(context, check, message.Password);
		if (message.NewPassword is not null) context.Validators.NewPassword(context, check, message.NewPassword);
		if (message.EncryptedPasswordMethod is not null) context.Validators.EncryptedPasswordMethod(context, check, message.EncryptedPasswordMethod);
		if (message.EncryptedPasswordLen is not null) context.Validators.EncryptedPasswordLen(context, check, message.EncryptedPasswordLen);
		if (message.EncryptedPassword is not null) context.Validators.EncryptedPassword(context, check, message.EncryptedPassword);
		if (message.EncryptedNewPasswordLen is not null) context.Validators.EncryptedNewPasswordLen(context, check, message.EncryptedNewPasswordLen);
		if (message.EncryptedNewPassword is not null) context.Validators.EncryptedNewPassword(context, check, message.EncryptedNewPassword);
		if (message.SessionStatus is not null) context.Validators.SessionStatus(context, check, message.SessionStatus);
		if (message.DefaultApplVerID is null) Missing(check, FixTag.DefaultApplVerID);
		else context.Validators.DefaultApplVerID(context, check, message.DefaultApplVerID);
		if (message.DefaultApplExtID is not null) context.Validators.DefaultApplExtID(context, check, message.DefaultApplExtID);
		if (message.DefaultCstmApplVerID is not null) context.Validators.DefaultCstmApplVerID(context, check, message.DefaultCstmApplVerID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateLogout(Fix50Context context, FixCheck check, FixMessage.Logout message)
	{
		if (message.SessionStatus is not null) context.Validators.SessionStatus(context, check, message.SessionStatus);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataIncrementalRefresh(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.MDBookType is not null) context.Validators.MDBookType(context, check, message.MDBookType);
		if (message.MDFeedType is not null) context.Validators.MDFeedType(context, check, message.MDFeedType);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.NoMDEntries is null) Missing(check, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, check, message.NoMDEntries);
		Counted(check, message.NoMDEntries, message.NoMDEntriesGroups);
		Each(context, check, FixTag.NoMDEntries, message.NoMDEntriesGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries);
		if (message.ApplQueueDepth is not null) context.Validators.ApplQueueDepth(context, check, message.ApplQueueDepth);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, check, message.ApplQueueResolution);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.MarketDataIncrementalRefresh_NoRoutingIDs);
	}

	static void ValidateMarketDataRequest(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.MarketDepth is null) Missing(check, FixTag.MarketDepth);
		else context.Validators.MarketDepth(context, check, message.MarketDepth);
		if (message.MDUpdateType is not null) context.Validators.MDUpdateType(context, check, message.MDUpdateType);
		if (message.AggregatedBook is not null) context.Validators.AggregatedBook(context, check, message.AggregatedBook);
		if (message.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, check, message.OpenCloseSettlFlag);
		if (message.Scope is not null) context.Validators.Scope(context, check, message.Scope);
		if (message.MDImplicitDelete is not null) context.Validators.MDImplicitDelete(context, check, message.MDImplicitDelete);
		if (message.NoMDEntryTypes is null) Missing(check, FixTag.NoMDEntryTypes);
		else context.Validators.NoMDEntryTypes(context, check, message.NoMDEntryTypes);
		Counted(check, message.NoMDEntryTypes, message.NoMDEntryTypesGroups);
		Each(context, check, FixTag.NoMDEntryTypes, message.NoMDEntryTypesGroups, context.Validators.MarketDataRequest_NoMDEntryTypes);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.MarketDataRequest_NoRelatedSym);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.MarketDataRequest_NoTradingSessions);
		if (message.ApplQueueAction is not null) context.Validators.ApplQueueAction(context, check, message.ApplQueueAction);
		if (message.ApplQueueMax is not null) context.Validators.ApplQueueMax(context, check, message.ApplQueueMax);
		if (message.MDQuoteType is not null) context.Validators.MDQuoteType(context, check, message.MDQuoteType);
	}

	static void ValidateMarketDataRequestReject(Fix50Context context, FixCheck check, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, check, message.MDReqRejReason);
		if (message.NoAltMDSource is not null) context.Validators.NoAltMDSource(context, check, message.NoAltMDSource);
		Counted(check, message.NoAltMDSource, message.NoAltMDSourceGroups);
		Each(context, check, FixTag.NoAltMDSource, message.NoAltMDSourceGroups, context.Validators.MarketDataRequestReject_NoAltMDSource);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataSnapshotFullRefresh(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, check, message.TotNumReports);
		if (message.MDReportID is not null) context.Validators.MDReportID(context, check, message.MDReportID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.MDBookType is not null) context.Validators.MDBookType(context, check, message.MDBookType);
		if (message.MDSubBookType is not null) context.Validators.MDSubBookType(context, check, message.MDSubBookType);
		if (message.MarketDepth is not null) context.Validators.MarketDepth(context, check, message.MarketDepth);
		if (message.MDFeedType is not null) context.Validators.MDFeedType(context, check, message.MDFeedType);
		if (message.RefreshIndicator is not null) context.Validators.RefreshIndicator(context, check, message.RefreshIndicator);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.MDStreamID is not null) context.Validators.MDStreamID(context, check, message.MDStreamID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.MarketDataSnapshotFullRefresh_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.MarketDataSnapshotFullRefresh_NoLegs);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (message.NetChgPrevDay is not null) context.Validators.NetChgPrevDay(context, check, message.NetChgPrevDay);
		if (message.NoMDEntries is null) Missing(check, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, check, message.NoMDEntries);
		Counted(check, message.NoMDEntries, message.NoMDEntriesGroups);
		Each(context, check, FixTag.NoMDEntries, message.NoMDEntriesGroups, context.Validators.MarketDataSnapshotFullRefresh_NoMDEntries);
		if (message.ApplQueueDepth is not null) context.Validators.ApplQueueDepth(context, check, message.ApplQueueDepth);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, check, message.ApplQueueResolution);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.MarketDataSnapshotFullRefresh_NoRoutingIDs);
	}

	static void ValidateMarketDefinition(Fix50Context context, FixCheck check, FixMessage.MarketDefinition message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.MarketReportID is null) Missing(check, FixTag.MarketReportID);
		else context.Validators.MarketReportID(context, check, message.MarketReportID);
		if (message.MarketReqID is not null) context.Validators.MarketReqID(context, check, message.MarketReqID);
		if (message.MarketID is null) Missing(check, FixTag.MarketID);
		else context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.MarketSegmentDesc is not null) context.Validators.MarketSegmentDesc(context, check, message.MarketSegmentDesc);
		if (message.EncodedMktSegmDescLen is not null) context.Validators.EncodedMktSegmDescLen(context, check, message.EncodedMktSegmDescLen);
		if (message.EncodedMktSegmDesc is not null) context.Validators.EncodedMktSegmDesc(context, check, message.EncodedMktSegmDesc);
		if (message.ParentMktSegmID is not null) context.Validators.ParentMktSegmID(context, check, message.ParentMktSegmID);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoTickRules is not null) context.Validators.NoTickRules(context, check, message.NoTickRules);
		Counted(check, message.NoTickRules, message.NoTickRulesGroups);
		Each(context, check, FixTag.NoTickRules, message.NoTickRulesGroups, context.Validators.MarketDefinition_NoTickRules);
		if (message.NoLotTypeRules is not null) context.Validators.NoLotTypeRules(context, check, message.NoLotTypeRules);
		Counted(check, message.NoLotTypeRules, message.NoLotTypeRulesGroups);
		Each(context, check, FixTag.NoLotTypeRules, message.NoLotTypeRulesGroups, context.Validators.MarketDefinition_NoLotTypeRules);
		if (message.PriceLimitType is not null) context.Validators.PriceLimitType(context, check, message.PriceLimitType);
		if (message.LowLimitPrice is not null) context.Validators.LowLimitPrice(context, check, message.LowLimitPrice);
		if (message.HighLimitPrice is not null) context.Validators.HighLimitPrice(context, check, message.HighLimitPrice);
		if (message.TradingReferencePrice is not null) context.Validators.TradingReferencePrice(context, check, message.TradingReferencePrice);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, message.ExpirationCycle);
		if (message.MinTradeVol is not null) context.Validators.MinTradeVol(context, check, message.MinTradeVol);
		if (message.MaxTradeVol is not null) context.Validators.MaxTradeVol(context, check, message.MaxTradeVol);
		if (message.MaxPriceVariation is not null) context.Validators.MaxPriceVariation(context, check, message.MaxPriceVariation);
		if (message.ImpliedMarketIndicator is not null) context.Validators.ImpliedMarketIndicator(context, check, message.ImpliedMarketIndicator);
		if (message.TradingCurrency is not null) context.Validators.TradingCurrency(context, check, message.TradingCurrency);
		if (message.RoundLot is not null) context.Validators.RoundLot(context, check, message.RoundLot);
		if (message.MultilegModel is not null) context.Validators.MultilegModel(context, check, message.MultilegModel);
		if (message.MultilegPriceMethod is not null) context.Validators.MultilegPriceMethod(context, check, message.MultilegPriceMethod);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.NoOrdTypeRules is not null) context.Validators.NoOrdTypeRules(context, check, message.NoOrdTypeRules);
		Counted(check, message.NoOrdTypeRules, message.NoOrdTypeRulesGroups);
		Each(context, check, FixTag.NoOrdTypeRules, message.NoOrdTypeRulesGroups, context.Validators.MarketDefinition_NoOrdTypeRules);
		if (message.NoTimeInForceRules is not null) context.Validators.NoTimeInForceRules(context, check, message.NoTimeInForceRules);
		Counted(check, message.NoTimeInForceRules, message.NoTimeInForceRulesGroups);
		Each(context, check, FixTag.NoTimeInForceRules, message.NoTimeInForceRulesGroups, context.Validators.MarketDefinition_NoTimeInForceRules);
		if (message.NoExecInstRules is not null) context.Validators.NoExecInstRules(context, check, message.NoExecInstRules);
		Counted(check, message.NoExecInstRules, message.NoExecInstRulesGroups);
		Each(context, check, FixTag.NoExecInstRules, message.NoExecInstRulesGroups, context.Validators.MarketDefinition_NoExecInstRules);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDefinitionRequest(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionRequest message)
	{
		if (message.MarketReqID is null) Missing(check, FixTag.MarketReqID);
		else context.Validators.MarketReqID(context, check, message.MarketReqID);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.ParentMktSegmID is not null) context.Validators.ParentMktSegmID(context, check, message.ParentMktSegmID);
	}

	static void ValidateMarketDefinitionUpdateReport(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.MarketReportID is null) Missing(check, FixTag.MarketReportID);
		else context.Validators.MarketReportID(context, check, message.MarketReportID);
		if (message.MarketReqID is not null) context.Validators.MarketReqID(context, check, message.MarketReqID);
		if (message.MarketUpdateAction is not null) context.Validators.MarketUpdateAction(context, check, message.MarketUpdateAction);
		if (message.MarketID is null) Missing(check, FixTag.MarketID);
		else context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.MarketSegmentDesc is not null) context.Validators.MarketSegmentDesc(context, check, message.MarketSegmentDesc);
		if (message.EncodedMktSegmDescLen is not null) context.Validators.EncodedMktSegmDescLen(context, check, message.EncodedMktSegmDescLen);
		if (message.EncodedMktSegmDesc is not null) context.Validators.EncodedMktSegmDesc(context, check, message.EncodedMktSegmDesc);
		if (message.ParentMktSegmID is not null) context.Validators.ParentMktSegmID(context, check, message.ParentMktSegmID);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoTickRules is not null) context.Validators.NoTickRules(context, check, message.NoTickRules);
		Counted(check, message.NoTickRules, message.NoTickRulesGroups);
		Each(context, check, FixTag.NoTickRules, message.NoTickRulesGroups, context.Validators.MarketDefinitionUpdateReport_NoTickRules);
		if (message.NoLotTypeRules is not null) context.Validators.NoLotTypeRules(context, check, message.NoLotTypeRules);
		Counted(check, message.NoLotTypeRules, message.NoLotTypeRulesGroups);
		Each(context, check, FixTag.NoLotTypeRules, message.NoLotTypeRulesGroups, context.Validators.MarketDefinitionUpdateReport_NoLotTypeRules);
		if (message.PriceLimitType is not null) context.Validators.PriceLimitType(context, check, message.PriceLimitType);
		if (message.LowLimitPrice is not null) context.Validators.LowLimitPrice(context, check, message.LowLimitPrice);
		if (message.HighLimitPrice is not null) context.Validators.HighLimitPrice(context, check, message.HighLimitPrice);
		if (message.TradingReferencePrice is not null) context.Validators.TradingReferencePrice(context, check, message.TradingReferencePrice);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, message.ExpirationCycle);
		if (message.MinTradeVol is not null) context.Validators.MinTradeVol(context, check, message.MinTradeVol);
		if (message.MaxTradeVol is not null) context.Validators.MaxTradeVol(context, check, message.MaxTradeVol);
		if (message.MaxPriceVariation is not null) context.Validators.MaxPriceVariation(context, check, message.MaxPriceVariation);
		if (message.ImpliedMarketIndicator is not null) context.Validators.ImpliedMarketIndicator(context, check, message.ImpliedMarketIndicator);
		if (message.TradingCurrency is not null) context.Validators.TradingCurrency(context, check, message.TradingCurrency);
		if (message.RoundLot is not null) context.Validators.RoundLot(context, check, message.RoundLot);
		if (message.MultilegModel is not null) context.Validators.MultilegModel(context, check, message.MultilegModel);
		if (message.MultilegPriceMethod is not null) context.Validators.MultilegPriceMethod(context, check, message.MultilegPriceMethod);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.NoOrdTypeRules is not null) context.Validators.NoOrdTypeRules(context, check, message.NoOrdTypeRules);
		Counted(check, message.NoOrdTypeRules, message.NoOrdTypeRulesGroups);
		Each(context, check, FixTag.NoOrdTypeRules, message.NoOrdTypeRulesGroups, context.Validators.MarketDefinitionUpdateReport_NoOrdTypeRules);
		if (message.NoTimeInForceRules is not null) context.Validators.NoTimeInForceRules(context, check, message.NoTimeInForceRules);
		Counted(check, message.NoTimeInForceRules, message.NoTimeInForceRulesGroups);
		Each(context, check, FixTag.NoTimeInForceRules, message.NoTimeInForceRulesGroups, context.Validators.MarketDefinitionUpdateReport_NoTimeInForceRules);
		if (message.NoExecInstRules is not null) context.Validators.NoExecInstRules(context, check, message.NoExecInstRules);
		Counted(check, message.NoExecInstRules, message.NoExecInstRulesGroups);
		Each(context, check, FixTag.NoExecInstRules, message.NoExecInstRulesGroups, context.Validators.MarketDefinitionUpdateReport_NoExecInstRules);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMassQuote(Fix50Context context, FixCheck check, FixMessage.MassQuote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DefBidSize is not null) context.Validators.DefBidSize(context, check, message.DefBidSize);
		if (message.DefOfferSize is not null) context.Validators.DefOfferSize(context, check, message.DefOfferSize);
		if (message.NoQuoteSets is null) Missing(check, FixTag.NoQuoteSets);
		else context.Validators.NoQuoteSets(context, check, message.NoQuoteSets);
		Counted(check, message.NoQuoteSets, message.NoQuoteSetsGroups);
		Each(context, check, FixTag.NoQuoteSets, message.NoQuoteSetsGroups, context.Validators.MassQuote_NoQuoteSets);
	}

	static void ValidateMassQuoteAcknowledgement(Fix50Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteStatus is null) Missing(check, FixTag.QuoteStatus);
		else context.Validators.QuoteStatus(context, check, message.QuoteStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, check, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.QuoteCancelType is not null) context.Validators.QuoteCancelType(context, check, message.QuoteCancelType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoQuoteSets is not null) context.Validators.NoQuoteSets(context, check, message.NoQuoteSets);
		Counted(check, message.NoQuoteSets, message.NoQuoteSetsGroups);
		Each(context, check, FixTag.NoQuoteSets, message.NoQuoteSetsGroups, context.Validators.MassQuoteAcknowledgement_NoQuoteSets);
	}

	static void ValidateMultilegOrderCancelReplace(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, message.OrigOrdModTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, check, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.MultilegOrderCancelReplace_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.MultilegOrderCancelReplace_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.MultilegOrderCancelReplace_NoUnderlyings);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.SwapPoints is not null) context.Validators.SwapPoints(context, check, message.SwapPoints);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.MultilegOrderCancelReplace_NoLegs);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.MultilegModel is not null) context.Validators.MultilegModel(context, check, message.MultilegModel);
		if (message.MultilegPriceMethod is not null) context.Validators.MultilegPriceMethod(context, check, message.MultilegPriceMethod);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.MultilegOrderCancelReplace_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.RiskFreeRate is not null) context.Validators.RiskFreeRate(context, check, message.RiskFreeRate);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, check, message.MultiLegRptTypeReq);
	}

	static void ValidateNetworkCounterpartySystemStatusRequest(Fix50Context context, FixCheck check, FixMessage.NetworkCounterpartySystemStatusRequest message)
	{
		if (message.NetworkRequestType is null) Missing(check, FixTag.NetworkRequestType);
		else context.Validators.NetworkRequestType(context, check, message.NetworkRequestType);
		if (message.NetworkRequestID is null) Missing(check, FixTag.NetworkRequestID);
		else context.Validators.NetworkRequestID(context, check, message.NetworkRequestID);
		if (message.NoCompIDs is not null) context.Validators.NoCompIDs(context, check, message.NoCompIDs);
		Counted(check, message.NoCompIDs, message.NoCompIDsGroups);
		Each(context, check, FixTag.NoCompIDs, message.NoCompIDsGroups, context.Validators.NetworkCounterpartySystemStatusRequest_NoCompIDs);
	}

	static void ValidateNetworkCounterpartySystemStatusResponse(Fix50Context context, FixCheck check, FixMessage.NetworkCounterpartySystemStatusResponse message)
	{
		if (message.NetworkStatusResponseType is null) Missing(check, FixTag.NetworkStatusResponseType);
		else context.Validators.NetworkStatusResponseType(context, check, message.NetworkStatusResponseType);
		if (message.NetworkRequestID is not null) context.Validators.NetworkRequestID(context, check, message.NetworkRequestID);
		if (message.NetworkResponseID is null) Missing(check, FixTag.NetworkResponseID);
		else context.Validators.NetworkResponseID(context, check, message.NetworkResponseID);
		if (message.LastNetworkResponseID is not null) context.Validators.LastNetworkResponseID(context, check, message.LastNetworkResponseID);
		if (message.NoCompIDs is null) Missing(check, FixTag.NoCompIDs);
		else context.Validators.NoCompIDs(context, check, message.NoCompIDs);
		Counted(check, message.NoCompIDs, message.NoCompIDsGroups);
		Each(context, check, FixTag.NoCompIDs, message.NoCompIDsGroups, context.Validators.NetworkCounterpartySystemStatusResponse_NoCompIDs);
	}

	static void ValidateNewOrderCross(Fix50Context context, FixCheck check, FixMessage.NewOrderCross message)
	{
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoSides is null) Missing(check, FixTag.NoSides);
		else context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.NewOrderCross_NoSides);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.NewOrderCross_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.NewOrderCross_NoLegs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderCross_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, message.TransBkdTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.NewOrderCross_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
	}

	static void ValidateNewOrderList(Fix50Context context, FixCheck check, FixMessage.NewOrderList message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, check, message.ProgRptReqs);
		if (message.BidType is null) Missing(check, FixTag.BidType);
		else context.Validators.BidType(context, check, message.BidType);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, check, message.ProgPeriodInterval);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.ListExecInstType is not null) context.Validators.ListExecInstType(context, check, message.ListExecInstType);
		if (message.ListExecInst is not null) context.Validators.ListExecInst(context, check, message.ListExecInst);
		if (message.ContingencyType is not null) context.Validators.ContingencyType(context, check, message.ContingencyType);
		if (message.EncodedListExecInstLen is not null) context.Validators.EncodedListExecInstLen(context, check, message.EncodedListExecInstLen);
		if (message.EncodedListExecInst is not null) context.Validators.EncodedListExecInst(context, check, message.EncodedListExecInst);
		if (message.AllowableOneSidednessPct is not null) context.Validators.AllowableOneSidednessPct(context, check, message.AllowableOneSidednessPct);
		if (message.AllowableOneSidednessValue is not null) context.Validators.AllowableOneSidednessValue(context, check, message.AllowableOneSidednessValue);
		if (message.AllowableOneSidednessCurr is not null) context.Validators.AllowableOneSidednessCurr(context, check, message.AllowableOneSidednessCurr);
		if (message.TotNoOrders is null) Missing(check, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, check, message.TotNoOrders);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoOrders is null) Missing(check, FixTag.NoOrders);
		else context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.NewOrderList_NoOrders);
	}

	static void ValidateNewOrderMultileg(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, check, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.NewOrderMultileg_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderMultileg_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.NewOrderMultileg_NoUnderlyings);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.SwapPoints is not null) context.Validators.SwapPoints(context, check, message.SwapPoints);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.NewOrderMultileg_NoLegs);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.MultilegModel is not null) context.Validators.MultilegModel(context, check, message.MultilegModel);
		if (message.MultilegPriceMethod is not null) context.Validators.MultilegPriceMethod(context, check, message.MultilegPriceMethod);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.RefOrderID is not null) context.Validators.RefOrderID(context, check, message.RefOrderID);
		if (message.RefOrderIDSource is not null) context.Validators.RefOrderIDSource(context, check, message.RefOrderIDSource);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.NewOrderMultileg_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.RiskFreeRate is not null) context.Validators.RiskFreeRate(context, check, message.RiskFreeRate);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, check, message.MultiLegRptTypeReq);
	}

	static void ValidateNewOrderSingle(Fix50Context context, FixCheck check, FixMessage.NewOrderSingle message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, check, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.NewOrderSingle_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderSingle_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.NewOrderSingle_NoUnderlyings);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Price2 is not null) context.Validators.Price2(context, check, message.Price2);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.NewOrderSingle_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.ManualOrderIndicator is not null) context.Validators.ManualOrderIndicator(context, check, message.ManualOrderIndicator);
		if (message.CustDirectedOrder is not null) context.Validators.CustDirectedOrder(context, check, message.CustDirectedOrder);
		if (message.ReceivedDeptID is not null) context.Validators.ReceivedDeptID(context, check, message.ReceivedDeptID);
		if (message.CustOrderHandlingInst is not null) context.Validators.CustOrderHandlingInst(context, check, message.CustOrderHandlingInst);
		if (message.OrderHandlingInstSource is not null) context.Validators.OrderHandlingInstSource(context, check, message.OrderHandlingInstSource);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.RefOrderID is not null) context.Validators.RefOrderID(context, check, message.RefOrderID);
		if (message.RefOrderIDSource is not null) context.Validators.RefOrderIDSource(context, check, message.RefOrderIDSource);
	}

	static void ValidateNews(Fix50Context context, FixCheck check, FixMessage.News message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.NewsID is not null) context.Validators.NewsID(context, check, message.NewsID);
		if (message.NoNewsRefIDs is not null) context.Validators.NoNewsRefIDs(context, check, message.NoNewsRefIDs);
		Counted(check, message.NoNewsRefIDs, message.NoNewsRefIDsGroups);
		Each(context, check, FixTag.NoNewsRefIDs, message.NoNewsRefIDsGroups, context.Validators.News_NoNewsRefIDs);
		if (message.NewsCategory is not null) context.Validators.NewsCategory(context, check, message.NewsCategory);
		if (message.LanguageCode is not null) context.Validators.LanguageCode(context, check, message.LanguageCode);
		if (message.OrigTime is not null) context.Validators.OrigTime(context, check, message.OrigTime);
		if (message.Urgency is not null) context.Validators.Urgency(context, check, message.Urgency);
		if (message.Headline is null) Missing(check, FixTag.Headline);
		else context.Validators.Headline(context, check, message.Headline);
		if (message.EncodedHeadlineLen is not null) context.Validators.EncodedHeadlineLen(context, check, message.EncodedHeadlineLen);
		if (message.EncodedHeadline is not null) context.Validators.EncodedHeadline(context, check, message.EncodedHeadline);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.News_NoRoutingIDs);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.News_NoRelatedSym);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.News_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.News_NoUnderlyings);
		if (message.NoLinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.NoLinesOfText(context, check, message.NoLinesOfText);
		Counted(check, message.NoLinesOfText, message.NoLinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.NoLinesOfTextGroups, context.Validators.News_NoLinesOfText);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateOrderCancelReject(Fix50Context context, FixCheck check, FixMessage.OrderCancelReject message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.OrdStatus is null) Missing(check, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, check, message.OrdStatus);
		if (message.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, check, message.WorkingIndicator);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, message.OrigOrdModTime);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.CxlRejResponseTo is null) Missing(check, FixTag.CxlRejResponseTo);
		else context.Validators.CxlRejResponseTo(context, check, message.CxlRejResponseTo);
		if (message.CxlRejReason is not null) context.Validators.CxlRejReason(context, check, message.CxlRejReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderCancelReplaceRequest(Fix50Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, message.OrigOrdModTime);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, check, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.OrderCancelReplaceRequest_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, message.MatchIncrement);
		if (message.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, message.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)message)) context.Validators.DisplayInstruction(context, check, message);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.OrderCancelReplaceRequest_NoTradingSessions);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.OrderCancelReplaceRequest_NoUnderlyings);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, message.PriceProtectionScope);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ITriggeringInstruction)message)) context.Validators.TriggeringInstruction(context, check, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
		if (message.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, message.NoStrategyParameters);
		Counted(check, message.NoStrategyParameters, message.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, message.NoStrategyParametersGroups, context.Validators.OrderCancelReplaceRequest_NoStrategyParameters);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Price2 is not null) context.Validators.Price2(context, check, message.Price2);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.ManualOrderIndicator is not null) context.Validators.ManualOrderIndicator(context, check, message.ManualOrderIndicator);
		if (message.CustDirectedOrder is not null) context.Validators.CustDirectedOrder(context, check, message.CustDirectedOrder);
		if (message.ReceivedDeptID is not null) context.Validators.ReceivedDeptID(context, check, message.ReceivedDeptID);
		if (message.CustOrderHandlingInst is not null) context.Validators.CustOrderHandlingInst(context, check, message.CustOrderHandlingInst);
		if (message.OrderHandlingInstSource is not null) context.Validators.OrderHandlingInstSource(context, check, message.OrderHandlingInstSource);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
	}

	static void ValidateOrderCancelRequest(Fix50Context context, FixCheck check, FixMessage.OrderCancelRequest message)
	{
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, message.OrigOrdModTime);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.OrderCancelRequest_NoUnderlyings);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassActionReport(Fix50Context context, FixCheck check, FixMessage.OrderMassActionReport message)
	{
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.MassActionReportID is null) Missing(check, FixTag.MassActionReportID);
		else context.Validators.MassActionReportID(context, check, message.MassActionReportID);
		if (message.MassActionType is null) Missing(check, FixTag.MassActionType);
		else context.Validators.MassActionType(context, check, message.MassActionType);
		if (message.MassActionScope is null) Missing(check, FixTag.MassActionScope);
		else context.Validators.MassActionScope(context, check, message.MassActionScope);
		if (message.MassActionResponse is null) Missing(check, FixTag.MassActionResponse);
		else context.Validators.MassActionResponse(context, check, message.MassActionResponse);
		if (message.MassActionRejectReason is not null) context.Validators.MassActionRejectReason(context, check, message.MassActionRejectReason);
		if (message.TotalAffectedOrders is not null) context.Validators.TotalAffectedOrders(context, check, message.TotalAffectedOrders);
		if (message.NoAffectedOrders is not null) context.Validators.NoAffectedOrders(context, check, message.NoAffectedOrders);
		Counted(check, message.NoAffectedOrders, message.NoAffectedOrdersGroups);
		Each(context, check, FixTag.NoAffectedOrders, message.NoAffectedOrdersGroups, context.Validators.OrderMassActionReport_NoAffectedOrders);
		if (message.NoNotAffectedOrders is not null) context.Validators.NoNotAffectedOrders(context, check, message.NoNotAffectedOrders);
		Counted(check, message.NoNotAffectedOrders, message.NoNotAffectedOrdersGroups);
		Each(context, check, FixTag.NoNotAffectedOrders, message.NoNotAffectedOrdersGroups, context.Validators.OrderMassActionReport_NoNotAffectedOrders);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassActionRequest(Fix50Context context, FixCheck check, FixMessage.OrderMassActionRequest message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.MassActionType is null) Missing(check, FixTag.MassActionType);
		else context.Validators.MassActionType(context, check, message.MassActionType);
		if (message.MassActionScope is null) Missing(check, FixTag.MassActionScope);
		else context.Validators.MassActionScope(context, check, message.MassActionScope);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassCancelReport(Fix50Context context, FixCheck check, FixMessage.OrderMassCancelReport message)
	{
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.MassActionReportID is null) Missing(check, FixTag.MassActionReportID);
		else context.Validators.MassActionReportID(context, check, message.MassActionReportID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.MassCancelRequestType is null) Missing(check, FixTag.MassCancelRequestType);
		else context.Validators.MassCancelRequestType(context, check, message.MassCancelRequestType);
		if (message.MassCancelResponse is null) Missing(check, FixTag.MassCancelResponse);
		else context.Validators.MassCancelResponse(context, check, message.MassCancelResponse);
		if (message.MassCancelRejectReason is not null) context.Validators.MassCancelRejectReason(context, check, message.MassCancelRejectReason);
		if (message.TotalAffectedOrders is not null) context.Validators.TotalAffectedOrders(context, check, message.TotalAffectedOrders);
		if (message.NoAffectedOrders is not null) context.Validators.NoAffectedOrders(context, check, message.NoAffectedOrders);
		Counted(check, message.NoAffectedOrders, message.NoAffectedOrdersGroups);
		Each(context, check, FixTag.NoAffectedOrders, message.NoAffectedOrdersGroups, context.Validators.OrderMassCancelReport_NoAffectedOrders);
		if (message.NoNotAffectedOrders is not null) context.Validators.NoNotAffectedOrders(context, check, message.NoNotAffectedOrders);
		Counted(check, message.NoNotAffectedOrders, message.NoNotAffectedOrdersGroups);
		Each(context, check, FixTag.NoNotAffectedOrders, message.NoNotAffectedOrdersGroups, context.Validators.OrderMassCancelReport_NoNotAffectedOrders);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassCancelRequest(Fix50Context context, FixCheck check, FixMessage.OrderMassCancelRequest message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.MassCancelRequestType is null) Missing(check, FixTag.MassCancelRequestType);
		else context.Validators.MassCancelRequestType(context, check, message.MassCancelRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassStatusRequest(Fix50Context context, FixCheck check, FixMessage.OrderMassStatusRequest message)
	{
		if (message.MassStatusReqID is null) Missing(check, FixTag.MassStatusReqID);
		else context.Validators.MassStatusReqID(context, check, message.MassStatusReqID);
		if (message.MassStatusReqType is null) Missing(check, FixTag.MassStatusReqType);
		else context.Validators.MassStatusReqType(context, check, message.MassStatusReqType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
	}

	static void ValidateOrderStatusRequest(Fix50Context context, FixCheck check, FixMessage.OrderStatusRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.OrdStatusReqID is not null) context.Validators.OrdStatusReqID(context, check, message.OrdStatusReqID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.OrderStatusRequest_NoUnderlyings);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
	}

	static void ValidatePositionMaintenanceReport(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceReport message)
	{
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosTransType is null) Missing(check, FixTag.PosTransType);
		else context.Validators.PosTransType(context, check, message.PosTransType);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosMaintAction is null) Missing(check, FixTag.PosMaintAction);
		else context.Validators.PosMaintAction(context, check, message.PosMaintAction);
		if (message.OrigPosReqRefID is not null) context.Validators.OrigPosReqRefID(context, check, message.OrigPosReqRefID);
		if (message.PosMaintStatus is null) Missing(check, FixTag.PosMaintStatus);
		else context.Validators.PosMaintStatus(context, check, message.PosMaintStatus);
		if (message.PosMaintResult is not null) context.Validators.PosMaintResult(context, check, message.PosMaintResult);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.PosMaintRptRefID is not null) context.Validators.PosMaintRptRefID(context, check, message.PosMaintRptRefID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.ContraryInstructionIndicator is not null) context.Validators.ContraryInstructionIndicator(context, check, message.ContraryInstructionIndicator);
		if (message.PriorSpreadIndicator is not null) context.Validators.PriorSpreadIndicator(context, check, message.PriorSpreadIndicator);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.PositionMaintenanceReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.PositionMaintenanceReport_NoUnderlyings);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.PositionMaintenanceReport_NoTradingSessions);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, check, message.AdjustmentType);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidatePositionMaintenanceRequest(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceRequest message)
	{
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosTransType is null) Missing(check, FixTag.PosTransType);
		else context.Validators.PosTransType(context, check, message.PosTransType);
		if (message.PosMaintAction is null) Missing(check, FixTag.PosMaintAction);
		else context.Validators.PosMaintAction(context, check, message.PosMaintAction);
		if (message.OrigPosReqRefID is not null) context.Validators.OrigPosReqRefID(context, check, message.OrigPosReqRefID);
		if (message.PosMaintRptRefID is not null) context.Validators.PosMaintRptRefID(context, check, message.PosMaintRptRefID);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.PositionMaintenanceRequest_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.PositionMaintenanceRequest_NoUnderlyings);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.PositionMaintenanceRequest_NoTradingSessions);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, check, message.AdjustmentType);
		if (message.ContraryInstructionIndicator is not null) context.Validators.ContraryInstructionIndicator(context, check, message.ContraryInstructionIndicator);
		if (message.PriorSpreadIndicator is not null) context.Validators.PriorSpreadIndicator(context, check, message.PriorSpreadIndicator);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
	}

	static void ValidatePositionReport(Fix50Context context, FixCheck check, FixMessage.PositionReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosReqType is not null) context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TotalNumPosReports is not null) context.Validators.TotalNumPosReports(context, check, message.TotalNumPosReports);
		if (message.PosReqResult is not null) context.Validators.PosReqResult(context, check, message.PosReqResult);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlPrice is not null) context.Validators.SettlPrice(context, check, message.SettlPrice);
		if (message.SettlPriceType is not null) context.Validators.SettlPriceType(context, check, message.SettlPriceType);
		if (message.PriorSettlPrice is not null) context.Validators.PriorSettlPrice(context, check, message.PriorSettlPrice);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.PositionReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.PositionReport_NoUnderlyings);
		if (!Empty((IPositionQty)message)) context.Validators.PositionQty(context, check, message);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.RegistStatus is not null) context.Validators.RegistStatus(context, check, message.RegistStatus);
		if (message.DeliveryDate is not null) context.Validators.DeliveryDate(context, check, message.DeliveryDate);
		if (message.ModelType is not null) context.Validators.ModelType(context, check, message.ModelType);
		if (message.PriceDelta is not null) context.Validators.PriceDelta(context, check, message.PriceDelta);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuote(Fix50Context context, FixCheck check, FixMessage.Quote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteMsgID is not null) context.Validators.QuoteMsgID(context, check, message.QuoteMsgID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.PrivateQuote is not null) context.Validators.PrivateQuote(context, check, message.PrivateQuote);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, check, message.NoQuoteQualifiers);
		Counted(check, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		Each(context, check, FixTag.NoQuoteQualifiers, message.NoQuoteQualifiersGroups, context.Validators.Quote_NoQuoteQualifiers);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.Quote_NoUnderlyings);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (!Empty((IRateSource)message)) context.Validators.RateSource(context, check, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.Quote_NoLegs);
		if (message.BidPx is not null) context.Validators.BidPx(context, check, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, check, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, check, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, check, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, check, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, check, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, check, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, check, message.OfferSize);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, message.OfferForwardPoints);
		if (message.BidSwapPoints is not null) context.Validators.BidSwapPoints(context, check, message.BidSwapPoints);
		if (message.OfferSwapPoints is not null) context.Validators.OfferSwapPoints(context, check, message.OfferSwapPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, check, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, check, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, check, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, check, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, check, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, check, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, check, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, check, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteCancel(Fix50Context context, FixCheck check, FixMessage.QuoteCancel message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteMsgID is not null) context.Validators.QuoteMsgID(context, check, message.QuoteMsgID);
		if (message.QuoteCancelType is null) Missing(check, FixTag.QuoteCancelType);
		else context.Validators.QuoteCancelType(context, check, message.QuoteCancelType);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, check, message.NoQuoteEntries);
		Counted(check, message.NoQuoteEntries, message.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, message.NoQuoteEntriesGroups, context.Validators.QuoteCancel_NoQuoteEntries);
	}

	static void ValidateQuoteRequest(Fix50Context context, FixCheck check, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID is null) Missing(check, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.PrivateQuote is not null) context.Validators.PrivateQuote(context, check, message.PrivateQuote);
		if (message.RespondentType is not null) context.Validators.RespondentType(context, check, message.RespondentType);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.QuoteRequest_NoRelatedSym);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteRequestReject(Fix50Context context, FixCheck check, FixMessage.QuoteRequestReject message)
	{
		if (message.QuoteReqID is null) Missing(check, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (message.QuoteRequestRejectReason is null) Missing(check, FixTag.QuoteRequestRejectReason);
		else context.Validators.QuoteRequestRejectReason(context, check, message.QuoteRequestRejectReason);
		if (message.PrivateQuote is not null) context.Validators.PrivateQuote(context, check, message.PrivateQuote);
		if (message.RespondentType is not null) context.Validators.RespondentType(context, check, message.RespondentType);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.QuoteRequestReject_NoRelatedSym);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteResponse(Fix50Context context, FixCheck check, FixMessage.QuoteResponse message)
	{
		if (message.QuoteRespID is null) Missing(check, FixTag.QuoteRespID);
		else context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteMsgID is not null) context.Validators.QuoteMsgID(context, check, message.QuoteMsgID);
		if (message.QuoteRespType is null) Missing(check, FixTag.QuoteRespType);
		else context.Validators.QuoteRespType(context, check, message.QuoteRespType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.IOIID is not null) context.Validators.IOIID(context, check, message.IOIID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, message.PreTradeAnonymity);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, check, message.NoQuoteQualifiers);
		Counted(check, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		Each(context, check, FixTag.NoQuoteQualifiers, message.NoQuoteQualifiersGroups, context.Validators.QuoteResponse_NoQuoteQualifiers);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.QuoteResponse_NoUnderlyings);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.QuoteResponse_NoLegs);
		if (message.BidPx is not null) context.Validators.BidPx(context, check, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, check, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, check, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, check, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, check, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, check, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, check, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, check, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, message.OfferForwardPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, check, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, check, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, check, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, check, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, check, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, check, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, check, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, check, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
	}

	static void ValidateQuoteStatusReport(Fix50Context context, FixCheck check, FixMessage.QuoteStatusReport message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, check, message.QuoteStatusReqID);
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteMsgID is not null) context.Validators.QuoteMsgID(context, check, message.QuoteMsgID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (message.QuoteCancelType is not null) context.Validators.QuoteCancelType(context, check, message.QuoteCancelType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.QuoteStatusReport_NoUnderlyings);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, check, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.QuoteStatusReport_NoLegs);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, check, message.NoQuoteQualifiers);
		Counted(check, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		Each(context, check, FixTag.NoQuoteQualifiers, message.NoQuoteQualifiersGroups, context.Validators.QuoteStatusReport_NoQuoteQualifiers);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.BidPx is not null) context.Validators.BidPx(context, check, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, check, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, check, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, check, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, check, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, check, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, check, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, check, message.OfferSize);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, message.OfferForwardPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, check, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, check, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, check, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, check, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, check, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, check, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, check, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, check, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, message.ExDestinationIDSource);
		if (message.BookingType is not null) context.Validators.BookingType(context, check, message.BookingType);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.QuoteStatus is not null) context.Validators.QuoteStatus(context, check, message.QuoteStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, check, message.QuoteRejectReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteStatusRequest(Fix50Context context, FixCheck check, FixMessage.QuoteStatusRequest message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, check, message.QuoteStatusReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.QuoteStatusRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.QuoteStatusRequest_NoLegs);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((ITargetParties)message)) context.Validators.TargetParties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateRFQRequest(Fix50Context context, FixCheck check, FixMessage.RFQRequest message)
	{
		if (message.RFQReqID is null) Missing(check, FixTag.RFQReqID);
		else context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.RFQRequest_NoRelatedSym);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.PrivateQuote is not null) context.Validators.PrivateQuote(context, check, message.PrivateQuote);
	}

	static void ValidateRegistrationInstructions(Fix50Context context, FixCheck check, FixMessage.RegistrationInstructions message)
	{
		if (message.RegistID is null) Missing(check, FixTag.RegistID);
		else context.Validators.RegistID(context, check, message.RegistID);
		if (message.RegistTransType is null) Missing(check, FixTag.RegistTransType);
		else context.Validators.RegistTransType(context, check, message.RegistTransType);
		if (message.RegistRefID is null) Missing(check, FixTag.RegistRefID);
		else context.Validators.RegistRefID(context, check, message.RegistRefID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.RegistAcctType is not null) context.Validators.RegistAcctType(context, check, message.RegistAcctType);
		if (message.TaxAdvantageType is not null) context.Validators.TaxAdvantageType(context, check, message.TaxAdvantageType);
		if (message.OwnershipType is not null) context.Validators.OwnershipType(context, check, message.OwnershipType);
		if (message.NoRegistDtls is not null) context.Validators.NoRegistDtls(context, check, message.NoRegistDtls);
		Counted(check, message.NoRegistDtls, message.NoRegistDtlsGroups);
		Each(context, check, FixTag.NoRegistDtls, message.NoRegistDtlsGroups, context.Validators.RegistrationInstructions_NoRegistDtls);
		if (message.NoDistribInsts is not null) context.Validators.NoDistribInsts(context, check, message.NoDistribInsts);
		Counted(check, message.NoDistribInsts, message.NoDistribInstsGroups);
		Each(context, check, FixTag.NoDistribInsts, message.NoDistribInstsGroups, context.Validators.RegistrationInstructions_NoDistribInsts);
	}

	static void ValidateRegistrationInstructionsResponse(Fix50Context context, FixCheck check, FixMessage.RegistrationInstructionsResponse message)
	{
		if (message.RegistID is null) Missing(check, FixTag.RegistID);
		else context.Validators.RegistID(context, check, message.RegistID);
		if (message.RegistTransType is null) Missing(check, FixTag.RegistTransType);
		else context.Validators.RegistTransType(context, check, message.RegistTransType);
		if (message.RegistRefID is null) Missing(check, FixTag.RegistRefID);
		else context.Validators.RegistRefID(context, check, message.RegistRefID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.RegistStatus is null) Missing(check, FixTag.RegistStatus);
		else context.Validators.RegistStatus(context, check, message.RegistStatus);
		if (message.RegistRejReasonCode is not null) context.Validators.RegistRejReasonCode(context, check, message.RegistRejReasonCode);
		if (message.RegistRejReasonText is not null) context.Validators.RegistRejReasonText(context, check, message.RegistRejReasonText);
	}

	static void ValidateReject(Fix50Context context, FixCheck check, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(check, FixTag.RefSeqNum);
		else context.Validators.RefSeqNum(context, check, message.RefSeqNum);
		if (message.RefTagID is not null) context.Validators.RefTagID(context, check, message.RefTagID);
		if (message.RefMsgType is not null) context.Validators.RefMsgType(context, check, message.RefMsgType);
		if (message.RefApplVerID is not null) context.Validators.RefApplVerID(context, check, message.RefApplVerID);
		if (message.RefApplExtID is not null) context.Validators.RefApplExtID(context, check, message.RefApplExtID);
		if (message.RefCstmApplVerID is not null) context.Validators.RefCstmApplVerID(context, check, message.RefCstmApplVerID);
		if (message.SessionRejectReason is not null) context.Validators.SessionRejectReason(context, check, message.SessionRejectReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateRequestForPositions(Fix50Context context, FixCheck check, FixMessage.RequestForPositions message)
	{
		if (message.PosReqID is null) Missing(check, FixTag.PosReqID);
		else context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosReqType is null) Missing(check, FixTag.PosReqType);
		else context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.RequestForPositions_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.RequestForPositions_NoUnderlyings);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.RequestForPositions_NoTradingSessions);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateRequestForPositionsAck(Fix50Context context, FixCheck check, FixMessage.RequestForPositionsAck message)
	{
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.TotalNumPosReports is not null) context.Validators.TotalNumPosReports(context, check, message.TotalNumPosReports);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.PosReqResult is null) Missing(check, FixTag.PosReqResult);
		else context.Validators.PosReqResult(context, check, message.PosReqResult);
		if (message.PosReqStatus is null) Missing(check, FixTag.PosReqStatus);
		else context.Validators.PosReqStatus(context, check, message.PosReqStatus);
		if (message.PosReqType is not null) context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.RequestForPositionsAck_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.RequestForPositionsAck_NoUnderlyings);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateResendRequest(Fix50Context context, FixCheck check, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(check, FixTag.BeginSeqNo);
		else context.Validators.BeginSeqNo(context, check, message.BeginSeqNo);
		if (message.EndSeqNo is null) Missing(check, FixTag.EndSeqNo);
		else context.Validators.EndSeqNo(context, check, message.EndSeqNo);
	}

	static void ValidateSecurityDefinition(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReportID is not null) context.Validators.SecurityReportID(context, check, message.SecurityReportID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityResponseType is not null) context.Validators.SecurityResponseType(context, check, message.SecurityResponseType);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityDefinition_NoUnderlyings);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityDefinition_NoLegs);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.NoMarketSegments is not null) context.Validators.NoMarketSegments(context, check, message.NoMarketSegments);
		Counted(check, message.NoMarketSegments, message.NoMarketSegmentsGroups);
		Each(context, check, FixTag.NoMarketSegments, message.NoMarketSegmentsGroups, context.Validators.SecurityDefinition_NoMarketSegments);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
	}

	static void ValidateSecurityDefinitionRequest(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityRequestType is null) Missing(check, FixTag.SecurityRequestType);
		else context.Validators.SecurityRequestType(context, check, message.SecurityRequestType);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityDefinitionRequest_NoUnderlyings);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityDefinitionRequest_NoLegs);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, message.ExpirationCycle);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateSecurityDefinitionUpdateReport(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReportID is not null) context.Validators.SecurityReportID(context, check, message.SecurityReportID);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityResponseType is not null) context.Validators.SecurityResponseType(context, check, message.SecurityResponseType);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SecurityUpdateAction is not null) context.Validators.SecurityUpdateAction(context, check, message.SecurityUpdateAction);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityDefinitionUpdateReport_NoUnderlyings);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityDefinitionUpdateReport_NoLegs);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.NoMarketSegments is not null) context.Validators.NoMarketSegments(context, check, message.NoMarketSegments);
		Counted(check, message.NoMarketSegments, message.NoMarketSegmentsGroups);
		Each(context, check, FixTag.NoMarketSegments, message.NoMarketSegmentsGroups, context.Validators.SecurityDefinitionUpdateReport_NoMarketSegments);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
	}

	static void ValidateSecurityList(Fix50Context context, FixCheck check, FixMessage.SecurityList message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReportID is not null) context.Validators.SecurityReportID(context, check, message.SecurityReportID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SecurityListID is not null) context.Validators.SecurityListID(context, check, message.SecurityListID);
		if (message.SecurityListRefID is not null) context.Validators.SecurityListRefID(context, check, message.SecurityListRefID);
		if (message.SecurityListDesc is not null) context.Validators.SecurityListDesc(context, check, message.SecurityListDesc);
		if (message.EncodedSecurityListDescLen is not null) context.Validators.EncodedSecurityListDescLen(context, check, message.EncodedSecurityListDescLen);
		if (message.EncodedSecurityListDesc is not null) context.Validators.EncodedSecurityListDesc(context, check, message.EncodedSecurityListDesc);
		if (message.SecurityListType is not null) context.Validators.SecurityListType(context, check, message.SecurityListType);
		if (message.SecurityListTypeSource is not null) context.Validators.SecurityListTypeSource(context, check, message.SecurityListTypeSource);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.SecurityList_NoRelatedSym);
	}

	static void ValidateSecurityListRequest(Fix50Context context, FixCheck check, FixMessage.SecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(check, FixTag.SecurityListRequestType);
		else context.Validators.SecurityListRequestType(context, check, message.SecurityListRequestType);
		if (message.SecurityListID is not null) context.Validators.SecurityListID(context, check, message.SecurityListID);
		if (message.SecurityListType is not null) context.Validators.SecurityListType(context, check, message.SecurityListType);
		if (message.SecurityListTypeSource is not null) context.Validators.SecurityListTypeSource(context, check, message.SecurityListTypeSource);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityListRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityListRequest_NoLegs);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateSecurityListUpdateReport(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReportID is not null) context.Validators.SecurityReportID(context, check, message.SecurityReportID);
		if (message.SecurityListID is not null) context.Validators.SecurityListID(context, check, message.SecurityListID);
		if (message.SecurityListRefID is not null) context.Validators.SecurityListRefID(context, check, message.SecurityListRefID);
		if (message.SecurityListDesc is not null) context.Validators.SecurityListDesc(context, check, message.SecurityListDesc);
		if (message.EncodedSecurityListDescLen is not null) context.Validators.EncodedSecurityListDescLen(context, check, message.EncodedSecurityListDescLen);
		if (message.EncodedSecurityListDesc is not null) context.Validators.EncodedSecurityListDesc(context, check, message.EncodedSecurityListDesc);
		if (message.SecurityListType is not null) context.Validators.SecurityListType(context, check, message.SecurityListType);
		if (message.SecurityListTypeSource is not null) context.Validators.SecurityListTypeSource(context, check, message.SecurityListTypeSource);
		if (message.SecurityReqID is not null) context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is not null) context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SecurityUpdateAction is not null) context.Validators.SecurityUpdateAction(context, check, message.SecurityUpdateAction);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.SecurityListUpdateReport_NoRelatedSym);
	}

	static void ValidateSecurityStatusMessage(Fix50Context context, FixCheck check, FixMessage.SecurityStatusMessage message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityStatusReqID is not null) context.Validators.SecurityStatusReqID(context, check, message.SecurityStatusReqID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityStatusMessage_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityStatusMessage_NoLegs);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, check, message.SecurityTradingStatus);
		if (message.SecurityTradingEvent is not null) context.Validators.SecurityTradingEvent(context, check, message.SecurityTradingEvent);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (message.HaltReasonInt is not null) context.Validators.HaltReasonInt(context, check, message.HaltReasonInt);
		if (message.InViewOfCommon is not null) context.Validators.InViewOfCommon(context, check, message.InViewOfCommon);
		if (message.DueToRelated is not null) context.Validators.DueToRelated(context, check, message.DueToRelated);
		if (message.MDBookType is not null) context.Validators.MDBookType(context, check, message.MDBookType);
		if (message.MarketDepth is not null) context.Validators.MarketDepth(context, check, message.MarketDepth);
		if (message.BuyVolume is not null) context.Validators.BuyVolume(context, check, message.BuyVolume);
		if (message.SellVolume is not null) context.Validators.SellVolume(context, check, message.SellVolume);
		if (message.HighPx is not null) context.Validators.HighPx(context, check, message.HighPx);
		if (message.LowPx is not null) context.Validators.LowPx(context, check, message.LowPx);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Adjustment is not null) context.Validators.Adjustment(context, check, message.Adjustment);
		if (message.FirstPx is not null) context.Validators.FirstPx(context, check, message.FirstPx);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateSecurityStatusRequest(Fix50Context context, FixCheck check, FixMessage.SecurityStatusRequest message)
	{
		if (message.SecurityStatusReqID is null) Missing(check, FixTag.SecurityStatusReqID);
		else context.Validators.SecurityStatusReqID(context, check, message.SecurityStatusReqID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityStatusRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityStatusRequest_NoLegs);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
	}

	static void ValidateSecurityTypeRequest(Fix50Context context, FixCheck check, FixMessage.SecurityTypeRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.Product is not null) context.Validators.Product(context, check, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, message.SecuritySubType);
	}

	static void ValidateSecurityTypes(Fix50Context context, FixCheck check, FixMessage.SecurityTypes message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(check, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityResponseType is null) Missing(check, FixTag.SecurityResponseType);
		else context.Validators.SecurityResponseType(context, check, message.SecurityResponseType);
		if (message.TotNoSecurityTypes is not null) context.Validators.TotNoSecurityTypes(context, check, message.TotNoSecurityTypes);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoSecurityTypes is not null) context.Validators.NoSecurityTypes(context, check, message.NoSecurityTypes);
		Counted(check, message.NoSecurityTypes, message.NoSecurityTypesGroups);
		Each(context, check, FixTag.NoSecurityTypes, message.NoSecurityTypesGroups, context.Validators.SecurityTypes_NoSecurityTypes);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateSequenceReset(Fix50Context context, FixCheck check, FixMessage.SequenceReset message)
	{
		if (message.GapFillFlag is not null) context.Validators.GapFillFlag(context, check, message.GapFillFlag);
		if (message.NewSeqNo is null) Missing(check, FixTag.NewSeqNo);
		else context.Validators.NewSeqNo(context, check, message.NewSeqNo);
	}

	static void ValidateSettlementInstructionRequest(Fix50Context context, FixCheck check, FixMessage.SettlementInstructionRequest message)
	{
		if (message.SettlInstReqID is null) Missing(check, FixTag.SettlInstReqID);
		else context.Validators.SettlInstReqID(context, check, message.SettlInstReqID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.AllocAccount is not null) context.Validators.AllocAccount(context, check, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, message.AllocAcctIDSource);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.Product is not null) context.Validators.Product(context, check, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.CFICode is not null) context.Validators.CFICode(context, check, message.CFICode);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, message.LastUpdateTime);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, check, message.StandInstDbType);
		if (message.StandInstDbName is not null) context.Validators.StandInstDbName(context, check, message.StandInstDbName);
		if (message.StandInstDbID is not null) context.Validators.StandInstDbID(context, check, message.StandInstDbID);
	}

	static void ValidateSettlementInstructions(Fix50Context context, FixCheck check, FixMessage.SettlementInstructions message)
	{
		if (message.SettlInstMsgID is null) Missing(check, FixTag.SettlInstMsgID);
		else context.Validators.SettlInstMsgID(context, check, message.SettlInstMsgID);
		if (message.SettlInstReqID is not null) context.Validators.SettlInstReqID(context, check, message.SettlInstReqID);
		if (message.SettlInstMode is null) Missing(check, FixTag.SettlInstMode);
		else context.Validators.SettlInstMode(context, check, message.SettlInstMode);
		if (message.SettlInstReqRejCode is not null) context.Validators.SettlInstReqRejCode(context, check, message.SettlInstReqRejCode);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.NoSettlInst is not null) context.Validators.NoSettlInst(context, check, message.NoSettlInst);
		Counted(check, message.NoSettlInst, message.NoSettlInstGroups);
		Each(context, check, FixTag.NoSettlInst, message.NoSettlInstGroups, context.Validators.SettlementInstructions_NoSettlInst);
	}

	static void ValidateSettlementObligationReport(Fix50Context context, FixCheck check, FixMessage.SettlementObligationReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlementCycleNo is not null) context.Validators.SettlementCycleNo(context, check, message.SettlementCycleNo);
		if (message.SettlObligMsgID is null) Missing(check, FixTag.SettlObligMsgID);
		else context.Validators.SettlObligMsgID(context, check, message.SettlObligMsgID);
		if (message.SettlObligMode is null) Missing(check, FixTag.SettlObligMode);
		else context.Validators.SettlObligMode(context, check, message.SettlObligMode);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.NoSettlOblig is null) Missing(check, FixTag.NoSettlOblig);
		else context.Validators.NoSettlOblig(context, check, message.NoSettlOblig);
		Counted(check, message.NoSettlOblig, message.NoSettlObligGroups);
		Each(context, check, FixTag.NoSettlOblig, message.NoSettlObligGroups, context.Validators.SettlementObligationReport_NoSettlOblig);
	}

	static void ValidateStreamAssignmentReport(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentReport message)
	{
		if (message.StreamAsgnRptID is null) Missing(check, FixTag.StreamAsgnRptID);
		else context.Validators.StreamAsgnRptID(context, check, message.StreamAsgnRptID);
		if (message.StreamAsgnReqType is not null) context.Validators.StreamAsgnReqType(context, check, message.StreamAsgnReqType);
		if (message.StreamAsgnReqID is not null) context.Validators.StreamAsgnReqID(context, check, message.StreamAsgnReqID);
		if (message.NoAsgnReqs is not null) context.Validators.NoAsgnReqs(context, check, message.NoAsgnReqs);
		Counted(check, message.NoAsgnReqs, message.NoAsgnReqsGroups);
		Each(context, check, FixTag.NoAsgnReqs, message.NoAsgnReqsGroups, context.Validators.StreamAssignmentReport_NoAsgnReqs);
	}

	static void ValidateStreamAssignmentReportACK(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentReportACK message)
	{
		if (message.StreamAsgnAckType is null) Missing(check, FixTag.StreamAsgnAckType);
		else context.Validators.StreamAsgnAckType(context, check, message.StreamAsgnAckType);
		if (message.StreamAsgnRptID is null) Missing(check, FixTag.StreamAsgnRptID);
		else context.Validators.StreamAsgnRptID(context, check, message.StreamAsgnRptID);
		if (message.StreamAsgnRejReason is not null) context.Validators.StreamAsgnRejReason(context, check, message.StreamAsgnRejReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateStreamAssignmentRequest(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentRequest message)
	{
		if (message.StreamAsgnReqID is null) Missing(check, FixTag.StreamAsgnReqID);
		else context.Validators.StreamAsgnReqID(context, check, message.StreamAsgnReqID);
		if (message.StreamAsgnReqType is null) Missing(check, FixTag.StreamAsgnReqType);
		else context.Validators.StreamAsgnReqType(context, check, message.StreamAsgnReqType);
		if (message.NoAsgnReqs is null) Missing(check, FixTag.NoAsgnReqs);
		else context.Validators.NoAsgnReqs(context, check, message.NoAsgnReqs);
		Counted(check, message.NoAsgnReqs, message.NoAsgnReqsGroups);
		Each(context, check, FixTag.NoAsgnReqs, message.NoAsgnReqsGroups, context.Validators.StreamAssignmentRequest_NoAsgnReqs);
	}

	static void ValidateTestRequest(Fix50Context context, FixCheck check, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(check, FixTag.TestReqID);
		else context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateTradeCaptureReport(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.TradeReportID is not null) context.Validators.TradeReportID(context, check, message.TradeReportID);
		if (message.TradeID is not null) context.Validators.TradeID(context, check, message.TradeID);
		if (message.SecondaryTradeID is not null) context.Validators.SecondaryTradeID(context, check, message.SecondaryTradeID);
		if (message.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, message.FirmTradeID);
		if (message.SecondaryFirmTradeID is not null) context.Validators.SecondaryFirmTradeID(context, check, message.SecondaryFirmTradeID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, check, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, check, message.TradeReportType);
		if (message.TrdRptStatus is not null) context.Validators.TrdRptStatus(context, check, message.TrdRptStatus);
		if (message.TradeRequestID is not null) context.Validators.TradeRequestID(context, check, message.TradeRequestID);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, check, message.SecondaryTrdType);
		if (message.TradeHandlingInstr is not null) context.Validators.TradeHandlingInstr(context, check, message.TradeHandlingInstr);
		if (message.OrigTradeHandlingInstr is not null) context.Validators.OrigTradeHandlingInstr(context, check, message.OrigTradeHandlingInstr);
		if (message.OrigTradeDate is not null) context.Validators.OrigTradeDate(context, check, message.OrigTradeDate);
		if (message.OrigTradeID is not null) context.Validators.OrigTradeID(context, check, message.OrigTradeID);
		if (message.OrigSecondaryTradeID is not null) context.Validators.OrigSecondaryTradeID(context, check, message.OrigSecondaryTradeID);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, check, message.TransferReason);
		if (message.ExecType is not null) context.Validators.ExecType(context, check, message.ExecType);
		if (message.TotNumTradeReports is not null) context.Validators.TotNumTradeReports(context, check, message.TotNumTradeReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, check, message.LastRptRequested);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TradeReportRefID is not null) context.Validators.TradeReportRefID(context, check, message.TradeReportRefID);
		if (message.SecondaryTradeReportRefID is not null) context.Validators.SecondaryTradeReportRefID(context, check, message.SecondaryTradeReportRefID);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, message.SecondaryTradeReportID);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, check, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, check, message.TrdMatchID);
		if (message.ExecID is not null) context.Validators.ExecID(context, check, message.ExecID);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, message.SecondaryExecID);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, check, message.ExecRestatementReason);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.AsOfIndicator is not null) context.Validators.AsOfIndicator(context, check, message.AsOfIndicator);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.VenueType is not null) context.Validators.VenueType(context, check, message.VenueType);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.TradeCaptureReport_NoUnderlyings);
		if (message.UnderlyingTradingSessionID is not null) context.Validators.UnderlyingTradingSessionID(context, check, message.UnderlyingTradingSessionID);
		if (message.UnderlyingTradingSessionSubID is not null) context.Validators.UnderlyingTradingSessionSubID(context, check, message.UnderlyingTradingSessionSubID);
		if (message.LastQty is null) Missing(check, FixTag.LastQty);
		else context.Validators.LastQty(context, check, message.LastQty);
		if (message.LastPx is null) Missing(check, FixTag.LastPx);
		else context.Validators.LastPx(context, check, message.LastPx);
		if (message.CalculatedCcyLastQty is not null) context.Validators.CalculatedCcyLastQty(context, check, message.CalculatedCcyLastQty);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, check, message.LastParPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, check, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, check, message.LastForwardPoints);
		if (message.LastSwapPoints is not null) context.Validators.LastSwapPoints(context, check, message.LastSwapPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.TradeLegRefID is not null) context.Validators.TradeLegRefID(context, check, message.TradeLegRefID);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.TradeCaptureReport_NoLegs);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.UnderlyingSettlementDate is not null) context.Validators.UnderlyingSettlementDate(context, check, message.UnderlyingSettlementDate);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.NoSides is null) Missing(check, FixTag.NoSides);
		else context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.TradeCaptureReport_NoSides);
		if (message.Volatility is not null) context.Validators.Volatility(context, check, message.Volatility);
		if (message.DividendYield is not null) context.Validators.DividendYield(context, check, message.DividendYield);
		if (message.RiskFreeRate is not null) context.Validators.RiskFreeRate(context, check, message.RiskFreeRate);
		if (message.CurrencyRatio is not null) context.Validators.CurrencyRatio(context, check, message.CurrencyRatio);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, check, message.CopyMsgIndicator);
		if (message.NoTrdRepIndicators is not null) context.Validators.NoTrdRepIndicators(context, check, message.NoTrdRepIndicators);
		Counted(check, message.NoTrdRepIndicators, message.NoTrdRepIndicatorsGroups);
		Each(context, check, FixTag.NoTrdRepIndicators, message.NoTrdRepIndicatorsGroups, context.Validators.TradeCaptureReport_NoTrdRepIndicators);
		if (message.PublishTrdIndicator is not null) context.Validators.PublishTrdIndicator(context, check, message.PublishTrdIndicator);
		if (message.TradePublishIndicator is not null) context.Validators.TradePublishIndicator(context, check, message.TradePublishIndicator);
		if (message.ShortSaleReason is not null) context.Validators.ShortSaleReason(context, check, message.ShortSaleReason);
		if (message.TierCode is not null) context.Validators.TierCode(context, check, message.TierCode);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (message.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, message.LastUpdateTime);
		if (message.RndPx is not null) context.Validators.RndPx(context, check, message.RndPx);
		if (message.TZTransactTime is not null) context.Validators.TZTransactTime(context, check, message.TZTransactTime);
		if (message.ReportedPxDiff is not null) context.Validators.ReportedPxDiff(context, check, message.ReportedPxDiff);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.RejectText is not null) context.Validators.RejectText(context, check, message.RejectText);
		if (message.FeeMultiplier is not null) context.Validators.FeeMultiplier(context, check, message.FeeMultiplier);
	}

	static void ValidateTradeCaptureReportAck(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck message)
	{
		if (message.TradeReportID is not null) context.Validators.TradeReportID(context, check, message.TradeReportID);
		if (message.TradeID is not null) context.Validators.TradeID(context, check, message.TradeID);
		if (message.SecondaryTradeID is not null) context.Validators.SecondaryTradeID(context, check, message.SecondaryTradeID);
		if (message.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, message.FirmTradeID);
		if (message.SecondaryFirmTradeID is not null) context.Validators.SecondaryFirmTradeID(context, check, message.SecondaryFirmTradeID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, check, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, check, message.TradeReportType);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, check, message.SecondaryTrdType);
		if (message.TradeHandlingInstr is not null) context.Validators.TradeHandlingInstr(context, check, message.TradeHandlingInstr);
		if (message.OrigTradeHandlingInstr is not null) context.Validators.OrigTradeHandlingInstr(context, check, message.OrigTradeHandlingInstr);
		if (message.OrigTradeDate is not null) context.Validators.OrigTradeDate(context, check, message.OrigTradeDate);
		if (message.OrigTradeID is not null) context.Validators.OrigTradeID(context, check, message.OrigTradeID);
		if (message.OrigSecondaryTradeID is not null) context.Validators.OrigSecondaryTradeID(context, check, message.OrigSecondaryTradeID);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, check, message.TransferReason);
		if (!Empty((IRootParties)message)) context.Validators.RootParties(context, check, message);
		if (message.ExecType is not null) context.Validators.ExecType(context, check, message.ExecType);
		if (message.TradeReportRefID is not null) context.Validators.TradeReportRefID(context, check, message.TradeReportRefID);
		if (message.SecondaryTradeReportRefID is not null) context.Validators.SecondaryTradeReportRefID(context, check, message.SecondaryTradeReportRefID);
		if (message.TrdRptStatus is not null) context.Validators.TrdRptStatus(context, check, message.TrdRptStatus);
		if (message.TradeReportRejectReason is not null) context.Validators.TradeReportRejectReason(context, check, message.TradeReportRejectReason);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, message.SecondaryTradeReportID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, check, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, check, message.TrdMatchID);
		if (message.ExecID is not null) context.Validators.ExecID(context, check, message.ExecID);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, message.SecondaryExecID);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, check, message.ExecRestatementReason);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.UnderlyingTradingSessionID is not null) context.Validators.UnderlyingTradingSessionID(context, check, message.UnderlyingTradingSessionID);
		if (message.UnderlyingTradingSessionSubID is not null) context.Validators.UnderlyingTradingSessionSubID(context, check, message.UnderlyingTradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (message.LastQty is not null) context.Validators.LastQty(context, check, message.LastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.VenueType is not null) context.Validators.VenueType(context, check, message.VenueType);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, check, message.LastParPx);
		if (message.CalculatedCcyLastQty is not null) context.Validators.CalculatedCcyLastQty(context, check, message.CalculatedCcyLastQty);
		if (message.LastSwapPoints is not null) context.Validators.LastSwapPoints(context, check, message.LastSwapPoints);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, check, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, check, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, check, message.AvgPxIndicator);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.TradeLegRefID is not null) context.Validators.TradeLegRefID(context, check, message.TradeLegRefID);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.TradeCaptureReportAck_NoUnderlyings);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, check, message.CopyMsgIndicator);
		if (message.NoTrdRepIndicators is not null) context.Validators.NoTrdRepIndicators(context, check, message.NoTrdRepIndicators);
		Counted(check, message.NoTrdRepIndicators, message.NoTrdRepIndicatorsGroups);
		Each(context, check, FixTag.NoTrdRepIndicators, message.NoTrdRepIndicatorsGroups, context.Validators.TradeCaptureReportAck_NoTrdRepIndicators);
		if (message.PublishTrdIndicator is not null) context.Validators.PublishTrdIndicator(context, check, message.PublishTrdIndicator);
		if (message.TradePublishIndicator is not null) context.Validators.TradePublishIndicator(context, check, message.TradePublishIndicator);
		if (message.ShortSaleReason is not null) context.Validators.ShortSaleReason(context, check, message.ShortSaleReason);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.TradeCaptureReportAck_NoLegs);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.AsOfIndicator is not null) context.Validators.AsOfIndicator(context, check, message.AsOfIndicator);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, check, message);
		if (message.TierCode is not null) context.Validators.TierCode(context, check, message.TierCode);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
		if (message.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, message.LastUpdateTime);
		if (message.RndPx is not null) context.Validators.RndPx(context, check, message.RndPx);
		if (message.NoSides is not null) context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.TradeCaptureReportAck_NoSides);
		if (message.RptSys is not null) context.Validators.RptSys(context, check, message.RptSys);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.FeeMultiplier is not null) context.Validators.FeeMultiplier(context, check, message.FeeMultiplier);
	}

	static void ValidateTradeCaptureReportRequest(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequest message)
	{
		if (message.TradeRequestID is null) Missing(check, FixTag.TradeRequestID);
		else context.Validators.TradeRequestID(context, check, message.TradeRequestID);
		if (message.TradeID is not null) context.Validators.TradeID(context, check, message.TradeID);
		if (message.SecondaryTradeID is not null) context.Validators.SecondaryTradeID(context, check, message.SecondaryTradeID);
		if (message.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, message.FirmTradeID);
		if (message.SecondaryFirmTradeID is not null) context.Validators.SecondaryFirmTradeID(context, check, message.SecondaryFirmTradeID);
		if (message.TradeRequestType is null) Missing(check, FixTag.TradeRequestType);
		else context.Validators.TradeRequestType(context, check, message.TradeRequestType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TradeReportID is not null) context.Validators.TradeReportID(context, check, message.TradeReportID);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, message.SecondaryTradeReportID);
		if (message.ExecID is not null) context.Validators.ExecID(context, check, message.ExecID);
		if (message.ExecType is not null) context.Validators.ExecType(context, check, message.ExecType);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.TradeHandlingInstr is not null) context.Validators.TradeHandlingInstr(context, check, message.TradeHandlingInstr);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, check, message.TransferReason);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, check, message.SecondaryTrdType);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, check, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, check, message.TrdMatchID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.TradeCaptureReportRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.TradeCaptureReportRequest_NoLegs);
		if (message.NoDates is not null) context.Validators.NoDates(context, check, message.NoDates);
		Counted(check, message.NoDates, message.NoDatesGroups);
		Each(context, check, FixTag.NoDates, message.NoDatesGroups, context.Validators.TradeCaptureReportRequest_NoDates);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TimeBracket is not null) context.Validators.TimeBracket(context, check, message.TimeBracket);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, message.TradeInputSource);
		if (message.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, check, message.TradeInputDevice);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
	}

	static void ValidateTradeCaptureReportRequestAck(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck message)
	{
		if (message.TradeRequestID is null) Missing(check, FixTag.TradeRequestID);
		else context.Validators.TradeRequestID(context, check, message.TradeRequestID);
		if (message.TradeID is not null) context.Validators.TradeID(context, check, message.TradeID);
		if (message.SecondaryTradeID is not null) context.Validators.SecondaryTradeID(context, check, message.SecondaryTradeID);
		if (message.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, message.FirmTradeID);
		if (message.SecondaryFirmTradeID is not null) context.Validators.SecondaryFirmTradeID(context, check, message.SecondaryFirmTradeID);
		if (message.TradeRequestType is null) Missing(check, FixTag.TradeRequestType);
		else context.Validators.TradeRequestType(context, check, message.TradeRequestType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TotNumTradeReports is not null) context.Validators.TotNumTradeReports(context, check, message.TotNumTradeReports);
		if (message.TradeRequestResult is null) Missing(check, FixTag.TradeRequestResult);
		else context.Validators.TradeRequestResult(context, check, message.TradeRequestResult);
		if (message.TradeRequestStatus is null) Missing(check, FixTag.TradeRequestStatus);
		else context.Validators.TradeRequestStatus(context, check, message.TradeRequestStatus);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.TradeCaptureReportRequestAck_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.TradeCaptureReportRequestAck_NoLegs);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.MessageEventSource is not null) context.Validators.MessageEventSource(context, check, message.MessageEventSource);
	}

	static void ValidateTradingSessionList(Fix50Context context, FixCheck check, FixMessage.TradingSessionList message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.NoTradingSessions is null) Missing(check, FixTag.NoTradingSessions);
		else context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.TradingSessionList_NoTradingSessions);
	}

	static void ValidateTradingSessionListRequest(Fix50Context context, FixCheck check, FixMessage.TradingSessionListRequest message)
	{
		if (message.TradSesReqID is null) Missing(check, FixTag.TradSesReqID);
		else context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateTradingSessionListUpdateReport(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.NoTradingSessions is null) Missing(check, FixTag.NoTradingSessions);
		else context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions);
	}

	static void ValidateTradingSessionStatus(Fix50Context context, FixCheck check, FixMessage.TradingSessionStatus message)
	{
		if (!Empty((IApplicationSequenceControl)message)) context.Validators.ApplicationSequenceControl(context, check, message);
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is null) Missing(check, FixTag.TradingSessionID);
		else context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.TradSesStatus is null) Missing(check, FixTag.TradSesStatus);
		else context.Validators.TradSesStatus(context, check, message.TradSesStatus);
		if (message.TradSesEvent is not null) context.Validators.TradSesEvent(context, check, message.TradSesEvent);
		if (message.TradSesStatusRejReason is not null) context.Validators.TradSesStatusRejReason(context, check, message.TradSesStatusRejReason);
		if (message.TradSesStartTime is not null) context.Validators.TradSesStartTime(context, check, message.TradSesStartTime);
		if (message.TradSesOpenTime is not null) context.Validators.TradSesOpenTime(context, check, message.TradSesOpenTime);
		if (message.TradSesPreCloseTime is not null) context.Validators.TradSesPreCloseTime(context, check, message.TradSesPreCloseTime);
		if (message.TradSesCloseTime is not null) context.Validators.TradSesCloseTime(context, check, message.TradSesCloseTime);
		if (message.TradSesEndTime is not null) context.Validators.TradSesEndTime(context, check, message.TradSesEndTime);
		if (message.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, check, message.TotalVolumeTraded);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
	}

	static void ValidateTradingSessionStatusRequest(Fix50Context context, FixCheck check, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.TradSesReqID is null) Missing(check, FixTag.TradSesReqID);
		else context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.MarketID is not null) context.Validators.MarketID(context, check, message.MarketID);
		if (message.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, message.MarketSegmentID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
	}

	static void ValidateUserNotification(Fix50Context context, FixCheck check, FixMessage.UserNotification message)
	{
		if (message.NoUsernames is not null) context.Validators.NoUsernames(context, check, message.NoUsernames);
		Counted(check, message.NoUsernames, message.NoUsernamesGroups);
		Each(context, check, FixTag.NoUsernames, message.NoUsernamesGroups, context.Validators.UserNotification_NoUsernames);
		if (message.UserStatus is null) Missing(check, FixTag.UserStatus);
		else context.Validators.UserStatus(context, check, message.UserStatus);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateUserRequest(Fix50Context context, FixCheck check, FixMessage.UserRequest message)
	{
		if (message.UserRequestID is null) Missing(check, FixTag.UserRequestID);
		else context.Validators.UserRequestID(context, check, message.UserRequestID);
		if (message.UserRequestType is null) Missing(check, FixTag.UserRequestType);
		else context.Validators.UserRequestType(context, check, message.UserRequestType);
		if (message.Username is null) Missing(check, FixTag.Username);
		else context.Validators.Username(context, check, message.Username);
		if (message.Password is not null) context.Validators.Password(context, check, message.Password);
		if (message.NewPassword is not null) context.Validators.NewPassword(context, check, message.NewPassword);
		if (message.EncryptedPasswordMethod is not null) context.Validators.EncryptedPasswordMethod(context, check, message.EncryptedPasswordMethod);
		if (message.EncryptedPasswordLen is not null) context.Validators.EncryptedPasswordLen(context, check, message.EncryptedPasswordLen);
		if (message.EncryptedPassword is not null) context.Validators.EncryptedPassword(context, check, message.EncryptedPassword);
		if (message.EncryptedNewPasswordLen is not null) context.Validators.EncryptedNewPasswordLen(context, check, message.EncryptedNewPasswordLen);
		if (message.EncryptedNewPassword is not null) context.Validators.EncryptedNewPassword(context, check, message.EncryptedNewPassword);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateUserResponse(Fix50Context context, FixCheck check, FixMessage.UserResponse message)
	{
		if (message.UserRequestID is null) Missing(check, FixTag.UserRequestID);
		else context.Validators.UserRequestID(context, check, message.UserRequestID);
		if (message.Username is null) Missing(check, FixTag.Username);
		else context.Validators.Username(context, check, message.Username);
		if (message.UserStatus is not null) context.Validators.UserStatus(context, check, message.UserStatus);
		if (message.UserStatusText is not null) context.Validators.UserStatusText(context, check, message.UserStatusText);
	}

	static void ValidateXMLnonFIX(Fix50Context context, FixCheck check, FixMessage.XMLnonFIX message)
	{
	}

	static void ValidateApplicationSequenceControl(Fix50Context context, FixCheck check, IApplicationSequenceControl block)
	{
		if (block.ApplID is not null) context.Validators.ApplID(context, check, block.ApplID);
		if (block.ApplSeqNum is not null) context.Validators.ApplSeqNum(context, check, block.ApplSeqNum);
		if (block.ApplLastSeqNum is not null) context.Validators.ApplLastSeqNum(context, check, block.ApplLastSeqNum);
		if (block.ApplResendFlag is not null) context.Validators.ApplResendFlag(context, check, block.ApplResendFlag);
	}

	static void ValidateCommissionData(Fix50Context context, FixCheck check, ICommissionData block)
	{
		if (block.Commission is not null) context.Validators.Commission(context, check, block.Commission);
		if (block.CommType is not null) context.Validators.CommType(context, check, block.CommType);
		if (block.CommCurrency is not null) context.Validators.CommCurrency(context, check, block.CommCurrency);
		if (block.FundRenewWaiv is not null) context.Validators.FundRenewWaiv(context, check, block.FundRenewWaiv);
	}

	static void ValidateDiscretionInstructions(Fix50Context context, FixCheck check, IDiscretionInstructions block)
	{
		if (block.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, block.DiscretionInst);
		if (block.DiscretionOffsetValue is not null) context.Validators.DiscretionOffsetValue(context, check, block.DiscretionOffsetValue);
		if (block.DiscretionMoveType is not null) context.Validators.DiscretionMoveType(context, check, block.DiscretionMoveType);
		if (block.DiscretionOffsetType is not null) context.Validators.DiscretionOffsetType(context, check, block.DiscretionOffsetType);
		if (block.DiscretionLimitType is not null) context.Validators.DiscretionLimitType(context, check, block.DiscretionLimitType);
		if (block.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, check, block.DiscretionRoundDirection);
		if (block.DiscretionScope is not null) context.Validators.DiscretionScope(context, check, block.DiscretionScope);
	}

	static void ValidateDisplayInstruction(Fix50Context context, FixCheck check, IDisplayInstruction block)
	{
		if (block.DisplayQty is not null) context.Validators.DisplayQty(context, check, block.DisplayQty);
		if (block.SecondaryDisplayQty is not null) context.Validators.SecondaryDisplayQty(context, check, block.SecondaryDisplayQty);
		if (block.DisplayWhen is not null) context.Validators.DisplayWhen(context, check, block.DisplayWhen);
		if (block.DisplayMethod is not null) context.Validators.DisplayMethod(context, check, block.DisplayMethod);
		if (block.DisplayLowQty is not null) context.Validators.DisplayLowQty(context, check, block.DisplayLowQty);
		if (block.DisplayHighQty is not null) context.Validators.DisplayHighQty(context, check, block.DisplayHighQty);
		if (block.DisplayMinIncr is not null) context.Validators.DisplayMinIncr(context, check, block.DisplayMinIncr);
		if (block.RefreshQty is not null) context.Validators.RefreshQty(context, check, block.RefreshQty);
	}

	static void ValidateExpirationQty(Fix50Context context, FixCheck check, IExpirationQty block)
	{
		if (block.NoExpiration is not null) context.Validators.NoExpiration(context, check, block.NoExpiration);
		Counted(check, block.NoExpiration, block.NoExpirationGroups);
		Each(context, check, FixTag.NoExpiration, block.NoExpirationGroups, context.Validators.ExpirationQty_NoExpiration);
	}

	static void ValidateFinancingDetails(Fix50Context context, FixCheck check, IFinancingDetails block)
	{
		if (block.AgreementDesc is not null) context.Validators.AgreementDesc(context, check, block.AgreementDesc);
		if (block.AgreementID is not null) context.Validators.AgreementID(context, check, block.AgreementID);
		if (block.AgreementDate is not null) context.Validators.AgreementDate(context, check, block.AgreementDate);
		if (block.AgreementCurrency is not null) context.Validators.AgreementCurrency(context, check, block.AgreementCurrency);
		if (block.TerminationType is not null) context.Validators.TerminationType(context, check, block.TerminationType);
		if (block.StartDate is not null) context.Validators.StartDate(context, check, block.StartDate);
		if (block.EndDate is not null) context.Validators.EndDate(context, check, block.EndDate);
		if (block.DeliveryType is not null) context.Validators.DeliveryType(context, check, block.DeliveryType);
		if (block.MarginRatio is not null) context.Validators.MarginRatio(context, check, block.MarginRatio);
	}

	static void ValidateInstrument(Fix50Context context, FixCheck check, IInstrument block)
	{
		if (block.Symbol is not null) context.Validators.Symbol(context, check, block.Symbol);
		if (block.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, block.SymbolSfx);
		if (block.SecurityID is not null) context.Validators.SecurityID(context, check, block.SecurityID);
		if (block.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, check, block.SecurityIDSource);
		if (block.NoSecurityAltID is not null) context.Validators.NoSecurityAltID(context, check, block.NoSecurityAltID);
		Counted(check, block.NoSecurityAltID, block.NoSecurityAltIDGroups);
		Each(context, check, FixTag.NoSecurityAltID, block.NoSecurityAltIDGroups, context.Validators.Instrument_NoSecurityAltID);
		if (block.Product is not null) context.Validators.Product(context, check, block.Product);
		if (block.ProductComplex is not null) context.Validators.ProductComplex(context, check, block.ProductComplex);
		if (block.SecurityGroup is not null) context.Validators.SecurityGroup(context, check, block.SecurityGroup);
		if (block.CFICode is not null) context.Validators.CFICode(context, check, block.CFICode);
		if (block.SecurityType is not null) context.Validators.SecurityType(context, check, block.SecurityType);
		if (block.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, block.SecuritySubType);
		if (block.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, block.MaturityMonthYear);
		if (block.MaturityDate is not null) context.Validators.MaturityDate(context, check, block.MaturityDate);
		if (block.MaturityTime is not null) context.Validators.MaturityTime(context, check, block.MaturityTime);
		if (block.SettleOnOpenFlag is not null) context.Validators.SettleOnOpenFlag(context, check, block.SettleOnOpenFlag);
		if (block.InstrmtAssignmentMethod is not null) context.Validators.InstrmtAssignmentMethod(context, check, block.InstrmtAssignmentMethod);
		if (block.SecurityStatus is not null) context.Validators.SecurityStatus(context, check, block.SecurityStatus);
		if (block.CouponPaymentDate is not null) context.Validators.CouponPaymentDate(context, check, block.CouponPaymentDate);
		if (block.RestructuringType is not null) context.Validators.RestructuringType(context, check, block.RestructuringType);
		if (block.Seniority is not null) context.Validators.Seniority(context, check, block.Seniority);
		if (block.NotionalPercentageOutstanding is not null) context.Validators.NotionalPercentageOutstanding(context, check, block.NotionalPercentageOutstanding);
		if (block.OriginalNotionalPercentageOutstanding is not null) context.Validators.OriginalNotionalPercentageOutstanding(context, check, block.OriginalNotionalPercentageOutstanding);
		if (block.AttachmentPoint is not null) context.Validators.AttachmentPoint(context, check, block.AttachmentPoint);
		if (block.DetachmentPoint is not null) context.Validators.DetachmentPoint(context, check, block.DetachmentPoint);
		if (block.IssueDate is not null) context.Validators.IssueDate(context, check, block.IssueDate);
		if (block.RepoCollateralSecurityType is not null) context.Validators.RepoCollateralSecurityType(context, check, block.RepoCollateralSecurityType);
		if (block.RepurchaseTerm is not null) context.Validators.RepurchaseTerm(context, check, block.RepurchaseTerm);
		if (block.RepurchaseRate is not null) context.Validators.RepurchaseRate(context, check, block.RepurchaseRate);
		if (block.Factor is not null) context.Validators.Factor(context, check, block.Factor);
		if (block.CreditRating is not null) context.Validators.CreditRating(context, check, block.CreditRating);
		if (block.InstrRegistry is not null) context.Validators.InstrRegistry(context, check, block.InstrRegistry);
		if (block.CountryOfIssue is not null) context.Validators.CountryOfIssue(context, check, block.CountryOfIssue);
		if (block.StateOrProvinceOfIssue is not null) context.Validators.StateOrProvinceOfIssue(context, check, block.StateOrProvinceOfIssue);
		if (block.LocaleOfIssue is not null) context.Validators.LocaleOfIssue(context, check, block.LocaleOfIssue);
		if (block.RedemptionDate is not null) context.Validators.RedemptionDate(context, check, block.RedemptionDate);
		if (block.StrikePrice is not null) context.Validators.StrikePrice(context, check, block.StrikePrice);
		if (block.StrikeCurrency is not null) context.Validators.StrikeCurrency(context, check, block.StrikeCurrency);
		if (block.StrikeMultiplier is not null) context.Validators.StrikeMultiplier(context, check, block.StrikeMultiplier);
		if (block.StrikeValue is not null) context.Validators.StrikeValue(context, check, block.StrikeValue);
		if (block.StrikePriceDeterminationMethod is not null) context.Validators.StrikePriceDeterminationMethod(context, check, block.StrikePriceDeterminationMethod);
		if (block.StrikePriceBoundaryMethod is not null) context.Validators.StrikePriceBoundaryMethod(context, check, block.StrikePriceBoundaryMethod);
		if (block.StrikePriceBoundaryPrecision is not null) context.Validators.StrikePriceBoundaryPrecision(context, check, block.StrikePriceBoundaryPrecision);
		if (block.UnderlyingPriceDeterminationMethod is not null) context.Validators.UnderlyingPriceDeterminationMethod(context, check, block.UnderlyingPriceDeterminationMethod);
		if (block.OptAttribute is not null) context.Validators.OptAttribute(context, check, block.OptAttribute);
		if (block.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, block.ContractMultiplier);
		if (block.ContractMultiplierUnit is not null) context.Validators.ContractMultiplierUnit(context, check, block.ContractMultiplierUnit);
		if (block.FlowScheduleType is not null) context.Validators.FlowScheduleType(context, check, block.FlowScheduleType);
		if (block.MinPriceIncrement is not null) context.Validators.MinPriceIncrement(context, check, block.MinPriceIncrement);
		if (block.MinPriceIncrementAmount is not null) context.Validators.MinPriceIncrementAmount(context, check, block.MinPriceIncrementAmount);
		if (block.UnitOfMeasure is not null) context.Validators.UnitOfMeasure(context, check, block.UnitOfMeasure);
		if (block.UnitOfMeasureQty is not null) context.Validators.UnitOfMeasureQty(context, check, block.UnitOfMeasureQty);
		if (block.PriceUnitOfMeasure is not null) context.Validators.PriceUnitOfMeasure(context, check, block.PriceUnitOfMeasure);
		if (block.PriceUnitOfMeasureQty is not null) context.Validators.PriceUnitOfMeasureQty(context, check, block.PriceUnitOfMeasureQty);
		if (block.SettlMethod is not null) context.Validators.SettlMethod(context, check, block.SettlMethod);
		if (block.ExerciseStyle is not null) context.Validators.ExerciseStyle(context, check, block.ExerciseStyle);
		if (block.OptPayoutType is not null) context.Validators.OptPayoutType(context, check, block.OptPayoutType);
		if (block.OptPayoutAmount is not null) context.Validators.OptPayoutAmount(context, check, block.OptPayoutAmount);
		if (block.PriceQuoteMethod is not null) context.Validators.PriceQuoteMethod(context, check, block.PriceQuoteMethod);
		if (block.ValuationMethod is not null) context.Validators.ValuationMethod(context, check, block.ValuationMethod);
		if (block.ListMethod is not null) context.Validators.ListMethod(context, check, block.ListMethod);
		if (block.CapPrice is not null) context.Validators.CapPrice(context, check, block.CapPrice);
		if (block.FloorPrice is not null) context.Validators.FloorPrice(context, check, block.FloorPrice);
		if (block.PutOrCall is not null) context.Validators.PutOrCall(context, check, block.PutOrCall);
		if (block.FlexibleIndicator is not null) context.Validators.FlexibleIndicator(context, check, block.FlexibleIndicator);
		if (block.FlexProductEligibilityIndicator is not null) context.Validators.FlexProductEligibilityIndicator(context, check, block.FlexProductEligibilityIndicator);
		if (block.TimeUnit is not null) context.Validators.TimeUnit(context, check, block.TimeUnit);
		if (block.CouponRate is not null) context.Validators.CouponRate(context, check, block.CouponRate);
		if (block.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, block.SecurityExchange);
		if (block.PositionLimit is not null) context.Validators.PositionLimit(context, check, block.PositionLimit);
		if (block.NTPositionLimit is not null) context.Validators.NTPositionLimit(context, check, block.NTPositionLimit);
		if (block.Issuer is not null) context.Validators.Issuer(context, check, block.Issuer);
		if (block.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, block.EncodedIssuerLen);
		if (block.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, block.EncodedIssuer);
		if (block.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, block.SecurityDesc);
		if (block.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, block.EncodedSecurityDescLen);
		if (block.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, block.EncodedSecurityDesc);
		if (block.SecurityXMLLen is not null) context.Validators.SecurityXMLLen(context, check, block.SecurityXMLLen);
		if (block.SecurityXML is not null) context.Validators.SecurityXML(context, check, block.SecurityXML);
		if (block.SecurityXMLSchema is not null) context.Validators.SecurityXMLSchema(context, check, block.SecurityXMLSchema);
		if (block.Pool is not null) context.Validators.Pool(context, check, block.Pool);
		if (block.ContractSettlMonth is not null) context.Validators.ContractSettlMonth(context, check, block.ContractSettlMonth);
		if (block.CPProgram is not null) context.Validators.CPProgram(context, check, block.CPProgram);
		if (block.CPRegType is not null) context.Validators.CPRegType(context, check, block.CPRegType);
		if (block.NoEvents is not null) context.Validators.NoEvents(context, check, block.NoEvents);
		Counted(check, block.NoEvents, block.NoEventsGroups);
		Each(context, check, FixTag.NoEvents, block.NoEventsGroups, context.Validators.Instrument_NoEvents);
		if (block.DatedDate is not null) context.Validators.DatedDate(context, check, block.DatedDate);
		if (block.InterestAccrualDate is not null) context.Validators.InterestAccrualDate(context, check, block.InterestAccrualDate);
		if (!Empty((IInstrumentParties)block)) context.Validators.InstrumentParties(context, check, block);
		if (block.NoComplexEvents is not null) context.Validators.NoComplexEvents(context, check, block.NoComplexEvents);
		Counted(check, block.NoComplexEvents, block.NoComplexEventsGroups);
		Each(context, check, FixTag.NoComplexEvents, block.NoComplexEventsGroups, context.Validators.Instrument_NoComplexEvents);
	}

	static void ValidateInstrumentExtension(Fix50Context context, FixCheck check, IInstrumentExtension block)
	{
		if (block.DeliveryForm is not null) context.Validators.DeliveryForm(context, check, block.DeliveryForm);
		if (block.PctAtRisk is not null) context.Validators.PctAtRisk(context, check, block.PctAtRisk);
		if (block.NoInstrAttrib is not null) context.Validators.NoInstrAttrib(context, check, block.NoInstrAttrib);
		Counted(check, block.NoInstrAttrib, block.NoInstrAttribGroups);
		Each(context, check, FixTag.NoInstrAttrib, block.NoInstrAttribGroups, context.Validators.InstrumentExtension_NoInstrAttrib);
	}

	static void ValidateInstrumentLeg(Fix50Context context, FixCheck check, IInstrumentLeg block)
	{
		if (block.LegSymbol is not null) context.Validators.LegSymbol(context, check, block.LegSymbol);
		if (block.LegSymbolSfx is not null) context.Validators.LegSymbolSfx(context, check, block.LegSymbolSfx);
		if (block.LegSecurityID is not null) context.Validators.LegSecurityID(context, check, block.LegSecurityID);
		if (block.LegSecurityIDSource is not null) context.Validators.LegSecurityIDSource(context, check, block.LegSecurityIDSource);
		if (block.NoLegSecurityAltID is not null) context.Validators.NoLegSecurityAltID(context, check, block.NoLegSecurityAltID);
		Counted(check, block.NoLegSecurityAltID, block.NoLegSecurityAltIDGroups);
		Each(context, check, FixTag.NoLegSecurityAltID, block.NoLegSecurityAltIDGroups, context.Validators.InstrumentLeg_NoLegSecurityAltID);
		if (block.LegProduct is not null) context.Validators.LegProduct(context, check, block.LegProduct);
		if (block.LegCFICode is not null) context.Validators.LegCFICode(context, check, block.LegCFICode);
		if (block.LegSecurityType is not null) context.Validators.LegSecurityType(context, check, block.LegSecurityType);
		if (block.LegSecuritySubType is not null) context.Validators.LegSecuritySubType(context, check, block.LegSecuritySubType);
		if (block.LegMaturityMonthYear is not null) context.Validators.LegMaturityMonthYear(context, check, block.LegMaturityMonthYear);
		if (block.LegMaturityDate is not null) context.Validators.LegMaturityDate(context, check, block.LegMaturityDate);
		if (block.LegMaturityTime is not null) context.Validators.LegMaturityTime(context, check, block.LegMaturityTime);
		if (block.LegCouponPaymentDate is not null) context.Validators.LegCouponPaymentDate(context, check, block.LegCouponPaymentDate);
		if (block.LegIssueDate is not null) context.Validators.LegIssueDate(context, check, block.LegIssueDate);
		if (block.LegRepoCollateralSecurityType is not null) context.Validators.LegRepoCollateralSecurityType(context, check, block.LegRepoCollateralSecurityType);
		if (block.LegRepurchaseTerm is not null) context.Validators.LegRepurchaseTerm(context, check, block.LegRepurchaseTerm);
		if (block.LegRepurchaseRate is not null) context.Validators.LegRepurchaseRate(context, check, block.LegRepurchaseRate);
		if (block.LegFactor is not null) context.Validators.LegFactor(context, check, block.LegFactor);
		if (block.LegCreditRating is not null) context.Validators.LegCreditRating(context, check, block.LegCreditRating);
		if (block.LegInstrRegistry is not null) context.Validators.LegInstrRegistry(context, check, block.LegInstrRegistry);
		if (block.LegCountryOfIssue is not null) context.Validators.LegCountryOfIssue(context, check, block.LegCountryOfIssue);
		if (block.LegStateOrProvinceOfIssue is not null) context.Validators.LegStateOrProvinceOfIssue(context, check, block.LegStateOrProvinceOfIssue);
		if (block.LegLocaleOfIssue is not null) context.Validators.LegLocaleOfIssue(context, check, block.LegLocaleOfIssue);
		if (block.LegRedemptionDate is not null) context.Validators.LegRedemptionDate(context, check, block.LegRedemptionDate);
		if (block.LegStrikePrice is not null) context.Validators.LegStrikePrice(context, check, block.LegStrikePrice);
		if (block.LegStrikeCurrency is not null) context.Validators.LegStrikeCurrency(context, check, block.LegStrikeCurrency);
		if (block.LegOptAttribute is not null) context.Validators.LegOptAttribute(context, check, block.LegOptAttribute);
		if (block.LegContractMultiplier is not null) context.Validators.LegContractMultiplier(context, check, block.LegContractMultiplier);
		if (block.LegContractMultiplierUnit is not null) context.Validators.LegContractMultiplierUnit(context, check, block.LegContractMultiplierUnit);
		if (block.LegFlowScheduleType is not null) context.Validators.LegFlowScheduleType(context, check, block.LegFlowScheduleType);
		if (block.LegUnitOfMeasure is not null) context.Validators.LegUnitOfMeasure(context, check, block.LegUnitOfMeasure);
		if (block.LegUnitOfMeasureQty is not null) context.Validators.LegUnitOfMeasureQty(context, check, block.LegUnitOfMeasureQty);
		if (block.LegPriceUnitOfMeasure is not null) context.Validators.LegPriceUnitOfMeasure(context, check, block.LegPriceUnitOfMeasure);
		if (block.LegPriceUnitOfMeasureQty is not null) context.Validators.LegPriceUnitOfMeasureQty(context, check, block.LegPriceUnitOfMeasureQty);
		if (block.LegTimeUnit is not null) context.Validators.LegTimeUnit(context, check, block.LegTimeUnit);
		if (block.LegExerciseStyle is not null) context.Validators.LegExerciseStyle(context, check, block.LegExerciseStyle);
		if (block.LegCouponRate is not null) context.Validators.LegCouponRate(context, check, block.LegCouponRate);
		if (block.LegSecurityExchange is not null) context.Validators.LegSecurityExchange(context, check, block.LegSecurityExchange);
		if (block.LegIssuer is not null) context.Validators.LegIssuer(context, check, block.LegIssuer);
		if (block.EncodedLegIssuerLen is not null) context.Validators.EncodedLegIssuerLen(context, check, block.EncodedLegIssuerLen);
		if (block.EncodedLegIssuer is not null) context.Validators.EncodedLegIssuer(context, check, block.EncodedLegIssuer);
		if (block.LegSecurityDesc is not null) context.Validators.LegSecurityDesc(context, check, block.LegSecurityDesc);
		if (block.EncodedLegSecurityDescLen is not null) context.Validators.EncodedLegSecurityDescLen(context, check, block.EncodedLegSecurityDescLen);
		if (block.EncodedLegSecurityDesc is not null) context.Validators.EncodedLegSecurityDesc(context, check, block.EncodedLegSecurityDesc);
		if (block.LegRatioQty is not null) context.Validators.LegRatioQty(context, check, block.LegRatioQty);
		if (block.LegSide is not null) context.Validators.LegSide(context, check, block.LegSide);
		if (block.LegCurrency is not null) context.Validators.LegCurrency(context, check, block.LegCurrency);
		if (block.LegPool is not null) context.Validators.LegPool(context, check, block.LegPool);
		if (block.LegDatedDate is not null) context.Validators.LegDatedDate(context, check, block.LegDatedDate);
		if (block.LegContractSettlMonth is not null) context.Validators.LegContractSettlMonth(context, check, block.LegContractSettlMonth);
		if (block.LegInterestAccrualDate is not null) context.Validators.LegInterestAccrualDate(context, check, block.LegInterestAccrualDate);
		if (block.LegPutOrCall is not null) context.Validators.LegPutOrCall(context, check, block.LegPutOrCall);
		if (block.LegOptionRatio is not null) context.Validators.LegOptionRatio(context, check, block.LegOptionRatio);
		if (block.LegPrice is not null) context.Validators.LegPrice(context, check, block.LegPrice);
	}

	static void ValidateInstrumentParties(Fix50Context context, FixCheck check, IInstrumentParties block)
	{
		if (block.NoInstrumentParties is not null) context.Validators.NoInstrumentParties(context, check, block.NoInstrumentParties);
		Counted(check, block.NoInstrumentParties, block.NoInstrumentPartiesGroups);
		Each(context, check, FixTag.NoInstrumentParties, block.NoInstrumentPartiesGroups, context.Validators.InstrumentParties_NoInstrumentParties);
	}

	static void ValidateLegBenchmarkCurveData(Fix50Context context, FixCheck check, ILegBenchmarkCurveData block)
	{
		if (block.LegBenchmarkCurveCurrency is not null) context.Validators.LegBenchmarkCurveCurrency(context, check, block.LegBenchmarkCurveCurrency);
		if (block.LegBenchmarkCurveName is not null) context.Validators.LegBenchmarkCurveName(context, check, block.LegBenchmarkCurveName);
		if (block.LegBenchmarkCurvePoint is not null) context.Validators.LegBenchmarkCurvePoint(context, check, block.LegBenchmarkCurvePoint);
		if (block.LegBenchmarkPrice is not null) context.Validators.LegBenchmarkPrice(context, check, block.LegBenchmarkPrice);
		if (block.LegBenchmarkPriceType is not null) context.Validators.LegBenchmarkPriceType(context, check, block.LegBenchmarkPriceType);
	}

	static void ValidateLegStipulations(Fix50Context context, FixCheck check, ILegStipulations block)
	{
		if (block.NoLegStipulations is not null) context.Validators.NoLegStipulations(context, check, block.NoLegStipulations);
		Counted(check, block.NoLegStipulations, block.NoLegStipulationsGroups);
		Each(context, check, FixTag.NoLegStipulations, block.NoLegStipulationsGroups, context.Validators.LegStipulations_NoLegStipulations);
	}

	static void ValidateNestedParties(Fix50Context context, FixCheck check, INestedParties block)
	{
		if (block.NoNestedPartyIDs is not null) context.Validators.NoNestedPartyIDs(context, check, block.NoNestedPartyIDs);
		Counted(check, block.NoNestedPartyIDs, block.NoNestedPartyIDsGroups);
		Each(context, check, FixTag.NoNestedPartyIDs, block.NoNestedPartyIDsGroups, context.Validators.NestedParties_NoNestedPartyIDs);
	}

	static void ValidateNestedParties2(Fix50Context context, FixCheck check, INestedParties2 block)
	{
		if (block.NoNested2PartyIDs is not null) context.Validators.NoNested2PartyIDs(context, check, block.NoNested2PartyIDs);
		Counted(check, block.NoNested2PartyIDs, block.NoNested2PartyIDsGroups);
		Each(context, check, FixTag.NoNested2PartyIDs, block.NoNested2PartyIDsGroups, context.Validators.NestedParties2_NoNested2PartyIDs);
	}

	static void ValidateNestedParties3(Fix50Context context, FixCheck check, INestedParties3 block)
	{
		if (block.NoNested3PartyIDs is not null) context.Validators.NoNested3PartyIDs(context, check, block.NoNested3PartyIDs);
		Counted(check, block.NoNested3PartyIDs, block.NoNested3PartyIDsGroups);
		Each(context, check, FixTag.NoNested3PartyIDs, block.NoNested3PartyIDsGroups, context.Validators.NestedParties3_NoNested3PartyIDs);
	}

	static void ValidateNestedParties4(Fix50Context context, FixCheck check, INestedParties4 block)
	{
		if (block.NoNested4PartyIDs is not null) context.Validators.NoNested4PartyIDs(context, check, block.NoNested4PartyIDs);
		Counted(check, block.NoNested4PartyIDs, block.NoNested4PartyIDsGroups);
		Each(context, check, FixTag.NoNested4PartyIDs, block.NoNested4PartyIDsGroups, context.Validators.NestedParties4_NoNested4PartyIDs);
	}

	static void ValidateOrderQtyData(Fix50Context context, FixCheck check, IOrderQtyData block)
	{
		if (block.OrderQty is not null) context.Validators.OrderQty(context, check, block.OrderQty);
		if (block.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, block.CashOrderQty);
		if (block.OrderPercent is not null) context.Validators.OrderPercent(context, check, block.OrderPercent);
		if (block.RoundingDirection is not null) context.Validators.RoundingDirection(context, check, block.RoundingDirection);
		if (block.RoundingModulus is not null) context.Validators.RoundingModulus(context, check, block.RoundingModulus);
	}

	static void ValidateParties(Fix50Context context, FixCheck check, IParties block)
	{
		if (block.NoPartyIDs is not null) context.Validators.NoPartyIDs(context, check, block.NoPartyIDs);
		Counted(check, block.NoPartyIDs, block.NoPartyIDsGroups);
		Each(context, check, FixTag.NoPartyIDs, block.NoPartyIDsGroups, context.Validators.Parties_NoPartyIDs);
	}

	static void ValidatePegInstructions(Fix50Context context, FixCheck check, IPegInstructions block)
	{
		if (block.PegOffsetValue is not null) context.Validators.PegOffsetValue(context, check, block.PegOffsetValue);
		if (block.PegPriceType is not null) context.Validators.PegPriceType(context, check, block.PegPriceType);
		if (block.PegMoveType is not null) context.Validators.PegMoveType(context, check, block.PegMoveType);
		if (block.PegOffsetType is not null) context.Validators.PegOffsetType(context, check, block.PegOffsetType);
		if (block.PegLimitType is not null) context.Validators.PegLimitType(context, check, block.PegLimitType);
		if (block.PegRoundDirection is not null) context.Validators.PegRoundDirection(context, check, block.PegRoundDirection);
		if (block.PegScope is not null) context.Validators.PegScope(context, check, block.PegScope);
		if (block.PegSecurityIDSource is not null) context.Validators.PegSecurityIDSource(context, check, block.PegSecurityIDSource);
		if (block.PegSecurityID is not null) context.Validators.PegSecurityID(context, check, block.PegSecurityID);
		if (block.PegSymbol is not null) context.Validators.PegSymbol(context, check, block.PegSymbol);
		if (block.PegSecurityDesc is not null) context.Validators.PegSecurityDesc(context, check, block.PegSecurityDesc);
	}

	static void ValidatePositionAmountData(Fix50Context context, FixCheck check, IPositionAmountData block)
	{
		if (block.NoPosAmt is not null) context.Validators.NoPosAmt(context, check, block.NoPosAmt);
		Counted(check, block.NoPosAmt, block.NoPosAmtGroups);
		Each(context, check, FixTag.NoPosAmt, block.NoPosAmtGroups, context.Validators.PositionAmountData_NoPosAmt);
	}

	static void ValidatePositionQty(Fix50Context context, FixCheck check, IPositionQty block)
	{
		if (block.NoPositions is not null) context.Validators.NoPositions(context, check, block.NoPositions);
		Counted(check, block.NoPositions, block.NoPositionsGroups);
		Each(context, check, FixTag.NoPositions, block.NoPositionsGroups, context.Validators.PositionQty_NoPositions);
	}

	static void ValidateRateSource(Fix50Context context, FixCheck check, IRateSource block)
	{
		if (block.NoRateSources is not null) context.Validators.NoRateSources(context, check, block.NoRateSources);
		Counted(check, block.NoRateSources, block.NoRateSourcesGroups);
		Each(context, check, FixTag.NoRateSources, block.NoRateSourcesGroups, context.Validators.RateSource_NoRateSources);
	}

	static void ValidateRootParties(Fix50Context context, FixCheck check, IRootParties block)
	{
		if (block.NoRootPartyIDs is not null) context.Validators.NoRootPartyIDs(context, check, block.NoRootPartyIDs);
		Counted(check, block.NoRootPartyIDs, block.NoRootPartyIDsGroups);
		Each(context, check, FixTag.NoRootPartyIDs, block.NoRootPartyIDsGroups, context.Validators.RootParties_NoRootPartyIDs);
	}

	static void ValidateSecurityTradingRules(Fix50Context context, FixCheck check, ISecurityTradingRules block)
	{
		if (block.NoTickRules is not null) context.Validators.NoTickRules(context, check, block.NoTickRules);
		Counted(check, block.NoTickRules, block.NoTickRulesGroups);
		Each(context, check, FixTag.NoTickRules, block.NoTickRulesGroups, context.Validators.SecurityTradingRules_NoTickRules);
		if (block.NoLotTypeRules is not null) context.Validators.NoLotTypeRules(context, check, block.NoLotTypeRules);
		Counted(check, block.NoLotTypeRules, block.NoLotTypeRulesGroups);
		Each(context, check, FixTag.NoLotTypeRules, block.NoLotTypeRulesGroups, context.Validators.SecurityTradingRules_NoLotTypeRules);
		if (block.PriceLimitType is not null) context.Validators.PriceLimitType(context, check, block.PriceLimitType);
		if (block.LowLimitPrice is not null) context.Validators.LowLimitPrice(context, check, block.LowLimitPrice);
		if (block.HighLimitPrice is not null) context.Validators.HighLimitPrice(context, check, block.HighLimitPrice);
		if (block.TradingReferencePrice is not null) context.Validators.TradingReferencePrice(context, check, block.TradingReferencePrice);
		if (block.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, block.ExpirationCycle);
		if (block.MinTradeVol is not null) context.Validators.MinTradeVol(context, check, block.MinTradeVol);
		if (block.MaxTradeVol is not null) context.Validators.MaxTradeVol(context, check, block.MaxTradeVol);
		if (block.MaxPriceVariation is not null) context.Validators.MaxPriceVariation(context, check, block.MaxPriceVariation);
		if (block.ImpliedMarketIndicator is not null) context.Validators.ImpliedMarketIndicator(context, check, block.ImpliedMarketIndicator);
		if (block.TradingCurrency is not null) context.Validators.TradingCurrency(context, check, block.TradingCurrency);
		if (block.RoundLot is not null) context.Validators.RoundLot(context, check, block.RoundLot);
		if (block.MultilegModel is not null) context.Validators.MultilegModel(context, check, block.MultilegModel);
		if (block.MultilegPriceMethod is not null) context.Validators.MultilegPriceMethod(context, check, block.MultilegPriceMethod);
		if (block.PriceType is not null) context.Validators.PriceType(context, check, block.PriceType);
		if (block.NoTradingSessionRules is not null) context.Validators.NoTradingSessionRules(context, check, block.NoTradingSessionRules);
		Counted(check, block.NoTradingSessionRules, block.NoTradingSessionRulesGroups);
		Each(context, check, FixTag.NoTradingSessionRules, block.NoTradingSessionRulesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules);
		if (block.NoNestedInstrAttrib is not null) context.Validators.NoNestedInstrAttrib(context, check, block.NoNestedInstrAttrib);
		Counted(check, block.NoNestedInstrAttrib, block.NoNestedInstrAttribGroups);
		Each(context, check, FixTag.NoNestedInstrAttrib, block.NoNestedInstrAttribGroups, context.Validators.SecurityTradingRules_NoNestedInstrAttrib);
	}

	static void ValidateSettlInstructionsData(Fix50Context context, FixCheck check, ISettlInstructionsData block)
	{
		if (block.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, check, block.SettlDeliveryType);
		if (block.StandInstDbType is not null) context.Validators.StandInstDbType(context, check, block.StandInstDbType);
		if (block.StandInstDbName is not null) context.Validators.StandInstDbName(context, check, block.StandInstDbName);
		if (block.StandInstDbID is not null) context.Validators.StandInstDbID(context, check, block.StandInstDbID);
		if (block.NoDlvyInst is not null) context.Validators.NoDlvyInst(context, check, block.NoDlvyInst);
		Counted(check, block.NoDlvyInst, block.NoDlvyInstGroups);
		Each(context, check, FixTag.NoDlvyInst, block.NoDlvyInstGroups, context.Validators.SettlInstructionsData_NoDlvyInst);
	}

	static void ValidateSettlParties(Fix50Context context, FixCheck check, ISettlParties block)
	{
		if (block.NoSettlPartyIDs is not null) context.Validators.NoSettlPartyIDs(context, check, block.NoSettlPartyIDs);
		Counted(check, block.NoSettlPartyIDs, block.NoSettlPartyIDsGroups);
		Each(context, check, FixTag.NoSettlPartyIDs, block.NoSettlPartyIDsGroups, context.Validators.SettlParties_NoSettlPartyIDs);
	}

	static void ValidateSideTrdRegTS(Fix50Context context, FixCheck check, ISideTrdRegTS block)
	{
		if (block.NoSideTrdRegTS is not null) context.Validators.NoSideTrdRegTS(context, check, block.NoSideTrdRegTS);
		Counted(check, block.NoSideTrdRegTS, block.NoSideTrdRegTSGroups);
		Each(context, check, FixTag.NoSideTrdRegTS, block.NoSideTrdRegTSGroups, context.Validators.SideTrdRegTS_NoSideTrdRegTS);
	}

	static void ValidateSpreadOrBenchmarkCurveData(Fix50Context context, FixCheck check, ISpreadOrBenchmarkCurveData block)
	{
		if (block.Spread is not null) context.Validators.Spread(context, check, block.Spread);
		if (block.BenchmarkCurveCurrency is not null) context.Validators.BenchmarkCurveCurrency(context, check, block.BenchmarkCurveCurrency);
		if (block.BenchmarkCurveName is not null) context.Validators.BenchmarkCurveName(context, check, block.BenchmarkCurveName);
		if (block.BenchmarkCurvePoint is not null) context.Validators.BenchmarkCurvePoint(context, check, block.BenchmarkCurvePoint);
		if (block.BenchmarkPrice is not null) context.Validators.BenchmarkPrice(context, check, block.BenchmarkPrice);
		if (block.BenchmarkPriceType is not null) context.Validators.BenchmarkPriceType(context, check, block.BenchmarkPriceType);
		if (block.BenchmarkSecurityID is not null) context.Validators.BenchmarkSecurityID(context, check, block.BenchmarkSecurityID);
		if (block.BenchmarkSecurityIDSource is not null) context.Validators.BenchmarkSecurityIDSource(context, check, block.BenchmarkSecurityIDSource);
	}

	static void ValidateStipulations(Fix50Context context, FixCheck check, IStipulations block)
	{
		if (block.NoStipulations is not null) context.Validators.NoStipulations(context, check, block.NoStipulations);
		Counted(check, block.NoStipulations, block.NoStipulationsGroups);
		Each(context, check, FixTag.NoStipulations, block.NoStipulationsGroups, context.Validators.Stipulations_NoStipulations);
	}

	static void ValidateTargetParties(Fix50Context context, FixCheck check, ITargetParties block)
	{
		if (block.NoTargetPartyIDs is not null) context.Validators.NoTargetPartyIDs(context, check, block.NoTargetPartyIDs);
		Counted(check, block.NoTargetPartyIDs, block.NoTargetPartyIDsGroups);
		Each(context, check, FixTag.NoTargetPartyIDs, block.NoTargetPartyIDsGroups, context.Validators.TargetParties_NoTargetPartyIDs);
	}

	static void ValidateTrdRegTimestamps(Fix50Context context, FixCheck check, ITrdRegTimestamps block)
	{
		if (block.NoTrdRegTimestamps is not null) context.Validators.NoTrdRegTimestamps(context, check, block.NoTrdRegTimestamps);
		Counted(check, block.NoTrdRegTimestamps, block.NoTrdRegTimestampsGroups);
		Each(context, check, FixTag.NoTrdRegTimestamps, block.NoTrdRegTimestampsGroups, context.Validators.TrdRegTimestamps_NoTrdRegTimestamps);
	}

	static void ValidateTriggeringInstruction(Fix50Context context, FixCheck check, ITriggeringInstruction block)
	{
		if (block.TriggerType is not null) context.Validators.TriggerType(context, check, block.TriggerType);
		if (block.TriggerAction is not null) context.Validators.TriggerAction(context, check, block.TriggerAction);
		if (block.TriggerPrice is not null) context.Validators.TriggerPrice(context, check, block.TriggerPrice);
		if (block.TriggerSymbol is not null) context.Validators.TriggerSymbol(context, check, block.TriggerSymbol);
		if (block.TriggerSecurityID is not null) context.Validators.TriggerSecurityID(context, check, block.TriggerSecurityID);
		if (block.TriggerSecurityIDSource is not null) context.Validators.TriggerSecurityIDSource(context, check, block.TriggerSecurityIDSource);
		if (block.TriggerSecurityDesc is not null) context.Validators.TriggerSecurityDesc(context, check, block.TriggerSecurityDesc);
		if (block.TriggerPriceType is not null) context.Validators.TriggerPriceType(context, check, block.TriggerPriceType);
		if (block.TriggerPriceTypeScope is not null) context.Validators.TriggerPriceTypeScope(context, check, block.TriggerPriceTypeScope);
		if (block.TriggerPriceDirection is not null) context.Validators.TriggerPriceDirection(context, check, block.TriggerPriceDirection);
		if (block.TriggerNewPrice is not null) context.Validators.TriggerNewPrice(context, check, block.TriggerNewPrice);
		if (block.TriggerOrderType is not null) context.Validators.TriggerOrderType(context, check, block.TriggerOrderType);
		if (block.TriggerNewQty is not null) context.Validators.TriggerNewQty(context, check, block.TriggerNewQty);
		if (block.TriggerTradingSessionID is not null) context.Validators.TriggerTradingSessionID(context, check, block.TriggerTradingSessionID);
		if (block.TriggerTradingSessionSubID is not null) context.Validators.TriggerTradingSessionSubID(context, check, block.TriggerTradingSessionSubID);
	}

	static void ValidateUnderlyingAmount(Fix50Context context, FixCheck check, IUnderlyingAmount block)
	{
		if (block.NoUnderlyingAmounts is not null) context.Validators.NoUnderlyingAmounts(context, check, block.NoUnderlyingAmounts);
		Counted(check, block.NoUnderlyingAmounts, block.NoUnderlyingAmountsGroups);
		Each(context, check, FixTag.NoUnderlyingAmounts, block.NoUnderlyingAmountsGroups, context.Validators.UnderlyingAmount_NoUnderlyingAmounts);
	}

	static void ValidateUnderlyingInstrument(Fix50Context context, FixCheck check, IUnderlyingInstrument block)
	{
		if (block.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, check, block.UnderlyingSymbol);
		if (block.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, check, block.UnderlyingSymbolSfx);
		if (block.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, check, block.UnderlyingSecurityID);
		if (block.UnderlyingSecurityIDSource is not null) context.Validators.UnderlyingSecurityIDSource(context, check, block.UnderlyingSecurityIDSource);
		if (block.NoUnderlyingSecurityAltID is not null) context.Validators.NoUnderlyingSecurityAltID(context, check, block.NoUnderlyingSecurityAltID);
		Counted(check, block.NoUnderlyingSecurityAltID, block.NoUnderlyingSecurityAltIDGroups);
		Each(context, check, FixTag.NoUnderlyingSecurityAltID, block.NoUnderlyingSecurityAltIDGroups, context.Validators.UnderlyingInstrument_NoUnderlyingSecurityAltID);
		if (block.UnderlyingProduct is not null) context.Validators.UnderlyingProduct(context, check, block.UnderlyingProduct);
		if (block.UnderlyingCFICode is not null) context.Validators.UnderlyingCFICode(context, check, block.UnderlyingCFICode);
		if (block.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, check, block.UnderlyingSecurityType);
		if (block.UnderlyingSecuritySubType is not null) context.Validators.UnderlyingSecuritySubType(context, check, block.UnderlyingSecuritySubType);
		if (block.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, check, block.UnderlyingMaturityMonthYear);
		if (block.UnderlyingMaturityDate is not null) context.Validators.UnderlyingMaturityDate(context, check, block.UnderlyingMaturityDate);
		if (block.UnderlyingMaturityTime is not null) context.Validators.UnderlyingMaturityTime(context, check, block.UnderlyingMaturityTime);
		if (block.UnderlyingCouponPaymentDate is not null) context.Validators.UnderlyingCouponPaymentDate(context, check, block.UnderlyingCouponPaymentDate);
		if (block.UnderlyingRestructuringType is not null) context.Validators.UnderlyingRestructuringType(context, check, block.UnderlyingRestructuringType);
		if (block.UnderlyingSeniority is not null) context.Validators.UnderlyingSeniority(context, check, block.UnderlyingSeniority);
		if (block.UnderlyingNotionalPercentageOutstanding is not null) context.Validators.UnderlyingNotionalPercentageOutstanding(context, check, block.UnderlyingNotionalPercentageOutstanding);
		if (block.UnderlyingOriginalNotionalPercentageOutstanding is not null) context.Validators.UnderlyingOriginalNotionalPercentageOutstanding(context, check, block.UnderlyingOriginalNotionalPercentageOutstanding);
		if (block.UnderlyingAttachmentPoint is not null) context.Validators.UnderlyingAttachmentPoint(context, check, block.UnderlyingAttachmentPoint);
		if (block.UnderlyingDetachmentPoint is not null) context.Validators.UnderlyingDetachmentPoint(context, check, block.UnderlyingDetachmentPoint);
		if (block.UnderlyingIssueDate is not null) context.Validators.UnderlyingIssueDate(context, check, block.UnderlyingIssueDate);
		if (block.UnderlyingRepoCollateralSecurityType is not null) context.Validators.UnderlyingRepoCollateralSecurityType(context, check, block.UnderlyingRepoCollateralSecurityType);
		if (block.UnderlyingRepurchaseTerm is not null) context.Validators.UnderlyingRepurchaseTerm(context, check, block.UnderlyingRepurchaseTerm);
		if (block.UnderlyingRepurchaseRate is not null) context.Validators.UnderlyingRepurchaseRate(context, check, block.UnderlyingRepurchaseRate);
		if (block.UnderlyingFactor is not null) context.Validators.UnderlyingFactor(context, check, block.UnderlyingFactor);
		if (block.UnderlyingCreditRating is not null) context.Validators.UnderlyingCreditRating(context, check, block.UnderlyingCreditRating);
		if (block.UnderlyingInstrRegistry is not null) context.Validators.UnderlyingInstrRegistry(context, check, block.UnderlyingInstrRegistry);
		if (block.UnderlyingCountryOfIssue is not null) context.Validators.UnderlyingCountryOfIssue(context, check, block.UnderlyingCountryOfIssue);
		if (block.UnderlyingStateOrProvinceOfIssue is not null) context.Validators.UnderlyingStateOrProvinceOfIssue(context, check, block.UnderlyingStateOrProvinceOfIssue);
		if (block.UnderlyingLocaleOfIssue is not null) context.Validators.UnderlyingLocaleOfIssue(context, check, block.UnderlyingLocaleOfIssue);
		if (block.UnderlyingRedemptionDate is not null) context.Validators.UnderlyingRedemptionDate(context, check, block.UnderlyingRedemptionDate);
		if (block.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, check, block.UnderlyingStrikePrice);
		if (block.UnderlyingStrikeCurrency is not null) context.Validators.UnderlyingStrikeCurrency(context, check, block.UnderlyingStrikeCurrency);
		if (block.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, check, block.UnderlyingOptAttribute);
		if (block.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, check, block.UnderlyingContractMultiplier);
		if (block.UnderlyingContractMultiplierUnit is not null) context.Validators.UnderlyingContractMultiplierUnit(context, check, block.UnderlyingContractMultiplierUnit);
		if (block.UnderlyingFlowScheduleType is not null) context.Validators.UnderlyingFlowScheduleType(context, check, block.UnderlyingFlowScheduleType);
		if (block.UnderlyingUnitOfMeasure is not null) context.Validators.UnderlyingUnitOfMeasure(context, check, block.UnderlyingUnitOfMeasure);
		if (block.UnderlyingUnitOfMeasureQty is not null) context.Validators.UnderlyingUnitOfMeasureQty(context, check, block.UnderlyingUnitOfMeasureQty);
		if (block.UnderlyingPriceUnitOfMeasure is not null) context.Validators.UnderlyingPriceUnitOfMeasure(context, check, block.UnderlyingPriceUnitOfMeasure);
		if (block.UnderlyingPriceUnitOfMeasureQty is not null) context.Validators.UnderlyingPriceUnitOfMeasureQty(context, check, block.UnderlyingPriceUnitOfMeasureQty);
		if (block.UnderlyingTimeUnit is not null) context.Validators.UnderlyingTimeUnit(context, check, block.UnderlyingTimeUnit);
		if (block.UnderlyingExerciseStyle is not null) context.Validators.UnderlyingExerciseStyle(context, check, block.UnderlyingExerciseStyle);
		if (block.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, check, block.UnderlyingCouponRate);
		if (block.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, check, block.UnderlyingSecurityExchange);
		if (block.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, check, block.UnderlyingIssuer);
		if (block.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, check, block.EncodedUnderlyingIssuerLen);
		if (block.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, check, block.EncodedUnderlyingIssuer);
		if (block.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, check, block.UnderlyingSecurityDesc);
		if (block.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, check, block.EncodedUnderlyingSecurityDescLen);
		if (block.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, check, block.EncodedUnderlyingSecurityDesc);
		if (block.UnderlyingCPProgram is not null) context.Validators.UnderlyingCPProgram(context, check, block.UnderlyingCPProgram);
		if (block.UnderlyingCPRegType is not null) context.Validators.UnderlyingCPRegType(context, check, block.UnderlyingCPRegType);
		if (block.UnderlyingAllocationPercent is not null) context.Validators.UnderlyingAllocationPercent(context, check, block.UnderlyingAllocationPercent);
		if (block.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, check, block.UnderlyingCurrency);
		if (block.UnderlyingQty is not null) context.Validators.UnderlyingQty(context, check, block.UnderlyingQty);
		if (block.UnderlyingSettlementType is not null) context.Validators.UnderlyingSettlementType(context, check, block.UnderlyingSettlementType);
		if (block.UnderlyingCashAmount is not null) context.Validators.UnderlyingCashAmount(context, check, block.UnderlyingCashAmount);
		if (block.UnderlyingCashType is not null) context.Validators.UnderlyingCashType(context, check, block.UnderlyingCashType);
		if (block.UnderlyingPx is not null) context.Validators.UnderlyingPx(context, check, block.UnderlyingPx);
		if (block.UnderlyingDirtyPrice is not null) context.Validators.UnderlyingDirtyPrice(context, check, block.UnderlyingDirtyPrice);
		if (block.UnderlyingEndPrice is not null) context.Validators.UnderlyingEndPrice(context, check, block.UnderlyingEndPrice);
		if (block.UnderlyingStartValue is not null) context.Validators.UnderlyingStartValue(context, check, block.UnderlyingStartValue);
		if (block.UnderlyingCurrentValue is not null) context.Validators.UnderlyingCurrentValue(context, check, block.UnderlyingCurrentValue);
		if (block.UnderlyingEndValue is not null) context.Validators.UnderlyingEndValue(context, check, block.UnderlyingEndValue);
		if (!Empty((IUnderlyingStipulations)block)) context.Validators.UnderlyingStipulations(context, check, block);
		if (block.UnderlyingAdjustedQuantity is not null) context.Validators.UnderlyingAdjustedQuantity(context, check, block.UnderlyingAdjustedQuantity);
		if (block.UnderlyingFXRate is not null) context.Validators.UnderlyingFXRate(context, check, block.UnderlyingFXRate);
		if (block.UnderlyingFXRateCalc is not null) context.Validators.UnderlyingFXRateCalc(context, check, block.UnderlyingFXRateCalc);
		if (block.UnderlyingCapValue is not null) context.Validators.UnderlyingCapValue(context, check, block.UnderlyingCapValue);
		if (!Empty((IUndlyInstrumentParties)block)) context.Validators.UndlyInstrumentParties(context, check, block);
		if (block.UnderlyingSettlMethod is not null) context.Validators.UnderlyingSettlMethod(context, check, block.UnderlyingSettlMethod);
		if (block.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, block.UnderlyingPutOrCall);
	}

	static void ValidateUnderlyingStipulations(Fix50Context context, FixCheck check, IUnderlyingStipulations block)
	{
		if (block.NoUnderlyingStips is not null) context.Validators.NoUnderlyingStips(context, check, block.NoUnderlyingStips);
		Counted(check, block.NoUnderlyingStips, block.NoUnderlyingStipsGroups);
		Each(context, check, FixTag.NoUnderlyingStips, block.NoUnderlyingStipsGroups, context.Validators.UnderlyingStipulations_NoUnderlyingStips);
	}

	static void ValidateUndlyInstrumentParties(Fix50Context context, FixCheck check, IUndlyInstrumentParties block)
	{
		if (block.NoUndlyInstrumentParties is not null) context.Validators.NoUndlyInstrumentParties(context, check, block.NoUndlyInstrumentParties);
		Counted(check, block.NoUndlyInstrumentParties, block.NoUndlyInstrumentPartiesGroups);
		Each(context, check, FixTag.NoUndlyInstrumentParties, block.NoUndlyInstrumentPartiesGroups, context.Validators.UndlyInstrumentParties_NoUndlyInstrumentParties);
	}

	static void ValidateYieldData(Fix50Context context, FixCheck check, IYieldData block)
	{
		if (block.YieldType is not null) context.Validators.YieldType(context, check, block.YieldType);
		if (block.Yield is not null) context.Validators.Yield(context, check, block.Yield);
		if (block.YieldCalcDate is not null) context.Validators.YieldCalcDate(context, check, block.YieldCalcDate);
		if (block.YieldRedemptionDate is not null) context.Validators.YieldRedemptionDate(context, check, block.YieldRedemptionDate);
		if (block.YieldRedemptionPrice is not null) context.Validators.YieldRedemptionPrice(context, check, block.YieldRedemptionPrice);
		if (block.YieldRedemptionPriceType is not null) context.Validators.YieldRedemptionPriceType(context, check, block.YieldRedemptionPriceType);
	}

	static void ValidateExpirationQty_NoExpiration(Fix50Context context, FixCheck check, IExpirationQty.NoExpirationGroup entry)
	{
		context.Validators.ExpirationQtyType(context, check, entry.ExpirationQtyType);
		if (entry.ExpQty is not null) context.Validators.ExpQty(context, check, entry.ExpQty);
	}

	static void ValidateInstrument_NoSecurityAltID(Fix50Context context, FixCheck check, IInstrument.NoSecurityAltIDGroup entry)
	{
		context.Validators.SecurityAltID(context, check, entry.SecurityAltID);
		if (entry.SecurityAltIDSource is not null) context.Validators.SecurityAltIDSource(context, check, entry.SecurityAltIDSource);
	}

	static void ValidateInstrument_NoEvents(Fix50Context context, FixCheck check, IInstrument.NoEventsGroup entry)
	{
		context.Validators.EventType(context, check, entry.EventType);
		if (entry.EventDate is not null) context.Validators.EventDate(context, check, entry.EventDate);
		if (entry.EventTime is not null) context.Validators.EventTime(context, check, entry.EventTime);
		if (entry.EventPx is not null) context.Validators.EventPx(context, check, entry.EventPx);
		if (entry.EventText is not null) context.Validators.EventText(context, check, entry.EventText);
	}

	static void ValidateInstrument_NoComplexEvents(Fix50Context context, FixCheck check, IInstrument.NoComplexEventsGroup entry)
	{
		context.Validators.ComplexEventType(context, check, entry.ComplexEventType);
		if (entry.ComplexOptPayoutAmount is not null) context.Validators.ComplexOptPayoutAmount(context, check, entry.ComplexOptPayoutAmount);
		if (entry.ComplexEventPrice is not null) context.Validators.ComplexEventPrice(context, check, entry.ComplexEventPrice);
		if (entry.ComplexEventPriceBoundaryMethod is not null) context.Validators.ComplexEventPriceBoundaryMethod(context, check, entry.ComplexEventPriceBoundaryMethod);
		if (entry.ComplexEventPriceBoundaryPrecision is not null) context.Validators.ComplexEventPriceBoundaryPrecision(context, check, entry.ComplexEventPriceBoundaryPrecision);
		if (entry.ComplexEventPriceTimeType is not null) context.Validators.ComplexEventPriceTimeType(context, check, entry.ComplexEventPriceTimeType);
		if (entry.ComplexEventCondition is not null) context.Validators.ComplexEventCondition(context, check, entry.ComplexEventCondition);
		if (entry.NoComplexEventDates is not null) context.Validators.NoComplexEventDates(context, check, entry.NoComplexEventDates);
		Counted(check, entry.NoComplexEventDates, entry.NoComplexEventDatesGroups);
		Each(context, check, FixTag.NoComplexEventDates, entry.NoComplexEventDatesGroups, context.Validators.Instrument_NoComplexEvents_NoComplexEventDates);
	}

	static void ValidateInstrument_NoComplexEvents_NoComplexEventDates(Fix50Context context, FixCheck check, IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup entry)
	{
		context.Validators.ComplexEventStartDate(context, check, entry.ComplexEventStartDate);
		if (entry.ComplexEventEndDate is not null) context.Validators.ComplexEventEndDate(context, check, entry.ComplexEventEndDate);
		if (entry.NoComplexEventTimes is not null) context.Validators.NoComplexEventTimes(context, check, entry.NoComplexEventTimes);
		Counted(check, entry.NoComplexEventTimes, entry.NoComplexEventTimesGroups);
		Each(context, check, FixTag.NoComplexEventTimes, entry.NoComplexEventTimesGroups, context.Validators.Instrument_NoComplexEvents_NoComplexEventDates_NoComplexEventTimes);
	}

	static void ValidateInstrument_NoComplexEvents_NoComplexEventDates_NoComplexEventTimes(Fix50Context context, FixCheck check, IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup.NoComplexEventTimesGroup entry)
	{
		context.Validators.ComplexEventStartTime(context, check, entry.ComplexEventStartTime);
		if (entry.ComplexEventEndTime is not null) context.Validators.ComplexEventEndTime(context, check, entry.ComplexEventEndTime);
	}

	static void ValidateInstrumentExtension_NoInstrAttrib(Fix50Context context, FixCheck check, IInstrumentExtension.NoInstrAttribGroup entry)
	{
		context.Validators.InstrAttribType(context, check, entry.InstrAttribType);
		if (entry.InstrAttribValue is not null) context.Validators.InstrAttribValue(context, check, entry.InstrAttribValue);
	}

	static void ValidateInstrumentLeg_NoLegSecurityAltID(Fix50Context context, FixCheck check, IInstrumentLeg.NoLegSecurityAltIDGroup entry)
	{
		context.Validators.LegSecurityAltID(context, check, entry.LegSecurityAltID);
		if (entry.LegSecurityAltIDSource is not null) context.Validators.LegSecurityAltIDSource(context, check, entry.LegSecurityAltIDSource);
	}

	static void ValidateInstrumentParties_NoInstrumentParties(Fix50Context context, FixCheck check, IInstrumentParties.NoInstrumentPartiesGroup entry)
	{
		context.Validators.InstrumentPartyID(context, check, entry.InstrumentPartyID);
		if (entry.InstrumentPartyIDSource is not null) context.Validators.InstrumentPartyIDSource(context, check, entry.InstrumentPartyIDSource);
		if (entry.InstrumentPartyRole is not null) context.Validators.InstrumentPartyRole(context, check, entry.InstrumentPartyRole);
		if (entry.NoInstrumentPartySubIDs is not null) context.Validators.NoInstrumentPartySubIDs(context, check, entry.NoInstrumentPartySubIDs);
		Counted(check, entry.NoInstrumentPartySubIDs, entry.NoInstrumentPartySubIDsGroups);
		Each(context, check, FixTag.NoInstrumentPartySubIDs, entry.NoInstrumentPartySubIDsGroups, context.Validators.InstrumentParties_NoInstrumentParties_NoInstrumentPartySubIDs);
	}

	static void ValidateInstrumentParties_NoInstrumentParties_NoInstrumentPartySubIDs(Fix50Context context, FixCheck check, IInstrumentParties.NoInstrumentPartiesGroup.NoInstrumentPartySubIDsGroup entry)
	{
		context.Validators.InstrumentPartySubID(context, check, entry.InstrumentPartySubID);
		if (entry.InstrumentPartySubIDType is not null) context.Validators.InstrumentPartySubIDType(context, check, entry.InstrumentPartySubIDType);
	}

	static void ValidateLegStipulations_NoLegStipulations(Fix50Context context, FixCheck check, ILegStipulations.NoLegStipulationsGroup entry)
	{
		context.Validators.LegStipulationType(context, check, entry.LegStipulationType);
		if (entry.LegStipulationValue is not null) context.Validators.LegStipulationValue(context, check, entry.LegStipulationValue);
	}

	static void ValidateNestedParties_NoNestedPartyIDs(Fix50Context context, FixCheck check, INestedParties.NoNestedPartyIDsGroup entry)
	{
		context.Validators.NestedPartyID(context, check, entry.NestedPartyID);
		if (entry.NestedPartyIDSource is not null) context.Validators.NestedPartyIDSource(context, check, entry.NestedPartyIDSource);
		if (entry.NestedPartyRole is not null) context.Validators.NestedPartyRole(context, check, entry.NestedPartyRole);
		if (entry.NoNestedPartySubIDs is not null) context.Validators.NoNestedPartySubIDs(context, check, entry.NoNestedPartySubIDs);
		Counted(check, entry.NoNestedPartySubIDs, entry.NoNestedPartySubIDsGroups);
		Each(context, check, FixTag.NoNestedPartySubIDs, entry.NoNestedPartySubIDsGroups, context.Validators.NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs);
	}

	static void ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs(Fix50Context context, FixCheck check, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup entry)
	{
		context.Validators.NestedPartySubID(context, check, entry.NestedPartySubID);
		if (entry.NestedPartySubIDType is not null) context.Validators.NestedPartySubIDType(context, check, entry.NestedPartySubIDType);
	}

	static void ValidateNestedParties2_NoNested2PartyIDs(Fix50Context context, FixCheck check, INestedParties2.NoNested2PartyIDsGroup entry)
	{
		context.Validators.Nested2PartyID(context, check, entry.Nested2PartyID);
		if (entry.Nested2PartyIDSource is not null) context.Validators.Nested2PartyIDSource(context, check, entry.Nested2PartyIDSource);
		if (entry.Nested2PartyRole is not null) context.Validators.Nested2PartyRole(context, check, entry.Nested2PartyRole);
		if (entry.NoNested2PartySubIDs is not null) context.Validators.NoNested2PartySubIDs(context, check, entry.NoNested2PartySubIDs);
		Counted(check, entry.NoNested2PartySubIDs, entry.NoNested2PartySubIDsGroups);
		Each(context, check, FixTag.NoNested2PartySubIDs, entry.NoNested2PartySubIDsGroups, context.Validators.NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs);
	}

	static void ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs(Fix50Context context, FixCheck check, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup entry)
	{
		context.Validators.Nested2PartySubID(context, check, entry.Nested2PartySubID);
		if (entry.Nested2PartySubIDType is not null) context.Validators.Nested2PartySubIDType(context, check, entry.Nested2PartySubIDType);
	}

	static void ValidateNestedParties3_NoNested3PartyIDs(Fix50Context context, FixCheck check, INestedParties3.NoNested3PartyIDsGroup entry)
	{
		context.Validators.Nested3PartyID(context, check, entry.Nested3PartyID);
		if (entry.Nested3PartyIDSource is not null) context.Validators.Nested3PartyIDSource(context, check, entry.Nested3PartyIDSource);
		if (entry.Nested3PartyRole is not null) context.Validators.Nested3PartyRole(context, check, entry.Nested3PartyRole);
		if (entry.NoNested3PartySubIDs is not null) context.Validators.NoNested3PartySubIDs(context, check, entry.NoNested3PartySubIDs);
		Counted(check, entry.NoNested3PartySubIDs, entry.NoNested3PartySubIDsGroups);
		Each(context, check, FixTag.NoNested3PartySubIDs, entry.NoNested3PartySubIDsGroups, context.Validators.NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs);
	}

	static void ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs(Fix50Context context, FixCheck check, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup entry)
	{
		context.Validators.Nested3PartySubID(context, check, entry.Nested3PartySubID);
		if (entry.Nested3PartySubIDType is not null) context.Validators.Nested3PartySubIDType(context, check, entry.Nested3PartySubIDType);
	}

	static void ValidateNestedParties4_NoNested4PartyIDs(Fix50Context context, FixCheck check, INestedParties4.NoNested4PartyIDsGroup entry)
	{
		context.Validators.Nested4PartyID(context, check, entry.Nested4PartyID);
		if (entry.Nested4PartyIDSource is not null) context.Validators.Nested4PartyIDSource(context, check, entry.Nested4PartyIDSource);
		if (entry.Nested4PartyRole is not null) context.Validators.Nested4PartyRole(context, check, entry.Nested4PartyRole);
		if (entry.NoNested4PartySubIDs is not null) context.Validators.NoNested4PartySubIDs(context, check, entry.NoNested4PartySubIDs);
		Counted(check, entry.NoNested4PartySubIDs, entry.NoNested4PartySubIDsGroups);
		Each(context, check, FixTag.NoNested4PartySubIDs, entry.NoNested4PartySubIDsGroups, context.Validators.NestedParties4_NoNested4PartyIDs_NoNested4PartySubIDs);
	}

	static void ValidateNestedParties4_NoNested4PartyIDs_NoNested4PartySubIDs(Fix50Context context, FixCheck check, INestedParties4.NoNested4PartyIDsGroup.NoNested4PartySubIDsGroup entry)
	{
		context.Validators.Nested4PartySubID(context, check, entry.Nested4PartySubID);
		if (entry.Nested4PartySubIDType is not null) context.Validators.Nested4PartySubIDType(context, check, entry.Nested4PartySubIDType);
	}

	static void ValidateParties_NoPartyIDs(Fix50Context context, FixCheck check, IParties.NoPartyIDsGroup entry)
	{
		context.Validators.PartyID(context, check, entry.PartyID);
		if (entry.PartyIDSource is not null) context.Validators.PartyIDSource(context, check, entry.PartyIDSource);
		if (entry.PartyRole is not null) context.Validators.PartyRole(context, check, entry.PartyRole);
		if (entry.NoPartySubIDs is not null) context.Validators.NoPartySubIDs(context, check, entry.NoPartySubIDs);
		Counted(check, entry.NoPartySubIDs, entry.NoPartySubIDsGroups);
		Each(context, check, FixTag.NoPartySubIDs, entry.NoPartySubIDsGroups, context.Validators.Parties_NoPartyIDs_NoPartySubIDs);
	}

	static void ValidateParties_NoPartyIDs_NoPartySubIDs(Fix50Context context, FixCheck check, IParties.NoPartyIDsGroup.NoPartySubIDsGroup entry)
	{
		context.Validators.PartySubID(context, check, entry.PartySubID);
		if (entry.PartySubIDType is not null) context.Validators.PartySubIDType(context, check, entry.PartySubIDType);
	}

	static void ValidatePositionAmountData_NoPosAmt(Fix50Context context, FixCheck check, IPositionAmountData.NoPosAmtGroup entry)
	{
		context.Validators.PosAmtType(context, check, entry.PosAmtType);
		if (entry.PosAmt is not null) context.Validators.PosAmt(context, check, entry.PosAmt);
		if (entry.PositionCurrency is not null) context.Validators.PositionCurrency(context, check, entry.PositionCurrency);
	}

	static void ValidatePositionQty_NoPositions(Fix50Context context, FixCheck check, IPositionQty.NoPositionsGroup entry)
	{
		context.Validators.PosType(context, check, entry.PosType);
		if (entry.LongQty is not null) context.Validators.LongQty(context, check, entry.LongQty);
		if (entry.ShortQty is not null) context.Validators.ShortQty(context, check, entry.ShortQty);
		if (entry.PosQtyStatus is not null) context.Validators.PosQtyStatus(context, check, entry.PosQtyStatus);
		if (entry.QuantityDate is not null) context.Validators.QuantityDate(context, check, entry.QuantityDate);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateRateSource_NoRateSources(Fix50Context context, FixCheck check, IRateSource.NoRateSourcesGroup entry)
	{
		context.Validators.RateSourceField(context, check, entry.RateSource);
		if (entry.RateSourceType is not null) context.Validators.RateSourceType(context, check, entry.RateSourceType);
		if (entry.ReferencePage is not null) context.Validators.ReferencePage(context, check, entry.ReferencePage);
	}

	static void ValidateRootParties_NoRootPartyIDs(Fix50Context context, FixCheck check, IRootParties.NoRootPartyIDsGroup entry)
	{
		context.Validators.RootPartyID(context, check, entry.RootPartyID);
		if (entry.RootPartyIDSource is not null) context.Validators.RootPartyIDSource(context, check, entry.RootPartyIDSource);
		if (entry.RootPartyRole is not null) context.Validators.RootPartyRole(context, check, entry.RootPartyRole);
		if (entry.NoRootPartySubIDs is not null) context.Validators.NoRootPartySubIDs(context, check, entry.NoRootPartySubIDs);
		Counted(check, entry.NoRootPartySubIDs, entry.NoRootPartySubIDsGroups);
		Each(context, check, FixTag.NoRootPartySubIDs, entry.NoRootPartySubIDsGroups, context.Validators.RootParties_NoRootPartyIDs_NoRootPartySubIDs);
	}

	static void ValidateRootParties_NoRootPartyIDs_NoRootPartySubIDs(Fix50Context context, FixCheck check, IRootParties.NoRootPartyIDsGroup.NoRootPartySubIDsGroup entry)
	{
		context.Validators.RootPartySubID(context, check, entry.RootPartySubID);
		if (entry.RootPartySubIDType is not null) context.Validators.RootPartySubIDType(context, check, entry.RootPartySubIDType);
	}

	static void ValidateSecurityTradingRules_NoTickRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTickRulesGroup entry)
	{
		context.Validators.StartTickPriceRange(context, check, entry.StartTickPriceRange);
		if (entry.EndTickPriceRange is not null) context.Validators.EndTickPriceRange(context, check, entry.EndTickPriceRange);
		if (entry.TickIncrement is not null) context.Validators.TickIncrement(context, check, entry.TickIncrement);
		if (entry.TickRuleType is not null) context.Validators.TickRuleType(context, check, entry.TickRuleType);
	}

	static void ValidateSecurityTradingRules_NoLotTypeRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoLotTypeRulesGroup entry)
	{
		context.Validators.LotType(context, check, entry.LotType);
		if (entry.MinLotSize is not null) context.Validators.MinLotSize(context, check, entry.MinLotSize);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.NoOrdTypeRules is not null) context.Validators.NoOrdTypeRules(context, check, entry.NoOrdTypeRules);
		Counted(check, entry.NoOrdTypeRules, entry.NoOrdTypeRulesGroups);
		Each(context, check, FixTag.NoOrdTypeRules, entry.NoOrdTypeRulesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules_NoOrdTypeRules);
		if (entry.NoTimeInForceRules is not null) context.Validators.NoTimeInForceRules(context, check, entry.NoTimeInForceRules);
		Counted(check, entry.NoTimeInForceRules, entry.NoTimeInForceRulesGroups);
		Each(context, check, FixTag.NoTimeInForceRules, entry.NoTimeInForceRulesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules_NoTimeInForceRules);
		if (entry.NoExecInstRules is not null) context.Validators.NoExecInstRules(context, check, entry.NoExecInstRules);
		Counted(check, entry.NoExecInstRules, entry.NoExecInstRulesGroups);
		Each(context, check, FixTag.NoExecInstRules, entry.NoExecInstRulesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules_NoExecInstRules);
		if (entry.NoMatchRules is not null) context.Validators.NoMatchRules(context, check, entry.NoMatchRules);
		Counted(check, entry.NoMatchRules, entry.NoMatchRulesGroups);
		Each(context, check, FixTag.NoMatchRules, entry.NoMatchRulesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules_NoMatchRules);
		if (entry.NoMDFeedTypes is not null) context.Validators.NoMDFeedTypes(context, check, entry.NoMDFeedTypes);
		Counted(check, entry.NoMDFeedTypes, entry.NoMDFeedTypesGroups);
		Each(context, check, FixTag.NoMDFeedTypes, entry.NoMDFeedTypesGroups, context.Validators.SecurityTradingRules_NoTradingSessionRules_NoMDFeedTypes);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules_NoOrdTypeRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup.NoOrdTypeRulesGroup entry)
	{
		context.Validators.OrdType(context, check, entry.OrdType);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules_NoTimeInForceRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup.NoTimeInForceRulesGroup entry)
	{
		context.Validators.TimeInForce(context, check, entry.TimeInForce);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules_NoExecInstRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup.NoExecInstRulesGroup entry)
	{
		context.Validators.ExecInstValue(context, check, entry.ExecInstValue);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules_NoMatchRules(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup.NoMatchRulesGroup entry)
	{
		context.Validators.MatchAlgorithm(context, check, entry.MatchAlgorithm);
		if (entry.MatchType is not null) context.Validators.MatchType(context, check, entry.MatchType);
	}

	static void ValidateSecurityTradingRules_NoTradingSessionRules_NoMDFeedTypes(Fix50Context context, FixCheck check, ISecurityTradingRules.NoTradingSessionRulesGroup.NoMDFeedTypesGroup entry)
	{
		context.Validators.MDFeedType(context, check, entry.MDFeedType);
		if (entry.MarketDepth is not null) context.Validators.MarketDepth(context, check, entry.MarketDepth);
		if (entry.MDBookType is not null) context.Validators.MDBookType(context, check, entry.MDBookType);
	}

	static void ValidateSecurityTradingRules_NoNestedInstrAttrib(Fix50Context context, FixCheck check, ISecurityTradingRules.NoNestedInstrAttribGroup entry)
	{
		context.Validators.NestedInstrAttribType(context, check, entry.NestedInstrAttribType);
		if (entry.NestedInstrAttribValue is not null) context.Validators.NestedInstrAttribValue(context, check, entry.NestedInstrAttribValue);
	}

	static void ValidateSettlInstructionsData_NoDlvyInst(Fix50Context context, FixCheck check, ISettlInstructionsData.NoDlvyInstGroup entry)
	{
		context.Validators.SettlInstSource(context, check, entry.SettlInstSource);
		if (entry.DlvyInstType is not null) context.Validators.DlvyInstType(context, check, entry.DlvyInstType);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, check, entry);
	}

	static void ValidateSettlParties_NoSettlPartyIDs(Fix50Context context, FixCheck check, ISettlParties.NoSettlPartyIDsGroup entry)
	{
		context.Validators.SettlPartyID(context, check, entry.SettlPartyID);
		if (entry.SettlPartyIDSource is not null) context.Validators.SettlPartyIDSource(context, check, entry.SettlPartyIDSource);
		if (entry.SettlPartyRole is not null) context.Validators.SettlPartyRole(context, check, entry.SettlPartyRole);
		if (entry.NoSettlPartySubIDs is not null) context.Validators.NoSettlPartySubIDs(context, check, entry.NoSettlPartySubIDs);
		Counted(check, entry.NoSettlPartySubIDs, entry.NoSettlPartySubIDsGroups);
		Each(context, check, FixTag.NoSettlPartySubIDs, entry.NoSettlPartySubIDsGroups, context.Validators.SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs);
	}

	static void ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs(Fix50Context context, FixCheck check, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup entry)
	{
		context.Validators.SettlPartySubID(context, check, entry.SettlPartySubID);
		if (entry.SettlPartySubIDType is not null) context.Validators.SettlPartySubIDType(context, check, entry.SettlPartySubIDType);
	}

	static void ValidateSideTrdRegTS_NoSideTrdRegTS(Fix50Context context, FixCheck check, ISideTrdRegTS.NoSideTrdRegTSGroup entry)
	{
		context.Validators.SideTrdRegTimestamp(context, check, entry.SideTrdRegTimestamp);
		if (entry.SideTrdRegTimestampType is not null) context.Validators.SideTrdRegTimestampType(context, check, entry.SideTrdRegTimestampType);
		if (entry.SideTrdRegTimestampSrc is not null) context.Validators.SideTrdRegTimestampSrc(context, check, entry.SideTrdRegTimestampSrc);
	}

	static void ValidateStipulations_NoStipulations(Fix50Context context, FixCheck check, IStipulations.NoStipulationsGroup entry)
	{
		context.Validators.StipulationType(context, check, entry.StipulationType);
		if (entry.StipulationValue is not null) context.Validators.StipulationValue(context, check, entry.StipulationValue);
	}

	static void ValidateTargetParties_NoTargetPartyIDs(Fix50Context context, FixCheck check, ITargetParties.NoTargetPartyIDsGroup entry)
	{
		context.Validators.TargetPartyID(context, check, entry.TargetPartyID);
		if (entry.TargetPartyIDSource is not null) context.Validators.TargetPartyIDSource(context, check, entry.TargetPartyIDSource);
		if (entry.TargetPartyRole is not null) context.Validators.TargetPartyRole(context, check, entry.TargetPartyRole);
	}

	static void ValidateTrdRegTimestamps_NoTrdRegTimestamps(Fix50Context context, FixCheck check, ITrdRegTimestamps.NoTrdRegTimestampsGroup entry)
	{
		context.Validators.TrdRegTimestamp(context, check, entry.TrdRegTimestamp);
		if (entry.TrdRegTimestampType is not null) context.Validators.TrdRegTimestampType(context, check, entry.TrdRegTimestampType);
		if (entry.TrdRegTimestampOrigin is not null) context.Validators.TrdRegTimestampOrigin(context, check, entry.TrdRegTimestampOrigin);
		if (entry.DeskType is not null) context.Validators.DeskType(context, check, entry.DeskType);
		if (entry.DeskTypeSource is not null) context.Validators.DeskTypeSource(context, check, entry.DeskTypeSource);
		if (entry.DeskOrderHandlingInst is not null) context.Validators.DeskOrderHandlingInst(context, check, entry.DeskOrderHandlingInst);
	}

	static void ValidateUnderlyingAmount_NoUnderlyingAmounts(Fix50Context context, FixCheck check, IUnderlyingAmount.NoUnderlyingAmountsGroup entry)
	{
		context.Validators.UnderlyingPayAmount(context, check, entry.UnderlyingPayAmount);
		if (entry.UnderlyingCollectAmount is not null) context.Validators.UnderlyingCollectAmount(context, check, entry.UnderlyingCollectAmount);
		if (entry.UnderlyingSettlementDate is not null) context.Validators.UnderlyingSettlementDate(context, check, entry.UnderlyingSettlementDate);
		if (entry.UnderlyingSettlementStatus is not null) context.Validators.UnderlyingSettlementStatus(context, check, entry.UnderlyingSettlementStatus);
	}

	static void ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID(Fix50Context context, FixCheck check, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup entry)
	{
		context.Validators.UnderlyingSecurityAltID(context, check, entry.UnderlyingSecurityAltID);
		if (entry.UnderlyingSecurityAltIDSource is not null) context.Validators.UnderlyingSecurityAltIDSource(context, check, entry.UnderlyingSecurityAltIDSource);
	}

	static void ValidateUnderlyingStipulations_NoUnderlyingStips(Fix50Context context, FixCheck check, IUnderlyingStipulations.NoUnderlyingStipsGroup entry)
	{
		context.Validators.UnderlyingStipType(context, check, entry.UnderlyingStipType);
		if (entry.UnderlyingStipValue is not null) context.Validators.UnderlyingStipValue(context, check, entry.UnderlyingStipValue);
	}

	static void ValidateUndlyInstrumentParties_NoUndlyInstrumentParties(Fix50Context context, FixCheck check, IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup entry)
	{
		context.Validators.UnderlyingInstrumentPartyID(context, check, entry.UnderlyingInstrumentPartyID);
		if (entry.UnderlyingInstrumentPartyIDSource is not null) context.Validators.UnderlyingInstrumentPartyIDSource(context, check, entry.UnderlyingInstrumentPartyIDSource);
		if (entry.UnderlyingInstrumentPartyRole is not null) context.Validators.UnderlyingInstrumentPartyRole(context, check, entry.UnderlyingInstrumentPartyRole);
		if (entry.NoUndlyInstrumentPartySubIDs is not null) context.Validators.NoUndlyInstrumentPartySubIDs(context, check, entry.NoUndlyInstrumentPartySubIDs);
		Counted(check, entry.NoUndlyInstrumentPartySubIDs, entry.NoUndlyInstrumentPartySubIDsGroups);
		Each(context, check, FixTag.NoUndlyInstrumentPartySubIDs, entry.NoUndlyInstrumentPartySubIDsGroups, context.Validators.UndlyInstrumentParties_NoUndlyInstrumentParties_NoUndlyInstrumentPartySubIDs);
	}

	static void ValidateUndlyInstrumentParties_NoUndlyInstrumentParties_NoUndlyInstrumentPartySubIDs(Fix50Context context, FixCheck check, IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup.NoUndlyInstrumentPartySubIDsGroup entry)
	{
		context.Validators.UnderlyingInstrumentPartySubID(context, check, entry.UnderlyingInstrumentPartySubID);
		if (entry.UnderlyingInstrumentPartySubIDType is not null) context.Validators.UnderlyingInstrumentPartySubIDType(context, check, entry.UnderlyingInstrumentPartySubIDType);
	}

	static void ValidateAdjustedPositionReport_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.AdjustedPositionReport.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
	}

	static void ValidateAdvertisement_NoLegs(Fix50Context context, FixCheck check, FixMessage.Advertisement.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAdvertisement_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.Advertisement.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoOrders(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, check, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, check, entry.OrderBookingQty);
	}

	static void ValidateAllocationInstruction_NoExecs(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoExecsGroup entry)
	{
		context.Validators.LastQty(context, check, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, check, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
		if (entry.TradeID is not null) context.Validators.TradeID(context, check, entry.TradeID);
		if (entry.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, entry.FirmTradeID);
	}

	static void ValidateAllocationInstruction_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoLegs(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoAllocs(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, check, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocMethod is not null) context.Validators.AllocMethod(context, check, entry.AllocMethod);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.AllocPositionEffect is not null) context.Validators.AllocPositionEffect(context, check, entry.AllocPositionEffect);
		if (entry.IndividualAllocType is not null) context.Validators.IndividualAllocType(context, check, entry.IndividualAllocType);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, check, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, check, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, check, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, check, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.AllocSettlCurrAmt is not null) context.Validators.AllocSettlCurrAmt(context, check, entry.AllocSettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.AllocAccruedInterestAmt is not null) context.Validators.AllocAccruedInterestAmt(context, check, entry.AllocAccruedInterestAmt);
		if (entry.AllocInterestAtMaturity is not null) context.Validators.AllocInterestAtMaturity(context, check, entry.AllocInterestAtMaturity);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.AllocationInstruction_NoAllocs_NoMiscFees);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, check, entry.NoClearingInstructions);
		Counted(check, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		Each(context, check, FixTag.NoClearingInstructions, entry.NoClearingInstructionsGroups, context.Validators.AllocationInstruction_NoAllocs_NoClearingInstructions);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, check, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoAllocs_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateAllocationInstruction_NoAllocs_NoClearingInstructions(Fix50Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateAllocationInstructionAck_NoAllocs(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAck.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocPositionEffect is not null) context.Validators.AllocPositionEffect(context, check, entry.AllocPositionEffect);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, check, entry.IndividualAllocRejCode);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.IndividualAllocType is not null) context.Validators.IndividualAllocType(context, check, entry.IndividualAllocType);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateAllocationInstructionAlert_NoOrders(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, check, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, check, entry.OrderBookingQty);
	}

	static void ValidateAllocationInstructionAlert_NoExecs(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoExecsGroup entry)
	{
		context.Validators.LastQty(context, check, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, check, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
		if (entry.TradeID is not null) context.Validators.TradeID(context, check, entry.TradeID);
		if (entry.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, entry.FirmTradeID);
	}

	static void ValidateAllocationInstructionAlert_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationInstructionAlert_NoLegs(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAllocationInstructionAlert_NoAllocs(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, check, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocMethod is not null) context.Validators.AllocMethod(context, check, entry.AllocMethod);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.AllocPositionEffect is not null) context.Validators.AllocPositionEffect(context, check, entry.AllocPositionEffect);
		if (entry.IndividualAllocType is not null) context.Validators.IndividualAllocType(context, check, entry.IndividualAllocType);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, check, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, check, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, check, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, check, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.AllocSettlCurrAmt is not null) context.Validators.AllocSettlCurrAmt(context, check, entry.AllocSettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.AllocAccruedInterestAmt is not null) context.Validators.AllocAccruedInterestAmt(context, check, entry.AllocAccruedInterestAmt);
		if (entry.AllocInterestAtMaturity is not null) context.Validators.AllocInterestAtMaturity(context, check, entry.AllocInterestAtMaturity);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.AllocationInstructionAlert_NoAllocs_NoMiscFees);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, check, entry.NoClearingInstructions);
		Counted(check, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		Each(context, check, FixTag.NoClearingInstructions, entry.NoClearingInstructionsGroups, context.Validators.AllocationInstructionAlert_NoAllocs_NoClearingInstructions);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, check, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
	}

	static void ValidateAllocationInstructionAlert_NoAllocs_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateAllocationInstructionAlert_NoAllocs_NoClearingInstructions(Fix50Context context, FixCheck check, FixMessage.AllocationInstructionAlert.NoAllocsGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateAllocationReport_NoOrders(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, check, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, check, entry.OrderBookingQty);
	}

	static void ValidateAllocationReport_NoExecs(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoExecsGroup entry)
	{
		context.Validators.LastQty(context, check, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, check, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
		if (entry.TradeID is not null) context.Validators.TradeID(context, check, entry.TradeID);
		if (entry.FirmTradeID is not null) context.Validators.FirmTradeID(context, check, entry.FirmTradeID);
	}

	static void ValidateAllocationReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAllocationReport_NoAllocs(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, check, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocMethod is not null) context.Validators.AllocMethod(context, check, entry.AllocMethod);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.AllocPositionEffect is not null) context.Validators.AllocPositionEffect(context, check, entry.AllocPositionEffect);
		if (entry.IndividualAllocType is not null) context.Validators.IndividualAllocType(context, check, entry.IndividualAllocType);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, check, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, check, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, check, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, check, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.AllocSettlCurrAmt is not null) context.Validators.AllocSettlCurrAmt(context, check, entry.AllocSettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.AllocAccruedInterestAmt is not null) context.Validators.AllocAccruedInterestAmt(context, check, entry.AllocAccruedInterestAmt);
		if (entry.AllocInterestAtMaturity is not null) context.Validators.AllocInterestAtMaturity(context, check, entry.AllocInterestAtMaturity);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.AllocationReport_NoAllocs_NoMiscFees);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, check, entry.NoClearingInstructions);
		Counted(check, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		Each(context, check, FixTag.NoClearingInstructions, entry.NoClearingInstructionsGroups, context.Validators.AllocationReport_NoAllocs_NoClearingInstructions);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, check, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
	}

	static void ValidateAllocationReport_NoAllocs_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateAllocationReport_NoAllocs_NoClearingInstructions(Fix50Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateAllocationReportAck_NoAllocs(Fix50Context context, FixCheck check, FixMessage.AllocationReportAck.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocPositionEffect is not null) context.Validators.AllocPositionEffect(context, check, entry.AllocPositionEffect);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, check, entry.IndividualAllocRejCode);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.IndividualAllocType is not null) context.Validators.IndividualAllocType(context, check, entry.IndividualAllocType);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateApplicationMessageReport_NoApplIDs(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageReport.NoApplIDsGroup entry)
	{
		context.Validators.RefApplID(context, check, entry.RefApplID);
		if (entry.ApplNewSeqNum is not null) context.Validators.ApplNewSeqNum(context, check, entry.ApplNewSeqNum);
		if (entry.RefApplLastSeqNum is not null) context.Validators.RefApplLastSeqNum(context, check, entry.RefApplLastSeqNum);
	}

	static void ValidateApplicationMessageRequest_NoApplIDs(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageRequest.NoApplIDsGroup entry)
	{
		context.Validators.RefApplID(context, check, entry.RefApplID);
		if (entry.RefApplReqID is not null) context.Validators.RefApplReqID(context, check, entry.RefApplReqID);
		if (entry.ApplBegSeqNum is not null) context.Validators.ApplBegSeqNum(context, check, entry.ApplBegSeqNum);
		if (entry.ApplEndSeqNum is not null) context.Validators.ApplEndSeqNum(context, check, entry.ApplEndSeqNum);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateApplicationMessageRequestAck_NoApplIDs(Fix50Context context, FixCheck check, FixMessage.ApplicationMessageRequestAck.NoApplIDsGroup entry)
	{
		context.Validators.RefApplID(context, check, entry.RefApplID);
		if (entry.RefApplReqID is not null) context.Validators.RefApplReqID(context, check, entry.RefApplReqID);
		if (entry.ApplBegSeqNum is not null) context.Validators.ApplBegSeqNum(context, check, entry.ApplBegSeqNum);
		if (entry.ApplEndSeqNum is not null) context.Validators.ApplEndSeqNum(context, check, entry.ApplEndSeqNum);
		if (entry.RefApplLastSeqNum is not null) context.Validators.RefApplLastSeqNum(context, check, entry.RefApplLastSeqNum);
		if (entry.ApplResponseError is not null) context.Validators.ApplResponseError(context, check, entry.ApplResponseError);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateAssignmentReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.AssignmentReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAssignmentReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.AssignmentReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateBidRequest_NoBidDescriptors(Fix50Context context, FixCheck check, FixMessage.BidRequest.NoBidDescriptorsGroup entry)
	{
		context.Validators.BidDescriptorType(context, check, entry.BidDescriptorType);
		if (entry.BidDescriptor is not null) context.Validators.BidDescriptor(context, check, entry.BidDescriptor);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, check, entry.SideValueInd);
		if (entry.LiquidityValue is not null) context.Validators.LiquidityValue(context, check, entry.LiquidityValue);
		if (entry.LiquidityNumSecurities is not null) context.Validators.LiquidityNumSecurities(context, check, entry.LiquidityNumSecurities);
		if (entry.LiquidityPctLow is not null) context.Validators.LiquidityPctLow(context, check, entry.LiquidityPctLow);
		if (entry.LiquidityPctHigh is not null) context.Validators.LiquidityPctHigh(context, check, entry.LiquidityPctHigh);
		if (entry.EFPTrackingError is not null) context.Validators.EFPTrackingError(context, check, entry.EFPTrackingError);
		if (entry.FairValue is not null) context.Validators.FairValue(context, check, entry.FairValue);
		if (entry.OutsideIndexPct is not null) context.Validators.OutsideIndexPct(context, check, entry.OutsideIndexPct);
		if (entry.ValueOfFutures is not null) context.Validators.ValueOfFutures(context, check, entry.ValueOfFutures);
	}

	static void ValidateBidRequest_NoBidComponents(Fix50Context context, FixCheck check, FixMessage.BidRequest.NoBidComponentsGroup entry)
	{
		context.Validators.ListID(context, check, entry.ListID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
	}

	static void ValidateBidResponse_NoBidComponents(Fix50Context context, FixCheck check, FixMessage.BidResponse.NoBidComponentsGroup entry)
	{
		if (Empty((ICommissionData)entry)) Absent(check, FixTag.Commission, entry.Commission.Position);
		else context.Validators.CommissionData(context, check, entry);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (entry.Country is not null) context.Validators.Country(context, check, entry.Country);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (entry.FairValue is not null) context.Validators.FairValue(context, check, entry.FairValue);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateCollateralAssignment_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralAssignment_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralAssignment_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralAssignment_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralAssignment_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.CollateralAssignment.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralInquiry_NoCollInquiryQualifier(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup entry)
	{
		context.Validators.CollInquiryQualifier(context, check, entry.CollInquiryQualifier);
	}

	static void ValidateCollateralInquiry_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralInquiry_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralInquiry_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralInquiry_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralInquiry.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralInquiryAck_NoCollInquiryQualifier(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup entry)
	{
		context.Validators.CollInquiryQualifier(context, check, entry.CollInquiryQualifier);
	}

	static void ValidateCollateralInquiryAck_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralInquiryAck_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralInquiryAck_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralInquiryAck_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralReport_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralReport.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralReport_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralReport.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralReport_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.CollateralReport.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralRequest_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralRequest.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralRequest_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralRequest.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralRequest_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.CollateralRequest.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralResponse_NoExecs(Fix50Context context, FixCheck check, FixMessage.CollateralResponse.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralResponse_NoTrades(Fix50Context context, FixCheck check, FixMessage.CollateralResponse.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralResponse_NoLegs(Fix50Context context, FixCheck check, FixMessage.CollateralResponse.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralResponse_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CollateralResponse.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralResponse_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.CollateralResponse.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateConfirmation_NoOrders(Fix50Context context, FixCheck check, FixMessage.Confirmation.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, check, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, check, entry.OrderBookingQty);
	}

	static void ValidateConfirmation_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.Confirmation.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateConfirmation_NoLegs(Fix50Context context, FixCheck check, FixMessage.Confirmation.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateConfirmation_NoCapacities(Fix50Context context, FixCheck check, FixMessage.Confirmation.NoCapacitiesGroup entry)
	{
		context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.OrderCapacityQty is null) Missing(check, FixTag.OrderCapacityQty, entry.OrderCapacity.Position);
		else context.Validators.OrderCapacityQty(context, check, entry.OrderCapacityQty);
	}

	static void ValidateConfirmation_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.Confirmation.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateConfirmationRequest_NoOrders(Fix50Context context, FixCheck check, FixMessage.ConfirmationRequest.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, check, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, check, entry.OrderBookingQty);
	}

	static void ValidateContraryIntentionReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.ContraryIntentionReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoSides(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.ClOrdID is null) Missing(check, FixTag.ClOrdID, entry.Side.Position);
		else context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, entry.ClOrdLinkID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, check, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, check, entry.BookingUnit);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, check, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.CrossOrderCancelReplaceRequest_NoSides_NoAllocs);
		if (entry.QtyType is not null) context.Validators.QtyType(context, check, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(check, FixTag.OrderQty, entry.Side.Position);
		else context.Validators.OrderQtyData(context, check, entry);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, entry.PreTradeAnonymity);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, check, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, check, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, entry.CoveredOrUncovered);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, check, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.SideComplianceID is not null) context.Validators.SideComplianceID(context, check, entry.SideComplianceID);
		if (entry.SideTimeInForce is not null) context.Validators.SideTimeInForce(context, check, entry.SideTimeInForce);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateCrossOrderCancelRequest_NoSides(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.ClOrdID is null) Missing(check, FixTag.ClOrdID, entry.Side.Position);
		else context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, entry.ClOrdLinkID);
		if (entry.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, entry.OrigOrdModTime);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, check, entry.TradeDate);
		if (Empty((IOrderQtyData)entry)) Absent(check, FixTag.OrderQty, entry.Side.Position);
		else context.Validators.OrderQtyData(context, check, entry);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateCrossOrderCancelRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCrossOrderCancelRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateDerivativeSecurityList_NoDerivativeSecurityAltID(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoDerivativeSecurityAltIDGroup entry)
	{
		context.Validators.DerivativeSecurityAltID(context, check, entry.DerivativeSecurityAltID);
		if (entry.DerivativeSecurityAltIDSource is not null) context.Validators.DerivativeSecurityAltIDSource(context, check, entry.DerivativeSecurityAltIDSource);
	}

	static void ValidateDerivativeSecurityList_NoDerivativeEvents(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoDerivativeEventsGroup entry)
	{
		context.Validators.DerivativeEventType(context, check, entry.DerivativeEventType);
		if (entry.DerivativeEventDate is not null) context.Validators.DerivativeEventDate(context, check, entry.DerivativeEventDate);
		if (entry.DerivativeEventTime is not null) context.Validators.DerivativeEventTime(context, check, entry.DerivativeEventTime);
		if (entry.DerivativeEventPx is not null) context.Validators.DerivativeEventPx(context, check, entry.DerivativeEventPx);
		if (entry.DerivativeEventText is not null) context.Validators.DerivativeEventText(context, check, entry.DerivativeEventText);
	}

	static void ValidateDerivativeSecurityList_NoDerivativeInstrumentParties(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup entry)
	{
		context.Validators.DerivativeInstrumentPartyID(context, check, entry.DerivativeInstrumentPartyID);
		if (entry.DerivativeInstrumentPartyIDSource is not null) context.Validators.DerivativeInstrumentPartyIDSource(context, check, entry.DerivativeInstrumentPartyIDSource);
		if (entry.DerivativeInstrumentPartyRole is not null) context.Validators.DerivativeInstrumentPartyRole(context, check, entry.DerivativeInstrumentPartyRole);
		if (entry.NoDerivativeInstrumentPartySubIDs is not null) context.Validators.NoDerivativeInstrumentPartySubIDs(context, check, entry.NoDerivativeInstrumentPartySubIDs);
		Counted(check, entry.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups, context.Validators.DerivativeSecurityList_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs);
	}

	static void ValidateDerivativeSecurityList_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup entry)
	{
		context.Validators.DerivativeInstrumentPartySubID(context, check, entry.DerivativeInstrumentPartySubID);
		if (entry.DerivativeInstrumentPartySubIDType is not null) context.Validators.DerivativeInstrumentPartySubIDType(context, check, entry.DerivativeInstrumentPartySubIDType);
	}

	static void ValidateDerivativeSecurityList_NoDerivativeInstrAttrib(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoDerivativeInstrAttribGroup entry)
	{
		context.Validators.DerivativeInstrAttribType(context, check, entry.DerivativeInstrAttribType);
		if (entry.DerivativeInstrAttribValue is not null) context.Validators.DerivativeInstrAttribValue(context, check, entry.DerivativeInstrAttribValue);
	}

	static void ValidateDerivativeSecurityList_NoMarketSegments(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup entry)
	{
		context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.DerivativeSecurityList_NoMarketSegments_NoStrikeRules);
	}

	static void ValidateDerivativeSecurityList_NoMarketSegments_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.DerivativeSecurityList_NoMarketSegments_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateDerivativeSecurityList_NoMarketSegments_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateDerivativeSecurityList_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.SecondaryPriceLimitType is not null) context.Validators.SecondaryPriceLimitType(context, check, entry.SecondaryPriceLimitType);
		if (entry.SecondaryLowLimitPrice is not null) context.Validators.SecondaryLowLimitPrice(context, check, entry.SecondaryLowLimitPrice);
		if (entry.SecondaryHighLimitPrice is not null) context.Validators.SecondaryHighLimitPrice(context, check, entry.SecondaryHighLimitPrice);
		if (entry.SecondaryTradingReferencePrice is not null) context.Validators.SecondaryTradingReferencePrice(context, check, entry.SecondaryTradingReferencePrice);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.CorporateAction is not null) context.Validators.CorporateAction(context, check, entry.CorporateAction);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.DerivativeSecurityList_NoRelatedSym_NoLegs);
		if (entry.RelSymTransactTime is not null) context.Validators.RelSymTransactTime(context, check, entry.RelSymTransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateDerivativeSecurityList_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateDerivativeSecurityListRequest_NoDerivativeSecurityAltID(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest.NoDerivativeSecurityAltIDGroup entry)
	{
		context.Validators.DerivativeSecurityAltID(context, check, entry.DerivativeSecurityAltID);
		if (entry.DerivativeSecurityAltIDSource is not null) context.Validators.DerivativeSecurityAltIDSource(context, check, entry.DerivativeSecurityAltIDSource);
	}

	static void ValidateDerivativeSecurityListRequest_NoDerivativeEvents(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest.NoDerivativeEventsGroup entry)
	{
		context.Validators.DerivativeEventType(context, check, entry.DerivativeEventType);
		if (entry.DerivativeEventDate is not null) context.Validators.DerivativeEventDate(context, check, entry.DerivativeEventDate);
		if (entry.DerivativeEventTime is not null) context.Validators.DerivativeEventTime(context, check, entry.DerivativeEventTime);
		if (entry.DerivativeEventPx is not null) context.Validators.DerivativeEventPx(context, check, entry.DerivativeEventPx);
		if (entry.DerivativeEventText is not null) context.Validators.DerivativeEventText(context, check, entry.DerivativeEventText);
	}

	static void ValidateDerivativeSecurityListRequest_NoDerivativeInstrumentParties(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup entry)
	{
		context.Validators.DerivativeInstrumentPartyID(context, check, entry.DerivativeInstrumentPartyID);
		if (entry.DerivativeInstrumentPartyIDSource is not null) context.Validators.DerivativeInstrumentPartyIDSource(context, check, entry.DerivativeInstrumentPartyIDSource);
		if (entry.DerivativeInstrumentPartyRole is not null) context.Validators.DerivativeInstrumentPartyRole(context, check, entry.DerivativeInstrumentPartyRole);
		if (entry.NoDerivativeInstrumentPartySubIDs is not null) context.Validators.NoDerivativeInstrumentPartySubIDs(context, check, entry.NoDerivativeInstrumentPartySubIDs);
		Counted(check, entry.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups, context.Validators.DerivativeSecurityListRequest_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs);
	}

	static void ValidateDerivativeSecurityListRequest_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup entry)
	{
		context.Validators.DerivativeInstrumentPartySubID(context, check, entry.DerivativeInstrumentPartySubID);
		if (entry.DerivativeInstrumentPartySubIDType is not null) context.Validators.DerivativeInstrumentPartySubIDType(context, check, entry.DerivativeInstrumentPartySubIDType);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoDerivativeSecurityAltID(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeSecurityAltIDGroup entry)
	{
		context.Validators.DerivativeSecurityAltID(context, check, entry.DerivativeSecurityAltID);
		if (entry.DerivativeSecurityAltIDSource is not null) context.Validators.DerivativeSecurityAltIDSource(context, check, entry.DerivativeSecurityAltIDSource);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoDerivativeEvents(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeEventsGroup entry)
	{
		context.Validators.DerivativeEventType(context, check, entry.DerivativeEventType);
		if (entry.DerivativeEventDate is not null) context.Validators.DerivativeEventDate(context, check, entry.DerivativeEventDate);
		if (entry.DerivativeEventTime is not null) context.Validators.DerivativeEventTime(context, check, entry.DerivativeEventTime);
		if (entry.DerivativeEventPx is not null) context.Validators.DerivativeEventPx(context, check, entry.DerivativeEventPx);
		if (entry.DerivativeEventText is not null) context.Validators.DerivativeEventText(context, check, entry.DerivativeEventText);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup entry)
	{
		context.Validators.DerivativeInstrumentPartyID(context, check, entry.DerivativeInstrumentPartyID);
		if (entry.DerivativeInstrumentPartyIDSource is not null) context.Validators.DerivativeInstrumentPartyIDSource(context, check, entry.DerivativeInstrumentPartyIDSource);
		if (entry.DerivativeInstrumentPartyRole is not null) context.Validators.DerivativeInstrumentPartyRole(context, check, entry.DerivativeInstrumentPartyRole);
		if (entry.NoDerivativeInstrumentPartySubIDs is not null) context.Validators.NoDerivativeInstrumentPartySubIDs(context, check, entry.NoDerivativeInstrumentPartySubIDs);
		Counted(check, entry.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups);
		Each(context, check, FixTag.NoDerivativeInstrumentPartySubIDs, entry.NoDerivativeInstrumentPartySubIDsGroups, context.Validators.DerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrumentParties_NoDerivativeInstrumentPartySubIDs(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrumentPartiesGroup.NoDerivativeInstrumentPartySubIDsGroup entry)
	{
		context.Validators.DerivativeInstrumentPartySubID(context, check, entry.DerivativeInstrumentPartySubID);
		if (entry.DerivativeInstrumentPartySubIDType is not null) context.Validators.DerivativeInstrumentPartySubIDType(context, check, entry.DerivativeInstrumentPartySubIDType);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoDerivativeInstrAttrib(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoDerivativeInstrAttribGroup entry)
	{
		context.Validators.DerivativeInstrAttribType(context, check, entry.DerivativeInstrAttribType);
		if (entry.DerivativeInstrAttribValue is not null) context.Validators.DerivativeInstrAttribValue(context, check, entry.DerivativeInstrAttribValue);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoMarketSegments(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup entry)
	{
		context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.DerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.DerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup entry)
	{
		context.Validators.ListUpdateAction(context, check, entry.ListUpdateAction);
		if (entry.CorporateAction is not null) context.Validators.CorporateAction(context, check, entry.CorporateAction);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (entry.SecondaryPriceLimitType is not null) context.Validators.SecondaryPriceLimitType(context, check, entry.SecondaryPriceLimitType);
		if (entry.SecondaryLowLimitPrice is not null) context.Validators.SecondaryLowLimitPrice(context, check, entry.SecondaryLowLimitPrice);
		if (entry.SecondaryHighLimitPrice is not null) context.Validators.SecondaryHighLimitPrice(context, check, entry.SecondaryHighLimitPrice);
		if (entry.SecondaryTradingReferencePrice is not null) context.Validators.SecondaryTradingReferencePrice(context, check, entry.SecondaryTradingReferencePrice);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.DerivativeSecurityListUpdateReport_NoRelatedSym_NoLegs);
		if (entry.RelSymTransactTime is not null) context.Validators.RelSymTransactTime(context, check, entry.RelSymTransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateDerivativeSecurityListUpdateReport_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.DerivativeSecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateDontKnowTrade_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.DontKnowTrade.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateDontKnowTrade_NoLegs(Fix50Context context, FixCheck check, FixMessage.DontKnowTrade.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateEmail_NoRoutingIDs(Fix50Context context, FixCheck check, FixMessage.Email.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateEmail_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.Email.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
	}

	static void ValidateEmail_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.Email.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateEmail_NoLegs(Fix50Context context, FixCheck check, FixMessage.Email.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateEmail_NoLinesOfText(Fix50Context context, FixCheck check, FixMessage.Email.NoLinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateExecutionAcknowledgement_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.ExecutionAcknowledgement.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateExecutionAcknowledgement_NoLegs(Fix50Context context, FixCheck check, FixMessage.ExecutionAcknowledgement.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateExecutionReport_NoContraBrokers(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoContraBrokersGroup entry)
	{
		context.Validators.ContraBroker(context, check, entry.ContraBroker);
		if (entry.ContraTrader is not null) context.Validators.ContraTrader(context, check, entry.ContraTrader);
		if (entry.ContraTradeQty is not null) context.Validators.ContraTradeQty(context, check, entry.ContraTradeQty);
		if (entry.ContraTradeTime is not null) context.Validators.ContraTradeTime(context, check, entry.ContraTradeTime);
		if (entry.ContraLegRefID is not null) context.Validators.ContraLegRefID(context, check, entry.ContraLegRefID);
	}

	static void ValidateExecutionReport_NoAllocs(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateExecutionReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateExecutionReport_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateExecutionReport_NoFills(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoFillsGroup entry)
	{
		context.Validators.FillExecID(context, check, entry.FillExecID);
		if (entry.FillPx is not null) context.Validators.FillPx(context, check, entry.FillPx);
		if (entry.FillQty is not null) context.Validators.FillQty(context, check, entry.FillQty);
		if (entry.FillLiquidityInd is not null) context.Validators.FillLiquidityInd(context, check, entry.FillLiquidityInd);
		if (!Empty((INestedParties4)entry)) context.Validators.NestedParties4(context, check, entry);
	}

	static void ValidateExecutionReport_NoContAmts(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoContAmtsGroup entry)
	{
		context.Validators.ContAmtType(context, check, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, check, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, check, entry.ContAmtCurr);
	}

	static void ValidateExecutionReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegAllocID is not null) context.Validators.LegAllocID(context, check, entry.LegAllocID);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, check, entry.NoLegAllocs);
		Counted(check, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		Each(context, check, FixTag.NoLegAllocs, entry.NoLegAllocsGroups, context.Validators.ExecutionReport_NoLegs_NoLegAllocs);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
		if (entry.LegLastForwardPoints is not null) context.Validators.LegLastForwardPoints(context, check, entry.LegLastForwardPoints);
		if (entry.LegCalculatedCcyLastQty is not null) context.Validators.LegCalculatedCcyLastQty(context, check, entry.LegCalculatedCcyLastQty);
		if (entry.LegGrossTradeAmt is not null) context.Validators.LegGrossTradeAmt(context, check, entry.LegGrossTradeAmt);
		if (entry.LegVolatility is not null) context.Validators.LegVolatility(context, check, entry.LegVolatility);
		if (entry.LegDividendYield is not null) context.Validators.LegDividendYield(context, check, entry.LegDividendYield);
		if (entry.LegCurrencyRatio is not null) context.Validators.LegCurrencyRatio(context, check, entry.LegCurrencyRatio);
		if (entry.LegExecInst is not null) context.Validators.LegExecInst(context, check, entry.LegExecInst);
		if (entry.LegLastQty is not null) context.Validators.LegLastQty(context, check, entry.LegLastQty);
	}

	static void ValidateExecutionReport_NoLegs_NoLegAllocs(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoLegsGroup.NoLegAllocsGroup entry)
	{
		context.Validators.LegAllocAccount(context, check, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, check, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, check, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, check, entry.LegAllocAcctIDSource);
		if (entry.LegAllocSettlCurrency is not null) context.Validators.LegAllocSettlCurrency(context, check, entry.LegAllocSettlCurrency);
	}

	static void ValidateExecutionReport_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.ExecutionReport.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateIOI_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.IOI.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateIOI_NoLegs(Fix50Context context, FixCheck check, FixMessage.IOI.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegIOIQty is not null) context.Validators.LegIOIQty(context, check, entry.LegIOIQty);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
	}

	static void ValidateIOI_NoIOIQualifiers(Fix50Context context, FixCheck check, FixMessage.IOI.NoIOIQualifiersGroup entry)
	{
		context.Validators.IOIQualifier(context, check, entry.IOIQualifier);
	}

	static void ValidateIOI_NoRoutingIDs(Fix50Context context, FixCheck check, FixMessage.IOI.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateListStatus_NoOrders(Fix50Context context, FixCheck check, FixMessage.ListStatus.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.CumQty is null) Missing(check, FixTag.CumQty, entry.ClOrdID.Position);
		else context.Validators.CumQty(context, check, entry.CumQty);
		if (entry.OrdStatus is null) Missing(check, FixTag.OrdStatus, entry.ClOrdID.Position);
		else context.Validators.OrdStatus(context, check, entry.OrdStatus);
		if (entry.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, check, entry.WorkingIndicator);
		if (entry.LeavesQty is null) Missing(check, FixTag.LeavesQty, entry.ClOrdID.Position);
		else context.Validators.LeavesQty(context, check, entry.LeavesQty);
		if (entry.CxlQty is null) Missing(check, FixTag.CxlQty, entry.ClOrdID.Position);
		else context.Validators.CxlQty(context, check, entry.CxlQty);
		if (entry.AvgPx is null) Missing(check, FixTag.AvgPx, entry.ClOrdID.Position);
		else context.Validators.AvgPx(context, check, entry.AvgPx);
		if (entry.OrdRejReason is not null) context.Validators.OrdRejReason(context, check, entry.OrdRejReason);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateListStrikePrice_NoStrikes(Fix50Context context, FixCheck check, FixMessage.ListStrikePrice.NoStrikesGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.ListStrikePrice_NoStrikes_NoUnderlyings);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateListStrikePrice_NoStrikes_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.ListStrikePrice.NoStrikesGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateLogon_NoMsgTypes(Fix50Context context, FixCheck check, FixMessage.Logon.NoMsgTypesGroup entry)
	{
		context.Validators.RefMsgType(context, check, entry.RefMsgType);
		if (entry.MsgDirection is not null) context.Validators.MsgDirection(context, check, entry.MsgDirection);
		if (entry.RefApplVerID is not null) context.Validators.RefApplVerID(context, check, entry.RefApplVerID);
		if (entry.RefApplExtID is not null) context.Validators.RefApplExtID(context, check, entry.RefApplExtID);
		if (entry.RefCstmApplVerID is not null) context.Validators.RefCstmApplVerID(context, check, entry.RefCstmApplVerID);
		if (entry.DefaultVerIndicator is not null) context.Validators.DefaultVerIndicator(context, check, entry.DefaultVerIndicator);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDUpdateAction(context, check, entry.MDUpdateAction);
		if (entry.DeleteReason is not null) context.Validators.DeleteReason(context, check, entry.DeleteReason);
		if (entry.MDSubBookType is not null) context.Validators.MDSubBookType(context, check, entry.MDSubBookType);
		if (entry.MarketDepth is not null) context.Validators.MarketDepth(context, check, entry.MarketDepth);
		if (entry.MDEntryType is not null) context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, check, entry.MDEntryID);
		if (entry.MDEntryRefID is not null) context.Validators.MDEntryRefID(context, check, entry.MDEntryRefID);
		if (entry.MDStreamID is not null) context.Validators.MDStreamID(context, check, entry.MDStreamID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoLegs);
		if (entry.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, entry.FinancialStatus);
		if (entry.CorporateAction is not null) context.Validators.CorporateAction(context, check, entry.CorporateAction);
		if (entry.MDEntryPx is not null) context.Validators.MDEntryPx(context, check, entry.MDEntryPx);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (!Empty((IRateSource)entry)) context.Validators.RateSource(context, check, entry);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.NoOfSecSizes is not null) context.Validators.NoOfSecSizes(context, check, entry.NoOfSecSizes);
		Counted(check, entry.NoOfSecSizes, entry.NoOfSecSizesGroups);
		Each(context, check, FixTag.NoOfSecSizes, entry.NoOfSecSizesGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoOfSecSizes);
		if (entry.LotType is not null) context.Validators.LotType(context, check, entry.LotType);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, check, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, check, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, check, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, check, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, check, entry.SecurityTradingStatus);
		if (entry.HaltReasonInt is not null) context.Validators.HaltReasonInt(context, check, entry.HaltReasonInt);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, check, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, check, entry.TradeCondition);
		if (entry.TrdType is not null) context.Validators.TrdType(context, check, entry.TrdType);
		if (entry.MatchType is not null) context.Validators.MatchType(context, check, entry.MatchType);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, check, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, check, entry.OpenCloseSettlFlag);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, check, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.MinQty is not null) context.Validators.MinQty(context, check, entry.MinQty);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.SellerDays is not null) context.Validators.SellerDays(context, check, entry.SellerDays);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (entry.TradeID is not null) context.Validators.TradeID(context, check, entry.TradeID);
		if (entry.MDEntryBuyer is not null) context.Validators.MDEntryBuyer(context, check, entry.MDEntryBuyer);
		if (entry.MDEntrySeller is not null) context.Validators.MDEntrySeller(context, check, entry.MDEntrySeller);
		if (entry.NumberOfOrders is not null) context.Validators.NumberOfOrders(context, check, entry.NumberOfOrders);
		if (entry.MDEntryPositionNo is not null) context.Validators.MDEntryPositionNo(context, check, entry.MDEntryPositionNo);
		if (entry.Scope is not null) context.Validators.Scope(context, check, entry.Scope);
		if (entry.PriceDelta is not null) context.Validators.PriceDelta(context, check, entry.PriceDelta);
		if (entry.NetChgPrevDay is not null) context.Validators.NetChgPrevDay(context, check, entry.NetChgPrevDay);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.MDPriceLevel is not null) context.Validators.MDPriceLevel(context, check, entry.MDPriceLevel);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.MDOriginType is not null) context.Validators.MDOriginType(context, check, entry.MDOriginType);
		if (entry.HighPx is not null) context.Validators.HighPx(context, check, entry.HighPx);
		if (entry.LowPx is not null) context.Validators.LowPx(context, check, entry.LowPx);
		if (entry.FirstPx is not null) context.Validators.FirstPx(context, check, entry.FirstPx);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.TradeVolume is not null) context.Validators.TradeVolume(context, check, entry.TradeVolume);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, entry.TransBkdTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.MDQuoteType is not null) context.Validators.MDQuoteType(context, check, entry.MDQuoteType);
		if (entry.RptSeq is not null) context.Validators.RptSeq(context, check, entry.RptSeq);
		if (entry.DealingCapacity is not null) context.Validators.DealingCapacity(context, check, entry.DealingCapacity);
		if (entry.MDEntrySpotRate is not null) context.Validators.MDEntrySpotRate(context, check, entry.MDEntrySpotRate);
		if (entry.MDEntryForwardPoints is not null) context.Validators.MDEntryForwardPoints(context, check, entry.MDEntryForwardPoints);
		if (entry.NoStatsIndicators is not null) context.Validators.NoStatsIndicators(context, check, entry.NoStatsIndicators);
		Counted(check, entry.NoStatsIndicators, entry.NoStatsIndicatorsGroups);
		Each(context, check, FixTag.NoStatsIndicators, entry.NoStatsIndicatorsGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoStatsIndicators);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoOfSecSizes(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoOfSecSizesGroup entry)
	{
		context.Validators.MDSecSizeType(context, check, entry.MDSecSizeType);
		if (entry.MDSecSize is not null) context.Validators.MDSecSize(context, check, entry.MDSecSize);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoStatsIndicators(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoStatsIndicatorsGroup entry)
	{
		context.Validators.StatsType(context, check, entry.StatsType);
	}

	static void ValidateMarketDataIncrementalRefresh_NoRoutingIDs(Fix50Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateMarketDataRequest_NoMDEntryTypes(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest.NoMDEntryTypesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
	}

	static void ValidateMarketDataRequest_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.MarketDataRequest_NoRelatedSym_NoUnderlyings);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.MarketDataRequest_NoRelatedSym_NoLegs);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, check, entry.QuoteType);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.MDStreamID is not null) context.Validators.MDStreamID(context, check, entry.MDStreamID);
	}

	static void ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataRequest_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataRequest_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.MarketDataRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateMarketDataRequestReject_NoAltMDSource(Fix50Context context, FixCheck check, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup entry)
	{
		context.Validators.AltMDSourceID(context, check, entry.AltMDSourceID);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoLegs(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoMDEntries(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, check, entry.MDEntryID);
		if (entry.MDEntryPx is not null) context.Validators.MDEntryPx(context, check, entry.MDEntryPx);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (!Empty((IRateSource)entry)) context.Validators.RateSource(context, check, entry);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.NoOfSecSizes is not null) context.Validators.NoOfSecSizes(context, check, entry.NoOfSecSizes);
		Counted(check, entry.NoOfSecSizes, entry.NoOfSecSizesGroups);
		Each(context, check, FixTag.NoOfSecSizes, entry.NoOfSecSizesGroups, context.Validators.MarketDataSnapshotFullRefresh_NoMDEntries_NoOfSecSizes);
		if (entry.LotType is not null) context.Validators.LotType(context, check, entry.LotType);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, check, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, check, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, check, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, check, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, check, entry.SecurityTradingStatus);
		if (entry.HaltReasonInt is not null) context.Validators.HaltReasonInt(context, check, entry.HaltReasonInt);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, check, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, check, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, check, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, check, entry.OpenCloseSettlFlag);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, check, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.MinQty is not null) context.Validators.MinQty(context, check, entry.MinQty);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.SellerDays is not null) context.Validators.SellerDays(context, check, entry.SellerDays);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (entry.MDEntryBuyer is not null) context.Validators.MDEntryBuyer(context, check, entry.MDEntryBuyer);
		if (entry.MDEntrySeller is not null) context.Validators.MDEntrySeller(context, check, entry.MDEntrySeller);
		if (entry.NumberOfOrders is not null) context.Validators.NumberOfOrders(context, check, entry.NumberOfOrders);
		if (entry.MDEntryPositionNo is not null) context.Validators.MDEntryPositionNo(context, check, entry.MDEntryPositionNo);
		if (entry.Scope is not null) context.Validators.Scope(context, check, entry.Scope);
		if (entry.PriceDelta is not null) context.Validators.PriceDelta(context, check, entry.PriceDelta);
		if (entry.TrdType is not null) context.Validators.TrdType(context, check, entry.TrdType);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.MDPriceLevel is not null) context.Validators.MDPriceLevel(context, check, entry.MDPriceLevel);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.MDOriginType is not null) context.Validators.MDOriginType(context, check, entry.MDOriginType);
		if (entry.HighPx is not null) context.Validators.HighPx(context, check, entry.HighPx);
		if (entry.LowPx is not null) context.Validators.LowPx(context, check, entry.LowPx);
		if (entry.FirstPx is not null) context.Validators.FirstPx(context, check, entry.FirstPx);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.TradeVolume is not null) context.Validators.TradeVolume(context, check, entry.TradeVolume);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.MDQuoteType is not null) context.Validators.MDQuoteType(context, check, entry.MDQuoteType);
		if (entry.RptSeq is not null) context.Validators.RptSeq(context, check, entry.RptSeq);
		if (entry.DealingCapacity is not null) context.Validators.DealingCapacity(context, check, entry.DealingCapacity);
		if (entry.MDEntrySpotRate is not null) context.Validators.MDEntrySpotRate(context, check, entry.MDEntrySpotRate);
		if (entry.MDEntryForwardPoints is not null) context.Validators.MDEntryForwardPoints(context, check, entry.MDEntryForwardPoints);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoMDEntries_NoOfSecSizes(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup.NoOfSecSizesGroup entry)
	{
		context.Validators.MDSecSizeType(context, check, entry.MDSecSizeType);
		if (entry.MDSecSize is not null) context.Validators.MDSecSize(context, check, entry.MDSecSize);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoRoutingIDs(Fix50Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateMarketDefinition_NoTickRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinition.NoTickRulesGroup entry)
	{
		context.Validators.StartTickPriceRange(context, check, entry.StartTickPriceRange);
		if (entry.EndTickPriceRange is not null) context.Validators.EndTickPriceRange(context, check, entry.EndTickPriceRange);
		if (entry.TickIncrement is not null) context.Validators.TickIncrement(context, check, entry.TickIncrement);
		if (entry.TickRuleType is not null) context.Validators.TickRuleType(context, check, entry.TickRuleType);
	}

	static void ValidateMarketDefinition_NoLotTypeRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinition.NoLotTypeRulesGroup entry)
	{
		context.Validators.LotType(context, check, entry.LotType);
		if (entry.MinLotSize is not null) context.Validators.MinLotSize(context, check, entry.MinLotSize);
	}

	static void ValidateMarketDefinition_NoOrdTypeRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinition.NoOrdTypeRulesGroup entry)
	{
		context.Validators.OrdType(context, check, entry.OrdType);
	}

	static void ValidateMarketDefinition_NoTimeInForceRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinition.NoTimeInForceRulesGroup entry)
	{
		context.Validators.TimeInForce(context, check, entry.TimeInForce);
	}

	static void ValidateMarketDefinition_NoExecInstRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinition.NoExecInstRulesGroup entry)
	{
		context.Validators.ExecInstValue(context, check, entry.ExecInstValue);
	}

	static void ValidateMarketDefinitionUpdateReport_NoTickRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport.NoTickRulesGroup entry)
	{
		context.Validators.StartTickPriceRange(context, check, entry.StartTickPriceRange);
		if (entry.EndTickPriceRange is not null) context.Validators.EndTickPriceRange(context, check, entry.EndTickPriceRange);
		if (entry.TickIncrement is not null) context.Validators.TickIncrement(context, check, entry.TickIncrement);
		if (entry.TickRuleType is not null) context.Validators.TickRuleType(context, check, entry.TickRuleType);
	}

	static void ValidateMarketDefinitionUpdateReport_NoLotTypeRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport.NoLotTypeRulesGroup entry)
	{
		context.Validators.LotType(context, check, entry.LotType);
		if (entry.MinLotSize is not null) context.Validators.MinLotSize(context, check, entry.MinLotSize);
	}

	static void ValidateMarketDefinitionUpdateReport_NoOrdTypeRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport.NoOrdTypeRulesGroup entry)
	{
		context.Validators.OrdType(context, check, entry.OrdType);
	}

	static void ValidateMarketDefinitionUpdateReport_NoTimeInForceRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport.NoTimeInForceRulesGroup entry)
	{
		context.Validators.TimeInForce(context, check, entry.TimeInForce);
	}

	static void ValidateMarketDefinitionUpdateReport_NoExecInstRules(Fix50Context context, FixCheck check, FixMessage.MarketDefinitionUpdateReport.NoExecInstRulesGroup entry)
	{
		context.Validators.ExecInstValue(context, check, entry.ExecInstValue);
	}

	static void ValidateMassQuote_NoQuoteSets(Fix50Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup entry)
	{
		context.Validators.QuoteSetID(context, check, entry.QuoteSetID);
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.QuoteSetValidUntilTime is not null) context.Validators.QuoteSetValidUntilTime(context, check, entry.QuoteSetValidUntilTime);
		if (entry.TotNoQuoteEntries is null) Missing(check, FixTag.TotNoQuoteEntries, entry.QuoteSetID.Position);
		else context.Validators.TotNoQuoteEntries(context, check, entry.TotNoQuoteEntries);
		if (entry.LastFragment is not null) context.Validators.LastFragment(context, check, entry.LastFragment);
		if (entry.NoQuoteEntries is null) Missing(check, FixTag.NoQuoteEntries, entry.QuoteSetID.Position);
		else context.Validators.NoQuoteEntries(context, check, entry.NoQuoteEntries);
		Counted(check, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, entry.NoQuoteEntriesGroups, context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries);
	}

	static void ValidateMassQuote_NoQuoteSets_NoQuoteEntries(Fix50Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
	{
		context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries_NoLegs);
		if (entry.BidPx is not null) context.Validators.BidPx(context, check, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, check, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, check, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, check, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, entry.OfferForwardPoints);
		if (entry.MidPx is not null) context.Validators.MidPx(context, check, entry.MidPx);
		if (entry.BidYield is not null) context.Validators.BidYield(context, check, entry.BidYield);
		if (entry.MidYield is not null) context.Validators.MidYield(context, check, entry.MidYield);
		if (entry.OfferYield is not null) context.Validators.OfferYield(context, check, entry.OfferYield);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, check, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, check, entry.BidForwardPoints2);
		if (entry.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, check, entry.OfferForwardPoints2);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
	}

	static void ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs(Fix50Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets(Fix50Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup entry)
	{
		context.Validators.QuoteSetID(context, check, entry.QuoteSetID);
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.QuoteSetValidUntilTime is not null) context.Validators.QuoteSetValidUntilTime(context, check, entry.QuoteSetValidUntilTime);
		if (entry.TotNoQuoteEntries is not null) context.Validators.TotNoQuoteEntries(context, check, entry.TotNoQuoteEntries);
		if (entry.TotNoCxldQuotes is not null) context.Validators.TotNoCxldQuotes(context, check, entry.TotNoCxldQuotes);
		if (entry.TotNoAccQuotes is not null) context.Validators.TotNoAccQuotes(context, check, entry.TotNoAccQuotes);
		if (entry.TotNoRejQuotes is not null) context.Validators.TotNoRejQuotes(context, check, entry.TotNoRejQuotes);
		if (entry.LastFragment is not null) context.Validators.LastFragment(context, check, entry.LastFragment);
		if (entry.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, check, entry.NoQuoteEntries);
		Counted(check, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, entry.NoQuoteEntriesGroups, context.Validators.MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(Fix50Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
	{
		context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs);
		if (entry.BidPx is not null) context.Validators.BidPx(context, check, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, check, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, check, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, check, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, entry.OfferForwardPoints);
		if (entry.MidPx is not null) context.Validators.MidPx(context, check, entry.MidPx);
		if (entry.BidYield is not null) context.Validators.BidYield(context, check, entry.BidYield);
		if (entry.MidYield is not null) context.Validators.MidYield(context, check, entry.MidYield);
		if (entry.OfferYield is not null) context.Validators.OfferYield(context, check, entry.OfferYield);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, check, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, check, entry.BidForwardPoints2);
		if (entry.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, check, entry.OfferForwardPoints2);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.QuoteEntryStatus is not null) context.Validators.QuoteEntryStatus(context, check, entry.QuoteEntryStatus);
		if (entry.QuoteEntryRejectReason is not null) context.Validators.QuoteEntryRejectReason(context, check, entry.QuoteEntryRejectReason);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs(Fix50Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMultilegOrderCancelReplace_NoAllocs(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateMultilegOrderCancelReplace_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateMultilegOrderCancelReplace_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMultilegOrderCancelReplace_NoLegs(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegAllocID is not null) context.Validators.LegAllocID(context, check, entry.LegAllocID);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, check, entry.NoLegAllocs);
		Counted(check, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		Each(context, check, FixTag.NoLegAllocs, entry.NoLegAllocsGroups, context.Validators.MultilegOrderCancelReplace_NoLegs_NoLegAllocs);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegVolatility is not null) context.Validators.LegVolatility(context, check, entry.LegVolatility);
		if (entry.LegDividendYield is not null) context.Validators.LegDividendYield(context, check, entry.LegDividendYield);
		if (entry.LegCurrencyRatio is not null) context.Validators.LegCurrencyRatio(context, check, entry.LegCurrencyRatio);
		if (entry.LegExecInst is not null) context.Validators.LegExecInst(context, check, entry.LegExecInst);
	}

	static void ValidateMultilegOrderCancelReplace_NoLegs_NoLegAllocs(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoLegsGroup.NoLegAllocsGroup entry)
	{
		context.Validators.LegAllocAccount(context, check, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, check, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, check, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, check, entry.LegAllocAcctIDSource);
		if (entry.LegAllocSettlCurrency is not null) context.Validators.LegAllocSettlCurrency(context, check, entry.LegAllocSettlCurrency);
	}

	static void ValidateMultilegOrderCancelReplace_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.MultilegOrderCancelReplace.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateNetworkCounterpartySystemStatusRequest_NoCompIDs(Fix50Context context, FixCheck check, FixMessage.NetworkCounterpartySystemStatusRequest.NoCompIDsGroup entry)
	{
		context.Validators.RefCompID(context, check, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, check, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
	}

	static void ValidateNetworkCounterpartySystemStatusResponse_NoCompIDs(Fix50Context context, FixCheck check, FixMessage.NetworkCounterpartySystemStatusResponse.NoCompIDsGroup entry)
	{
		context.Validators.RefCompID(context, check, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, check, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.StatusValue is null) Missing(check, FixTag.StatusValue, entry.RefCompID.Position);
		else context.Validators.StatusValue(context, check, entry.StatusValue);
		if (entry.StatusText is not null) context.Validators.StatusText(context, check, entry.StatusText);
	}

	static void ValidateNewOrderCross_NoSides(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.ClOrdID is null) Missing(check, FixTag.ClOrdID, entry.Side.Position);
		else context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, entry.ClOrdLinkID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, check, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, check, entry.BookingUnit);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, check, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.NewOrderCross_NoSides_NoAllocs);
		if (entry.QtyType is not null) context.Validators.QtyType(context, check, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(check, FixTag.OrderQty, entry.Side.Position);
		else context.Validators.OrderQtyData(context, check, entry);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, entry.PreTradeAnonymity);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, check, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, check, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, entry.CoveredOrUncovered);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, check, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.SideComplianceID is not null) context.Validators.SideComplianceID(context, check, entry.SideComplianceID);
		if (entry.SideTimeInForce is not null) context.Validators.SideTimeInForce(context, check, entry.SideTimeInForce);
	}

	static void ValidateNewOrderCross_NoSides_NoAllocs(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderCross_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderCross_NoLegs(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateNewOrderCross_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderCross_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.NewOrderCross.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateNewOrderList_NoOrders(Fix50Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListSeqNo is null) Missing(check, FixTag.ListSeqNo, entry.ClOrdID.Position);
		else context.Validators.ListSeqNo(context, check, entry.ListSeqNo);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, entry.ClOrdLinkID);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, check, entry.SettlInstMode);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, check, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, check, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, check, entry.BookingUnit);
		if (entry.AllocID is not null) context.Validators.AllocID(context, check, entry.AllocID);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, entry.PreallocMethod);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.NewOrderList_NoOrders_NoAllocs);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, check, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, entry.ClearingFeeIndicator);
		if (entry.HandlInst is not null) context.Validators.HandlInst(context, check, entry.HandlInst);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.MinQty is not null) context.Validators.MinQty(context, check, entry.MinQty);
		if (entry.MatchIncrement is not null) context.Validators.MatchIncrement(context, check, entry.MatchIncrement);
		if (entry.MaxPriceLevels is not null) context.Validators.MaxPriceLevels(context, check, entry.MaxPriceLevels);
		if (!Empty((IDisplayInstruction)entry)) context.Validators.DisplayInstruction(context, check, entry);
		if (entry.MaxFloor is not null) context.Validators.MaxFloor(context, check, entry.MaxFloor);
		if (entry.ExDestination is not null) context.Validators.ExDestination(context, check, entry.ExDestination);
		if (entry.ExDestinationIDSource is not null) context.Validators.ExDestinationIDSource(context, check, entry.ExDestinationIDSource);
		if (entry.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, entry.NoTradingSessions);
		Counted(check, entry.NoTradingSessions, entry.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, entry.NoTradingSessionsGroups, context.Validators.NewOrderList_NoOrders_NoTradingSessions);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.ClOrdID.Position);
		else context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.NewOrderList_NoOrders_NoUnderlyings);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.Side is null) Missing(check, FixTag.Side, entry.ClOrdID.Position);
		else context.Validators.Side(context, check, entry.Side);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, check, entry.SideValueInd);
		if (entry.LocateReqd is not null) context.Validators.LocateReqd(context, check, entry.LocateReqd);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.QtyType is not null) context.Validators.QtyType(context, check, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(check, FixTag.OrderQty, entry.ClOrdID.Position);
		else context.Validators.OrderQtyData(context, check, entry);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.PriceProtectionScope is not null) context.Validators.PriceProtectionScope(context, check, entry.PriceProtectionScope);
		if (entry.StopPx is not null) context.Validators.StopPx(context, check, entry.StopPx);
		if (!Empty((ITriggeringInstruction)entry)) context.Validators.TriggeringInstruction(context, check, entry);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.IOIID is not null) context.Validators.IOIID(context, check, entry.IOIID);
		if (entry.QuoteID is not null) context.Validators.QuoteID(context, check, entry.QuoteID);
		if (entry.RefOrderID is not null) context.Validators.RefOrderID(context, check, entry.RefOrderID);
		if (entry.RefOrderIDSource is not null) context.Validators.RefOrderIDSource(context, check, entry.RefOrderIDSource);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, entry.EffectiveTime);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, check, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, entry.GTBookingInst);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.PreTradeAnonymity is not null) context.Validators.PreTradeAnonymity(context, check, entry.PreTradeAnonymity);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, check, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, check, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.Price2 is not null) context.Validators.Price2(context, check, entry.Price2);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, check, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, entry.CoveredOrUncovered);
		if (entry.MaxShow is not null) context.Validators.MaxShow(context, check, entry.MaxShow);
		if (!Empty((IPegInstructions)entry)) context.Validators.PegInstructions(context, check, entry);
		if (!Empty((IDiscretionInstructions)entry)) context.Validators.DiscretionInstructions(context, check, entry);
		if (entry.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, entry.TargetStrategy);
		if (entry.NoStrategyParameters is not null) context.Validators.NoStrategyParameters(context, check, entry.NoStrategyParameters);
		Counted(check, entry.NoStrategyParameters, entry.NoStrategyParametersGroups);
		Each(context, check, FixTag.NoStrategyParameters, entry.NoStrategyParametersGroups, context.Validators.NewOrderList_NoOrders_NoStrategyParameters);
		if (entry.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, entry.TargetStrategyParameters);
		if (entry.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, entry.ParticipationRate);
		if (entry.Designation is not null) context.Validators.Designation(context, check, entry.Designation);
	}

	static void ValidateNewOrderList_NoOrders_NoAllocs(Fix50Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderList_NoOrders_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderList_NoOrders_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderList_NoOrders_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateNewOrderMultileg_NoAllocs(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderMultileg_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderMultileg_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderMultileg_NoLegs(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegAllocID is not null) context.Validators.LegAllocID(context, check, entry.LegAllocID);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, check, entry.NoLegAllocs);
		Counted(check, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		Each(context, check, FixTag.NoLegAllocs, entry.NoLegAllocsGroups, context.Validators.NewOrderMultileg_NoLegs_NoLegAllocs);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegVolatility is not null) context.Validators.LegVolatility(context, check, entry.LegVolatility);
		if (entry.LegDividendYield is not null) context.Validators.LegDividendYield(context, check, entry.LegDividendYield);
		if (entry.LegCurrencyRatio is not null) context.Validators.LegCurrencyRatio(context, check, entry.LegCurrencyRatio);
		if (entry.LegExecInst is not null) context.Validators.LegExecInst(context, check, entry.LegExecInst);
	}

	static void ValidateNewOrderMultileg_NoLegs_NoLegAllocs(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup entry)
	{
		context.Validators.LegAllocAccount(context, check, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, check, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, check, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, check, entry.LegAllocAcctIDSource);
		if (entry.LegAllocSettlCurrency is not null) context.Validators.LegAllocSettlCurrency(context, check, entry.LegAllocSettlCurrency);
	}

	static void ValidateNewOrderMultileg_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.NewOrderMultileg.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateNewOrderSingle_NoAllocs(Fix50Context context, FixCheck check, FixMessage.NewOrderSingle.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderSingle_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.NewOrderSingle.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderSingle_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.NewOrderSingle.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderSingle_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.NewOrderSingle.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateNews_NoNewsRefIDs(Fix50Context context, FixCheck check, FixMessage.News.NoNewsRefIDsGroup entry)
	{
		context.Validators.NewsRefID(context, check, entry.NewsRefID);
		if (entry.NewsRefType is not null) context.Validators.NewsRefType(context, check, entry.NewsRefType);
	}

	static void ValidateNews_NoRoutingIDs(Fix50Context context, FixCheck check, FixMessage.News.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateNews_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.News.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
	}

	static void ValidateNews_NoLegs(Fix50Context context, FixCheck check, FixMessage.News.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateNews_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.News.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNews_NoLinesOfText(Fix50Context context, FixCheck check, FixMessage.News.NoLinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateOrderCancelReplaceRequest_NoAllocs(Fix50Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateOrderCancelReplaceRequest_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateOrderCancelReplaceRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateOrderCancelReplaceRequest_NoStrategyParameters(Fix50Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoStrategyParametersGroup entry)
	{
		context.Validators.StrategyParameterName(context, check, entry.StrategyParameterName);
		if (entry.StrategyParameterType is not null) context.Validators.StrategyParameterType(context, check, entry.StrategyParameterType);
		if (entry.StrategyParameterValue is not null) context.Validators.StrategyParameterValue(context, check, entry.StrategyParameterValue);
	}

	static void ValidateOrderCancelRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.OrderCancelRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateOrderMassActionReport_NoAffectedOrders(Fix50Context context, FixCheck check, FixMessage.OrderMassActionReport.NoAffectedOrdersGroup entry)
	{
		context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.AffectedOrderID is not null) context.Validators.AffectedOrderID(context, check, entry.AffectedOrderID);
		if (entry.AffectedSecondaryOrderID is not null) context.Validators.AffectedSecondaryOrderID(context, check, entry.AffectedSecondaryOrderID);
	}

	static void ValidateOrderMassActionReport_NoNotAffectedOrders(Fix50Context context, FixCheck check, FixMessage.OrderMassActionReport.NoNotAffectedOrdersGroup entry)
	{
		context.Validators.NotAffOrigClOrdID(context, check, entry.NotAffOrigClOrdID);
		if (entry.NotAffectedOrderID is not null) context.Validators.NotAffectedOrderID(context, check, entry.NotAffectedOrderID);
	}

	static void ValidateOrderMassCancelReport_NoAffectedOrders(Fix50Context context, FixCheck check, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup entry)
	{
		context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.AffectedOrderID is not null) context.Validators.AffectedOrderID(context, check, entry.AffectedOrderID);
		if (entry.AffectedSecondaryOrderID is not null) context.Validators.AffectedSecondaryOrderID(context, check, entry.AffectedSecondaryOrderID);
	}

	static void ValidateOrderMassCancelReport_NoNotAffectedOrders(Fix50Context context, FixCheck check, FixMessage.OrderMassCancelReport.NoNotAffectedOrdersGroup entry)
	{
		context.Validators.NotAffOrigClOrdID(context, check, entry.NotAffOrigClOrdID);
		if (entry.NotAffectedOrderID is not null) context.Validators.NotAffectedOrderID(context, check, entry.NotAffectedOrderID);
	}

	static void ValidateOrderStatusRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.OrderStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidatePositionMaintenanceRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionMaintenanceRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceRequest_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidatePositionReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.PositionReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.PositionReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.UnderlyingSettlPrice is not null) context.Validators.UnderlyingSettlPrice(context, check, entry.UnderlyingSettlPrice);
		if (entry.UnderlyingSettlPriceType is not null) context.Validators.UnderlyingSettlPriceType(context, check, entry.UnderlyingSettlPriceType);
		if (entry.UnderlyingDeliveryAmount is not null) context.Validators.UnderlyingDeliveryAmount(context, check, entry.UnderlyingDeliveryAmount);
		if (!Empty((IUnderlyingAmount)entry)) context.Validators.UnderlyingAmount(context, check, entry);
	}

	static void ValidateQuote_NoQuoteQualifiers(Fix50Context context, FixCheck check, FixMessage.Quote.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuote_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.Quote.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuote_NoLegs(Fix50Context context, FixCheck check, FixMessage.Quote.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, check, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, check, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, check, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegBidForwardPoints is not null) context.Validators.LegBidForwardPoints(context, check, entry.LegBidForwardPoints);
		if (entry.LegOfferForwardPoints is not null) context.Validators.LegOfferForwardPoints(context, check, entry.LegOfferForwardPoints);
	}

	static void ValidateQuoteCancel_NoQuoteEntries(Fix50Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.QuoteCancel_NoQuoteEntries_NoUnderlyings);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.QuoteCancel_NoQuoteEntries_NoLegs);
	}

	static void ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteCancel_NoQuoteEntries_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.QuoteRequest_NoRelatedSym_NoUnderlyings);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, check, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, check, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.QtyType is not null) context.Validators.QtyType(context, check, entry.QtyType);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, check, entry);
		if (entry.MinQty is not null) context.Validators.MinQty(context, check, entry.MinQty);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, check, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (!Empty((IRateSource)entry)) context.Validators.RateSource(context, check, entry);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.QuoteRequest_NoRelatedSym_NoLegs);
		if (entry.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, check, entry.NoQuoteQualifiers);
		Counted(check, entry.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups);
		Each(context, check, FixTag.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups, context.Validators.QuoteRequest_NoRelatedSym_NoQuoteQualifiers);
		if (entry.QuotePriceType is not null) context.Validators.QuotePriceType(context, check, entry.QuotePriceType);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, entry.ValidUntilTime);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.Price2 is not null) context.Validators.Price2(context, check, entry.Price2);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
	}

	static void ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers(Fix50Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.QuoteRequestReject_NoRelatedSym_NoUnderlyings);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, check, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, check, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, entry.TradeOriginationDate);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.QtyType is not null) context.Validators.QtyType(context, check, entry.QtyType);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, check, entry);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, check, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.QuoteRequestReject_NoRelatedSym_NoLegs);
		if (entry.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, check, entry.NoQuoteQualifiers);
		Counted(check, entry.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups);
		Each(context, check, FixTag.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups, context.Validators.QuoteRequestReject_NoRelatedSym_NoQuoteQualifiers);
		if (entry.QuotePriceType is not null) context.Validators.QuotePriceType(context, check, entry.QuotePriceType);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.Price2 is not null) context.Validators.Price2(context, check, entry.Price2);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers(Fix50Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteResponse_NoQuoteQualifiers(Fix50Context context, FixCheck check, FixMessage.QuoteResponse.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteResponse_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteResponse.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteResponse_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteResponse.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, check, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, check, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, check, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegBidForwardPoints is not null) context.Validators.LegBidForwardPoints(context, check, entry.LegBidForwardPoints);
		if (entry.LegOfferForwardPoints is not null) context.Validators.LegOfferForwardPoints(context, check, entry.LegOfferForwardPoints);
	}

	static void ValidateQuoteStatusReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteStatusReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteStatusReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteStatusReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegOrderQty is not null) context.Validators.LegOrderQty(context, check, entry.LegOrderQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateQuoteStatusReport_NoQuoteQualifiers(Fix50Context context, FixCheck check, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteStatusRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteStatusRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.QuoteStatusRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRFQRequest_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.RFQRequest_NoRelatedSym_NoUnderlyings);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.RFQRequest_NoRelatedSym_NoLegs);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, check, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, check, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateRFQRequest_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateRFQRequest_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRegistrationInstructions_NoRegistDtls(Fix50Context context, FixCheck check, FixMessage.RegistrationInstructions.NoRegistDtlsGroup entry)
	{
		context.Validators.RegistDtls(context, check, entry.RegistDtls);
		if (entry.RegistEmail is not null) context.Validators.RegistEmail(context, check, entry.RegistEmail);
		if (entry.MailingDtls is not null) context.Validators.MailingDtls(context, check, entry.MailingDtls);
		if (entry.MailingInst is not null) context.Validators.MailingInst(context, check, entry.MailingInst);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.OwnerType is not null) context.Validators.OwnerType(context, check, entry.OwnerType);
		if (entry.DateOfBirth is not null) context.Validators.DateOfBirth(context, check, entry.DateOfBirth);
		if (entry.InvestorCountryOfResidence is not null) context.Validators.InvestorCountryOfResidence(context, check, entry.InvestorCountryOfResidence);
	}

	static void ValidateRegistrationInstructions_NoDistribInsts(Fix50Context context, FixCheck check, FixMessage.RegistrationInstructions.NoDistribInstsGroup entry)
	{
		context.Validators.DistribPaymentMethod(context, check, entry.DistribPaymentMethod);
		if (entry.DistribPercentage is not null) context.Validators.DistribPercentage(context, check, entry.DistribPercentage);
		if (entry.CashDistribCurr is not null) context.Validators.CashDistribCurr(context, check, entry.CashDistribCurr);
		if (entry.CashDistribAgentName is not null) context.Validators.CashDistribAgentName(context, check, entry.CashDistribAgentName);
		if (entry.CashDistribAgentCode is not null) context.Validators.CashDistribAgentCode(context, check, entry.CashDistribAgentCode);
		if (entry.CashDistribAgentAcctNumber is not null) context.Validators.CashDistribAgentAcctNumber(context, check, entry.CashDistribAgentAcctNumber);
		if (entry.CashDistribPayRef is not null) context.Validators.CashDistribPayRef(context, check, entry.CashDistribPayRef);
		if (entry.CashDistribAgentAcctName is not null) context.Validators.CashDistribAgentAcctName(context, check, entry.CashDistribAgentAcctName);
	}

	static void ValidateRequestForPositions_NoLegs(Fix50Context context, FixCheck check, FixMessage.RequestForPositions.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRequestForPositions_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.RequestForPositions.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateRequestForPositions_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.RequestForPositions.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateRequestForPositionsAck_NoLegs(Fix50Context context, FixCheck check, FixMessage.RequestForPositionsAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRequestForPositionsAck_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinition_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinition_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityDefinition_NoMarketSegments(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition.NoMarketSegmentsGroup entry)
	{
		context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.SecurityDefinition_NoMarketSegments_NoStrikeRules);
	}

	static void ValidateSecurityDefinition_NoMarketSegments_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.SecurityDefinition_NoMarketSegments_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateSecurityDefinition_NoMarketSegments_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.SecurityDefinition.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateSecurityDefinitionRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinitionRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityDefinitionUpdateReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinitionUpdateReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityDefinitionUpdateReport_NoMarketSegments(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup entry)
	{
		context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.SecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules);
	}

	static void ValidateSecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.SecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateSecurityDefinitionUpdateReport_NoMarketSegments_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.SecurityDefinitionUpdateReport.NoMarketSegmentsGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateSecurityList_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.SecurityList_NoRelatedSym_NoStrikeRules);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.SecurityList_NoRelatedSym_NoUnderlyings);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.SecurityList_NoRelatedSym_NoLegs);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (entry.RelSymTransactTime is not null) context.Validators.RelSymTransactTime(context, check, entry.RelSymTransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateSecurityList_NoRelatedSym_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.SecurityList_NoRelatedSym_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateSecurityList_NoRelatedSym_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateSecurityList_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityList_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateSecurityListRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityListRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityListRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityListRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityListUpdateReport_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup entry)
	{
		context.Validators.ListUpdateAction(context, check, entry.ListUpdateAction);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
		if (!Empty((ISecurityTradingRules)entry)) context.Validators.SecurityTradingRules(context, check, entry);
		if (entry.NoStrikeRules is not null) context.Validators.NoStrikeRules(context, check, entry.NoStrikeRules);
		Counted(check, entry.NoStrikeRules, entry.NoStrikeRulesGroups);
		Each(context, check, FixTag.NoStrikeRules, entry.NoStrikeRulesGroups, context.Validators.SecurityListUpdateReport_NoRelatedSym_NoStrikeRules);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.SecurityListUpdateReport_NoRelatedSym_NoUnderlyings);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.SecurityListUpdateReport_NoRelatedSym_NoLegs);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (entry.RelSymTransactTime is not null) context.Validators.RelSymTransactTime(context, check, entry.RelSymTransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateSecurityListUpdateReport_NoRelatedSym_NoStrikeRules(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup entry)
	{
		context.Validators.StrikeRuleID(context, check, entry.StrikeRuleID);
		if (entry.StartStrikePxRange is not null) context.Validators.StartStrikePxRange(context, check, entry.StartStrikePxRange);
		if (entry.EndStrikePxRange is not null) context.Validators.EndStrikePxRange(context, check, entry.EndStrikePxRange);
		if (entry.StrikeIncrement is not null) context.Validators.StrikeIncrement(context, check, entry.StrikeIncrement);
		if (entry.StrikeExerciseStyle is not null) context.Validators.StrikeExerciseStyle(context, check, entry.StrikeExerciseStyle);
		if (entry.NoMaturityRules is not null) context.Validators.NoMaturityRules(context, check, entry.NoMaturityRules);
		Counted(check, entry.NoMaturityRules, entry.NoMaturityRulesGroups);
		Each(context, check, FixTag.NoMaturityRules, entry.NoMaturityRulesGroups, context.Validators.SecurityListUpdateReport_NoRelatedSym_NoStrikeRules_NoMaturityRules);
	}

	static void ValidateSecurityListUpdateReport_NoRelatedSym_NoStrikeRules_NoMaturityRules(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoStrikeRulesGroup.NoMaturityRulesGroup entry)
	{
		context.Validators.MaturityRuleID(context, check, entry.MaturityRuleID);
		if (entry.MaturityMonthYearFormat is not null) context.Validators.MaturityMonthYearFormat(context, check, entry.MaturityMonthYearFormat);
		if (entry.MaturityMonthYearIncrementUnits is not null) context.Validators.MaturityMonthYearIncrementUnits(context, check, entry.MaturityMonthYearIncrementUnits);
		if (entry.StartMaturityMonthYear is not null) context.Validators.StartMaturityMonthYear(context, check, entry.StartMaturityMonthYear);
		if (entry.EndMaturityMonthYear is not null) context.Validators.EndMaturityMonthYear(context, check, entry.EndMaturityMonthYear);
		if (entry.MaturityMonthYearIncrement is not null) context.Validators.MaturityMonthYearIncrement(context, check, entry.MaturityMonthYearIncrement);
	}

	static void ValidateSecurityListUpdateReport_NoRelatedSym_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityListUpdateReport_NoRelatedSym_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityListUpdateReport.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateSecurityStatusMessage_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityStatusMessage.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityStatusMessage_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityStatusMessage.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityStatusRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityStatusRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.SecurityStatusRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityTypes_NoSecurityTypes(Fix50Context context, FixCheck check, FixMessage.SecurityTypes.NoSecurityTypesGroup entry)
	{
		context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, entry.SecuritySubType);
		if (entry.Product is not null) context.Validators.Product(context, check, entry.Product);
		if (entry.CFICode is not null) context.Validators.CFICode(context, check, entry.CFICode);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
	}

	static void ValidateSettlementInstructions_NoSettlInst(Fix50Context context, FixCheck check, FixMessage.SettlementInstructions.NoSettlInstGroup entry)
	{
		context.Validators.SettlInstID(context, check, entry.SettlInstID);
		if (entry.SettlInstTransType is not null) context.Validators.SettlInstTransType(context, check, entry.SettlInstTransType);
		if (entry.SettlInstRefID is not null) context.Validators.SettlInstRefID(context, check, entry.SettlInstRefID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Product is not null) context.Validators.Product(context, check, entry.Product);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.CFICode is not null) context.Validators.CFICode(context, check, entry.CFICode);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, entry.EffectiveTime);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, entry.LastUpdateTime);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
		if (entry.PaymentMethod is not null) context.Validators.PaymentMethod(context, check, entry.PaymentMethod);
		if (entry.PaymentRef is not null) context.Validators.PaymentRef(context, check, entry.PaymentRef);
		if (entry.CardHolderName is not null) context.Validators.CardHolderName(context, check, entry.CardHolderName);
		if (entry.CardNumber is not null) context.Validators.CardNumber(context, check, entry.CardNumber);
		if (entry.CardStartDate is not null) context.Validators.CardStartDate(context, check, entry.CardStartDate);
		if (entry.CardExpDate is not null) context.Validators.CardExpDate(context, check, entry.CardExpDate);
		if (entry.CardIssNum is not null) context.Validators.CardIssNum(context, check, entry.CardIssNum);
		if (entry.PaymentDate is not null) context.Validators.PaymentDate(context, check, entry.PaymentDate);
		if (entry.PaymentRemitterID is not null) context.Validators.PaymentRemitterID(context, check, entry.PaymentRemitterID);
	}

	static void ValidateSettlementObligationReport_NoSettlOblig(Fix50Context context, FixCheck check, FixMessage.SettlementObligationReport.NoSettlObligGroup entry)
	{
		context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SettlObligID is not null) context.Validators.SettlObligID(context, check, entry.SettlObligID);
		if (entry.SettlObligTransType is not null) context.Validators.SettlObligTransType(context, check, entry.SettlObligTransType);
		if (entry.SettlObligRefID is not null) context.Validators.SettlObligRefID(context, check, entry.SettlObligRefID);
		if (entry.CcyAmt is not null) context.Validators.CcyAmt(context, check, entry.CcyAmt);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, check, entry.SettlDate);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, entry.EffectiveTime);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, entry.LastUpdateTime);
		if (entry.NoSettlDetails is not null) context.Validators.NoSettlDetails(context, check, entry.NoSettlDetails);
		Counted(check, entry.NoSettlDetails, entry.NoSettlDetailsGroups);
		Each(context, check, FixTag.NoSettlDetails, entry.NoSettlDetailsGroups, context.Validators.SettlementObligationReport_NoSettlOblig_NoSettlDetails);
	}

	static void ValidateSettlementObligationReport_NoSettlOblig_NoSettlDetails(Fix50Context context, FixCheck check, FixMessage.SettlementObligationReport.NoSettlObligGroup.NoSettlDetailsGroup entry)
	{
		context.Validators.SettlObligSource(context, check, entry.SettlObligSource);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, check, entry);
	}

	static void ValidateStreamAssignmentReport_NoAsgnReqs(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentReport.NoAsgnReqsGroup entry)
	{
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, entry.NoRelatedSym);
		Counted(check, entry.NoRelatedSym, entry.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, entry.NoRelatedSymGroups, context.Validators.StreamAssignmentReport_NoAsgnReqs_NoRelatedSym);
	}

	static void ValidateStreamAssignmentReport_NoAsgnReqs_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentReport.NoAsgnReqsGroup.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.StreamAsgnType is not null) context.Validators.StreamAsgnType(context, check, entry.StreamAsgnType);
		if (entry.MDStreamID is not null) context.Validators.MDStreamID(context, check, entry.MDStreamID);
		if (entry.StreamAsgnRejReason is not null) context.Validators.StreamAsgnRejReason(context, check, entry.StreamAsgnRejReason);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateStreamAssignmentRequest_NoAsgnReqs(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup entry)
	{
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, entry.NoRelatedSym);
		Counted(check, entry.NoRelatedSym, entry.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, entry.NoRelatedSymGroups, context.Validators.StreamAssignmentRequest_NoAsgnReqs_NoRelatedSym);
	}

	static void ValidateStreamAssignmentRequest_NoAsgnReqs_NoRelatedSym(Fix50Context context, FixCheck check, FixMessage.StreamAssignmentRequest.NoAsgnReqsGroup.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.SettlType is not null) context.Validators.SettlType(context, check, entry.SettlType);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.MDStreamID is not null) context.Validators.MDStreamID(context, check, entry.MDStreamID);
	}

	static void ValidateTradeCaptureReport_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReport_NoLegs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegReportID is not null) context.Validators.LegReportID(context, check, entry.LegReportID);
		if (entry.LegNumber is not null) context.Validators.LegNumber(context, check, entry.LegNumber);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
		if (entry.LegLastForwardPoints is not null) context.Validators.LegLastForwardPoints(context, check, entry.LegLastForwardPoints);
		if (entry.LegCalculatedCcyLastQty is not null) context.Validators.LegCalculatedCcyLastQty(context, check, entry.LegCalculatedCcyLastQty);
		if (entry.LegGrossTradeAmt is not null) context.Validators.LegGrossTradeAmt(context, check, entry.LegGrossTradeAmt);
		if (entry.LegVolatility is not null) context.Validators.LegVolatility(context, check, entry.LegVolatility);
		if (entry.LegDividendYield is not null) context.Validators.LegDividendYield(context, check, entry.LegDividendYield);
		if (entry.LegCurrencyRatio is not null) context.Validators.LegCurrencyRatio(context, check, entry.LegCurrencyRatio);
		if (entry.LegExecInst is not null) context.Validators.LegExecInst(context, check, entry.LegExecInst);
		if (entry.LegLastQty is not null) context.Validators.LegLastQty(context, check, entry.LegLastQty);
		if (entry.NoOfLegUnderlyings is not null) context.Validators.NoOfLegUnderlyings(context, check, entry.NoOfLegUnderlyings);
		Counted(check, entry.NoOfLegUnderlyings, entry.NoOfLegUnderlyingsGroups);
		Each(context, check, FixTag.NoOfLegUnderlyings, entry.NoOfLegUnderlyingsGroups, context.Validators.TradeCaptureReport_NoLegs_NoOfLegUnderlyings);
	}

	static void ValidateTradeCaptureReport_NoLegs_NoOfLegUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup entry)
	{
		context.Validators.UnderlyingLegSymbol(context, check, entry.UnderlyingLegSymbol);
		if (entry.UnderlyingLegSymbolSfx is not null) context.Validators.UnderlyingLegSymbolSfx(context, check, entry.UnderlyingLegSymbolSfx);
		if (entry.UnderlyingLegSecurityID is not null) context.Validators.UnderlyingLegSecurityID(context, check, entry.UnderlyingLegSecurityID);
		if (entry.UnderlyingLegSecurityIDSource is not null) context.Validators.UnderlyingLegSecurityIDSource(context, check, entry.UnderlyingLegSecurityIDSource);
		if (entry.NoUnderlyingLegSecurityAltID is not null) context.Validators.NoUnderlyingLegSecurityAltID(context, check, entry.NoUnderlyingLegSecurityAltID);
		Counted(check, entry.NoUnderlyingLegSecurityAltID, entry.NoUnderlyingLegSecurityAltIDGroups);
		Each(context, check, FixTag.NoUnderlyingLegSecurityAltID, entry.NoUnderlyingLegSecurityAltIDGroups, context.Validators.TradeCaptureReport_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID);
		if (entry.UnderlyingLegCFICode is not null) context.Validators.UnderlyingLegCFICode(context, check, entry.UnderlyingLegCFICode);
		if (entry.UnderlyingLegSecurityType is not null) context.Validators.UnderlyingLegSecurityType(context, check, entry.UnderlyingLegSecurityType);
		if (entry.UnderlyingLegSecuritySubType is not null) context.Validators.UnderlyingLegSecuritySubType(context, check, entry.UnderlyingLegSecuritySubType);
		if (entry.UnderlyingLegMaturityMonthYear is not null) context.Validators.UnderlyingLegMaturityMonthYear(context, check, entry.UnderlyingLegMaturityMonthYear);
		if (entry.UnderlyingLegMaturityDate is not null) context.Validators.UnderlyingLegMaturityDate(context, check, entry.UnderlyingLegMaturityDate);
		if (entry.UnderlyingLegMaturityTime is not null) context.Validators.UnderlyingLegMaturityTime(context, check, entry.UnderlyingLegMaturityTime);
		if (entry.UnderlyingLegStrikePrice is not null) context.Validators.UnderlyingLegStrikePrice(context, check, entry.UnderlyingLegStrikePrice);
		if (entry.UnderlyingLegOptAttribute is not null) context.Validators.UnderlyingLegOptAttribute(context, check, entry.UnderlyingLegOptAttribute);
		if (entry.UnderlyingLegPutOrCall is not null) context.Validators.UnderlyingLegPutOrCall(context, check, entry.UnderlyingLegPutOrCall);
		if (entry.UnderlyingLegSecurityExchange is not null) context.Validators.UnderlyingLegSecurityExchange(context, check, entry.UnderlyingLegSecurityExchange);
		if (entry.UnderlyingLegSecurityDesc is not null) context.Validators.UnderlyingLegSecurityDesc(context, check, entry.UnderlyingLegSecurityDesc);
	}

	static void ValidateTradeCaptureReport_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup entry)
	{
		context.Validators.UnderlyingLegSecurityAltID(context, check, entry.UnderlyingLegSecurityAltID);
		if (entry.UnderlyingLegSecurityAltIDSource is not null) context.Validators.UnderlyingLegSecurityAltIDSource(context, check, entry.UnderlyingLegSecurityAltIDSource);
	}

	static void ValidateTradeCaptureReport_NoSides(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.SideExecID is not null) context.Validators.SideExecID(context, check, entry.SideExecID);
		if (entry.OrderDelay is not null) context.Validators.OrderDelay(context, check, entry.OrderDelay);
		if (entry.OrderDelayUnit is not null) context.Validators.OrderDelayUnit(context, check, entry.OrderDelayUnit);
		if (entry.SideLastQty is not null) context.Validators.SideLastQty(context, check, entry.SideLastQty);
		if (entry.SideTradeReportID is not null) context.Validators.SideTradeReportID(context, check, entry.SideTradeReportID);
		if (entry.SideFillStationCd is not null) context.Validators.SideFillStationCd(context, check, entry.SideFillStationCd);
		if (entry.SideReasonCd is not null) context.Validators.SideReasonCd(context, check, entry.SideReasonCd);
		if (entry.RptSeq is not null) context.Validators.RptSeq(context, check, entry.RptSeq);
		if (entry.SideTrdSubTyp is not null) context.Validators.SideTrdSubTyp(context, check, entry.SideTrdSubTyp);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SideCurrency is not null) context.Validators.SideCurrency(context, check, entry.SideCurrency);
		if (entry.SideSettlCurrency is not null) context.Validators.SideSettlCurrency(context, check, entry.SideSettlCurrency);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.OddLot is not null) context.Validators.OddLot(context, check, entry.OddLot);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, check, entry.NoClearingInstructions);
		Counted(check, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		Each(context, check, FixTag.NoClearingInstructions, entry.NoClearingInstructionsGroups, context.Validators.TradeCaptureReport_NoSides_NoClearingInstructions);
		if (entry.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, entry.TradeInputSource);
		if (entry.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, check, entry.TradeInputDevice);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TimeBracket is not null) context.Validators.TimeBracket(context, check, entry.TimeBracket);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, entry.NumDaysInterest);
		if (entry.ExDate is not null) context.Validators.ExDate(context, check, entry.ExDate);
		if (entry.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, entry.AccruedInterestRate);
		if (entry.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, entry.AccruedInterestAmt);
		if (entry.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, entry.InterestAtMaturity);
		if (entry.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, entry.EndAccruedInterestAmt);
		if (entry.StartCash is not null) context.Validators.StartCash(context, check, entry.StartCash);
		if (entry.EndCash is not null) context.Validators.EndCash(context, check, entry.EndCash);
		if (entry.Concession is not null) context.Validators.Concession(context, check, entry.Concession);
		if (entry.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, entry.TotalTakedown);
		if (entry.NetMoney is not null) context.Validators.NetMoney(context, check, entry.NetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, check, entry.PositionEffect);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.SideMultiLegReportingType is not null) context.Validators.SideMultiLegReportingType(context, check, entry.SideMultiLegReportingType);
		if (entry.NoContAmts is not null) context.Validators.NoContAmts(context, check, entry.NoContAmts);
		Counted(check, entry.NoContAmts, entry.NoContAmtsGroups);
		Each(context, check, FixTag.NoContAmts, entry.NoContAmtsGroups, context.Validators.TradeCaptureReport_NoSides_NoContAmts);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.TradeCaptureReport_NoSides_NoMiscFees);
		if (entry.ExchangeRule is not null) context.Validators.ExchangeRule(context, check, entry.ExchangeRule);
		if (entry.TradeAllocIndicator is not null) context.Validators.TradeAllocIndicator(context, check, entry.TradeAllocIndicator);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, check, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.TradeCaptureReport_NoSides_NoAllocs);
		if (!Empty((ISideTrdRegTS)entry)) context.Validators.SideTrdRegTS(context, check, entry);
		if (entry.NoSettlDetails is not null) context.Validators.NoSettlDetails(context, check, entry.NoSettlDetails);
		Counted(check, entry.NoSettlDetails, entry.NoSettlDetailsGroups);
		Each(context, check, FixTag.NoSettlDetails, entry.NoSettlDetailsGroups, context.Validators.TradeCaptureReport_NoSides_NoSettlDetails);
		if (entry.SideGrossTradeAmt is not null) context.Validators.SideGrossTradeAmt(context, check, entry.SideGrossTradeAmt);
		if (entry.AggressorIndicator is not null) context.Validators.AggressorIndicator(context, check, entry.AggressorIndicator);
		if (entry.ExchangeSpecialInstructions is not null) context.Validators.ExchangeSpecialInstructions(context, check, entry.ExchangeSpecialInstructions);
		if (entry.OrderCategory is not null) context.Validators.OrderCategory(context, check, entry.OrderCategory);
		if (entry.SideLiquidityInd is not null) context.Validators.SideLiquidityInd(context, check, entry.SideLiquidityInd);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (entry.RefOrderID is not null) context.Validators.RefOrderID(context, check, entry.RefOrderID);
		if (entry.RefOrderIDSource is not null) context.Validators.RefOrderIDSource(context, check, entry.RefOrderIDSource);
		if (entry.RefOrdIDReason is not null) context.Validators.RefOrdIDReason(context, check, entry.RefOrdIDReason);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.StopPx is not null) context.Validators.StopPx(context, check, entry.StopPx);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.OrdStatus is not null) context.Validators.OrdStatus(context, check, entry.OrdStatus);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, check, entry);
		if (entry.LeavesQty is not null) context.Validators.LeavesQty(context, check, entry.LeavesQty);
		if (entry.CumQty is not null) context.Validators.CumQty(context, check, entry.CumQty);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (!Empty((IDisplayInstruction)entry)) context.Validators.DisplayInstruction(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.OrigCustOrderCapacity is not null) context.Validators.OrigCustOrderCapacity(context, check, entry.OrigCustOrderCapacity);
		if (entry.OrderInputDevice is not null) context.Validators.OrderInputDevice(context, check, entry.OrderInputDevice);
		if (entry.LotType is not null) context.Validators.LotType(context, check, entry.LotType);
		if (entry.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, entry.TransBkdTime);
		if (entry.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, entry.OrigOrdModTime);
	}

	static void ValidateTradeCaptureReport_NoSides_NoClearingInstructions(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateTradeCaptureReport_NoSides_NoContAmts(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup entry)
	{
		context.Validators.ContAmtType(context, check, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, check, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, check, entry.ContAmtCurr);
	}

	static void ValidateTradeCaptureReport_NoSides_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateTradeCaptureReport_NoSides_NoAllocs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.AllocMethod is not null) context.Validators.AllocMethod(context, check, entry.AllocMethod);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocClearingFeeIndicator is not null) context.Validators.AllocClearingFeeIndicator(context, check, entry.AllocClearingFeeIndicator);
	}

	static void ValidateTradeCaptureReport_NoSides_NoSettlDetails(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoSettlDetailsGroup entry)
	{
		context.Validators.SettlObligSource(context, check, entry.SettlObligSource);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, check, entry);
	}

	static void ValidateTradeCaptureReport_NoTrdRepIndicators(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReport.NoTrdRepIndicatorsGroup entry)
	{
		context.Validators.TrdRepPartyRole(context, check, entry.TrdRepPartyRole);
		if (entry.TrdRepIndicator is not null) context.Validators.TrdRepIndicator(context, check, entry.TrdRepIndicator);
	}

	static void ValidateTradeCaptureReportAck_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReportAck_NoTrdRepIndicators(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoTrdRepIndicatorsGroup entry)
	{
		context.Validators.TrdRepPartyRole(context, check, entry.TrdRepPartyRole);
		if (entry.TrdRepIndicator is not null) context.Validators.TrdRepIndicator(context, check, entry.TrdRepIndicator);
	}

	static void ValidateTradeCaptureReportAck_NoLegs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegReportID is not null) context.Validators.LegReportID(context, check, entry.LegReportID);
		if (entry.LegNumber is not null) context.Validators.LegNumber(context, check, entry.LegNumber);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
		if (entry.LegLastForwardPoints is not null) context.Validators.LegLastForwardPoints(context, check, entry.LegLastForwardPoints);
		if (entry.LegCalculatedCcyLastQty is not null) context.Validators.LegCalculatedCcyLastQty(context, check, entry.LegCalculatedCcyLastQty);
		if (entry.LegGrossTradeAmt is not null) context.Validators.LegGrossTradeAmt(context, check, entry.LegGrossTradeAmt);
		if (entry.LegVolatility is not null) context.Validators.LegVolatility(context, check, entry.LegVolatility);
		if (entry.LegDividendYield is not null) context.Validators.LegDividendYield(context, check, entry.LegDividendYield);
		if (entry.LegCurrencyRatio is not null) context.Validators.LegCurrencyRatio(context, check, entry.LegCurrencyRatio);
		if (entry.LegExecInst is not null) context.Validators.LegExecInst(context, check, entry.LegExecInst);
		if (entry.LegLastQty is not null) context.Validators.LegLastQty(context, check, entry.LegLastQty);
		if (entry.NoOfLegUnderlyings is not null) context.Validators.NoOfLegUnderlyings(context, check, entry.NoOfLegUnderlyings);
		Counted(check, entry.NoOfLegUnderlyings, entry.NoOfLegUnderlyingsGroups);
		Each(context, check, FixTag.NoOfLegUnderlyings, entry.NoOfLegUnderlyingsGroups, context.Validators.TradeCaptureReportAck_NoLegs_NoOfLegUnderlyings);
	}

	static void ValidateTradeCaptureReportAck_NoLegs_NoOfLegUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup entry)
	{
		context.Validators.UnderlyingLegSymbol(context, check, entry.UnderlyingLegSymbol);
		if (entry.UnderlyingLegSymbolSfx is not null) context.Validators.UnderlyingLegSymbolSfx(context, check, entry.UnderlyingLegSymbolSfx);
		if (entry.UnderlyingLegSecurityID is not null) context.Validators.UnderlyingLegSecurityID(context, check, entry.UnderlyingLegSecurityID);
		if (entry.UnderlyingLegSecurityIDSource is not null) context.Validators.UnderlyingLegSecurityIDSource(context, check, entry.UnderlyingLegSecurityIDSource);
		if (entry.NoUnderlyingLegSecurityAltID is not null) context.Validators.NoUnderlyingLegSecurityAltID(context, check, entry.NoUnderlyingLegSecurityAltID);
		Counted(check, entry.NoUnderlyingLegSecurityAltID, entry.NoUnderlyingLegSecurityAltIDGroups);
		Each(context, check, FixTag.NoUnderlyingLegSecurityAltID, entry.NoUnderlyingLegSecurityAltIDGroups, context.Validators.TradeCaptureReportAck_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID);
		if (entry.UnderlyingLegCFICode is not null) context.Validators.UnderlyingLegCFICode(context, check, entry.UnderlyingLegCFICode);
		if (entry.UnderlyingLegSecurityType is not null) context.Validators.UnderlyingLegSecurityType(context, check, entry.UnderlyingLegSecurityType);
		if (entry.UnderlyingLegSecuritySubType is not null) context.Validators.UnderlyingLegSecuritySubType(context, check, entry.UnderlyingLegSecuritySubType);
		if (entry.UnderlyingLegMaturityMonthYear is not null) context.Validators.UnderlyingLegMaturityMonthYear(context, check, entry.UnderlyingLegMaturityMonthYear);
		if (entry.UnderlyingLegMaturityDate is not null) context.Validators.UnderlyingLegMaturityDate(context, check, entry.UnderlyingLegMaturityDate);
		if (entry.UnderlyingLegMaturityTime is not null) context.Validators.UnderlyingLegMaturityTime(context, check, entry.UnderlyingLegMaturityTime);
		if (entry.UnderlyingLegStrikePrice is not null) context.Validators.UnderlyingLegStrikePrice(context, check, entry.UnderlyingLegStrikePrice);
		if (entry.UnderlyingLegOptAttribute is not null) context.Validators.UnderlyingLegOptAttribute(context, check, entry.UnderlyingLegOptAttribute);
		if (entry.UnderlyingLegPutOrCall is not null) context.Validators.UnderlyingLegPutOrCall(context, check, entry.UnderlyingLegPutOrCall);
		if (entry.UnderlyingLegSecurityExchange is not null) context.Validators.UnderlyingLegSecurityExchange(context, check, entry.UnderlyingLegSecurityExchange);
		if (entry.UnderlyingLegSecurityDesc is not null) context.Validators.UnderlyingLegSecurityDesc(context, check, entry.UnderlyingLegSecurityDesc);
	}

	static void ValidateTradeCaptureReportAck_NoLegs_NoOfLegUnderlyings_NoUnderlyingLegSecurityAltID(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoLegsGroup.NoOfLegUnderlyingsGroup.NoUnderlyingLegSecurityAltIDGroup entry)
	{
		context.Validators.UnderlyingLegSecurityAltID(context, check, entry.UnderlyingLegSecurityAltID);
		if (entry.UnderlyingLegSecurityAltIDSource is not null) context.Validators.UnderlyingLegSecurityAltIDSource(context, check, entry.UnderlyingLegSecurityAltIDSource);
	}

	static void ValidateTradeCaptureReportAck_NoSides(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.SideExecID is not null) context.Validators.SideExecID(context, check, entry.SideExecID);
		if (entry.OrderDelay is not null) context.Validators.OrderDelay(context, check, entry.OrderDelay);
		if (entry.OrderDelayUnit is not null) context.Validators.OrderDelayUnit(context, check, entry.OrderDelayUnit);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, check, entry.AccountType);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.OddLot is not null) context.Validators.OddLot(context, check, entry.OddLot);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, check, entry.NoClearingInstructions);
		Counted(check, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		Each(context, check, FixTag.NoClearingInstructions, entry.NoClearingInstructionsGroups, context.Validators.TradeCaptureReportAck_NoSides_NoClearingInstructions);
		if (entry.TradeInputSource is not null) context.Validators.TradeInputSource(context, check, entry.TradeInputSource);
		if (entry.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, check, entry.TradeInputDevice);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TimeBracket is not null) context.Validators.TimeBracket(context, check, entry.TimeBracket);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SideCurrency is not null) context.Validators.SideCurrency(context, check, entry.SideCurrency);
		if (entry.SideSettlCurrency is not null) context.Validators.SideSettlCurrency(context, check, entry.SideSettlCurrency);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, entry.NumDaysInterest);
		if (entry.ExDate is not null) context.Validators.ExDate(context, check, entry.ExDate);
		if (entry.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, entry.AccruedInterestRate);
		if (entry.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, entry.AccruedInterestAmt);
		if (entry.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, check, entry.InterestAtMaturity);
		if (entry.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, check, entry.EndAccruedInterestAmt);
		if (entry.StartCash is not null) context.Validators.StartCash(context, check, entry.StartCash);
		if (entry.EndCash is not null) context.Validators.EndCash(context, check, entry.EndCash);
		if (entry.Concession is not null) context.Validators.Concession(context, check, entry.Concession);
		if (entry.TotalTakedown is not null) context.Validators.TotalTakedown(context, check, entry.TotalTakedown);
		if (entry.NetMoney is not null) context.Validators.NetMoney(context, check, entry.NetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, check, entry.PositionEffect);
		if (entry.SideMultiLegReportingType is not null) context.Validators.SideMultiLegReportingType(context, check, entry.SideMultiLegReportingType);
		if (entry.NoContAmts is not null) context.Validators.NoContAmts(context, check, entry.NoContAmts);
		Counted(check, entry.NoContAmts, entry.NoContAmtsGroups);
		Each(context, check, FixTag.NoContAmts, entry.NoContAmtsGroups, context.Validators.TradeCaptureReportAck_NoSides_NoContAmts);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, check, entry);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.TradeCaptureReportAck_NoSides_NoMiscFees);
		if (entry.ExchangeRule is not null) context.Validators.ExchangeRule(context, check, entry.ExchangeRule);
		if (entry.NoSettlDetails is not null) context.Validators.NoSettlDetails(context, check, entry.NoSettlDetails);
		Counted(check, entry.NoSettlDetails, entry.NoSettlDetailsGroups);
		Each(context, check, FixTag.NoSettlDetails, entry.NoSettlDetailsGroups, context.Validators.TradeCaptureReportAck_NoSides_NoSettlDetails);
		if (entry.TradeAllocIndicator is not null) context.Validators.TradeAllocIndicator(context, check, entry.TradeAllocIndicator);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, check, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.TradeCaptureReportAck_NoSides_NoAllocs);
		if (entry.SideGrossTradeAmt is not null) context.Validators.SideGrossTradeAmt(context, check, entry.SideGrossTradeAmt);
		if (entry.AggressorIndicator is not null) context.Validators.AggressorIndicator(context, check, entry.AggressorIndicator);
		if (entry.SideLastQty is not null) context.Validators.SideLastQty(context, check, entry.SideLastQty);
		if (entry.SideTradeReportID is not null) context.Validators.SideTradeReportID(context, check, entry.SideTradeReportID);
		if (entry.SideFillStationCd is not null) context.Validators.SideFillStationCd(context, check, entry.SideFillStationCd);
		if (entry.SideReasonCd is not null) context.Validators.SideReasonCd(context, check, entry.SideReasonCd);
		if (entry.RptSeq is not null) context.Validators.RptSeq(context, check, entry.RptSeq);
		if (entry.SideTrdSubTyp is not null) context.Validators.SideTrdSubTyp(context, check, entry.SideTrdSubTyp);
		if (entry.OrderCategory is not null) context.Validators.OrderCategory(context, check, entry.OrderCategory);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (entry.RefOrderID is not null) context.Validators.RefOrderID(context, check, entry.RefOrderID);
		if (entry.RefOrderIDSource is not null) context.Validators.RefOrderIDSource(context, check, entry.RefOrderIDSource);
		if (entry.RefOrdIDReason is not null) context.Validators.RefOrdIDReason(context, check, entry.RefOrdIDReason);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.StopPx is not null) context.Validators.StopPx(context, check, entry.StopPx);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.OrdStatus is not null) context.Validators.OrdStatus(context, check, entry.OrdStatus);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, check, entry);
		if (entry.LeavesQty is not null) context.Validators.LeavesQty(context, check, entry.LeavesQty);
		if (entry.CumQty is not null) context.Validators.CumQty(context, check, entry.CumQty);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (!Empty((IDisplayInstruction)entry)) context.Validators.DisplayInstruction(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.BookingType is not null) context.Validators.BookingType(context, check, entry.BookingType);
		if (entry.OrigCustOrderCapacity is not null) context.Validators.OrigCustOrderCapacity(context, check, entry.OrigCustOrderCapacity);
		if (entry.OrderInputDevice is not null) context.Validators.OrderInputDevice(context, check, entry.OrderInputDevice);
		if (entry.LotType is not null) context.Validators.LotType(context, check, entry.LotType);
		if (entry.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, entry.TransBkdTime);
		if (entry.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, check, entry.OrigOrdModTime);
		if (!Empty((ISideTrdRegTS)entry)) context.Validators.SideTrdRegTS(context, check, entry);
	}

	static void ValidateTradeCaptureReportAck_NoSides_NoClearingInstructions(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateTradeCaptureReportAck_NoSides_NoContAmts(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoContAmtsGroup entry)
	{
		context.Validators.ContAmtType(context, check, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, check, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, check, entry.ContAmtCurr);
	}

	static void ValidateTradeCaptureReportAck_NoSides_NoMiscFees(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateTradeCaptureReportAck_NoSides_NoSettlDetails(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoSettlDetailsGroup entry)
	{
		context.Validators.SettlObligSource(context, check, entry.SettlObligSource);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, check, entry);
	}

	static void ValidateTradeCaptureReportAck_NoSides_NoAllocs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.AllocCustomerCapacity is not null) context.Validators.AllocCustomerCapacity(context, check, entry.AllocCustomerCapacity);
		if (entry.AllocMethod is not null) context.Validators.AllocMethod(context, check, entry.AllocMethod);
		if (entry.SecondaryIndividualAllocID is not null) context.Validators.SecondaryIndividualAllocID(context, check, entry.SecondaryIndividualAllocID);
		if (entry.AllocClearingFeeIndicator is not null) context.Validators.AllocClearingFeeIndicator(context, check, entry.AllocClearingFeeIndicator);
	}

	static void ValidateTradeCaptureReportRequest_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequest_NoLegs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequest_NoDates(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoDatesGroup entry)
	{
		context.Validators.TradeDate(context, check, entry.TradeDate);
		if (entry.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, entry.LastUpdateTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
	}

	static void ValidateTradeCaptureReportRequestAck_NoUnderlyings(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequestAck_NoLegs(Fix50Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateTradingSessionList_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TradSesUpdateAction is not null) context.Validators.TradSesUpdateAction(context, check, entry.TradSesUpdateAction);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.MarketID is not null) context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (entry.TradingSessionDesc is not null) context.Validators.TradingSessionDesc(context, check, entry.TradingSessionDesc);
		if (entry.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, entry.TradSesMethod);
		if (entry.TradSesMode is not null) context.Validators.TradSesMode(context, check, entry.TradSesMode);
		if (entry.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, entry.UnsolicitedIndicator);
		if (entry.TradSesStatus is null) Missing(check, FixTag.TradSesStatus, entry.TradingSessionID.Position);
		else context.Validators.TradSesStatus(context, check, entry.TradSesStatus);
		if (entry.TradSesStatusRejReason is not null) context.Validators.TradSesStatusRejReason(context, check, entry.TradSesStatusRejReason);
		if (entry.TradSesStartTime is not null) context.Validators.TradSesStartTime(context, check, entry.TradSesStartTime);
		if (entry.TradSesOpenTime is not null) context.Validators.TradSesOpenTime(context, check, entry.TradSesOpenTime);
		if (entry.TradSesPreCloseTime is not null) context.Validators.TradSesPreCloseTime(context, check, entry.TradSesPreCloseTime);
		if (entry.TradSesCloseTime is not null) context.Validators.TradSesCloseTime(context, check, entry.TradSesCloseTime);
		if (entry.TradSesEndTime is not null) context.Validators.TradSesEndTime(context, check, entry.TradSesEndTime);
		if (entry.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, check, entry.TotalVolumeTraded);
		if (entry.NoOrdTypeRules is not null) context.Validators.NoOrdTypeRules(context, check, entry.NoOrdTypeRules);
		Counted(check, entry.NoOrdTypeRules, entry.NoOrdTypeRulesGroups);
		Each(context, check, FixTag.NoOrdTypeRules, entry.NoOrdTypeRulesGroups, context.Validators.TradingSessionList_NoTradingSessions_NoOrdTypeRules);
		if (entry.NoTimeInForceRules is not null) context.Validators.NoTimeInForceRules(context, check, entry.NoTimeInForceRules);
		Counted(check, entry.NoTimeInForceRules, entry.NoTimeInForceRulesGroups);
		Each(context, check, FixTag.NoTimeInForceRules, entry.NoTimeInForceRulesGroups, context.Validators.TradingSessionList_NoTradingSessions_NoTimeInForceRules);
		if (entry.NoExecInstRules is not null) context.Validators.NoExecInstRules(context, check, entry.NoExecInstRules);
		Counted(check, entry.NoExecInstRules, entry.NoExecInstRulesGroups);
		Each(context, check, FixTag.NoExecInstRules, entry.NoExecInstRulesGroups, context.Validators.TradingSessionList_NoTradingSessions_NoExecInstRules);
		if (entry.NoMatchRules is not null) context.Validators.NoMatchRules(context, check, entry.NoMatchRules);
		Counted(check, entry.NoMatchRules, entry.NoMatchRulesGroups);
		Each(context, check, FixTag.NoMatchRules, entry.NoMatchRulesGroups, context.Validators.TradingSessionList_NoTradingSessions_NoMatchRules);
		if (entry.NoMDFeedTypes is not null) context.Validators.NoMDFeedTypes(context, check, entry.NoMDFeedTypes);
		Counted(check, entry.NoMDFeedTypes, entry.NoMDFeedTypesGroups);
		Each(context, check, FixTag.NoMDFeedTypes, entry.NoMDFeedTypesGroups, context.Validators.TradingSessionList_NoTradingSessions_NoMDFeedTypes);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateTradingSessionList_NoTradingSessions_NoOrdTypeRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoOrdTypeRulesGroup entry)
	{
		context.Validators.OrdType(context, check, entry.OrdType);
	}

	static void ValidateTradingSessionList_NoTradingSessions_NoTimeInForceRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoTimeInForceRulesGroup entry)
	{
		context.Validators.TimeInForce(context, check, entry.TimeInForce);
	}

	static void ValidateTradingSessionList_NoTradingSessions_NoExecInstRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoExecInstRulesGroup entry)
	{
		context.Validators.ExecInstValue(context, check, entry.ExecInstValue);
	}

	static void ValidateTradingSessionList_NoTradingSessions_NoMatchRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMatchRulesGroup entry)
	{
		context.Validators.MatchAlgorithm(context, check, entry.MatchAlgorithm);
		if (entry.MatchType is not null) context.Validators.MatchType(context, check, entry.MatchType);
	}

	static void ValidateTradingSessionList_NoTradingSessions_NoMDFeedTypes(Fix50Context context, FixCheck check, FixMessage.TradingSessionList.NoTradingSessionsGroup.NoMDFeedTypesGroup entry)
	{
		context.Validators.MDFeedType(context, check, entry.MDFeedType);
		if (entry.MarketDepth is not null) context.Validators.MarketDepth(context, check, entry.MarketDepth);
		if (entry.MDBookType is not null) context.Validators.MDBookType(context, check, entry.MDBookType);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TradSesUpdateAction is not null) context.Validators.TradSesUpdateAction(context, check, entry.TradSesUpdateAction);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.MarketID is not null) context.Validators.MarketID(context, check, entry.MarketID);
		if (entry.MarketSegmentID is not null) context.Validators.MarketSegmentID(context, check, entry.MarketSegmentID);
		if (entry.TradingSessionDesc is not null) context.Validators.TradingSessionDesc(context, check, entry.TradingSessionDesc);
		if (entry.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, entry.TradSesMethod);
		if (entry.TradSesMode is not null) context.Validators.TradSesMode(context, check, entry.TradSesMode);
		if (entry.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, entry.UnsolicitedIndicator);
		if (entry.TradSesStatus is null) Missing(check, FixTag.TradSesStatus, entry.TradingSessionID.Position);
		else context.Validators.TradSesStatus(context, check, entry.TradSesStatus);
		if (entry.TradSesStatusRejReason is not null) context.Validators.TradSesStatusRejReason(context, check, entry.TradSesStatusRejReason);
		if (entry.TradSesStartTime is not null) context.Validators.TradSesStartTime(context, check, entry.TradSesStartTime);
		if (entry.TradSesOpenTime is not null) context.Validators.TradSesOpenTime(context, check, entry.TradSesOpenTime);
		if (entry.TradSesPreCloseTime is not null) context.Validators.TradSesPreCloseTime(context, check, entry.TradSesPreCloseTime);
		if (entry.TradSesCloseTime is not null) context.Validators.TradSesCloseTime(context, check, entry.TradSesCloseTime);
		if (entry.TradSesEndTime is not null) context.Validators.TradSesEndTime(context, check, entry.TradSesEndTime);
		if (entry.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, check, entry.TotalVolumeTraded);
		if (entry.NoOrdTypeRules is not null) context.Validators.NoOrdTypeRules(context, check, entry.NoOrdTypeRules);
		Counted(check, entry.NoOrdTypeRules, entry.NoOrdTypeRulesGroups);
		Each(context, check, FixTag.NoOrdTypeRules, entry.NoOrdTypeRulesGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions_NoOrdTypeRules);
		if (entry.NoTimeInForceRules is not null) context.Validators.NoTimeInForceRules(context, check, entry.NoTimeInForceRules);
		Counted(check, entry.NoTimeInForceRules, entry.NoTimeInForceRulesGroups);
		Each(context, check, FixTag.NoTimeInForceRules, entry.NoTimeInForceRulesGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions_NoTimeInForceRules);
		if (entry.NoExecInstRules is not null) context.Validators.NoExecInstRules(context, check, entry.NoExecInstRules);
		Counted(check, entry.NoExecInstRules, entry.NoExecInstRulesGroups);
		Each(context, check, FixTag.NoExecInstRules, entry.NoExecInstRulesGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions_NoExecInstRules);
		if (entry.NoMatchRules is not null) context.Validators.NoMatchRules(context, check, entry.NoMatchRules);
		Counted(check, entry.NoMatchRules, entry.NoMatchRulesGroups);
		Each(context, check, FixTag.NoMatchRules, entry.NoMatchRulesGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions_NoMatchRules);
		if (entry.NoMDFeedTypes is not null) context.Validators.NoMDFeedTypes(context, check, entry.NoMDFeedTypes);
		Counted(check, entry.NoMDFeedTypes, entry.NoMDFeedTypesGroups);
		Each(context, check, FixTag.NoMDFeedTypes, entry.NoMDFeedTypesGroups, context.Validators.TradingSessionListUpdateReport_NoTradingSessions_NoMDFeedTypes);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions_NoOrdTypeRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoOrdTypeRulesGroup entry)
	{
		context.Validators.OrdType(context, check, entry.OrdType);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions_NoTimeInForceRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoTimeInForceRulesGroup entry)
	{
		context.Validators.TimeInForce(context, check, entry.TimeInForce);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions_NoExecInstRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoExecInstRulesGroup entry)
	{
		context.Validators.ExecInstValue(context, check, entry.ExecInstValue);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions_NoMatchRules(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMatchRulesGroup entry)
	{
		context.Validators.MatchAlgorithm(context, check, entry.MatchAlgorithm);
		if (entry.MatchType is not null) context.Validators.MatchType(context, check, entry.MatchType);
	}

	static void ValidateTradingSessionListUpdateReport_NoTradingSessions_NoMDFeedTypes(Fix50Context context, FixCheck check, FixMessage.TradingSessionListUpdateReport.NoTradingSessionsGroup.NoMDFeedTypesGroup entry)
	{
		context.Validators.MDFeedType(context, check, entry.MDFeedType);
		if (entry.MarketDepth is not null) context.Validators.MarketDepth(context, check, entry.MarketDepth);
		if (entry.MDBookType is not null) context.Validators.MDBookType(context, check, entry.MDBookType);
	}

	static void ValidateUserNotification_NoUsernames(Fix50Context context, FixCheck check, FixMessage.UserNotification.NoUsernamesGroup entry)
	{
		context.Validators.Username(context, check, entry.Username);
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 ApplicationSequenceControl has none of its fields.</summary>
	internal static bool Empty(IApplicationSequenceControl block)
	{
		return block.ApplID is null &&
			block.ApplSeqNum is null &&
			block.ApplLastSeqNum is null &&
			block.ApplResendFlag is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 CommissionData has none of its fields.</summary>
	internal static bool Empty(ICommissionData block)
	{
		return block.Commission is null &&
			block.CommType is null &&
			block.CommCurrency is null &&
			block.FundRenewWaiv is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 DiscretionInstructions has none of its fields.</summary>
	internal static bool Empty(IDiscretionInstructions block)
	{
		return block.DiscretionInst is null &&
			block.DiscretionOffsetValue is null &&
			block.DiscretionMoveType is null &&
			block.DiscretionOffsetType is null &&
			block.DiscretionLimitType is null &&
			block.DiscretionRoundDirection is null &&
			block.DiscretionScope is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 DisplayInstruction has none of its fields.</summary>
	internal static bool Empty(IDisplayInstruction block)
	{
		return block.DisplayQty is null &&
			block.SecondaryDisplayQty is null &&
			block.DisplayWhen is null &&
			block.DisplayMethod is null &&
			block.DisplayLowQty is null &&
			block.DisplayHighQty is null &&
			block.DisplayMinIncr is null &&
			block.RefreshQty is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 ExpirationQty has none of its fields.</summary>
	internal static bool Empty(IExpirationQty block)
	{
		return block.NoExpiration is null &&
			block.NoExpirationGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 FinancingDetails has none of its fields.</summary>
	internal static bool Empty(IFinancingDetails block)
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

	/// <summary>Whether a carrier of the FIX 5.0 SP2 Instrument has none of its fields.</summary>
	internal static bool Empty(IInstrument block)
	{
		return block.Symbol is null &&
			block.SymbolSfx is null &&
			block.SecurityID is null &&
			block.SecurityIDSource is null &&
			block.NoSecurityAltID is null &&
			block.NoSecurityAltIDGroups is null &&
			block.Product is null &&
			block.ProductComplex is null &&
			block.SecurityGroup is null &&
			block.CFICode is null &&
			block.SecurityType is null &&
			block.SecuritySubType is null &&
			block.MaturityMonthYear is null &&
			block.MaturityDate is null &&
			block.MaturityTime is null &&
			block.SettleOnOpenFlag is null &&
			block.InstrmtAssignmentMethod is null &&
			block.SecurityStatus is null &&
			block.CouponPaymentDate is null &&
			block.RestructuringType is null &&
			block.Seniority is null &&
			block.NotionalPercentageOutstanding is null &&
			block.OriginalNotionalPercentageOutstanding is null &&
			block.AttachmentPoint is null &&
			block.DetachmentPoint is null &&
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
			block.StrikeMultiplier is null &&
			block.StrikeValue is null &&
			block.StrikePriceDeterminationMethod is null &&
			block.StrikePriceBoundaryMethod is null &&
			block.StrikePriceBoundaryPrecision is null &&
			block.UnderlyingPriceDeterminationMethod is null &&
			block.OptAttribute is null &&
			block.ContractMultiplier is null &&
			block.ContractMultiplierUnit is null &&
			block.FlowScheduleType is null &&
			block.MinPriceIncrement is null &&
			block.MinPriceIncrementAmount is null &&
			block.UnitOfMeasure is null &&
			block.UnitOfMeasureQty is null &&
			block.PriceUnitOfMeasure is null &&
			block.PriceUnitOfMeasureQty is null &&
			block.SettlMethod is null &&
			block.ExerciseStyle is null &&
			block.OptPayoutType is null &&
			block.OptPayoutAmount is null &&
			block.PriceQuoteMethod is null &&
			block.ValuationMethod is null &&
			block.ListMethod is null &&
			block.CapPrice is null &&
			block.FloorPrice is null &&
			block.PutOrCall is null &&
			block.FlexibleIndicator is null &&
			block.FlexProductEligibilityIndicator is null &&
			block.TimeUnit is null &&
			block.CouponRate is null &&
			block.SecurityExchange is null &&
			block.PositionLimit is null &&
			block.NTPositionLimit is null &&
			block.Issuer is null &&
			block.EncodedIssuerLen is null &&
			block.EncodedIssuer is null &&
			block.SecurityDesc is null &&
			block.EncodedSecurityDescLen is null &&
			block.EncodedSecurityDesc is null &&
			block.SecurityXMLLen is null &&
			block.SecurityXML is null &&
			block.SecurityXMLSchema is null &&
			block.Pool is null &&
			block.ContractSettlMonth is null &&
			block.CPProgram is null &&
			block.CPRegType is null &&
			block.NoEvents is null &&
			block.NoEventsGroups is null &&
			block.DatedDate is null &&
			block.InterestAccrualDate is null &&
			block.NoInstrumentParties is null &&
			block.NoInstrumentPartiesGroups is null &&
			block.NoComplexEvents is null &&
			block.NoComplexEventsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 InstrumentExtension has none of its fields.</summary>
	internal static bool Empty(IInstrumentExtension block)
	{
		return block.DeliveryForm is null &&
			block.PctAtRisk is null &&
			block.NoInstrAttrib is null &&
			block.NoInstrAttribGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 InstrumentLeg has none of its fields.</summary>
	internal static bool Empty(IInstrumentLeg block)
	{
		return block.LegSymbol is null &&
			block.LegSymbolSfx is null &&
			block.LegSecurityID is null &&
			block.LegSecurityIDSource is null &&
			block.NoLegSecurityAltID is null &&
			block.NoLegSecurityAltIDGroups is null &&
			block.LegProduct is null &&
			block.LegCFICode is null &&
			block.LegSecurityType is null &&
			block.LegSecuritySubType is null &&
			block.LegMaturityMonthYear is null &&
			block.LegMaturityDate is null &&
			block.LegMaturityTime is null &&
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
			block.LegContractMultiplierUnit is null &&
			block.LegFlowScheduleType is null &&
			block.LegUnitOfMeasure is null &&
			block.LegUnitOfMeasureQty is null &&
			block.LegPriceUnitOfMeasure is null &&
			block.LegPriceUnitOfMeasureQty is null &&
			block.LegTimeUnit is null &&
			block.LegExerciseStyle is null &&
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
			block.LegInterestAccrualDate is null &&
			block.LegPutOrCall is null &&
			block.LegOptionRatio is null &&
			block.LegPrice is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 InstrumentParties has none of its fields.</summary>
	internal static bool Empty(IInstrumentParties block)
	{
		return block.NoInstrumentParties is null &&
			block.NoInstrumentPartiesGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 LegBenchmarkCurveData has none of its fields.</summary>
	internal static bool Empty(ILegBenchmarkCurveData block)
	{
		return block.LegBenchmarkCurveCurrency is null &&
			block.LegBenchmarkCurveName is null &&
			block.LegBenchmarkCurvePoint is null &&
			block.LegBenchmarkPrice is null &&
			block.LegBenchmarkPriceType is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 LegStipulations has none of its fields.</summary>
	internal static bool Empty(ILegStipulations block)
	{
		return block.NoLegStipulations is null &&
			block.NoLegStipulationsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 NestedParties has none of its fields.</summary>
	internal static bool Empty(INestedParties block)
	{
		return block.NoNestedPartyIDs is null &&
			block.NoNestedPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 NestedParties2 has none of its fields.</summary>
	internal static bool Empty(INestedParties2 block)
	{
		return block.NoNested2PartyIDs is null &&
			block.NoNested2PartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 NestedParties3 has none of its fields.</summary>
	internal static bool Empty(INestedParties3 block)
	{
		return block.NoNested3PartyIDs is null &&
			block.NoNested3PartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 NestedParties4 has none of its fields.</summary>
	internal static bool Empty(INestedParties4 block)
	{
		return block.NoNested4PartyIDs is null &&
			block.NoNested4PartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 OrderQtyData has none of its fields.</summary>
	internal static bool Empty(IOrderQtyData block)
	{
		return block.OrderQty is null &&
			block.CashOrderQty is null &&
			block.OrderPercent is null &&
			block.RoundingDirection is null &&
			block.RoundingModulus is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 Parties has none of its fields.</summary>
	internal static bool Empty(IParties block)
	{
		return block.NoPartyIDs is null &&
			block.NoPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 PegInstructions has none of its fields.</summary>
	internal static bool Empty(IPegInstructions block)
	{
		return block.PegOffsetValue is null &&
			block.PegPriceType is null &&
			block.PegMoveType is null &&
			block.PegOffsetType is null &&
			block.PegLimitType is null &&
			block.PegRoundDirection is null &&
			block.PegScope is null &&
			block.PegSecurityIDSource is null &&
			block.PegSecurityID is null &&
			block.PegSymbol is null &&
			block.PegSecurityDesc is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 PositionAmountData has none of its fields.</summary>
	internal static bool Empty(IPositionAmountData block)
	{
		return block.NoPosAmt is null &&
			block.NoPosAmtGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 PositionQty has none of its fields.</summary>
	internal static bool Empty(IPositionQty block)
	{
		return block.NoPositions is null &&
			block.NoPositionsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 RateSource has none of its fields.</summary>
	internal static bool Empty(IRateSource block)
	{
		return block.NoRateSources is null &&
			block.NoRateSourcesGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 RootParties has none of its fields.</summary>
	internal static bool Empty(IRootParties block)
	{
		return block.NoRootPartyIDs is null &&
			block.NoRootPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 SecurityTradingRules has none of its fields.</summary>
	internal static bool Empty(ISecurityTradingRules block)
	{
		return block.NoTickRules is null &&
			block.NoTickRulesGroups is null &&
			block.NoLotTypeRules is null &&
			block.NoLotTypeRulesGroups is null &&
			block.PriceLimitType is null &&
			block.LowLimitPrice is null &&
			block.HighLimitPrice is null &&
			block.TradingReferencePrice is null &&
			block.ExpirationCycle is null &&
			block.MinTradeVol is null &&
			block.MaxTradeVol is null &&
			block.MaxPriceVariation is null &&
			block.ImpliedMarketIndicator is null &&
			block.TradingCurrency is null &&
			block.RoundLot is null &&
			block.MultilegModel is null &&
			block.MultilegPriceMethod is null &&
			block.PriceType is null &&
			block.NoTradingSessionRules is null &&
			block.NoTradingSessionRulesGroups is null &&
			block.NoNestedInstrAttrib is null &&
			block.NoNestedInstrAttribGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 SettlInstructionsData has none of its fields.</summary>
	internal static bool Empty(ISettlInstructionsData block)
	{
		return block.SettlDeliveryType is null &&
			block.StandInstDbType is null &&
			block.StandInstDbName is null &&
			block.StandInstDbID is null &&
			block.NoDlvyInst is null &&
			block.NoDlvyInstGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 SettlParties has none of its fields.</summary>
	internal static bool Empty(ISettlParties block)
	{
		return block.NoSettlPartyIDs is null &&
			block.NoSettlPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 SideTrdRegTS has none of its fields.</summary>
	internal static bool Empty(ISideTrdRegTS block)
	{
		return block.NoSideTrdRegTS is null &&
			block.NoSideTrdRegTSGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 SpreadOrBenchmarkCurveData has none of its fields.</summary>
	internal static bool Empty(ISpreadOrBenchmarkCurveData block)
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

	/// <summary>Whether a carrier of the FIX 5.0 SP2 Stipulations has none of its fields.</summary>
	internal static bool Empty(IStipulations block)
	{
		return block.NoStipulations is null &&
			block.NoStipulationsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 TargetParties has none of its fields.</summary>
	internal static bool Empty(ITargetParties block)
	{
		return block.NoTargetPartyIDs is null &&
			block.NoTargetPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 TrdRegTimestamps has none of its fields.</summary>
	internal static bool Empty(ITrdRegTimestamps block)
	{
		return block.NoTrdRegTimestamps is null &&
			block.NoTrdRegTimestampsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 TriggeringInstruction has none of its fields.</summary>
	internal static bool Empty(ITriggeringInstruction block)
	{
		return block.TriggerType is null &&
			block.TriggerAction is null &&
			block.TriggerPrice is null &&
			block.TriggerSymbol is null &&
			block.TriggerSecurityID is null &&
			block.TriggerSecurityIDSource is null &&
			block.TriggerSecurityDesc is null &&
			block.TriggerPriceType is null &&
			block.TriggerPriceTypeScope is null &&
			block.TriggerPriceDirection is null &&
			block.TriggerNewPrice is null &&
			block.TriggerOrderType is null &&
			block.TriggerNewQty is null &&
			block.TriggerTradingSessionID is null &&
			block.TriggerTradingSessionSubID is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 UnderlyingAmount has none of its fields.</summary>
	internal static bool Empty(IUnderlyingAmount block)
	{
		return block.NoUnderlyingAmounts is null &&
			block.NoUnderlyingAmountsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 UnderlyingInstrument has none of its fields.</summary>
	internal static bool Empty(IUnderlyingInstrument block)
	{
		return block.UnderlyingSymbol is null &&
			block.UnderlyingSymbolSfx is null &&
			block.UnderlyingSecurityID is null &&
			block.UnderlyingSecurityIDSource is null &&
			block.NoUnderlyingSecurityAltID is null &&
			block.NoUnderlyingSecurityAltIDGroups is null &&
			block.UnderlyingProduct is null &&
			block.UnderlyingCFICode is null &&
			block.UnderlyingSecurityType is null &&
			block.UnderlyingSecuritySubType is null &&
			block.UnderlyingMaturityMonthYear is null &&
			block.UnderlyingMaturityDate is null &&
			block.UnderlyingMaturityTime is null &&
			block.UnderlyingCouponPaymentDate is null &&
			block.UnderlyingRestructuringType is null &&
			block.UnderlyingSeniority is null &&
			block.UnderlyingNotionalPercentageOutstanding is null &&
			block.UnderlyingOriginalNotionalPercentageOutstanding is null &&
			block.UnderlyingAttachmentPoint is null &&
			block.UnderlyingDetachmentPoint is null &&
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
			block.UnderlyingContractMultiplierUnit is null &&
			block.UnderlyingFlowScheduleType is null &&
			block.UnderlyingUnitOfMeasure is null &&
			block.UnderlyingUnitOfMeasureQty is null &&
			block.UnderlyingPriceUnitOfMeasure is null &&
			block.UnderlyingPriceUnitOfMeasureQty is null &&
			block.UnderlyingTimeUnit is null &&
			block.UnderlyingExerciseStyle is null &&
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
			block.UnderlyingAllocationPercent is null &&
			block.UnderlyingCurrency is null &&
			block.UnderlyingQty is null &&
			block.UnderlyingSettlementType is null &&
			block.UnderlyingCashAmount is null &&
			block.UnderlyingCashType is null &&
			block.UnderlyingPx is null &&
			block.UnderlyingDirtyPrice is null &&
			block.UnderlyingEndPrice is null &&
			block.UnderlyingStartValue is null &&
			block.UnderlyingCurrentValue is null &&
			block.UnderlyingEndValue is null &&
			block.NoUnderlyingStips is null &&
			block.NoUnderlyingStipsGroups is null &&
			block.UnderlyingAdjustedQuantity is null &&
			block.UnderlyingFXRate is null &&
			block.UnderlyingFXRateCalc is null &&
			block.UnderlyingCapValue is null &&
			block.NoUndlyInstrumentParties is null &&
			block.NoUndlyInstrumentPartiesGroups is null &&
			block.UnderlyingSettlMethod is null &&
			block.UnderlyingPutOrCall is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 UnderlyingStipulations has none of its fields.</summary>
	internal static bool Empty(IUnderlyingStipulations block)
	{
		return block.NoUnderlyingStips is null &&
			block.NoUnderlyingStipsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 UndlyInstrumentParties has none of its fields.</summary>
	internal static bool Empty(IUndlyInstrumentParties block)
	{
		return block.NoUndlyInstrumentParties is null &&
			block.NoUndlyInstrumentPartiesGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 5.0 SP2 YieldData has none of its fields.</summary>
	internal static bool Empty(IYieldData block)
	{
		return block.YieldType is null &&
			block.Yield is null &&
			block.YieldCalcDate is null &&
			block.YieldRedemptionDate is null &&
			block.YieldRedemptionPrice is null &&
			block.YieldRedemptionPriceType is null;
	}
}
