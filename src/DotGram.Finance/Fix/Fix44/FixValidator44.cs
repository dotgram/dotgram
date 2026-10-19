using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.cs from the FIX 4.4 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The check of every FIX 4.4 message type, of every component it reuses, and of every entry of
/// every repeating group; the fields' own are in FixValidator44.Fields.cs.
/// </summary>
/// <remarks>
/// Straight-line checks written against the fields of the class they are about, and nothing else:
/// a field the repository marks required, a component it marks required and the carrier left empty,
/// every field handed to its own slot, and every count held to the entries that follow it. Every
/// slot is typed on what it checks and named for it — a message type, a component, and a group
/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run
/// time replaces the slots it describes by name and leaves the rest.
/// </remarks>
partial class FixValidator44
{
	/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>
	public static readonly FixValidator44 Default = new();

	/// <summary>The tag of a field this version names otherwise than FixTag does.</summary>
	private protected override int FieldTag(string name)
	{
		return name switch
		{
			"IOIid"       => 23,
			"LinesOfText" => 33,
			_             => 0,
		};
	}

	/// <summary>Holds a FIX 4.4 Advertisement to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Advertisement> Advertisement { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 4.4 AllocationInstruction to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction> AllocationInstruction { get; set; } = ValidateAllocationInstruction;

	/// <summary>Holds a FIX 4.4 AllocationInstructionAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstructionAck> AllocationInstructionAck { get; set; } = ValidateAllocationInstructionAck;

	/// <summary>Holds a FIX 4.4 AllocationReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport> AllocationReport { get; set; } = ValidateAllocationReport;

	/// <summary>Holds a FIX 4.4 AllocationReportAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReportAck> AllocationReportAck { get; set; } = ValidateAllocationReportAck;

	/// <summary>Holds a FIX 4.4 AssignmentReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AssignmentReport> AssignmentReport { get; set; } = ValidateAssignmentReport;

	/// <summary>Holds a FIX 4.4 BidRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BidRequest> BidRequest { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 4.4 BidResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BidResponse> BidResponse { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 4.4 BusinessMessageReject to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BusinessMessageReject> BusinessMessageReject { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 4.4 CollateralAssignment to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment> CollateralAssignment { get; set; } = ValidateCollateralAssignment;

	/// <summary>Holds a FIX 4.4 CollateralInquiry to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry> CollateralInquiry { get; set; } = ValidateCollateralInquiry;

	/// <summary>Holds a FIX 4.4 CollateralInquiryAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck> CollateralInquiryAck { get; set; } = ValidateCollateralInquiryAck;

	/// <summary>Holds a FIX 4.4 CollateralReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport> CollateralReport { get; set; } = ValidateCollateralReport;

	/// <summary>Holds a FIX 4.4 CollateralRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest> CollateralRequest { get; set; } = ValidateCollateralRequest;

	/// <summary>Holds a FIX 4.4 CollateralResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse> CollateralResponse { get; set; } = ValidateCollateralResponse;

	/// <summary>Holds a FIX 4.4 Confirmation to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation> Confirmation { get; set; } = ValidateConfirmation;

	/// <summary>Holds a FIX 4.4 ConfirmationAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ConfirmationAck> ConfirmationAck { get; set; } = ValidateConfirmationAck;

	/// <summary>Holds a FIX 4.4 ConfirmationRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ConfirmationRequest> ConfirmationRequest { get; set; } = ValidateConfirmationRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest> CrossOrderCancelReplaceRequest { get; set; } = ValidateCrossOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelRequest> CrossOrderCancelRequest { get; set; } = ValidateCrossOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityList to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DerivativeSecurityList> DerivativeSecurityList { get; set; } = ValidateDerivativeSecurityList;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityListRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DerivativeSecurityListRequest> DerivativeSecurityListRequest { get; set; } = ValidateDerivativeSecurityListRequest;

	/// <summary>Holds a FIX 4.4 DontKnowTrade to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DontKnowTrade> DontKnowTrade { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 4.4 Email to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email> Email { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 4.4 ExecutionReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport> ExecutionReport { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 4.4 Heartbeat to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Heartbeat> Heartbeat { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 4.4 IndicationOfInterest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.IndicationOfInterest> IndicationOfInterest { get; set; } = ValidateIndicationOfInterest;

	/// <summary>Holds a FIX 4.4 ListCancelRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListCancelRequest> ListCancelRequest { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 4.4 ListExecute to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListExecute> ListExecute { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 4.4 ListStatus to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStatus> ListStatus { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 4.4 ListStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStatusRequest> ListStatusRequest { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 4.4 ListStrikePrice to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStrikePrice> ListStrikePrice { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 4.4 Logon to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Logon> Logon { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 4.4 Logout to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Logout> Logout { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 4.4 MarketDataIncrementalRefresh to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataIncrementalRefresh> MarketDataIncrementalRefresh { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 4.4 MarketDataRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest> MarketDataRequest { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 4.4 MarketDataRequestReject to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequestReject> MarketDataRequestReject { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 4.4 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh> MarketDataSnapshotFullRefresh { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 4.4 MassQuote to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuote> MassQuote { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 4.4 MassQuoteAcknowledgement to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuoteAcknowledgement> MassQuoteAcknowledgement { get; set; } = ValidateMassQuoteAcknowledgement;

	/// <summary>Holds a FIX 4.4 MultilegOrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest> MultilegOrderCancelReplaceRequest { get; set; } = ValidateMultilegOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 NetworkStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NetworkStatusRequest> NetworkStatusRequest { get; set; } = ValidateNetworkStatusRequest;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NetworkStatusResponse> NetworkStatusResponse { get; set; } = ValidateNetworkStatusResponse;

	/// <summary>Holds a FIX 4.4 NewOrderCross to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross> NewOrderCross { get; set; } = ValidateNewOrderCross;

	/// <summary>Holds a FIX 4.4 NewOrderList to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderList> NewOrderList { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 4.4 NewOrderMultileg to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg> NewOrderMultileg { get; set; } = ValidateNewOrderMultileg;

	/// <summary>Holds a FIX 4.4 NewOrderSingle to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderSingle> NewOrderSingle { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 4.4 News to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News> News { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 4.4 OrderCancelReject to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelReject> OrderCancelReject { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 4.4 OrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelReplaceRequest> OrderCancelReplaceRequest { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 OrderCancelRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelRequest> OrderCancelRequest { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassCancelReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderMassCancelReport> OrderMassCancelReport { get; set; } = ValidateOrderMassCancelReport;

	/// <summary>Holds a FIX 4.4 OrderMassCancelRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderMassCancelRequest> OrderMassCancelRequest { get; set; } = ValidateOrderMassCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderMassStatusRequest> OrderMassStatusRequest { get; set; } = ValidateOrderMassStatusRequest;

	/// <summary>Holds a FIX 4.4 OrderStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderStatusRequest> OrderStatusRequest { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceReport> PositionMaintenanceReport { get; set; } = ValidatePositionMaintenanceReport;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceRequest> PositionMaintenanceRequest { get; set; } = ValidatePositionMaintenanceRequest;

	/// <summary>Holds a FIX 4.4 PositionReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionReport> PositionReport { get; set; } = ValidatePositionReport;

	/// <summary>Holds a FIX 4.4 Quote to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Quote> Quote { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 4.4 QuoteCancel to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteCancel> QuoteCancel { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 4.4 QuoteRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequest> QuoteRequest { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 4.4 QuoteRequestReject to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequestReject> QuoteRequestReject { get; set; } = ValidateQuoteRequestReject;

	/// <summary>Holds a FIX 4.4 QuoteResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteResponse> QuoteResponse { get; set; } = ValidateQuoteResponse;

	/// <summary>Holds a FIX 4.4 QuoteStatusReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusReport> QuoteStatusReport { get; set; } = ValidateQuoteStatusReport;

	/// <summary>Holds a FIX 4.4 QuoteStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusRequest> QuoteStatusRequest { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 4.4 RFQRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RFQRequest> RFQRequest { get; set; } = ValidateRFQRequest;

	/// <summary>Holds a FIX 4.4 RegistrationInstructions to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RegistrationInstructions> RegistrationInstructions { get; set; } = ValidateRegistrationInstructions;

	/// <summary>Holds a FIX 4.4 RegistrationInstructionsResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RegistrationInstructionsResponse> RegistrationInstructionsResponse { get; set; } = ValidateRegistrationInstructionsResponse;

	/// <summary>Holds a FIX 4.4 Reject to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Reject> Reject { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 4.4 RequestForPositions to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositions> RequestForPositions { get; set; } = ValidateRequestForPositions;

	/// <summary>Holds a FIX 4.4 RequestForPositionsAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositionsAck> RequestForPositionsAck { get; set; } = ValidateRequestForPositionsAck;

	/// <summary>Holds a FIX 4.4 ResendRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ResendRequest> ResendRequest { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 4.4 SecurityDefinition to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinition> SecurityDefinition { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 4.4 SecurityDefinitionRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinitionRequest> SecurityDefinitionRequest { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 4.4 SecurityList to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityList> SecurityList { get; set; } = ValidateSecurityList;

	/// <summary>Holds a FIX 4.4 SecurityListRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityListRequest> SecurityListRequest { get; set; } = ValidateSecurityListRequest;

	/// <summary>Holds a FIX 4.4 SecurityStatus to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatus> SecurityStatus { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 4.4 SecurityStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatusRequest> SecurityStatusRequest { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypeRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityTypeRequest> SecurityTypeRequest { get; set; } = ValidateSecurityTypeRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypes to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityTypes> SecurityTypes { get; set; } = ValidateSecurityTypes;

	/// <summary>Holds a FIX 4.4 SequenceReset to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SequenceReset> SequenceReset { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 4.4 SettlementInstructionRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SettlementInstructionRequest> SettlementInstructionRequest { get; set; } = ValidateSettlementInstructionRequest;

	/// <summary>Holds a FIX 4.4 SettlementInstructions to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SettlementInstructions> SettlementInstructions { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 4.4 TestRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TestRequest> TestRequest { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReport to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport> TradeCaptureReport { get; set; } = ValidateTradeCaptureReport;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportAck> TradeCaptureReportAck { get; set; } = ValidateTradeCaptureReportAck;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequest> TradeCaptureReportRequest { get; set; } = ValidateTradeCaptureReportRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequestAck to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequestAck> TradeCaptureReportRequestAck { get; set; } = ValidateTradeCaptureReportRequestAck;

	/// <summary>Holds a FIX 4.4 TradingSessionStatus to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradingSessionStatus> TradingSessionStatus { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 4.4 TradingSessionStatusRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradingSessionStatusRequest> TradingSessionStatusRequest { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds a FIX 4.4 UserRequest to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.UserRequest> UserRequest { get; set; } = ValidateUserRequest;

	/// <summary>Holds a FIX 4.4 UserResponse to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.UserResponse> UserResponse { get; set; } = ValidateUserResponse;

	/// <summary>Holds a FIX 4.4 XMLnonFIX to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.XMLnonFIX> XMLnonFIX { get; set; } = ValidateXMLnonFIX;

	/// <summary>Holds a FIX 4.4 CommissionData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ICommissionData> CommissionData { get; set; } = ValidateCommissionData;

	/// <summary>Holds a FIX 4.4 DiscretionInstructions to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IDiscretionInstructions> DiscretionInstructions { get; set; } = ValidateDiscretionInstructions;

	/// <summary>Holds a FIX 4.4 FinancingDetails to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IFinancingDetails> FinancingDetails { get; set; } = ValidateFinancingDetails;

	/// <summary>Holds a FIX 4.4 Instrument to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IInstrument> Instrument { get; set; } = ValidateInstrument;

	/// <summary>Holds a FIX 4.4 InstrumentExtension to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IInstrumentExtension> InstrumentExtension { get; set; } = ValidateInstrumentExtension;

	/// <summary>Holds a FIX 4.4 InstrumentLeg to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IInstrumentLeg> InstrumentLeg { get; set; } = ValidateInstrumentLeg;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ILegBenchmarkCurveData> LegBenchmarkCurveData { get; set; } = ValidateLegBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 LegStipulations to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ILegStipulations> LegStipulations { get; set; } = ValidateLegStipulations;

	/// <summary>Holds a FIX 4.4 NestedParties to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, INestedParties> NestedParties { get; set; } = ValidateNestedParties;

	/// <summary>Holds a FIX 4.4 NestedParties2 to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, INestedParties2> NestedParties2 { get; set; } = ValidateNestedParties2;

	/// <summary>Holds a FIX 4.4 NestedParties3 to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, INestedParties3> NestedParties3 { get; set; } = ValidateNestedParties3;

	/// <summary>Holds a FIX 4.4 OrderQtyData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IOrderQtyData> OrderQtyData { get; set; } = ValidateOrderQtyData;

	/// <summary>Holds a FIX 4.4 Parties to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IParties> Parties { get; set; } = ValidateParties;

	/// <summary>Holds a FIX 4.4 PegInstructions to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IPegInstructions> PegInstructions { get; set; } = ValidatePegInstructions;

	/// <summary>Holds a FIX 4.4 PositionAmountData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IPositionAmountData> PositionAmountData { get; set; } = ValidatePositionAmountData;

	/// <summary>Holds a FIX 4.4 PositionQty to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IPositionQty> PositionQty { get; set; } = ValidatePositionQty;

	/// <summary>Holds a FIX 4.4 SettlInstructionsData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ISettlInstructionsData> SettlInstructionsData { get; set; } = ValidateSettlInstructionsData;

	/// <summary>Holds a FIX 4.4 SettlParties to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ISettlParties> SettlParties { get; set; } = ValidateSettlParties;

	/// <summary>Holds a FIX 4.4 SpreadOrBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ISpreadOrBenchmarkCurveData> SpreadOrBenchmarkCurveData { get; set; } = ValidateSpreadOrBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 Stipulations to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IStipulations> Stipulations { get; set; } = ValidateStipulations;

	/// <summary>Holds a FIX 4.4 TrdRegTimestamps to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, ITrdRegTimestamps> TrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrument to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IUnderlyingInstrument> UnderlyingInstrument { get; set; } = ValidateUnderlyingInstrument;

	/// <summary>Holds a FIX 4.4 UnderlyingStipulations to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IUnderlyingStipulations> UnderlyingStipulations { get; set; } = ValidateUnderlyingStipulations;

	/// <summary>Holds a FIX 4.4 YieldData to the schema, wherever it is carried.</summary>
	public Action<Fix44Context, FixCheck, IYieldData> YieldData { get; set; } = ValidateYieldData;

	/// <summary>Holds one entry of IInstrument.NoSecurityAltIDGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IInstrument.NoSecurityAltIDGroup> Instrument_NoSecurityAltID { get; set; } = ValidateInstrument_NoSecurityAltID;

	/// <summary>Holds one entry of IInstrument.NoEventsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IInstrument.NoEventsGroup> Instrument_NoEvents { get; set; } = ValidateInstrument_NoEvents;

	/// <summary>Holds one entry of IInstrumentExtension.NoInstrAttribGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IInstrumentExtension.NoInstrAttribGroup> InstrumentExtension_NoInstrAttrib { get; set; } = ValidateInstrumentExtension_NoInstrAttrib;

	/// <summary>Holds one entry of IInstrumentLeg.NoLegSecurityAltIDGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IInstrumentLeg.NoLegSecurityAltIDGroup> InstrumentLeg_NoLegSecurityAltID { get; set; } = ValidateInstrumentLeg_NoLegSecurityAltID;

	/// <summary>Holds one entry of ILegStipulations.NoLegStipulationsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, ILegStipulations.NoLegStipulationsGroup> LegStipulations_NoLegStipulations { get; set; } = ValidateLegStipulations_NoLegStipulations;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties.NoNestedPartyIDsGroup> NestedParties_NoNestedPartyIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup> NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties2.NoNested2PartyIDsGroup> NestedParties2_NoNested2PartyIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup> NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties3.NoNested3PartyIDsGroup> NestedParties3_NoNested3PartyIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup> NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IParties.NoPartyIDsGroup> Parties_NoPartyIDs { get; set; } = ValidateParties_NoPartyIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup.NoPartySubIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IParties.NoPartyIDsGroup.NoPartySubIDsGroup> Parties_NoPartyIDs_NoPartySubIDs { get; set; } = ValidateParties_NoPartyIDs_NoPartySubIDs;

	/// <summary>Holds one entry of IPositionAmountData.NoPosAmtGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IPositionAmountData.NoPosAmtGroup> PositionAmountData_NoPosAmt { get; set; } = ValidatePositionAmountData_NoPosAmt;

	/// <summary>Holds one entry of IPositionQty.NoPositionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IPositionQty.NoPositionsGroup> PositionQty_NoPositions { get; set; } = ValidatePositionQty_NoPositions;

	/// <summary>Holds one entry of ISettlInstructionsData.NoDlvyInstGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, ISettlInstructionsData.NoDlvyInstGroup> SettlInstructionsData_NoDlvyInst { get; set; } = ValidateSettlInstructionsData_NoDlvyInst;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, ISettlParties.NoSettlPartyIDsGroup> SettlParties_NoSettlPartyIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup> SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs;

	/// <summary>Holds one entry of IStipulations.NoStipulationsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IStipulations.NoStipulationsGroup> Stipulations_NoStipulations { get; set; } = ValidateStipulations_NoStipulations;

	/// <summary>Holds one entry of ITrdRegTimestamps.NoTrdRegTimestampsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, ITrdRegTimestamps.NoTrdRegTimestampsGroup> TrdRegTimestamps_NoTrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps_NoTrdRegTimestamps;

	/// <summary>Holds one entry of IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup> UnderlyingInstrument_NoUnderlyingSecurityAltID { get; set; } = ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID;

	/// <summary>Holds one entry of IUnderlyingStipulations.NoUnderlyingStipsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, IUnderlyingStipulations.NoUnderlyingStipsGroup> UnderlyingStipulations_NoUnderlyingStips { get; set; } = ValidateUnderlyingStipulations_NoUnderlyingStips;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Advertisement.NoLegsGroup> Advertisement_NoLegs { get; set; } = ValidateAdvertisement_NoLegs;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Advertisement.NoUnderlyingsGroup> Advertisement_NoUnderlyings { get; set; } = ValidateAdvertisement_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoOrdersGroup> AllocationInstruction_NoOrders { get; set; } = ValidateAllocationInstruction_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoExecsGroup> AllocationInstruction_NoExecs { get; set; } = ValidateAllocationInstruction_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoUnderlyingsGroup> AllocationInstruction_NoUnderlyings { get; set; } = ValidateAllocationInstruction_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoLegsGroup> AllocationInstruction_NoLegs { get; set; } = ValidateAllocationInstruction_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup> AllocationInstruction_NoAllocs { get; set; } = ValidateAllocationInstruction_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup> AllocationInstruction_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationInstruction_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup> AllocationInstruction_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationInstruction_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAck.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationInstructionAck.NoAllocsGroup> AllocationInstructionAck_NoAllocs { get; set; } = ValidateAllocationInstructionAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoOrdersGroup> AllocationReport_NoOrders { get; set; } = ValidateAllocationReport_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoExecsGroup> AllocationReport_NoExecs { get; set; } = ValidateAllocationReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoUnderlyingsGroup> AllocationReport_NoUnderlyings { get; set; } = ValidateAllocationReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoLegsGroup> AllocationReport_NoLegs { get; set; } = ValidateAllocationReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup> AllocationReport_NoAllocs { get; set; } = ValidateAllocationReport_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup> AllocationReport_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationReport_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup> AllocationReport_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationReport_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationReportAck.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AllocationReportAck.NoAllocsGroup> AllocationReportAck_NoAllocs { get; set; } = ValidateAllocationReportAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AssignmentReport.NoLegsGroup> AssignmentReport_NoLegs { get; set; } = ValidateAssignmentReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.AssignmentReport.NoUnderlyingsGroup> AssignmentReport_NoUnderlyings { get; set; } = ValidateAssignmentReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidDescriptorsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BidRequest.NoBidDescriptorsGroup> BidRequest_NoBidDescriptors { get; set; } = ValidateBidRequest_NoBidDescriptors;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BidRequest.NoBidComponentsGroup> BidRequest_NoBidComponents { get; set; } = ValidateBidRequest_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.BidResponse.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.BidResponse.NoBidComponentsGroup> BidResponse_NoBidComponents { get; set; } = ValidateBidResponse_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment.NoExecsGroup> CollateralAssignment_NoExecs { get; set; } = ValidateCollateralAssignment_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment.NoTradesGroup> CollateralAssignment_NoTrades { get; set; } = ValidateCollateralAssignment_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment.NoLegsGroup> CollateralAssignment_NoLegs { get; set; } = ValidateCollateralAssignment_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment.NoUnderlyingsGroup> CollateralAssignment_NoUnderlyings { get; set; } = ValidateCollateralAssignment_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralAssignment.NoMiscFeesGroup> CollateralAssignment_NoMiscFees { get; set; } = ValidateCollateralAssignment_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup> CollateralInquiry_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiry_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry.NoExecsGroup> CollateralInquiry_NoExecs { get; set; } = ValidateCollateralInquiry_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry.NoTradesGroup> CollateralInquiry_NoTrades { get; set; } = ValidateCollateralInquiry_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry.NoLegsGroup> CollateralInquiry_NoLegs { get; set; } = ValidateCollateralInquiry_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiry.NoUnderlyingsGroup> CollateralInquiry_NoUnderlyings { get; set; } = ValidateCollateralInquiry_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup> CollateralInquiryAck_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiryAck_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck.NoExecsGroup> CollateralInquiryAck_NoExecs { get; set; } = ValidateCollateralInquiryAck_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck.NoTradesGroup> CollateralInquiryAck_NoTrades { get; set; } = ValidateCollateralInquiryAck_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck.NoLegsGroup> CollateralInquiryAck_NoLegs { get; set; } = ValidateCollateralInquiryAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup> CollateralInquiryAck_NoUnderlyings { get; set; } = ValidateCollateralInquiryAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport.NoExecsGroup> CollateralReport_NoExecs { get; set; } = ValidateCollateralReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport.NoTradesGroup> CollateralReport_NoTrades { get; set; } = ValidateCollateralReport_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport.NoLegsGroup> CollateralReport_NoLegs { get; set; } = ValidateCollateralReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport.NoUnderlyingsGroup> CollateralReport_NoUnderlyings { get; set; } = ValidateCollateralReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralReport.NoMiscFeesGroup> CollateralReport_NoMiscFees { get; set; } = ValidateCollateralReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest.NoExecsGroup> CollateralRequest_NoExecs { get; set; } = ValidateCollateralRequest_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest.NoTradesGroup> CollateralRequest_NoTrades { get; set; } = ValidateCollateralRequest_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest.NoLegsGroup> CollateralRequest_NoLegs { get; set; } = ValidateCollateralRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest.NoUnderlyingsGroup> CollateralRequest_NoUnderlyings { get; set; } = ValidateCollateralRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralRequest.NoMiscFeesGroup> CollateralRequest_NoMiscFees { get; set; } = ValidateCollateralRequest_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoExecsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse.NoExecsGroup> CollateralResponse_NoExecs { get; set; } = ValidateCollateralResponse_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoTradesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse.NoTradesGroup> CollateralResponse_NoTrades { get; set; } = ValidateCollateralResponse_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse.NoLegsGroup> CollateralResponse_NoLegs { get; set; } = ValidateCollateralResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse.NoUnderlyingsGroup> CollateralResponse_NoUnderlyings { get; set; } = ValidateCollateralResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CollateralResponse.NoMiscFeesGroup> CollateralResponse_NoMiscFees { get; set; } = ValidateCollateralResponse_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation.NoOrdersGroup> Confirmation_NoOrders { get; set; } = ValidateConfirmation_NoOrders;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation.NoUnderlyingsGroup> Confirmation_NoUnderlyings { get; set; } = ValidateConfirmation_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation.NoLegsGroup> Confirmation_NoLegs { get; set; } = ValidateConfirmation_NoLegs;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoCapacitiesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation.NoCapacitiesGroup> Confirmation_NoCapacities { get; set; } = ValidateConfirmation_NoCapacities;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Confirmation.NoMiscFeesGroup> Confirmation_NoMiscFees { get; set; } = ValidateConfirmation_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.ConfirmationRequest.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ConfirmationRequest.NoOrdersGroup> ConfirmationRequest_NoOrders { get; set; } = ValidateConfirmationRequest_NoOrders;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup> CrossOrderCancelReplaceRequest_NoSides { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup> CrossOrderCancelReplaceRequest_NoSides_NoAllocs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup> CrossOrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup> CrossOrderCancelReplaceRequest_NoLegs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup> CrossOrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoSidesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoSidesGroup> CrossOrderCancelRequest_NoSides { get; set; } = ValidateCrossOrderCancelRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup> CrossOrderCancelRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.CrossOrderCancelRequest.NoLegsGroup> CrossOrderCancelRequest_NoLegs { get; set; } = ValidateCrossOrderCancelRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DerivativeSecurityList.NoRelatedSymGroup> DerivativeSecurityList_NoRelatedSym { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup> DerivativeSecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DontKnowTrade.NoUnderlyingsGroup> DontKnowTrade_NoUnderlyings { get; set; } = ValidateDontKnowTrade_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.DontKnowTrade.NoLegsGroup> DontKnowTrade_NoLegs { get; set; } = ValidateDontKnowTrade_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email.NoRoutingIDsGroup> Email_NoRoutingIDs { get; set; } = ValidateEmail_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.Email.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email.NoRelatedSymGroup> Email_NoRelatedSym { get; set; } = ValidateEmail_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Email.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email.NoUnderlyingsGroup> Email_NoUnderlyings { get; set; } = ValidateEmail_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Email.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email.NoLegsGroup> Email_NoLegs { get; set; } = ValidateEmail_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.LinesOfTextGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Email.LinesOfTextGroup> Email_LinesOfText { get; set; } = ValidateEmail_LinesOfText;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContraBrokersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport.NoContraBrokersGroup> ExecutionReport_NoContraBrokers { get; set; } = ValidateExecutionReport_NoContraBrokers;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport.NoUnderlyingsGroup> ExecutionReport_NoUnderlyings { get; set; } = ValidateExecutionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContAmtsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport.NoContAmtsGroup> ExecutionReport_NoContAmts { get; set; } = ValidateExecutionReport_NoContAmts;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport.NoLegsGroup> ExecutionReport_NoLegs { get; set; } = ValidateExecutionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ExecutionReport.NoMiscFeesGroup> ExecutionReport_NoMiscFees { get; set; } = ValidateExecutionReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.IndicationOfInterest.NoUnderlyingsGroup> IndicationOfInterest_NoUnderlyings { get; set; } = ValidateIndicationOfInterest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.IndicationOfInterest.NoLegsGroup> IndicationOfInterest_NoLegs { get; set; } = ValidateIndicationOfInterest_NoLegs;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoIOIQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.IndicationOfInterest.NoIOIQualifiersGroup> IndicationOfInterest_NoIOIQualifiers { get; set; } = ValidateIndicationOfInterest_NoIOIQualifiers;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.IndicationOfInterest.NoRoutingIDsGroup> IndicationOfInterest_NoRoutingIDs { get; set; } = ValidateIndicationOfInterest_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.ListStatus.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStatus.NoOrdersGroup> ListStatus_NoOrders { get; set; } = ValidateListStatus_NoOrders;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStrikePrice.NoStrikesGroup> ListStrikePrice_NoStrikes { get; set; } = ValidateListStrikePrice_NoStrikes;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.ListStrikePrice.NoUnderlyingsGroup> ListStrikePrice_NoUnderlyings { get; set; } = ValidateListStrikePrice_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Logon.NoMsgTypesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Logon.NoMsgTypesGroup> Logon_NoMsgTypes { get; set; } = ValidateLogon_NoMsgTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup> MarketDataIncrementalRefresh_NoMDEntries { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup> MarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup> MarketDataIncrementalRefresh_NoMDEntries_NoLegs { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoMDEntryTypesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest.NoMDEntryTypesGroup> MarketDataRequest_NoMDEntryTypes { get; set; } = ValidateMarketDataRequest_NoMDEntryTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup> MarketDataRequest_NoRelatedSym { get; set; } = ValidateMarketDataRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup> MarketDataRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup> MarketDataRequest_NoRelatedSym_NoLegs { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequest.NoTradingSessionsGroup> MarketDataRequest_NoTradingSessions { get; set; } = ValidateMarketDataRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MarketDataRequestReject.NoAltMDSourceGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup> MarketDataRequestReject_NoAltMDSource { get; set; } = ValidateMarketDataRequestReject_NoAltMDSource;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup> MarketDataSnapshotFullRefresh_NoUnderlyings { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup> MarketDataSnapshotFullRefresh_NoLegs { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup> MarketDataSnapshotFullRefresh_NoMDEntries { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup> MassQuote_NoQuoteSets { get; set; } = ValidateMassQuote_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup> MassQuote_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup> MassQuote_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup> MassQuoteAcknowledgement_NoQuoteSets { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup> MultilegOrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup> MultilegOrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup> MultilegOrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup> MultilegOrderCancelReplaceRequest_NoLegs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup> MultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.NetworkStatusRequest.NoCompIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NetworkStatusRequest.NoCompIDsGroup> NetworkStatusRequest_NoCompIDs { get; set; } = ValidateNetworkStatusRequest_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NetworkStatusResponse.NoCompIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NetworkStatusResponse.NoCompIDsGroup> NetworkStatusResponse_NoCompIDs { get; set; } = ValidateNetworkStatusResponse_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross.NoSidesGroup> NewOrderCross_NoSides { get; set; } = ValidateNewOrderCross_NoSides;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup> NewOrderCross_NoSides_NoAllocs { get; set; } = ValidateNewOrderCross_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross.NoUnderlyingsGroup> NewOrderCross_NoUnderlyings { get; set; } = ValidateNewOrderCross_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross.NoLegsGroup> NewOrderCross_NoLegs { get; set; } = ValidateNewOrderCross_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderCross.NoTradingSessionsGroup> NewOrderCross_NoTradingSessions { get; set; } = ValidateNewOrderCross_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup> NewOrderList_NoOrders { get; set; } = ValidateNewOrderList_NoOrders;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup> NewOrderList_NoOrders_NoAllocs { get; set; } = ValidateNewOrderList_NoOrders_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup> NewOrderList_NoOrders_NoTradingSessions { get; set; } = ValidateNewOrderList_NoOrders_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup> NewOrderList_NoOrders_NoUnderlyings { get; set; } = ValidateNewOrderList_NoOrders_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg.NoAllocsGroup> NewOrderMultileg_NoAllocs { get; set; } = ValidateNewOrderMultileg_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg.NoTradingSessionsGroup> NewOrderMultileg_NoTradingSessions { get; set; } = ValidateNewOrderMultileg_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg.NoUnderlyingsGroup> NewOrderMultileg_NoUnderlyings { get; set; } = ValidateNewOrderMultileg_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg.NoLegsGroup> NewOrderMultileg_NoLegs { get; set; } = ValidateNewOrderMultileg_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup> NewOrderMultileg_NoLegs_NoLegAllocs { get; set; } = ValidateNewOrderMultileg_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderSingle.NoAllocsGroup> NewOrderSingle_NoAllocs { get; set; } = ValidateNewOrderSingle_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderSingle.NoTradingSessionsGroup> NewOrderSingle_NoTradingSessions { get; set; } = ValidateNewOrderSingle_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.NewOrderSingle.NoUnderlyingsGroup> NewOrderSingle_NoUnderlyings { get; set; } = ValidateNewOrderSingle_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.News.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News.NoRoutingIDsGroup> News_NoRoutingIDs { get; set; } = ValidateNews_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News.NoRelatedSymGroup> News_NoRelatedSym { get; set; } = ValidateNews_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.News.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News.NoLegsGroup> News_NoLegs { get; set; } = ValidateNews_NoLegs;

	/// <summary>Holds one entry of FixMessage.News.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News.NoUnderlyingsGroup> News_NoUnderlyings { get; set; } = ValidateNews_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.News.LinesOfTextGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.News.LinesOfTextGroup> News_LinesOfText { get; set; } = ValidateNews_LinesOfText;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup> OrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup> OrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup> OrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderCancelRequest.NoUnderlyingsGroup> OrderCancelRequest_NoUnderlyings { get; set; } = ValidateOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup> OrderMassCancelReport_NoAffectedOrders { get; set; } = ValidateOrderMassCancelReport_NoAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.OrderStatusRequest.NoUnderlyingsGroup> OrderStatusRequest_NoUnderlyings { get; set; } = ValidateOrderStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceReport.NoLegsGroup> PositionMaintenanceReport_NoLegs { get; set; } = ValidatePositionMaintenanceReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup> PositionMaintenanceReport_NoUnderlyings { get; set; } = ValidatePositionMaintenanceReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup> PositionMaintenanceReport_NoTradingSessions { get; set; } = ValidatePositionMaintenanceReport_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoLegsGroup> PositionMaintenanceRequest_NoLegs { get; set; } = ValidatePositionMaintenanceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup> PositionMaintenanceRequest_NoUnderlyings { get; set; } = ValidatePositionMaintenanceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup> PositionMaintenanceRequest_NoTradingSessions { get; set; } = ValidatePositionMaintenanceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionReport.NoLegsGroup> PositionReport_NoLegs { get; set; } = ValidatePositionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.PositionReport.NoUnderlyingsGroup> PositionReport_NoUnderlyings { get; set; } = ValidatePositionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Quote.NoQuoteQualifiersGroup> Quote_NoQuoteQualifiers { get; set; } = ValidateQuote_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.Quote.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Quote.NoUnderlyingsGroup> Quote_NoUnderlyings { get; set; } = ValidateQuote_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.Quote.NoLegsGroup> Quote_NoLegs { get; set; } = ValidateQuote_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup> QuoteCancel_NoQuoteEntries { get; set; } = ValidateQuoteCancel_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup> QuoteCancel_NoQuoteEntries_NoUnderlyings { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup> QuoteCancel_NoQuoteEntries_NoLegs { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup> QuoteRequest_NoRelatedSym { get; set; } = ValidateQuoteRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup> QuoteRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup> QuoteRequest_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup> QuoteRequest_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup> QuoteRequestReject_NoRelatedSym { get; set; } = ValidateQuoteRequestReject_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup> QuoteRequestReject_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup> QuoteRequestReject_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup> QuoteRequestReject_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteResponse.NoQuoteQualifiersGroup> QuoteResponse_NoQuoteQualifiers { get; set; } = ValidateQuoteResponse_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteResponse.NoUnderlyingsGroup> QuoteResponse_NoUnderlyings { get; set; } = ValidateQuoteResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteResponse.NoLegsGroup> QuoteResponse_NoLegs { get; set; } = ValidateQuoteResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusReport.NoUnderlyingsGroup> QuoteStatusReport_NoUnderlyings { get; set; } = ValidateQuoteStatusReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusReport.NoLegsGroup> QuoteStatusReport_NoLegs { get; set; } = ValidateQuoteStatusReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup> QuoteStatusReport_NoQuoteQualifiers { get; set; } = ValidateQuoteStatusReport_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup> QuoteStatusRequest_NoUnderlyings { get; set; } = ValidateQuoteStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.QuoteStatusRequest.NoLegsGroup> QuoteStatusRequest_NoLegs { get; set; } = ValidateQuoteStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup> RFQRequest_NoRelatedSym { get; set; } = ValidateRFQRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup> RFQRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateRFQRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup> RFQRequest_NoRelatedSym_NoLegs { get; set; } = ValidateRFQRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoRegistDtlsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RegistrationInstructions.NoRegistDtlsGroup> RegistrationInstructions_NoRegistDtls { get; set; } = ValidateRegistrationInstructions_NoRegistDtls;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoDistribInstsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RegistrationInstructions.NoDistribInstsGroup> RegistrationInstructions_NoDistribInsts { get; set; } = ValidateRegistrationInstructions_NoDistribInsts;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositions.NoLegsGroup> RequestForPositions_NoLegs { get; set; } = ValidateRequestForPositions_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositions.NoUnderlyingsGroup> RequestForPositions_NoUnderlyings { get; set; } = ValidateRequestForPositions_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositions.NoTradingSessionsGroup> RequestForPositions_NoTradingSessions { get; set; } = ValidateRequestForPositions_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositionsAck.NoLegsGroup> RequestForPositionsAck_NoLegs { get; set; } = ValidateRequestForPositionsAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup> RequestForPositionsAck_NoUnderlyings { get; set; } = ValidateRequestForPositionsAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinition.NoUnderlyingsGroup> SecurityDefinition_NoUnderlyings { get; set; } = ValidateSecurityDefinition_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinition.NoLegsGroup> SecurityDefinition_NoLegs { get; set; } = ValidateSecurityDefinition_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup> SecurityDefinitionRequest_NoUnderlyings { get; set; } = ValidateSecurityDefinitionRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityDefinitionRequest.NoLegsGroup> SecurityDefinitionRequest_NoLegs { get; set; } = ValidateSecurityDefinitionRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup> SecurityList_NoRelatedSym { get; set; } = ValidateSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup> SecurityList_NoRelatedSym_NoUnderlyings { get; set; } = ValidateSecurityList_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup> SecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityListRequest.NoUnderlyingsGroup> SecurityListRequest_NoUnderlyings { get; set; } = ValidateSecurityListRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityListRequest.NoLegsGroup> SecurityListRequest_NoLegs { get; set; } = ValidateSecurityListRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatus.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatus.NoUnderlyingsGroup> SecurityStatus_NoUnderlyings { get; set; } = ValidateSecurityStatus_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatus.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatus.NoLegsGroup> SecurityStatus_NoLegs { get; set; } = ValidateSecurityStatus_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup> SecurityStatusRequest_NoUnderlyings { get; set; } = ValidateSecurityStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityStatusRequest.NoLegsGroup> SecurityStatusRequest_NoLegs { get; set; } = ValidateSecurityStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityTypes.NoSecurityTypesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SecurityTypes.NoSecurityTypesGroup> SecurityTypes_NoSecurityTypes { get; set; } = ValidateSecurityTypes_NoSecurityTypes;

	/// <summary>Holds one entry of FixMessage.SettlementInstructions.NoSettlInstGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.SettlementInstructions.NoSettlInstGroup> SettlementInstructions_NoSettlInst { get; set; } = ValidateSettlementInstructions_NoSettlInst;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoUnderlyingsGroup> TradeCaptureReport_NoUnderlyings { get; set; } = ValidateTradeCaptureReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoLegsGroup> TradeCaptureReport_NoLegs { get; set; } = ValidateTradeCaptureReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup> TradeCaptureReport_NoSides { get; set; } = ValidateTradeCaptureReport_NoSides;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup> TradeCaptureReport_NoSides_NoClearingInstructions { get; set; } = ValidateTradeCaptureReport_NoSides_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup> TradeCaptureReport_NoSides_NoContAmts { get; set; } = ValidateTradeCaptureReport_NoSides_NoContAmts;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup> TradeCaptureReport_NoSides_NoMiscFees { get; set; } = ValidateTradeCaptureReport_NoSides_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup> TradeCaptureReport_NoSides_NoAllocs { get; set; } = ValidateTradeCaptureReport_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportAck.NoLegsGroup> TradeCaptureReportAck_NoLegs { get; set; } = ValidateTradeCaptureReportAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoAllocsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportAck.NoAllocsGroup> TradeCaptureReportAck_NoAllocs { get; set; } = ValidateTradeCaptureReportAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup> TradeCaptureReportRequest_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoLegsGroup> TradeCaptureReportRequest_NoLegs { get; set; } = ValidateTradeCaptureReportRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoDatesGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequest.NoDatesGroup> TradeCaptureReportRequest_NoDates { get; set; } = ValidateTradeCaptureReportRequest_NoDates;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup> TradeCaptureReportRequestAck_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequestAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoLegsGroup to the schema.</summary>
	public Action<Fix44Context, FixCheck, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup> TradeCaptureReportRequestAck_NoLegs { get; set; } = ValidateTradeCaptureReportRequestAck_NoLegs;

	static void ValidateAdvertisement(Fix44Context context, FixCheck check, FixMessage.Advertisement message)
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

	static void ValidateAllocationInstruction(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction message)
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
		if (message.AllocNoOrdersType is null) Missing(check, FixTag.AllocNoOrdersType);
		else context.Validators.AllocNoOrdersType(context, check, message.AllocNoOrdersType);
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
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, check, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationInstruction_NoAllocs);
	}

	static void ValidateAllocationInstructionAck(Fix44Context context, FixCheck check, FixMessage.AllocationInstructionAck message)
	{
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
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

	static void ValidateAllocationReport(Fix44Context context, FixCheck check, FixMessage.AllocationReport message)
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
		if (message.AllocNoOrdersType is null) Missing(check, FixTag.AllocNoOrdersType);
		else context.Validators.AllocNoOrdersType(context, check, message.AllocNoOrdersType);
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
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, check, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.AllocationReport_NoAllocs);
	}

	static void ValidateAllocationReportAck(Fix44Context context, FixCheck check, FixMessage.AllocationReportAck message)
	{
		if (message.AllocReportID is null) Missing(check, FixTag.AllocReportID);
		else context.Validators.AllocReportID(context, check, message.AllocReportID);
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, check, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AllocStatus is null) Missing(check, FixTag.AllocStatus);
		else context.Validators.AllocStatus(context, check, message.AllocStatus);
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

	static void ValidateAssignmentReport(Fix44Context context, FixCheck check, FixMessage.AssignmentReport message)
	{
		if (message.AsgnRptID is null) Missing(check, FixTag.AsgnRptID);
		else context.Validators.AsgnRptID(context, check, message.AsgnRptID);
		if (message.TotNumAssignmentReports is not null) context.Validators.TotNumAssignmentReports(context, check, message.TotNumAssignmentReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, check, message.LastRptRequested);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.AssignmentReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.AssignmentReport_NoUnderlyings);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (Empty((IPositionAmountData)message)) Absent(check, FixTag.NoPosAmt);
		else context.Validators.PositionAmountData(context, check, message);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.SettlPrice is null) Missing(check, FixTag.SettlPrice);
		else context.Validators.SettlPrice(context, check, message.SettlPrice);
		if (message.SettlPriceType is null) Missing(check, FixTag.SettlPriceType);
		else context.Validators.SettlPriceType(context, check, message.SettlPriceType);
		if (message.UnderlyingSettlPrice is null) Missing(check, FixTag.UnderlyingSettlPrice);
		else context.Validators.UnderlyingSettlPrice(context, check, message.UnderlyingSettlPrice);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.AssignmentMethod is null) Missing(check, FixTag.AssignmentMethod);
		else context.Validators.AssignmentMethod(context, check, message.AssignmentMethod);
		if (message.AssignmentUnit is not null) context.Validators.AssignmentUnit(context, check, message.AssignmentUnit);
		if (message.OpenInterest is null) Missing(check, FixTag.OpenInterest);
		else context.Validators.OpenInterest(context, check, message.OpenInterest);
		if (message.ExerciseMethod is null) Missing(check, FixTag.ExerciseMethod);
		else context.Validators.ExerciseMethod(context, check, message.ExerciseMethod);
		if (message.SettlSessID is null) Missing(check, FixTag.SettlSessID);
		else context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is null) Missing(check, FixTag.SettlSessSubID);
		else context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateBidRequest(Fix44Context context, FixCheck check, FixMessage.BidRequest message)
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

	static void ValidateBidResponse(Fix44Context context, FixCheck check, FixMessage.BidResponse message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.NoBidComponents is null) Missing(check, FixTag.NoBidComponents);
		else context.Validators.NoBidComponents(context, check, message.NoBidComponents);
		Counted(check, message.NoBidComponents, message.NoBidComponentsGroups);
		Each(context, check, FixTag.NoBidComponents, message.NoBidComponentsGroups, context.Validators.BidResponse_NoBidComponents);
	}

	static void ValidateBusinessMessageReject(Fix44Context context, FixCheck check, FixMessage.BusinessMessageReject message)
	{
		if (message.RefSeqNum is not null) context.Validators.RefSeqNum(context, check, message.RefSeqNum);
		if (message.RefMsgType is null) Missing(check, FixTag.RefMsgType);
		else context.Validators.RefMsgType(context, check, message.RefMsgType);
		if (message.BusinessRejectRefID is not null) context.Validators.BusinessRejectRefID(context, check, message.BusinessRejectRefID);
		if (message.BusinessRejectReason is null) Missing(check, FixTag.BusinessRejectReason);
		else context.Validators.BusinessRejectReason(context, check, message.BusinessRejectReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateCollateralAssignment(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment message)
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

	static void ValidateCollateralInquiry(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry message)
	{
		if (message.CollInquiryID is not null) context.Validators.CollInquiryID(context, check, message.CollInquiryID);
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

	static void ValidateCollateralInquiryAck(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck message)
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

	static void ValidateCollateralReport(Fix44Context context, FixCheck check, FixMessage.CollateralReport message)
	{
		if (message.CollRptID is null) Missing(check, FixTag.CollRptID);
		else context.Validators.CollRptID(context, check, message.CollRptID);
		if (message.CollInquiryID is not null) context.Validators.CollInquiryID(context, check, message.CollInquiryID);
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

	static void ValidateCollateralRequest(Fix44Context context, FixCheck check, FixMessage.CollateralRequest message)
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

	static void ValidateCollateralResponse(Fix44Context context, FixCheck check, FixMessage.CollateralResponse message)
	{
		if (message.CollRespID is null) Missing(check, FixTag.CollRespID);
		else context.Validators.CollRespID(context, check, message.CollRespID);
		if (message.CollAsgnID is null) Missing(check, FixTag.CollAsgnID);
		else context.Validators.CollAsgnID(context, check, message.CollAsgnID);
		if (message.CollReqID is not null) context.Validators.CollReqID(context, check, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(check, FixTag.CollAsgnReason);
		else context.Validators.CollAsgnReason(context, check, message.CollAsgnReason);
		if (message.CollAsgnTransType is not null) context.Validators.CollAsgnTransType(context, check, message.CollAsgnTransType);
		if (message.CollAsgnRespType is null) Missing(check, FixTag.CollAsgnRespType);
		else context.Validators.CollAsgnRespType(context, check, message.CollAsgnRespType);
		if (message.CollAsgnRejectReason is not null) context.Validators.CollAsgnRejectReason(context, check, message.CollAsgnRejectReason);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
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

	static void ValidateConfirmation(Fix44Context context, FixCheck check, FixMessage.Confirmation message)
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
		if (message.NoUnderlyings is null) Missing(check, FixTag.NoUnderlyings);
		else context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.Confirmation_NoUnderlyings);
		if (message.NoLegs is null) Missing(check, FixTag.NoLegs);
		else context.Validators.NoLegs(context, check, message.NoLegs);
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

	static void ValidateConfirmationAck(Fix44Context context, FixCheck check, FixMessage.ConfirmationAck message)
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

	static void ValidateConfirmationRequest(Fix44Context context, FixCheck check, FixMessage.ConfirmationRequest message)
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

	static void ValidateCrossOrderCancelReplaceRequest(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.OrigCrossID is null) Missing(check, FixTag.OrigCrossID);
		else context.Validators.OrigCrossID(context, check, message.OrigCrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
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
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.CrossOrderCancelReplaceRequest_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
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
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
	}

	static void ValidateCrossOrderCancelRequest(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.OrigCrossID is null) Missing(check, FixTag.OrigCrossID);
		else context.Validators.OrigCrossID(context, check, message.OrigCrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
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

	static void ValidateDerivativeSecurityList(Fix44Context context, FixCheck check, FixMessage.DerivativeSecurityList message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(check, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is null) Missing(check, FixTag.SecurityRequestResult);
		else context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.DerivativeSecurityList_NoRelatedSym);
	}

	static void ValidateDerivativeSecurityListRequest(Fix44Context context, FixCheck check, FixMessage.DerivativeSecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(check, FixTag.SecurityListRequestType);
		else context.Validators.SecurityListRequestType(context, check, message.SecurityListRequestType);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, message.SecuritySubType);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateDontKnowTrade(Fix44Context context, FixCheck check, FixMessage.DontKnowTrade message)
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

	static void ValidateEmail(Fix44Context context, FixCheck check, FixMessage.Email message)
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
		if (message.LinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.LinesOfText(context, check, message.LinesOfText);
		Counted(check, message.LinesOfText, message.LinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.LinesOfTextGroups, context.Validators.Email_LinesOfText);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateExecutionReport(Fix44Context context, FixCheck check, FixMessage.ExecutionReport message)
	{
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
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
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
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.PeggedPrice is not null) context.Validators.PeggedPrice(context, check, message.PeggedPrice);
		if (message.DiscretionPrice is not null) context.Validators.DiscretionPrice(context, check, message.DiscretionPrice);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
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
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.LastQty is not null) context.Validators.LastQty(context, check, message.LastQty);
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
		if (message.AvgPx is null) Missing(check, FixTag.AvgPx);
		else context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.DayOrderQty is not null) context.Validators.DayOrderQty(context, check, message.DayOrderQty);
		if (message.DayCumQty is not null) context.Validators.DayCumQty(context, check, message.DayCumQty);
		if (message.DayAvgPx is not null) context.Validators.DayAvgPx(context, check, message.DayAvgPx);
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
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
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
	}

	static void ValidateHeartbeat(Fix44Context context, FixCheck check, FixMessage.Heartbeat message)
	{
		if (message.TestReqID is not null) context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateIndicationOfInterest(Fix44Context context, FixCheck check, FixMessage.IndicationOfInterest message)
	{
		if (message.IOIid is null) Missing(check, FixTag.IOIID);
		else context.Validators.IOIid(context, check, message.IOIid);
		if (message.IOITransType is null) Missing(check, FixTag.IOITransType);
		else context.Validators.IOITransType(context, check, message.IOITransType);
		if (message.IOIRefID is not null) context.Validators.IOIRefID(context, check, message.IOIRefID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.IndicationOfInterest_NoUnderlyings);
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
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.IndicationOfInterest_NoLegs);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.IOIQltyInd is not null) context.Validators.IOIQltyInd(context, check, message.IOIQltyInd);
		if (message.IOINaturalFlag is not null) context.Validators.IOINaturalFlag(context, check, message.IOINaturalFlag);
		if (message.NoIOIQualifiers is not null) context.Validators.NoIOIQualifiers(context, check, message.NoIOIQualifiers);
		Counted(check, message.NoIOIQualifiers, message.NoIOIQualifiersGroups);
		Each(context, check, FixTag.NoIOIQualifiers, message.NoIOIQualifiersGroups, context.Validators.IndicationOfInterest_NoIOIQualifiers);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.IndicationOfInterest_NoRoutingIDs);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
	}

	static void ValidateListCancelRequest(Fix44Context context, FixCheck check, FixMessage.ListCancelRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListExecute(Fix44Context context, FixCheck check, FixMessage.ListExecute message)
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

	static void ValidateListStatus(Fix44Context context, FixCheck check, FixMessage.ListStatus message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.ListStatusType is null) Missing(check, FixTag.ListStatusType);
		else context.Validators.ListStatusType(context, check, message.ListStatusType);
		if (message.NoRpts is null) Missing(check, FixTag.NoRpts);
		else context.Validators.NoRpts(context, check, message.NoRpts);
		if (message.ListOrderStatus is null) Missing(check, FixTag.ListOrderStatus);
		else context.Validators.ListOrderStatus(context, check, message.ListOrderStatus);
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

	static void ValidateListStatusRequest(Fix44Context context, FixCheck check, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListStrikePrice(Fix44Context context, FixCheck check, FixMessage.ListStrikePrice message)
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
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.ListStrikePrice_NoUnderlyings);
	}

	static void ValidateLogon(Fix44Context context, FixCheck check, FixMessage.Logon message)
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
	}

	static void ValidateLogout(Fix44Context context, FixCheck check, FixMessage.Logout message)
	{
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataIncrementalRefresh(Fix44Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.NoMDEntries is null) Missing(check, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, check, message.NoMDEntries);
		Counted(check, message.NoMDEntries, message.NoMDEntriesGroups);
		Each(context, check, FixTag.NoMDEntries, message.NoMDEntriesGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries);
		if (message.ApplQueueDepth is not null) context.Validators.ApplQueueDepth(context, check, message.ApplQueueDepth);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, check, message.ApplQueueResolution);
	}

	static void ValidateMarketDataRequest(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
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
	}

	static void ValidateMarketDataRequestReject(Fix44Context context, FixCheck check, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, check, message.MDReqRejReason);
		if (message.NoAltMDSource is not null) context.Validators.NoAltMDSource(context, check, message.NoAltMDSource);
		Counted(check, message.NoAltMDSource, message.NoAltMDSourceGroups);
		Each(context, check, FixTag.NoAltMDSource, message.NoAltMDSourceGroups, context.Validators.MarketDataRequestReject_NoAltMDSource);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataSnapshotFullRefresh(Fix44Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
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
	}

	static void ValidateMassQuote(Fix44Context context, FixCheck check, FixMessage.MassQuote message)
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

	static void ValidateMassQuoteAcknowledgement(Fix44Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteStatus is null) Missing(check, FixTag.QuoteStatus);
		else context.Validators.QuoteStatus(context, check, message.QuoteStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, check, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
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

	static void ValidateMultilegOrderCancelReplaceRequest(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
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
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.MultilegOrderCancelReplaceRequest_NoAllocs);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, check, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.MultilegOrderCancelReplaceRequest_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.MultilegOrderCancelReplaceRequest_NoUnderlyings);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.NoLegs is null) Missing(check, FixTag.NoLegs);
		else context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.MultilegOrderCancelReplaceRequest_NoLegs);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
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
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, check, message.MultiLegRptTypeReq);
	}

	static void ValidateNetworkStatusRequest(Fix44Context context, FixCheck check, FixMessage.NetworkStatusRequest message)
	{
		if (message.NetworkRequestType is null) Missing(check, FixTag.NetworkRequestType);
		else context.Validators.NetworkRequestType(context, check, message.NetworkRequestType);
		if (message.NetworkRequestID is null) Missing(check, FixTag.NetworkRequestID);
		else context.Validators.NetworkRequestID(context, check, message.NetworkRequestID);
		if (message.NoCompIDs is not null) context.Validators.NoCompIDs(context, check, message.NoCompIDs);
		Counted(check, message.NoCompIDs, message.NoCompIDsGroups);
		Each(context, check, FixTag.NoCompIDs, message.NoCompIDsGroups, context.Validators.NetworkStatusRequest_NoCompIDs);
	}

	static void ValidateNetworkStatusResponse(Fix44Context context, FixCheck check, FixMessage.NetworkStatusResponse message)
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
		Each(context, check, FixTag.NoCompIDs, message.NoCompIDsGroups, context.Validators.NetworkStatusResponse_NoCompIDs);
	}

	static void ValidateNewOrderCross(Fix44Context context, FixCheck check, FixMessage.NewOrderCross message)
	{
		if (message.CrossID is null) Missing(check, FixTag.CrossID);
		else context.Validators.CrossID(context, check, message.CrossID);
		if (message.CrossType is null) Missing(check, FixTag.CrossType);
		else context.Validators.CrossType(context, check, message.CrossType);
		if (message.CrossPrioritization is null) Missing(check, FixTag.CrossPrioritization);
		else context.Validators.CrossPrioritization(context, check, message.CrossPrioritization);
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
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderCross_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
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
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
	}

	static void ValidateNewOrderList(Fix44Context context, FixCheck check, FixMessage.NewOrderList message)
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
		if (message.EncodedListExecInstLen is not null) context.Validators.EncodedListExecInstLen(context, check, message.EncodedListExecInstLen);
		if (message.EncodedListExecInst is not null) context.Validators.EncodedListExecInst(context, check, message.EncodedListExecInst);
		if (message.AllowableOneSidednessPct is not null) context.Validators.AllowableOneSidednessPct(context, check, message.AllowableOneSidednessPct);
		if (message.AllowableOneSidednessValue is not null) context.Validators.AllowableOneSidednessValue(context, check, message.AllowableOneSidednessValue);
		if (message.AllowableOneSidednessCurr is not null) context.Validators.AllowableOneSidednessCurr(context, check, message.AllowableOneSidednessCurr);
		if (message.TotNoOrders is null) Missing(check, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, check, message.TotNoOrders);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoOrders is null) Missing(check, FixTag.NoOrders);
		else context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.NewOrderList_NoOrders);
	}

	static void ValidateNewOrderMultileg(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg message)
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
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderMultileg_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.NewOrderMultileg_NoUnderlyings);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.NoLegs is null) Missing(check, FixTag.NoLegs);
		else context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.NewOrderMultileg_NoLegs);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, check, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(check, FixTag.OrderQty);
		else context.Validators.OrderQtyData(context, check, message);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
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
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, check, message.MultiLegRptTypeReq);
	}

	static void ValidateNewOrderSingle(Fix44Context context, FixCheck check, FixMessage.NewOrderSingle message)
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
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
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
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, check, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
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
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, check, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, check, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, check, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, check, message.Designation);
	}

	static void ValidateNews(Fix44Context context, FixCheck check, FixMessage.News message)
	{
		if (message.OrigTime is not null) context.Validators.OrigTime(context, check, message.OrigTime);
		if (message.Urgency is not null) context.Validators.Urgency(context, check, message.Urgency);
		if (message.Headline is null) Missing(check, FixTag.Headline);
		else context.Validators.Headline(context, check, message.Headline);
		if (message.EncodedHeadlineLen is not null) context.Validators.EncodedHeadlineLen(context, check, message.EncodedHeadlineLen);
		if (message.EncodedHeadline is not null) context.Validators.EncodedHeadline(context, check, message.EncodedHeadline);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.News_NoRoutingIDs);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.News_NoRelatedSym);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.News_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.News_NoUnderlyings);
		if (message.LinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.LinesOfText(context, check, message.LinesOfText);
		Counted(check, message.LinesOfText, message.LinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.LinesOfTextGroups, context.Validators.News_LinesOfText);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateOrderCancelReject(Fix44Context context, FixCheck check, FixMessage.OrderCancelReject message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, check, message.ClOrdLinkID);
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
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

	static void ValidateOrderCancelReplaceRequest(Fix44Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, check, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
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
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
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
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, check, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, check, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, check, message.TargetStrategy);
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
	}

	static void ValidateOrderCancelRequest(Fix44Context context, FixCheck check, FixMessage.OrderCancelRequest message)
	{
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
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

	static void ValidateOrderMassCancelReport(Fix44Context context, FixCheck check, FixMessage.OrderMassCancelReport message)
	{
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
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
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassCancelRequest(Fix44Context context, FixCheck check, FixMessage.OrderMassCancelRequest message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, message.SecondaryClOrdID);
		if (message.MassCancelRequestType is null) Missing(check, FixTag.MassCancelRequestType);
		else context.Validators.MassCancelRequestType(context, check, message.MassCancelRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderMassStatusRequest(Fix44Context context, FixCheck check, FixMessage.OrderMassStatusRequest message)
	{
		if (message.MassStatusReqID is null) Missing(check, FixTag.MassStatusReqID);
		else context.Validators.MassStatusReqID(context, check, message.MassStatusReqID);
		if (message.MassStatusReqType is null) Missing(check, FixTag.MassStatusReqType);
		else context.Validators.MassStatusReqType(context, check, message.MassStatusReqType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, check, message);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
	}

	static void ValidateOrderStatusRequest(Fix44Context context, FixCheck check, FixMessage.OrderStatusRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
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

	static void ValidatePositionMaintenanceReport(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceReport message)
	{
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosTransType is null) Missing(check, FixTag.PosTransType);
		else context.Validators.PosTransType(context, check, message.PosTransType);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosMaintAction is null) Missing(check, FixTag.PosMaintAction);
		else context.Validators.PosMaintAction(context, check, message.PosMaintAction);
		if (message.OrigPosReqRefID is null) Missing(check, FixTag.OrigPosReqRefID);
		else context.Validators.OrigPosReqRefID(context, check, message.OrigPosReqRefID);
		if (message.PosMaintStatus is null) Missing(check, FixTag.PosMaintStatus);
		else context.Validators.PosMaintStatus(context, check, message.PosMaintStatus);
		if (message.PosMaintResult is not null) context.Validators.PosMaintResult(context, check, message.PosMaintResult);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is null) Missing(check, FixTag.Account);
		else context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.PositionMaintenanceReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.PositionMaintenanceReport_NoUnderlyings);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.PositionMaintenanceReport_NoTradingSessions);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (Empty((IPositionAmountData)message)) Absent(check, FixTag.NoPosAmt);
		else context.Validators.PositionAmountData(context, check, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, check, message.AdjustmentType);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidatePositionMaintenanceRequest(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceRequest message)
	{
		if (message.PosReqID is null) Missing(check, FixTag.PosReqID);
		else context.Validators.PosReqID(context, check, message.PosReqID);
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
		if (message.Account is null) Missing(check, FixTag.Account);
		else context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
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
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, check, message.AdjustmentType);
		if (message.ContraryInstructionIndicator is not null) context.Validators.ContraryInstructionIndicator(context, check, message.ContraryInstructionIndicator);
		if (message.PriorSpreadIndicator is not null) context.Validators.PriorSpreadIndicator(context, check, message.PriorSpreadIndicator);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, check, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidatePositionReport(Fix44Context context, FixCheck check, FixMessage.PositionReport message)
	{
		if (message.PosMaintRptID is null) Missing(check, FixTag.PosMaintRptID);
		else context.Validators.PosMaintRptID(context, check, message.PosMaintRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosReqType is not null) context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TotalNumPosReports is not null) context.Validators.TotalNumPosReports(context, check, message.TotalNumPosReports);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.PosReqResult is null) Missing(check, FixTag.PosReqResult);
		else context.Validators.PosReqResult(context, check, message.PosReqResult);
		if (message.ClearingBusinessDate is null) Missing(check, FixTag.ClearingBusinessDate);
		else context.Validators.ClearingBusinessDate(context, check, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, check, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, check, message.SettlSessSubID);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is null) Missing(check, FixTag.Account);
		else context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SettlPrice is null) Missing(check, FixTag.SettlPrice);
		else context.Validators.SettlPrice(context, check, message.SettlPrice);
		if (message.SettlPriceType is null) Missing(check, FixTag.SettlPriceType);
		else context.Validators.SettlPriceType(context, check, message.SettlPriceType);
		if (message.PriorSettlPrice is null) Missing(check, FixTag.PriorSettlPrice);
		else context.Validators.PriorSettlPrice(context, check, message.PriorSettlPrice);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.PositionReport_NoLegs);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.PositionReport_NoUnderlyings);
		if (Empty((IPositionQty)message)) Absent(check, FixTag.NoPositions);
		else context.Validators.PositionQty(context, check, message);
		if (Empty((IPositionAmountData)message)) Absent(check, FixTag.NoPosAmt);
		else context.Validators.PositionAmountData(context, check, message);
		if (message.RegistStatus is not null) context.Validators.RegistStatus(context, check, message.RegistStatus);
		if (message.DeliveryDate is not null) context.Validators.DeliveryDate(context, check, message.DeliveryDate);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuote(Fix44Context context, FixCheck check, FixMessage.Quote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
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
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteCancel(Fix44Context context, FixCheck check, FixMessage.QuoteCancel message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteCancelType is null) Missing(check, FixTag.QuoteCancelType);
		else context.Validators.QuoteCancelType(context, check, message.QuoteCancelType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, check, message.NoQuoteEntries);
		Counted(check, message.NoQuoteEntries, message.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, message.NoQuoteEntriesGroups, context.Validators.QuoteCancel_NoQuoteEntries);
	}

	static void ValidateQuoteRequest(Fix44Context context, FixCheck check, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID is null) Missing(check, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.QuoteRequest_NoRelatedSym);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteRequestReject(Fix44Context context, FixCheck check, FixMessage.QuoteRequestReject message)
	{
		if (message.QuoteReqID is null) Missing(check, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (message.QuoteRequestRejectReason is null) Missing(check, FixTag.QuoteRequestRejectReason);
		else context.Validators.QuoteRequestRejectReason(context, check, message.QuoteRequestRejectReason);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.QuoteRequestReject_NoRelatedSym);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteResponse(Fix44Context context, FixCheck check, FixMessage.QuoteResponse message)
	{
		if (message.QuoteRespID is null) Missing(check, FixTag.QuoteRespID);
		else context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteRespType is null) Missing(check, FixTag.QuoteRespType);
		else context.Validators.QuoteRespType(context, check, message.QuoteRespType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.IOIid is not null) context.Validators.IOIid(context, check, message.IOIid);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
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
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, check, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, check, message);
	}

	static void ValidateQuoteStatusReport(Fix44Context context, FixCheck check, FixMessage.QuoteStatusReport message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, check, message.QuoteStatusReqID);
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, check, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, check, message.QuoteType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
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
		if (message.QuoteStatus is not null) context.Validators.QuoteStatus(context, check, message.QuoteStatus);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateQuoteStatusRequest(Fix44Context context, FixCheck check, FixMessage.QuoteStatusRequest message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, check, message.QuoteStatusReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.QuoteStatusRequest_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.QuoteStatusRequest_NoLegs);
		if (!Empty((IParties)message)) context.Validators.Parties(context, check, message);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateRFQRequest(Fix44Context context, FixCheck check, FixMessage.RFQRequest message)
	{
		if (message.RFQReqID is null) Missing(check, FixTag.RFQReqID);
		else context.Validators.RFQReqID(context, check, message.RFQReqID);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.RFQRequest_NoRelatedSym);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateRegistrationInstructions(Fix44Context context, FixCheck check, FixMessage.RegistrationInstructions message)
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

	static void ValidateRegistrationInstructionsResponse(Fix44Context context, FixCheck check, FixMessage.RegistrationInstructionsResponse message)
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

	static void ValidateReject(Fix44Context context, FixCheck check, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(check, FixTag.RefSeqNum);
		else context.Validators.RefSeqNum(context, check, message.RefSeqNum);
		if (message.RefTagID is not null) context.Validators.RefTagID(context, check, message.RefTagID);
		if (message.RefMsgType is not null) context.Validators.RefMsgType(context, check, message.RefMsgType);
		if (message.SessionRejectReason is not null) context.Validators.SessionRejectReason(context, check, message.SessionRejectReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateRequestForPositions(Fix44Context context, FixCheck check, FixMessage.RequestForPositions message)
	{
		if (message.PosReqID is null) Missing(check, FixTag.PosReqID);
		else context.Validators.PosReqID(context, check, message.PosReqID);
		if (message.PosReqType is null) Missing(check, FixTag.PosReqType);
		else context.Validators.PosReqType(context, check, message.PosReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is null) Missing(check, FixTag.Account);
		else context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
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

	static void ValidateRequestForPositionsAck(Fix44Context context, FixCheck check, FixMessage.RequestForPositionsAck message)
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
		if (Empty((IParties)message)) Absent(check, FixTag.NoPartyIDs);
		else context.Validators.Parties(context, check, message);
		if (message.Account is null) Missing(check, FixTag.Account);
		else context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is null) Missing(check, FixTag.AccountType);
		else context.Validators.AccountType(context, check, message.AccountType);
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

	static void ValidateResendRequest(Fix44Context context, FixCheck check, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(check, FixTag.BeginSeqNo);
		else context.Validators.BeginSeqNo(context, check, message.BeginSeqNo);
		if (message.EndSeqNo is null) Missing(check, FixTag.EndSeqNo);
		else context.Validators.EndSeqNo(context, check, message.EndSeqNo);
	}

	static void ValidateSecurityDefinition(Fix44Context context, FixCheck check, FixMessage.SecurityDefinition message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(check, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityResponseType is null) Missing(check, FixTag.SecurityResponseType);
		else context.Validators.SecurityResponseType(context, check, message.SecurityResponseType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityDefinition_NoUnderlyings);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityDefinition_NoLegs);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, message.ExpirationCycle);
		if (message.RoundLot is not null) context.Validators.RoundLot(context, check, message.RoundLot);
		if (message.MinTradeVol is not null) context.Validators.MinTradeVol(context, check, message.MinTradeVol);
	}

	static void ValidateSecurityDefinitionRequest(Fix44Context context, FixCheck check, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityRequestType is null) Missing(check, FixTag.SecurityRequestType);
		else context.Validators.SecurityRequestType(context, check, message.SecurityRequestType);
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
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityDefinitionRequest_NoLegs);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, message.ExpirationCycle);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateSecurityList(Fix44Context context, FixCheck check, FixMessage.SecurityList message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(check, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityRequestResult is null) Missing(check, FixTag.SecurityRequestResult);
		else context.Validators.SecurityRequestResult(context, check, message.SecurityRequestResult);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, check, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, check, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.SecurityList_NoRelatedSym);
	}

	static void ValidateSecurityListRequest(Fix44Context context, FixCheck check, FixMessage.SecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(check, FixTag.SecurityListRequestType);
		else context.Validators.SecurityListRequestType(context, check, message.SecurityListRequestType);
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

	static void ValidateSecurityStatus(Fix44Context context, FixCheck check, FixMessage.SecurityStatus message)
	{
		if (message.SecurityStatusReqID is not null) context.Validators.SecurityStatusReqID(context, check, message.SecurityStatusReqID);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, check, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, message.NoUnderlyings);
		Counted(check, message.NoUnderlyings, message.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, message.NoUnderlyingsGroups, context.Validators.SecurityStatus_NoUnderlyings);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.SecurityStatus_NoLegs);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, check, message.SecurityTradingStatus);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (message.HaltReason is not null) context.Validators.HaltReason(context, check, message.HaltReason);
		if (message.InViewOfCommon is not null) context.Validators.InViewOfCommon(context, check, message.InViewOfCommon);
		if (message.DueToRelated is not null) context.Validators.DueToRelated(context, check, message.DueToRelated);
		if (message.BuyVolume is not null) context.Validators.BuyVolume(context, check, message.BuyVolume);
		if (message.SellVolume is not null) context.Validators.SellVolume(context, check, message.SellVolume);
		if (message.HighPx is not null) context.Validators.HighPx(context, check, message.HighPx);
		if (message.LowPx is not null) context.Validators.LowPx(context, check, message.LowPx);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Adjustment is not null) context.Validators.Adjustment(context, check, message.Adjustment);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateSecurityStatusRequest(Fix44Context context, FixCheck check, FixMessage.SecurityStatusRequest message)
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
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
	}

	static void ValidateSecurityTypeRequest(Fix44Context context, FixCheck check, FixMessage.SecurityTypeRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.Product is not null) context.Validators.Product(context, check, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, message.SecuritySubType);
	}

	static void ValidateSecurityTypes(Fix44Context context, FixCheck check, FixMessage.SecurityTypes message)
	{
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
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateSequenceReset(Fix44Context context, FixCheck check, FixMessage.SequenceReset message)
	{
		if (message.GapFillFlag is not null) context.Validators.GapFillFlag(context, check, message.GapFillFlag);
		if (message.NewSeqNo is null) Missing(check, FixTag.NewSeqNo);
		else context.Validators.NewSeqNo(context, check, message.NewSeqNo);
	}

	static void ValidateSettlementInstructionRequest(Fix44Context context, FixCheck check, FixMessage.SettlementInstructionRequest message)
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
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, check, message.LastUpdateTime);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, check, message.StandInstDbType);
		if (message.StandInstDbName is not null) context.Validators.StandInstDbName(context, check, message.StandInstDbName);
		if (message.StandInstDbID is not null) context.Validators.StandInstDbID(context, check, message.StandInstDbID);
	}

	static void ValidateSettlementInstructions(Fix44Context context, FixCheck check, FixMessage.SettlementInstructions message)
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

	static void ValidateTestRequest(Fix44Context context, FixCheck check, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(check, FixTag.TestReqID);
		else context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateTradeCaptureReport(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport message)
	{
		if (message.TradeReportID is null) Missing(check, FixTag.TradeReportID);
		else context.Validators.TradeReportID(context, check, message.TradeReportID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, check, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, check, message.TradeReportType);
		if (message.TradeRequestID is not null) context.Validators.TradeRequestID(context, check, message.TradeRequestID);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, check, message.SecondaryTrdType);
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
		if (message.OrdStatus is not null) context.Validators.OrdStatus(context, check, message.OrdStatus);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, message.SecondaryExecID);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, check, message.ExecRestatementReason);
		if (message.PreviouslyReported is null) Missing(check, FixTag.PreviouslyReported);
		else context.Validators.PreviouslyReported(context, check, message.PreviouslyReported);
		if (message.PriceType is not null) context.Validators.PriceType(context, check, message.PriceType);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, check, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, check, message);
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
		if (message.LastParPx is not null) context.Validators.LastParPx(context, check, message.LastParPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, check, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, check, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
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
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, check, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, check, message.SettlDate);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, check, message.MatchStatus);
		if (message.MatchType is not null) context.Validators.MatchType(context, check, message.MatchType);
		if (message.NoSides is null) Missing(check, FixTag.NoSides);
		else context.Validators.NoSides(context, check, message.NoSides);
		Counted(check, message.NoSides, message.NoSidesGroups);
		Each(context, check, FixTag.NoSides, message.NoSidesGroups, context.Validators.TradeCaptureReport_NoSides);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, check, message.CopyMsgIndicator);
		if (message.PublishTrdIndicator is not null) context.Validators.PublishTrdIndicator(context, check, message.PublishTrdIndicator);
		if (message.ShortSaleReason is not null) context.Validators.ShortSaleReason(context, check, message.ShortSaleReason);
	}

	static void ValidateTradeCaptureReportAck(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportAck message)
	{
		if (message.TradeReportID is null) Missing(check, FixTag.TradeReportID);
		else context.Validators.TradeReportID(context, check, message.TradeReportID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, check, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, check, message.TradeReportType);
		if (message.TrdType is not null) context.Validators.TrdType(context, check, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, check, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, check, message.SecondaryTrdType);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, check, message.TransferReason);
		if (message.ExecType is null) Missing(check, FixTag.ExecType);
		else context.Validators.ExecType(context, check, message.ExecType);
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
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, check, message);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, check, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, check, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, check, message.NoLegs);
		Counted(check, message.NoLegs, message.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, message.NoLegsGroups, context.Validators.TradeCaptureReportAck_NoLegs);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, check, message.ClearingFeeIndicator);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, message.CustOrderCapacity);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, check, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, check, message.AccountType);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, check, message.PositionEffect);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, check, message.PreallocMethod);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.TradeCaptureReportAck_NoAllocs);
	}

	static void ValidateTradeCaptureReportRequest(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequest message)
	{
		if (message.TradeRequestID is null) Missing(check, FixTag.TradeRequestID);
		else context.Validators.TradeRequestID(context, check, message.TradeRequestID);
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
	}

	static void ValidateTradeCaptureReportRequestAck(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck message)
	{
		if (message.TradeRequestID is null) Missing(check, FixTag.TradeRequestID);
		else context.Validators.TradeRequestID(context, check, message.TradeRequestID);
		if (message.TradeRequestType is null) Missing(check, FixTag.TradeRequestType);
		else context.Validators.TradeRequestType(context, check, message.TradeRequestType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TotNumTradeReports is not null) context.Validators.TotNumTradeReports(context, check, message.TotNumTradeReports);
		if (message.TradeRequestResult is null) Missing(check, FixTag.TradeRequestResult);
		else context.Validators.TradeRequestResult(context, check, message.TradeRequestResult);
		if (message.TradeRequestStatus is null) Missing(check, FixTag.TradeRequestStatus);
		else context.Validators.TradeRequestStatus(context, check, message.TradeRequestStatus);
		if (Empty((IInstrument)message)) Absent(check, FixTag.Symbol);
		else context.Validators.Instrument(context, check, message);
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
	}

	static void ValidateTradingSessionStatus(Fix44Context context, FixCheck check, FixMessage.TradingSessionStatus message)
	{
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.TradingSessionID is null) Missing(check, FixTag.TradingSessionID);
		else context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.TradSesStatus is null) Missing(check, FixTag.TradSesStatus);
		else context.Validators.TradSesStatus(context, check, message.TradSesStatus);
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
	}

	static void ValidateTradingSessionStatusRequest(Fix44Context context, FixCheck check, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.TradSesReqID is null) Missing(check, FixTag.TradSesReqID);
		else context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateUserRequest(Fix44Context context, FixCheck check, FixMessage.UserRequest message)
	{
		if (message.UserRequestID is null) Missing(check, FixTag.UserRequestID);
		else context.Validators.UserRequestID(context, check, message.UserRequestID);
		if (message.UserRequestType is null) Missing(check, FixTag.UserRequestType);
		else context.Validators.UserRequestType(context, check, message.UserRequestType);
		if (message.Username is null) Missing(check, FixTag.Username);
		else context.Validators.Username(context, check, message.Username);
		if (message.Password is not null) context.Validators.Password(context, check, message.Password);
		if (message.NewPassword is not null) context.Validators.NewPassword(context, check, message.NewPassword);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateUserResponse(Fix44Context context, FixCheck check, FixMessage.UserResponse message)
	{
		if (message.UserRequestID is null) Missing(check, FixTag.UserRequestID);
		else context.Validators.UserRequestID(context, check, message.UserRequestID);
		if (message.Username is null) Missing(check, FixTag.Username);
		else context.Validators.Username(context, check, message.Username);
		if (message.UserStatus is not null) context.Validators.UserStatus(context, check, message.UserStatus);
		if (message.UserStatusText is not null) context.Validators.UserStatusText(context, check, message.UserStatusText);
	}

	static void ValidateXMLnonFIX(Fix44Context context, FixCheck check, FixMessage.XMLnonFIX message)
	{
	}

	static void ValidateCommissionData(Fix44Context context, FixCheck check, ICommissionData block)
	{
		if (block.Commission is not null) context.Validators.Commission(context, check, block.Commission);
		if (block.CommType is not null) context.Validators.CommType(context, check, block.CommType);
		if (block.CommCurrency is not null) context.Validators.CommCurrency(context, check, block.CommCurrency);
		if (block.FundRenewWaiv is not null) context.Validators.FundRenewWaiv(context, check, block.FundRenewWaiv);
	}

	static void ValidateDiscretionInstructions(Fix44Context context, FixCheck check, IDiscretionInstructions block)
	{
		if (block.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, block.DiscretionInst);
		if (block.DiscretionOffsetValue is not null) context.Validators.DiscretionOffsetValue(context, check, block.DiscretionOffsetValue);
		if (block.DiscretionMoveType is not null) context.Validators.DiscretionMoveType(context, check, block.DiscretionMoveType);
		if (block.DiscretionOffsetType is not null) context.Validators.DiscretionOffsetType(context, check, block.DiscretionOffsetType);
		if (block.DiscretionLimitType is not null) context.Validators.DiscretionLimitType(context, check, block.DiscretionLimitType);
		if (block.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, check, block.DiscretionRoundDirection);
		if (block.DiscretionScope is not null) context.Validators.DiscretionScope(context, check, block.DiscretionScope);
	}

	static void ValidateFinancingDetails(Fix44Context context, FixCheck check, IFinancingDetails block)
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

	static void ValidateInstrument(Fix44Context context, FixCheck check, IInstrument block)
	{
		if (block.Symbol is not null) context.Validators.Symbol(context, check, block.Symbol);
		if (block.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, block.SymbolSfx);
		if (block.SecurityID is not null) context.Validators.SecurityID(context, check, block.SecurityID);
		if (block.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, check, block.SecurityIDSource);
		if (block.NoSecurityAltID is not null) context.Validators.NoSecurityAltID(context, check, block.NoSecurityAltID);
		Counted(check, block.NoSecurityAltID, block.NoSecurityAltIDGroups);
		Each(context, check, FixTag.NoSecurityAltID, block.NoSecurityAltIDGroups, context.Validators.Instrument_NoSecurityAltID);
		if (block.Product is not null) context.Validators.Product(context, check, block.Product);
		if (block.CFICode is not null) context.Validators.CFICode(context, check, block.CFICode);
		if (block.SecurityType is not null) context.Validators.SecurityType(context, check, block.SecurityType);
		if (block.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, block.SecuritySubType);
		if (block.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, block.MaturityMonthYear);
		if (block.MaturityDate is not null) context.Validators.MaturityDate(context, check, block.MaturityDate);
		if (block.PutOrCall is not null) context.Validators.PutOrCall(context, check, block.PutOrCall);
		if (block.CouponPaymentDate is not null) context.Validators.CouponPaymentDate(context, check, block.CouponPaymentDate);
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
		if (block.OptAttribute is not null) context.Validators.OptAttribute(context, check, block.OptAttribute);
		if (block.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, block.ContractMultiplier);
		if (block.CouponRate is not null) context.Validators.CouponRate(context, check, block.CouponRate);
		if (block.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, block.SecurityExchange);
		if (block.Issuer is not null) context.Validators.Issuer(context, check, block.Issuer);
		if (block.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, block.EncodedIssuerLen);
		if (block.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, block.EncodedIssuer);
		if (block.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, block.SecurityDesc);
		if (block.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, block.EncodedSecurityDescLen);
		if (block.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, block.EncodedSecurityDesc);
		if (block.Pool is not null) context.Validators.Pool(context, check, block.Pool);
		if (block.ContractSettlMonth is not null) context.Validators.ContractSettlMonth(context, check, block.ContractSettlMonth);
		if (block.CPProgram is not null) context.Validators.CPProgram(context, check, block.CPProgram);
		if (block.CPRegType is not null) context.Validators.CPRegType(context, check, block.CPRegType);
		if (block.NoEvents is not null) context.Validators.NoEvents(context, check, block.NoEvents);
		Counted(check, block.NoEvents, block.NoEventsGroups);
		Each(context, check, FixTag.NoEvents, block.NoEventsGroups, context.Validators.Instrument_NoEvents);
		if (block.DatedDate is not null) context.Validators.DatedDate(context, check, block.DatedDate);
		if (block.InterestAccrualDate is not null) context.Validators.InterestAccrualDate(context, check, block.InterestAccrualDate);
	}

	static void ValidateInstrumentExtension(Fix44Context context, FixCheck check, IInstrumentExtension block)
	{
		if (block.DeliveryForm is not null) context.Validators.DeliveryForm(context, check, block.DeliveryForm);
		if (block.PctAtRisk is not null) context.Validators.PctAtRisk(context, check, block.PctAtRisk);
		if (block.NoInstrAttrib is not null) context.Validators.NoInstrAttrib(context, check, block.NoInstrAttrib);
		Counted(check, block.NoInstrAttrib, block.NoInstrAttribGroups);
		Each(context, check, FixTag.NoInstrAttrib, block.NoInstrAttribGroups, context.Validators.InstrumentExtension_NoInstrAttrib);
	}

	static void ValidateInstrumentLeg(Fix44Context context, FixCheck check, IInstrumentLeg block)
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
	}

	static void ValidateLegBenchmarkCurveData(Fix44Context context, FixCheck check, ILegBenchmarkCurveData block)
	{
		if (block.LegBenchmarkCurveCurrency is not null) context.Validators.LegBenchmarkCurveCurrency(context, check, block.LegBenchmarkCurveCurrency);
		if (block.LegBenchmarkCurveName is not null) context.Validators.LegBenchmarkCurveName(context, check, block.LegBenchmarkCurveName);
		if (block.LegBenchmarkCurvePoint is not null) context.Validators.LegBenchmarkCurvePoint(context, check, block.LegBenchmarkCurvePoint);
		if (block.LegBenchmarkPrice is not null) context.Validators.LegBenchmarkPrice(context, check, block.LegBenchmarkPrice);
		if (block.LegBenchmarkPriceType is not null) context.Validators.LegBenchmarkPriceType(context, check, block.LegBenchmarkPriceType);
	}

	static void ValidateLegStipulations(Fix44Context context, FixCheck check, ILegStipulations block)
	{
		if (block.NoLegStipulations is not null) context.Validators.NoLegStipulations(context, check, block.NoLegStipulations);
		Counted(check, block.NoLegStipulations, block.NoLegStipulationsGroups);
		Each(context, check, FixTag.NoLegStipulations, block.NoLegStipulationsGroups, context.Validators.LegStipulations_NoLegStipulations);
	}

	static void ValidateNestedParties(Fix44Context context, FixCheck check, INestedParties block)
	{
		if (block.NoNestedPartyIDs is not null) context.Validators.NoNestedPartyIDs(context, check, block.NoNestedPartyIDs);
		Counted(check, block.NoNestedPartyIDs, block.NoNestedPartyIDsGroups);
		Each(context, check, FixTag.NoNestedPartyIDs, block.NoNestedPartyIDsGroups, context.Validators.NestedParties_NoNestedPartyIDs);
	}

	static void ValidateNestedParties2(Fix44Context context, FixCheck check, INestedParties2 block)
	{
		if (block.NoNested2PartyIDs is not null) context.Validators.NoNested2PartyIDs(context, check, block.NoNested2PartyIDs);
		Counted(check, block.NoNested2PartyIDs, block.NoNested2PartyIDsGroups);
		Each(context, check, FixTag.NoNested2PartyIDs, block.NoNested2PartyIDsGroups, context.Validators.NestedParties2_NoNested2PartyIDs);
	}

	static void ValidateNestedParties3(Fix44Context context, FixCheck check, INestedParties3 block)
	{
		if (block.NoNested3PartyIDs is not null) context.Validators.NoNested3PartyIDs(context, check, block.NoNested3PartyIDs);
		Counted(check, block.NoNested3PartyIDs, block.NoNested3PartyIDsGroups);
		Each(context, check, FixTag.NoNested3PartyIDs, block.NoNested3PartyIDsGroups, context.Validators.NestedParties3_NoNested3PartyIDs);
	}

	static void ValidateOrderQtyData(Fix44Context context, FixCheck check, IOrderQtyData block)
	{
		if (block.OrderQty is not null) context.Validators.OrderQty(context, check, block.OrderQty);
		if (block.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, block.CashOrderQty);
		if (block.OrderPercent is not null) context.Validators.OrderPercent(context, check, block.OrderPercent);
		if (block.RoundingDirection is not null) context.Validators.RoundingDirection(context, check, block.RoundingDirection);
		if (block.RoundingModulus is not null) context.Validators.RoundingModulus(context, check, block.RoundingModulus);
	}

	static void ValidateParties(Fix44Context context, FixCheck check, IParties block)
	{
		if (block.NoPartyIDs is not null) context.Validators.NoPartyIDs(context, check, block.NoPartyIDs);
		Counted(check, block.NoPartyIDs, block.NoPartyIDsGroups);
		Each(context, check, FixTag.NoPartyIDs, block.NoPartyIDsGroups, context.Validators.Parties_NoPartyIDs);
	}

	static void ValidatePegInstructions(Fix44Context context, FixCheck check, IPegInstructions block)
	{
		if (block.PegOffsetValue is not null) context.Validators.PegOffsetValue(context, check, block.PegOffsetValue);
		if (block.PegMoveType is not null) context.Validators.PegMoveType(context, check, block.PegMoveType);
		if (block.PegOffsetType is not null) context.Validators.PegOffsetType(context, check, block.PegOffsetType);
		if (block.PegLimitType is not null) context.Validators.PegLimitType(context, check, block.PegLimitType);
		if (block.PegRoundDirection is not null) context.Validators.PegRoundDirection(context, check, block.PegRoundDirection);
		if (block.PegScope is not null) context.Validators.PegScope(context, check, block.PegScope);
	}

	static void ValidatePositionAmountData(Fix44Context context, FixCheck check, IPositionAmountData block)
	{
		if (block.NoPosAmt is not null) context.Validators.NoPosAmt(context, check, block.NoPosAmt);
		Counted(check, block.NoPosAmt, block.NoPosAmtGroups);
		Each(context, check, FixTag.NoPosAmt, block.NoPosAmtGroups, context.Validators.PositionAmountData_NoPosAmt);
	}

	static void ValidatePositionQty(Fix44Context context, FixCheck check, IPositionQty block)
	{
		if (block.NoPositions is not null) context.Validators.NoPositions(context, check, block.NoPositions);
		Counted(check, block.NoPositions, block.NoPositionsGroups);
		Each(context, check, FixTag.NoPositions, block.NoPositionsGroups, context.Validators.PositionQty_NoPositions);
	}

	static void ValidateSettlInstructionsData(Fix44Context context, FixCheck check, ISettlInstructionsData block)
	{
		if (block.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, check, block.SettlDeliveryType);
		if (block.StandInstDbType is not null) context.Validators.StandInstDbType(context, check, block.StandInstDbType);
		if (block.StandInstDbName is not null) context.Validators.StandInstDbName(context, check, block.StandInstDbName);
		if (block.StandInstDbID is not null) context.Validators.StandInstDbID(context, check, block.StandInstDbID);
		if (block.NoDlvyInst is not null) context.Validators.NoDlvyInst(context, check, block.NoDlvyInst);
		Counted(check, block.NoDlvyInst, block.NoDlvyInstGroups);
		Each(context, check, FixTag.NoDlvyInst, block.NoDlvyInstGroups, context.Validators.SettlInstructionsData_NoDlvyInst);
	}

	static void ValidateSettlParties(Fix44Context context, FixCheck check, ISettlParties block)
	{
		if (block.NoSettlPartyIDs is not null) context.Validators.NoSettlPartyIDs(context, check, block.NoSettlPartyIDs);
		Counted(check, block.NoSettlPartyIDs, block.NoSettlPartyIDsGroups);
		Each(context, check, FixTag.NoSettlPartyIDs, block.NoSettlPartyIDsGroups, context.Validators.SettlParties_NoSettlPartyIDs);
	}

	static void ValidateSpreadOrBenchmarkCurveData(Fix44Context context, FixCheck check, ISpreadOrBenchmarkCurveData block)
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

	static void ValidateStipulations(Fix44Context context, FixCheck check, IStipulations block)
	{
		if (block.NoStipulations is not null) context.Validators.NoStipulations(context, check, block.NoStipulations);
		Counted(check, block.NoStipulations, block.NoStipulationsGroups);
		Each(context, check, FixTag.NoStipulations, block.NoStipulationsGroups, context.Validators.Stipulations_NoStipulations);
	}

	static void ValidateTrdRegTimestamps(Fix44Context context, FixCheck check, ITrdRegTimestamps block)
	{
		if (block.NoTrdRegTimestamps is not null) context.Validators.NoTrdRegTimestamps(context, check, block.NoTrdRegTimestamps);
		Counted(check, block.NoTrdRegTimestamps, block.NoTrdRegTimestampsGroups);
		Each(context, check, FixTag.NoTrdRegTimestamps, block.NoTrdRegTimestampsGroups, context.Validators.TrdRegTimestamps_NoTrdRegTimestamps);
	}

	static void ValidateUnderlyingInstrument(Fix44Context context, FixCheck check, IUnderlyingInstrument block)
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
		if (block.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, block.UnderlyingPutOrCall);
		if (block.UnderlyingCouponPaymentDate is not null) context.Validators.UnderlyingCouponPaymentDate(context, check, block.UnderlyingCouponPaymentDate);
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
		if (block.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, check, block.UnderlyingCurrency);
		if (block.UnderlyingQty is not null) context.Validators.UnderlyingQty(context, check, block.UnderlyingQty);
		if (block.UnderlyingPx is not null) context.Validators.UnderlyingPx(context, check, block.UnderlyingPx);
		if (block.UnderlyingDirtyPrice is not null) context.Validators.UnderlyingDirtyPrice(context, check, block.UnderlyingDirtyPrice);
		if (block.UnderlyingEndPrice is not null) context.Validators.UnderlyingEndPrice(context, check, block.UnderlyingEndPrice);
		if (block.UnderlyingStartValue is not null) context.Validators.UnderlyingStartValue(context, check, block.UnderlyingStartValue);
		if (block.UnderlyingCurrentValue is not null) context.Validators.UnderlyingCurrentValue(context, check, block.UnderlyingCurrentValue);
		if (block.UnderlyingEndValue is not null) context.Validators.UnderlyingEndValue(context, check, block.UnderlyingEndValue);
		if (!Empty((IUnderlyingStipulations)block)) context.Validators.UnderlyingStipulations(context, check, block);
	}

	static void ValidateUnderlyingStipulations(Fix44Context context, FixCheck check, IUnderlyingStipulations block)
	{
		if (block.NoUnderlyingStips is not null) context.Validators.NoUnderlyingStips(context, check, block.NoUnderlyingStips);
		Counted(check, block.NoUnderlyingStips, block.NoUnderlyingStipsGroups);
		Each(context, check, FixTag.NoUnderlyingStips, block.NoUnderlyingStipsGroups, context.Validators.UnderlyingStipulations_NoUnderlyingStips);
	}

	static void ValidateYieldData(Fix44Context context, FixCheck check, IYieldData block)
	{
		if (block.YieldType is not null) context.Validators.YieldType(context, check, block.YieldType);
		if (block.Yield is not null) context.Validators.Yield(context, check, block.Yield);
		if (block.YieldCalcDate is not null) context.Validators.YieldCalcDate(context, check, block.YieldCalcDate);
		if (block.YieldRedemptionDate is not null) context.Validators.YieldRedemptionDate(context, check, block.YieldRedemptionDate);
		if (block.YieldRedemptionPrice is not null) context.Validators.YieldRedemptionPrice(context, check, block.YieldRedemptionPrice);
		if (block.YieldRedemptionPriceType is not null) context.Validators.YieldRedemptionPriceType(context, check, block.YieldRedemptionPriceType);
	}

	static void ValidateInstrument_NoSecurityAltID(Fix44Context context, FixCheck check, IInstrument.NoSecurityAltIDGroup entry)
	{
		context.Validators.SecurityAltID(context, check, entry.SecurityAltID);
		if (entry.SecurityAltIDSource is not null) context.Validators.SecurityAltIDSource(context, check, entry.SecurityAltIDSource);
	}

	static void ValidateInstrument_NoEvents(Fix44Context context, FixCheck check, IInstrument.NoEventsGroup entry)
	{
		context.Validators.EventType(context, check, entry.EventType);
		if (entry.EventDate is not null) context.Validators.EventDate(context, check, entry.EventDate);
		if (entry.EventPx is not null) context.Validators.EventPx(context, check, entry.EventPx);
		if (entry.EventText is not null) context.Validators.EventText(context, check, entry.EventText);
	}

	static void ValidateInstrumentExtension_NoInstrAttrib(Fix44Context context, FixCheck check, IInstrumentExtension.NoInstrAttribGroup entry)
	{
		context.Validators.InstrAttribType(context, check, entry.InstrAttribType);
		if (entry.InstrAttribValue is not null) context.Validators.InstrAttribValue(context, check, entry.InstrAttribValue);
	}

	static void ValidateInstrumentLeg_NoLegSecurityAltID(Fix44Context context, FixCheck check, IInstrumentLeg.NoLegSecurityAltIDGroup entry)
	{
		context.Validators.LegSecurityAltID(context, check, entry.LegSecurityAltID);
		if (entry.LegSecurityAltIDSource is not null) context.Validators.LegSecurityAltIDSource(context, check, entry.LegSecurityAltIDSource);
	}

	static void ValidateLegStipulations_NoLegStipulations(Fix44Context context, FixCheck check, ILegStipulations.NoLegStipulationsGroup entry)
	{
		context.Validators.LegStipulationType(context, check, entry.LegStipulationType);
		if (entry.LegStipulationValue is not null) context.Validators.LegStipulationValue(context, check, entry.LegStipulationValue);
	}

	static void ValidateNestedParties_NoNestedPartyIDs(Fix44Context context, FixCheck check, INestedParties.NoNestedPartyIDsGroup entry)
	{
		context.Validators.NestedPartyID(context, check, entry.NestedPartyID);
		if (entry.NestedPartyIDSource is not null) context.Validators.NestedPartyIDSource(context, check, entry.NestedPartyIDSource);
		if (entry.NestedPartyRole is not null) context.Validators.NestedPartyRole(context, check, entry.NestedPartyRole);
		if (entry.NoNestedPartySubIDs is not null) context.Validators.NoNestedPartySubIDs(context, check, entry.NoNestedPartySubIDs);
		Counted(check, entry.NoNestedPartySubIDs, entry.NoNestedPartySubIDsGroups);
		Each(context, check, FixTag.NoNestedPartySubIDs, entry.NoNestedPartySubIDsGroups, context.Validators.NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs);
	}

	static void ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs(Fix44Context context, FixCheck check, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup entry)
	{
		context.Validators.NestedPartySubID(context, check, entry.NestedPartySubID);
		if (entry.NestedPartySubIDType is not null) context.Validators.NestedPartySubIDType(context, check, entry.NestedPartySubIDType);
	}

	static void ValidateNestedParties2_NoNested2PartyIDs(Fix44Context context, FixCheck check, INestedParties2.NoNested2PartyIDsGroup entry)
	{
		context.Validators.Nested2PartyID(context, check, entry.Nested2PartyID);
		if (entry.Nested2PartyIDSource is not null) context.Validators.Nested2PartyIDSource(context, check, entry.Nested2PartyIDSource);
		if (entry.Nested2PartyRole is not null) context.Validators.Nested2PartyRole(context, check, entry.Nested2PartyRole);
		if (entry.NoNested2PartySubIDs is not null) context.Validators.NoNested2PartySubIDs(context, check, entry.NoNested2PartySubIDs);
		Counted(check, entry.NoNested2PartySubIDs, entry.NoNested2PartySubIDsGroups);
		Each(context, check, FixTag.NoNested2PartySubIDs, entry.NoNested2PartySubIDsGroups, context.Validators.NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs);
	}

	static void ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs(Fix44Context context, FixCheck check, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup entry)
	{
		context.Validators.Nested2PartySubID(context, check, entry.Nested2PartySubID);
		if (entry.Nested2PartySubIDType is not null) context.Validators.Nested2PartySubIDType(context, check, entry.Nested2PartySubIDType);
	}

	static void ValidateNestedParties3_NoNested3PartyIDs(Fix44Context context, FixCheck check, INestedParties3.NoNested3PartyIDsGroup entry)
	{
		context.Validators.Nested3PartyID(context, check, entry.Nested3PartyID);
		if (entry.Nested3PartyIDSource is not null) context.Validators.Nested3PartyIDSource(context, check, entry.Nested3PartyIDSource);
		if (entry.Nested3PartyRole is not null) context.Validators.Nested3PartyRole(context, check, entry.Nested3PartyRole);
		if (entry.NoNested3PartySubIDs is not null) context.Validators.NoNested3PartySubIDs(context, check, entry.NoNested3PartySubIDs);
		Counted(check, entry.NoNested3PartySubIDs, entry.NoNested3PartySubIDsGroups);
		Each(context, check, FixTag.NoNested3PartySubIDs, entry.NoNested3PartySubIDsGroups, context.Validators.NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs);
	}

	static void ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs(Fix44Context context, FixCheck check, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup entry)
	{
		context.Validators.Nested3PartySubID(context, check, entry.Nested3PartySubID);
		if (entry.Nested3PartySubIDType is not null) context.Validators.Nested3PartySubIDType(context, check, entry.Nested3PartySubIDType);
	}

	static void ValidateParties_NoPartyIDs(Fix44Context context, FixCheck check, IParties.NoPartyIDsGroup entry)
	{
		context.Validators.PartyID(context, check, entry.PartyID);
		if (entry.PartyIDSource is not null) context.Validators.PartyIDSource(context, check, entry.PartyIDSource);
		if (entry.PartyRole is not null) context.Validators.PartyRole(context, check, entry.PartyRole);
		if (entry.NoPartySubIDs is not null) context.Validators.NoPartySubIDs(context, check, entry.NoPartySubIDs);
		Counted(check, entry.NoPartySubIDs, entry.NoPartySubIDsGroups);
		Each(context, check, FixTag.NoPartySubIDs, entry.NoPartySubIDsGroups, context.Validators.Parties_NoPartyIDs_NoPartySubIDs);
	}

	static void ValidateParties_NoPartyIDs_NoPartySubIDs(Fix44Context context, FixCheck check, IParties.NoPartyIDsGroup.NoPartySubIDsGroup entry)
	{
		context.Validators.PartySubID(context, check, entry.PartySubID);
		if (entry.PartySubIDType is not null) context.Validators.PartySubIDType(context, check, entry.PartySubIDType);
	}

	static void ValidatePositionAmountData_NoPosAmt(Fix44Context context, FixCheck check, IPositionAmountData.NoPosAmtGroup entry)
	{
		context.Validators.PosAmtType(context, check, entry.PosAmtType);
		if (entry.PosAmt is not null) context.Validators.PosAmt(context, check, entry.PosAmt);
	}

	static void ValidatePositionQty_NoPositions(Fix44Context context, FixCheck check, IPositionQty.NoPositionsGroup entry)
	{
		context.Validators.PosType(context, check, entry.PosType);
		if (entry.LongQty is not null) context.Validators.LongQty(context, check, entry.LongQty);
		if (entry.ShortQty is not null) context.Validators.ShortQty(context, check, entry.ShortQty);
		if (entry.PosQtyStatus is not null) context.Validators.PosQtyStatus(context, check, entry.PosQtyStatus);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateSettlInstructionsData_NoDlvyInst(Fix44Context context, FixCheck check, ISettlInstructionsData.NoDlvyInstGroup entry)
	{
		context.Validators.SettlInstSource(context, check, entry.SettlInstSource);
		if (entry.DlvyInstType is not null) context.Validators.DlvyInstType(context, check, entry.DlvyInstType);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, check, entry);
	}

	static void ValidateSettlParties_NoSettlPartyIDs(Fix44Context context, FixCheck check, ISettlParties.NoSettlPartyIDsGroup entry)
	{
		context.Validators.SettlPartyID(context, check, entry.SettlPartyID);
		if (entry.SettlPartyIDSource is not null) context.Validators.SettlPartyIDSource(context, check, entry.SettlPartyIDSource);
		if (entry.SettlPartyRole is not null) context.Validators.SettlPartyRole(context, check, entry.SettlPartyRole);
		if (entry.NoSettlPartySubIDs is not null) context.Validators.NoSettlPartySubIDs(context, check, entry.NoSettlPartySubIDs);
		Counted(check, entry.NoSettlPartySubIDs, entry.NoSettlPartySubIDsGroups);
		Each(context, check, FixTag.NoSettlPartySubIDs, entry.NoSettlPartySubIDsGroups, context.Validators.SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs);
	}

	static void ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs(Fix44Context context, FixCheck check, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup entry)
	{
		context.Validators.SettlPartySubID(context, check, entry.SettlPartySubID);
		if (entry.SettlPartySubIDType is not null) context.Validators.SettlPartySubIDType(context, check, entry.SettlPartySubIDType);
	}

	static void ValidateStipulations_NoStipulations(Fix44Context context, FixCheck check, IStipulations.NoStipulationsGroup entry)
	{
		context.Validators.StipulationType(context, check, entry.StipulationType);
		if (entry.StipulationValue is not null) context.Validators.StipulationValue(context, check, entry.StipulationValue);
	}

	static void ValidateTrdRegTimestamps_NoTrdRegTimestamps(Fix44Context context, FixCheck check, ITrdRegTimestamps.NoTrdRegTimestampsGroup entry)
	{
		context.Validators.TrdRegTimestamp(context, check, entry.TrdRegTimestamp);
		if (entry.TrdRegTimestampType is not null) context.Validators.TrdRegTimestampType(context, check, entry.TrdRegTimestampType);
		if (entry.TrdRegTimestampOrigin is not null) context.Validators.TrdRegTimestampOrigin(context, check, entry.TrdRegTimestampOrigin);
	}

	static void ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID(Fix44Context context, FixCheck check, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup entry)
	{
		context.Validators.UnderlyingSecurityAltID(context, check, entry.UnderlyingSecurityAltID);
		if (entry.UnderlyingSecurityAltIDSource is not null) context.Validators.UnderlyingSecurityAltIDSource(context, check, entry.UnderlyingSecurityAltIDSource);
	}

	static void ValidateUnderlyingStipulations_NoUnderlyingStips(Fix44Context context, FixCheck check, IUnderlyingStipulations.NoUnderlyingStipsGroup entry)
	{
		context.Validators.UnderlyingStipType(context, check, entry.UnderlyingStipType);
		if (entry.UnderlyingStipValue is not null) context.Validators.UnderlyingStipValue(context, check, entry.UnderlyingStipValue);
	}

	static void ValidateAdvertisement_NoLegs(Fix44Context context, FixCheck check, FixMessage.Advertisement.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAdvertisement_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.Advertisement.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoOrders(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoOrdersGroup entry)
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

	static void ValidateAllocationInstruction_NoExecs(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoExecsGroup entry)
	{
		context.Validators.LastQty(context, check, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, check, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
	}

	static void ValidateAllocationInstruction_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoLegs(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoAllocs(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, check, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
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
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, check, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
	}

	static void ValidateAllocationInstruction_NoAllocs_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateAllocationInstruction_NoAllocs_NoClearingInstructions(Fix44Context context, FixCheck check, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateAllocationInstructionAck_NoAllocs(Fix44Context context, FixCheck check, FixMessage.AllocationInstructionAck.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, check, entry.IndividualAllocRejCode);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
	}

	static void ValidateAllocationReport_NoOrders(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoOrdersGroup entry)
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

	static void ValidateAllocationReport_NoExecs(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoExecsGroup entry)
	{
		context.Validators.LastQty(context, check, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, check, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, check, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
	}

	static void ValidateAllocationReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateAllocationReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAllocationReport_NoAllocs(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, check, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
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
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, check, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, check, entry);
	}

	static void ValidateAllocationReport_NoAllocs_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateAllocationReport_NoAllocs_NoClearingInstructions(Fix44Context context, FixCheck check, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateAllocationReportAck_NoAllocs(Fix44Context context, FixCheck check, FixMessage.AllocationReportAck.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, check, entry.IndividualAllocRejCode);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
	}

	static void ValidateAssignmentReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.AssignmentReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateAssignmentReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.AssignmentReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateBidRequest_NoBidDescriptors(Fix44Context context, FixCheck check, FixMessage.BidRequest.NoBidDescriptorsGroup entry)
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

	static void ValidateBidRequest_NoBidComponents(Fix44Context context, FixCheck check, FixMessage.BidRequest.NoBidComponentsGroup entry)
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

	static void ValidateBidResponse_NoBidComponents(Fix44Context context, FixCheck check, FixMessage.BidResponse.NoBidComponentsGroup entry)
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

	static void ValidateCollateralAssignment_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralAssignment_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralAssignment_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralAssignment_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralAssignment_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.CollateralAssignment.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralInquiry_NoCollInquiryQualifier(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup entry)
	{
		context.Validators.CollInquiryQualifier(context, check, entry.CollInquiryQualifier);
	}

	static void ValidateCollateralInquiry_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralInquiry_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralInquiry_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralInquiry_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralInquiry.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralInquiryAck_NoCollInquiryQualifier(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup entry)
	{
		context.Validators.CollInquiryQualifier(context, check, entry.CollInquiryQualifier);
	}

	static void ValidateCollateralInquiryAck_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralInquiryAck_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralInquiryAck_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralInquiryAck_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralReport_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralReport.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralReport_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralReport.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCollateralReport_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.CollateralReport.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralRequest_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralRequest.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralRequest_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralRequest.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralRequest_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.CollateralRequest.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateCollateralResponse_NoExecs(Fix44Context context, FixCheck check, FixMessage.CollateralResponse.NoExecsGroup entry)
	{
		context.Validators.ExecID(context, check, entry.ExecID);
	}

	static void ValidateCollateralResponse_NoTrades(Fix44Context context, FixCheck check, FixMessage.CollateralResponse.NoTradesGroup entry)
	{
		context.Validators.TradeReportID(context, check, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, check, entry.SecondaryTradeReportID);
	}

	static void ValidateCollateralResponse_NoLegs(Fix44Context context, FixCheck check, FixMessage.CollateralResponse.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCollateralResponse_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CollateralResponse.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, check, entry.CollAction);
	}

	static void ValidateCollateralResponse_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.CollateralResponse.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateConfirmation_NoOrders(Fix44Context context, FixCheck check, FixMessage.Confirmation.NoOrdersGroup entry)
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

	static void ValidateConfirmation_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.Confirmation.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateConfirmation_NoLegs(Fix44Context context, FixCheck check, FixMessage.Confirmation.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateConfirmation_NoCapacities(Fix44Context context, FixCheck check, FixMessage.Confirmation.NoCapacitiesGroup entry)
	{
		context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.OrderCapacityQty is null) Missing(check, FixTag.OrderCapacityQty, entry.OrderCapacity.Position);
		else context.Validators.OrderCapacityQty(context, check, entry.OrderCapacityQty);
	}

	static void ValidateConfirmation_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.Confirmation.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateConfirmationRequest_NoOrders(Fix44Context context, FixCheck check, FixMessage.ConfirmationRequest.NoOrdersGroup entry)
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

	static void ValidateCrossOrderCancelReplaceRequest_NoSides(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
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
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateCrossOrderCancelReplaceRequest_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateCrossOrderCancelRequest_NoSides(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID, entry.Side.Position);
		else context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
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

	static void ValidateCrossOrderCancelRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateCrossOrderCancelRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.CrossOrderCancelRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateDerivativeSecurityList_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, entry.ExpirationCycle);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.DerivativeSecurityList_NoRelatedSym_NoLegs);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateDerivativeSecurityList_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateDontKnowTrade_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.DontKnowTrade.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateDontKnowTrade_NoLegs(Fix44Context context, FixCheck check, FixMessage.DontKnowTrade.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateEmail_NoRoutingIDs(Fix44Context context, FixCheck check, FixMessage.Email.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateEmail_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.Email.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
	}

	static void ValidateEmail_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.Email.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateEmail_NoLegs(Fix44Context context, FixCheck check, FixMessage.Email.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateEmail_LinesOfText(Fix44Context context, FixCheck check, FixMessage.Email.LinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateExecutionReport_NoContraBrokers(Fix44Context context, FixCheck check, FixMessage.ExecutionReport.NoContraBrokersGroup entry)
	{
		context.Validators.ContraBroker(context, check, entry.ContraBroker);
		if (entry.ContraTrader is not null) context.Validators.ContraTrader(context, check, entry.ContraTrader);
		if (entry.ContraTradeQty is not null) context.Validators.ContraTradeQty(context, check, entry.ContraTradeQty);
		if (entry.ContraTradeTime is not null) context.Validators.ContraTradeTime(context, check, entry.ContraTradeTime);
		if (entry.ContraLegRefID is not null) context.Validators.ContraLegRefID(context, check, entry.ContraLegRefID);
	}

	static void ValidateExecutionReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.ExecutionReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateExecutionReport_NoContAmts(Fix44Context context, FixCheck check, FixMessage.ExecutionReport.NoContAmtsGroup entry)
	{
		context.Validators.ContAmtType(context, check, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, check, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, check, entry.ContAmtCurr);
	}

	static void ValidateExecutionReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.ExecutionReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, check, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
	}

	static void ValidateExecutionReport_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.ExecutionReport.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateIndicationOfInterest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.IndicationOfInterest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateIndicationOfInterest_NoLegs(Fix44Context context, FixCheck check, FixMessage.IndicationOfInterest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegIOIQty is not null) context.Validators.LegIOIQty(context, check, entry.LegIOIQty);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
	}

	static void ValidateIndicationOfInterest_NoIOIQualifiers(Fix44Context context, FixCheck check, FixMessage.IndicationOfInterest.NoIOIQualifiersGroup entry)
	{
		context.Validators.IOIQualifier(context, check, entry.IOIQualifier);
	}

	static void ValidateIndicationOfInterest_NoRoutingIDs(Fix44Context context, FixCheck check, FixMessage.IndicationOfInterest.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateListStatus_NoOrders(Fix44Context context, FixCheck check, FixMessage.ListStatus.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
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

	static void ValidateListStrikePrice_NoStrikes(Fix44Context context, FixCheck check, FixMessage.ListStrikePrice.NoStrikesGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
	}

	static void ValidateListStrikePrice_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.ListStrikePrice.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Price is null) Missing(check, FixTag.Price, entry.UnderlyingSymbol.Position);
		else context.Validators.Price(context, check, entry.Price);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateLogon_NoMsgTypes(Fix44Context context, FixCheck check, FixMessage.Logon.NoMsgTypesGroup entry)
	{
		context.Validators.RefMsgType(context, check, entry.RefMsgType);
		if (entry.MsgDirection is not null) context.Validators.MsgDirection(context, check, entry.MsgDirection);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries(Fix44Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDUpdateAction(context, check, entry.MDUpdateAction);
		if (entry.DeleteReason is not null) context.Validators.DeleteReason(context, check, entry.DeleteReason);
		if (entry.MDEntryType is not null) context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, check, entry.MDEntryID);
		if (entry.MDEntryRefID is not null) context.Validators.MDEntryRefID(context, check, entry.MDEntryRefID);
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
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, check, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, check, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, check, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, check, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
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
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
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
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs(Fix44Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataRequest_NoMDEntryTypes(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest.NoMDEntryTypesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
	}

	static void ValidateMarketDataRequest_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup entry)
	{
		if (Empty((IInstrument)entry)) Absent(check, FixTag.Symbol, entry.Symbol.Position);
		else context.Validators.Instrument(context, check, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, check, entry.NoUnderlyings);
		Counted(check, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		Each(context, check, FixTag.NoUnderlyings, entry.NoUnderlyingsGroups, context.Validators.MarketDataRequest_NoRelatedSym_NoUnderlyings);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, check, entry.NoLegs);
		Counted(check, entry.NoLegs, entry.NoLegsGroups);
		Each(context, check, FixTag.NoLegs, entry.NoLegsGroups, context.Validators.MarketDataRequest_NoRelatedSym_NoLegs);
	}

	static void ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataRequest_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataRequest_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.MarketDataRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateMarketDataRequestReject_NoAltMDSource(Fix44Context context, FixCheck check, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup entry)
	{
		context.Validators.AltMDSourceID(context, check, entry.AltMDSourceID);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoLegs(Fix44Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoMDEntries(Fix44Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryPx is not null) context.Validators.MDEntryPx(context, check, entry.MDEntryPx);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, check, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, check, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, check, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, check, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
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
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (entry.MDEntryBuyer is not null) context.Validators.MDEntryBuyer(context, check, entry.MDEntryBuyer);
		if (entry.MDEntrySeller is not null) context.Validators.MDEntrySeller(context, check, entry.MDEntrySeller);
		if (entry.NumberOfOrders is not null) context.Validators.NumberOfOrders(context, check, entry.NumberOfOrders);
		if (entry.MDEntryPositionNo is not null) context.Validators.MDEntryPositionNo(context, check, entry.MDEntryPositionNo);
		if (entry.Scope is not null) context.Validators.Scope(context, check, entry.Scope);
		if (entry.PriceDelta is not null) context.Validators.PriceDelta(context, check, entry.PriceDelta);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateMassQuote_NoQuoteSets(Fix44Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup entry)
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

	static void ValidateMassQuote_NoQuoteSets_NoQuoteEntries(Fix44Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
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
	}

	static void ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs(Fix44Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets(Fix44Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup entry)
	{
		context.Validators.QuoteSetID(context, check, entry.QuoteSetID);
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.TotNoQuoteEntries is not null) context.Validators.TotNoQuoteEntries(context, check, entry.TotNoQuoteEntries);
		if (entry.LastFragment is not null) context.Validators.LastFragment(context, check, entry.LastFragment);
		if (entry.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, check, entry.NoQuoteEntries);
		Counted(check, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, entry.NoQuoteEntriesGroups, context.Validators.MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(Fix44Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
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
		if (entry.QuoteEntryRejectReason is not null) context.Validators.QuoteEntryRejectReason(context, check, entry.QuoteEntryRejectReason);
	}

	static void ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs(Fix44Context context, FixCheck check, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateMultilegOrderCancelReplaceRequest_NoAllocs(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateMultilegOrderCancelReplaceRequest_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateMultilegOrderCancelReplaceRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateMultilegOrderCancelReplaceRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, check, entry.NoLegAllocs);
		Counted(check, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		Each(context, check, FixTag.NoLegAllocs, entry.NoLegAllocsGroups, context.Validators.MultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, check, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
	}

	static void ValidateMultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs(Fix44Context context, FixCheck check, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup entry)
	{
		context.Validators.LegAllocAccount(context, check, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, check, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, check, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, check, entry.LegAllocAcctIDSource);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
	}

	static void ValidateNetworkStatusRequest_NoCompIDs(Fix44Context context, FixCheck check, FixMessage.NetworkStatusRequest.NoCompIDsGroup entry)
	{
		context.Validators.RefCompID(context, check, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, check, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
	}

	static void ValidateNetworkStatusResponse_NoCompIDs(Fix44Context context, FixCheck check, FixMessage.NetworkStatusResponse.NoCompIDsGroup entry)
	{
		context.Validators.RefCompID(context, check, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, check, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.StatusValue is not null) context.Validators.StatusValue(context, check, entry.StatusValue);
		if (entry.StatusText is not null) context.Validators.StatusText(context, check, entry.StatusText);
	}

	static void ValidateNewOrderCross_NoSides(Fix44Context context, FixCheck check, FixMessage.NewOrderCross.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
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
	}

	static void ValidateNewOrderCross_NoSides_NoAllocs(Fix44Context context, FixCheck check, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderCross_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.NewOrderCross.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderCross_NoLegs(Fix44Context context, FixCheck check, FixMessage.NewOrderCross.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateNewOrderCross_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.NewOrderCross.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderList_NoOrders(Fix44Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup entry)
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
		if (entry.MaxFloor is not null) context.Validators.MaxFloor(context, check, entry.MaxFloor);
		if (entry.ExDestination is not null) context.Validators.ExDestination(context, check, entry.ExDestination);
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
		if (entry.StopPx is not null) context.Validators.StopPx(context, check, entry.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, check, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, check, entry);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.IOIid is not null) context.Validators.IOIid(context, check, entry.IOIid);
		if (entry.QuoteID is not null) context.Validators.QuoteID(context, check, entry.QuoteID);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, check, entry.TimeInForce);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, entry.EffectiveTime);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, check, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, entry.GTBookingInst);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
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
		if (entry.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, check, entry.TargetStrategyParameters);
		if (entry.ParticipationRate is not null) context.Validators.ParticipationRate(context, check, entry.ParticipationRate);
		if (entry.Designation is not null) context.Validators.Designation(context, check, entry.Designation);
	}

	static void ValidateNewOrderList_NoOrders_NoAllocs(Fix44Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderList_NoOrders_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderList_NoOrders_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderMultileg_NoAllocs(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderMultileg_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderMultileg_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNewOrderMultileg_NoLegs(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, check, entry.NoLegAllocs);
		Counted(check, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		Each(context, check, FixTag.NoLegAllocs, entry.NoLegAllocsGroups, context.Validators.NewOrderMultileg_NoLegs_NoLegAllocs);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, check, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
	}

	static void ValidateNewOrderMultileg_NoLegs_NoLegAllocs(Fix44Context context, FixCheck check, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup entry)
	{
		context.Validators.LegAllocAccount(context, check, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, check, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, check, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, check, entry.LegAllocAcctIDSource);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, check, entry.LegSettlCurrency);
	}

	static void ValidateNewOrderSingle_NoAllocs(Fix44Context context, FixCheck check, FixMessage.NewOrderSingle.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateNewOrderSingle_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.NewOrderSingle.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateNewOrderSingle_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.NewOrderSingle.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNews_NoRoutingIDs(Fix44Context context, FixCheck check, FixMessage.News.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateNews_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.News.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
	}

	static void ValidateNews_NoLegs(Fix44Context context, FixCheck check, FixMessage.News.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateNews_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.News.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateNews_LinesOfText(Fix44Context context, FixCheck check, FixMessage.News.LinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateOrderCancelReplaceRequest_NoAllocs(Fix44Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateOrderCancelReplaceRequest_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateOrderCancelReplaceRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateOrderCancelRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.OrderCancelRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateOrderMassCancelReport_NoAffectedOrders(Fix44Context context, FixCheck check, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup entry)
	{
		context.Validators.OrigClOrdID(context, check, entry.OrigClOrdID);
		if (entry.AffectedOrderID is not null) context.Validators.AffectedOrderID(context, check, entry.AffectedOrderID);
		if (entry.AffectedSecondaryOrderID is not null) context.Validators.AffectedSecondaryOrderID(context, check, entry.AffectedSecondaryOrderID);
	}

	static void ValidateOrderStatusRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.OrderStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceReport_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidatePositionMaintenanceRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionMaintenanceRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidatePositionMaintenanceRequest_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidatePositionReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.PositionReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidatePositionReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.PositionReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
		if (entry.UnderlyingSettlPrice is null) Missing(check, FixTag.UnderlyingSettlPrice, entry.UnderlyingSymbol.Position);
		else context.Validators.UnderlyingSettlPrice(context, check, entry.UnderlyingSettlPrice);
		if (entry.UnderlyingSettlPriceType is null) Missing(check, FixTag.UnderlyingSettlPriceType, entry.UnderlyingSymbol.Position);
		else context.Validators.UnderlyingSettlPriceType(context, check, entry.UnderlyingSettlPriceType);
	}

	static void ValidateQuote_NoQuoteQualifiers(Fix44Context context, FixCheck check, FixMessage.Quote.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuote_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.Quote.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuote_NoLegs(Fix44Context context, FixCheck check, FixMessage.Quote.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, check, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, check, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, check, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateQuoteCancel_NoQuoteEntries(Fix44Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup entry)
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

	static void ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteCancel_NoQuoteEntries_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup entry)
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

	static void ValidateQuoteRequest_NoRelatedSym_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers(Fix44Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup entry)
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

	static void ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers(Fix44Context context, FixCheck check, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteResponse_NoQuoteQualifiers(Fix44Context context, FixCheck check, FixMessage.QuoteResponse.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteResponse_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteResponse.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteResponse_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteResponse.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, check, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, check, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, check, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateQuoteStatusReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteStatusReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteStatusReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteStatusReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
	}

	static void ValidateQuoteStatusReport_NoQuoteQualifiers(Fix44Context context, FixCheck check, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup entry)
	{
		context.Validators.QuoteQualifier(context, check, entry.QuoteQualifier);
	}

	static void ValidateQuoteStatusRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateQuoteStatusRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.QuoteStatusRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRFQRequest_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup entry)
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

	static void ValidateRFQRequest_NoRelatedSym_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateRFQRequest_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRegistrationInstructions_NoRegistDtls(Fix44Context context, FixCheck check, FixMessage.RegistrationInstructions.NoRegistDtlsGroup entry)
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

	static void ValidateRegistrationInstructions_NoDistribInsts(Fix44Context context, FixCheck check, FixMessage.RegistrationInstructions.NoDistribInstsGroup entry)
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

	static void ValidateRequestForPositions_NoLegs(Fix44Context context, FixCheck check, FixMessage.RequestForPositions.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRequestForPositions_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.RequestForPositions.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateRequestForPositions_NoTradingSessions(Fix44Context context, FixCheck check, FixMessage.RequestForPositions.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
	}

	static void ValidateRequestForPositionsAck_NoLegs(Fix44Context context, FixCheck check, FixMessage.RequestForPositionsAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateRequestForPositionsAck_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinition_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityDefinition.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinition_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityDefinition.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityDefinitionRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityDefinitionRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityDefinitionRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityList_NoRelatedSym(Fix44Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup entry)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, check, entry);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, check, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, check, entry);
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
		if (entry.RoundLot is not null) context.Validators.RoundLot(context, check, entry.RoundLot);
		if (entry.MinTradeVol is not null) context.Validators.MinTradeVol(context, check, entry.MinTradeVol);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, check, entry.ExpirationCycle);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateSecurityList_NoRelatedSym_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityList_NoRelatedSym_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, check, entry);
	}

	static void ValidateSecurityListRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityListRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityListRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityListRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityStatus_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityStatus.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityStatus_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityStatus.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityStatusRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateSecurityStatusRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.SecurityStatusRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateSecurityTypes_NoSecurityTypes(Fix44Context context, FixCheck check, FixMessage.SecurityTypes.NoSecurityTypesGroup entry)
	{
		context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.SecuritySubType is not null) context.Validators.SecuritySubType(context, check, entry.SecuritySubType);
		if (entry.Product is not null) context.Validators.Product(context, check, entry.Product);
		if (entry.CFICode is not null) context.Validators.CFICode(context, check, entry.CFICode);
	}

	static void ValidateSettlementInstructions_NoSettlInst(Fix44Context context, FixCheck check, FixMessage.SettlementInstructions.NoSettlInstGroup entry)
	{
		context.Validators.SettlInstID(context, check, entry.SettlInstID);
		if (entry.SettlInstTransType is not null) context.Validators.SettlInstTransType(context, check, entry.SettlInstTransType);
		if (entry.SettlInstRefID is not null) context.Validators.SettlInstRefID(context, check, entry.SettlInstRefID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, check, entry);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Product is not null) context.Validators.Product(context, check, entry.Product);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.CFICode is not null) context.Validators.CFICode(context, check, entry.CFICode);
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

	static void ValidateTradeCaptureReport_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReport_NoLegs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, check, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
	}

	static void ValidateTradeCaptureReport_NoSides(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup entry)
	{
		context.Validators.Side(context, check, entry.Side);
		if (entry.OrderID is null) Missing(check, FixTag.OrderID, entry.Side.Position);
		else context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, check, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
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
		if (entry.OrderInputDevice is not null) context.Validators.OrderInputDevice(context, check, entry.OrderInputDevice);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, check, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, entry.SolicitedFlag);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, check, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, check, entry.OrderRestrictions);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, check, entry.CustOrderCapacity);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.TransBkdTime is not null) context.Validators.TransBkdTime(context, check, entry.TransBkdTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, check, entry.TradingSessionSubID);
		if (entry.TimeBracket is not null) context.Validators.TimeBracket(context, check, entry.TimeBracket);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, check, entry);
		if (entry.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, entry.GrossTradeAmt);
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
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
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
	}

	static void ValidateTradeCaptureReport_NoSides_NoClearingInstructions(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup entry)
	{
		context.Validators.ClearingInstruction(context, check, entry.ClearingInstruction);
	}

	static void ValidateTradeCaptureReport_NoSides_NoContAmts(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup entry)
	{
		context.Validators.ContAmtType(context, check, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, check, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, check, entry.ContAmtCurr);
	}

	static void ValidateTradeCaptureReport_NoSides_NoMiscFees(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, check, entry.MiscFeeBasis);
	}

	static void ValidateTradeCaptureReport_NoSides_NoAllocs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateTradeCaptureReportAck_NoLegs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, check, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, check, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, check, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, check, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, check, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, check, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, check, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, check, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, check, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, check, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, check, entry.LegLastPx);
	}

	static void ValidateTradeCaptureReportAck_NoAllocs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportAck.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, check, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, check, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, check, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, check, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, check, entry.AllocQty);
	}

	static void ValidateTradeCaptureReportRequest_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequest_NoLegs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequest_NoDates(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequest.NoDatesGroup entry)
	{
		context.Validators.TradeDate(context, check, entry.TradeDate);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
	}

	static void ValidateTradeCaptureReportRequestAck_NoUnderlyings(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup entry)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, check, entry);
	}

	static void ValidateTradeCaptureReportRequestAck_NoLegs(Fix44Context context, FixCheck check, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup entry)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, check, entry);
	}

	/// <summary>Whether a carrier of the FIX 4.4 CommissionData has none of its fields.</summary>
	internal static bool Empty(ICommissionData block)
	{
		return block.Commission is null &&
			block.CommType is null &&
			block.CommCurrency is null &&
			block.FundRenewWaiv is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 DiscretionInstructions has none of its fields.</summary>
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

	/// <summary>Whether a carrier of the FIX 4.4 FinancingDetails has none of its fields.</summary>
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

	/// <summary>Whether a carrier of the FIX 4.4 Instrument has none of its fields.</summary>
	internal static bool Empty(IInstrument block)
	{
		return block.Symbol is null &&
			block.SymbolSfx is null &&
			block.SecurityID is null &&
			block.SecurityIDSource is null &&
			block.NoSecurityAltID is null &&
			block.NoSecurityAltIDGroups is null &&
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
			block.NoEventsGroups is null &&
			block.DatedDate is null &&
			block.InterestAccrualDate is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 InstrumentExtension has none of its fields.</summary>
	internal static bool Empty(IInstrumentExtension block)
	{
		return block.DeliveryForm is null &&
			block.PctAtRisk is null &&
			block.NoInstrAttrib is null &&
			block.NoInstrAttribGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 InstrumentLeg has none of its fields.</summary>
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

	/// <summary>Whether a carrier of the FIX 4.4 LegBenchmarkCurveData has none of its fields.</summary>
	internal static bool Empty(ILegBenchmarkCurveData block)
	{
		return block.LegBenchmarkCurveCurrency is null &&
			block.LegBenchmarkCurveName is null &&
			block.LegBenchmarkCurvePoint is null &&
			block.LegBenchmarkPrice is null &&
			block.LegBenchmarkPriceType is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 LegStipulations has none of its fields.</summary>
	internal static bool Empty(ILegStipulations block)
	{
		return block.NoLegStipulations is null &&
			block.NoLegStipulationsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 NestedParties has none of its fields.</summary>
	internal static bool Empty(INestedParties block)
	{
		return block.NoNestedPartyIDs is null &&
			block.NoNestedPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 NestedParties2 has none of its fields.</summary>
	internal static bool Empty(INestedParties2 block)
	{
		return block.NoNested2PartyIDs is null &&
			block.NoNested2PartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 NestedParties3 has none of its fields.</summary>
	internal static bool Empty(INestedParties3 block)
	{
		return block.NoNested3PartyIDs is null &&
			block.NoNested3PartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 OrderQtyData has none of its fields.</summary>
	internal static bool Empty(IOrderQtyData block)
	{
		return block.OrderQty is null &&
			block.CashOrderQty is null &&
			block.OrderPercent is null &&
			block.RoundingDirection is null &&
			block.RoundingModulus is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 Parties has none of its fields.</summary>
	internal static bool Empty(IParties block)
	{
		return block.NoPartyIDs is null &&
			block.NoPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 PegInstructions has none of its fields.</summary>
	internal static bool Empty(IPegInstructions block)
	{
		return block.PegOffsetValue is null &&
			block.PegMoveType is null &&
			block.PegOffsetType is null &&
			block.PegLimitType is null &&
			block.PegRoundDirection is null &&
			block.PegScope is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 PositionAmountData has none of its fields.</summary>
	internal static bool Empty(IPositionAmountData block)
	{
		return block.NoPosAmt is null &&
			block.NoPosAmtGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 PositionQty has none of its fields.</summary>
	internal static bool Empty(IPositionQty block)
	{
		return block.NoPositions is null &&
			block.NoPositionsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 SettlInstructionsData has none of its fields.</summary>
	internal static bool Empty(ISettlInstructionsData block)
	{
		return block.SettlDeliveryType is null &&
			block.StandInstDbType is null &&
			block.StandInstDbName is null &&
			block.StandInstDbID is null &&
			block.NoDlvyInst is null &&
			block.NoDlvyInstGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 SettlParties has none of its fields.</summary>
	internal static bool Empty(ISettlParties block)
	{
		return block.NoSettlPartyIDs is null &&
			block.NoSettlPartyIDsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 SpreadOrBenchmarkCurveData has none of its fields.</summary>
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

	/// <summary>Whether a carrier of the FIX 4.4 Stipulations has none of its fields.</summary>
	internal static bool Empty(IStipulations block)
	{
		return block.NoStipulations is null &&
			block.NoStipulationsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 TrdRegTimestamps has none of its fields.</summary>
	internal static bool Empty(ITrdRegTimestamps block)
	{
		return block.NoTrdRegTimestamps is null &&
			block.NoTrdRegTimestampsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 UnderlyingInstrument has none of its fields.</summary>
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
			block.NoUnderlyingStipsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 UnderlyingStipulations has none of its fields.</summary>
	internal static bool Empty(IUnderlyingStipulations block)
	{
		return block.NoUnderlyingStips is null &&
			block.NoUnderlyingStipsGroups is null;
	}

	/// <summary>Whether a carrier of the FIX 4.4 YieldData has none of its fields.</summary>
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
