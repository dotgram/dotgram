using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix44;

// Written by generate.py from the FIX 4.4 repository; not edited by hand.

/// <summary>
/// The check of every FIX 4.4 message type, of every component it reuses, and of every entry of
/// every repeating group; the fields' own are in FixValidators.Fields.cs.
/// </summary>
/// <remarks>
/// Straight-line checks written against the fields of the class they are about, and nothing else:
/// a field the repository marks required, a component it marks required and the carrier left empty,
/// every field handed to its own slot, and every count held to the entries that follow it. Every
/// slot is typed on what it checks and named for it — a message type, a component, and a group
/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run
/// time replaces the slots it describes by name and leaves the rest.
/// </remarks>
partial class FixValidators
{
	/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>
	public static readonly FixValidators Default = new();

	/// <summary>Holds a FIX 4.4 Advertisement to the schema.</summary>
	public Func<FixContext, FixMessage.Advertisement, bool> Advertisement { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 4.4 AllocationInstruction to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationInstruction, bool> AllocationInstruction { get; set; } = ValidateAllocationInstruction;

	/// <summary>Holds a FIX 4.4 AllocationInstructionAck to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationInstructionAck, bool> AllocationInstructionAck { get; set; } = ValidateAllocationInstructionAck;

	/// <summary>Holds a FIX 4.4 AllocationReport to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationReport, bool> AllocationReport { get; set; } = ValidateAllocationReport;

	/// <summary>Holds a FIX 4.4 AllocationReportAck to the schema.</summary>
	public Func<FixContext, FixMessage.AllocationReportAck, bool> AllocationReportAck { get; set; } = ValidateAllocationReportAck;

	/// <summary>Holds a FIX 4.4 AssignmentReport to the schema.</summary>
	public Func<FixContext, FixMessage.AssignmentReport, bool> AssignmentReport { get; set; } = ValidateAssignmentReport;

	/// <summary>Holds a FIX 4.4 BidRequest to the schema.</summary>
	public Func<FixContext, FixMessage.BidRequest, bool> BidRequest { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 4.4 BidResponse to the schema.</summary>
	public Func<FixContext, FixMessage.BidResponse, bool> BidResponse { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 4.4 BusinessMessageReject to the schema.</summary>
	public Func<FixContext, FixMessage.BusinessMessageReject, bool> BusinessMessageReject { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 4.4 CollateralAssignment to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralAssignment, bool> CollateralAssignment { get; set; } = ValidateCollateralAssignment;

	/// <summary>Holds a FIX 4.4 CollateralInquiry to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralInquiry, bool> CollateralInquiry { get; set; } = ValidateCollateralInquiry;

	/// <summary>Holds a FIX 4.4 CollateralInquiryAck to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralInquiryAck, bool> CollateralInquiryAck { get; set; } = ValidateCollateralInquiryAck;

	/// <summary>Holds a FIX 4.4 CollateralReport to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralReport, bool> CollateralReport { get; set; } = ValidateCollateralReport;

	/// <summary>Holds a FIX 4.4 CollateralRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralRequest, bool> CollateralRequest { get; set; } = ValidateCollateralRequest;

	/// <summary>Holds a FIX 4.4 CollateralResponse to the schema.</summary>
	public Func<FixContext, FixMessage.CollateralResponse, bool> CollateralResponse { get; set; } = ValidateCollateralResponse;

	/// <summary>Holds a FIX 4.4 Confirmation to the schema.</summary>
	public Func<FixContext, FixMessage.Confirmation, bool> Confirmation { get; set; } = ValidateConfirmation;

	/// <summary>Holds a FIX 4.4 ConfirmationAck to the schema.</summary>
	public Func<FixContext, FixMessage.ConfirmationAck, bool> ConfirmationAck { get; set; } = ValidateConfirmationAck;

	/// <summary>Holds a FIX 4.4 ConfirmationRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ConfirmationRequest, bool> ConfirmationRequest { get; set; } = ValidateConfirmationRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelReplaceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CrossOrderCancelReplaceRequest, bool> CrossOrderCancelReplaceRequest { get; set; } = ValidateCrossOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 CrossOrderCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.CrossOrderCancelRequest, bool> CrossOrderCancelRequest { get; set; } = ValidateCrossOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityList to the schema.</summary>
	public Func<FixContext, FixMessage.DerivativeSecurityList, bool> DerivativeSecurityList { get; set; } = ValidateDerivativeSecurityList;

	/// <summary>Holds a FIX 4.4 DerivativeSecurityListRequest to the schema.</summary>
	public Func<FixContext, FixMessage.DerivativeSecurityListRequest, bool> DerivativeSecurityListRequest { get; set; } = ValidateDerivativeSecurityListRequest;

	/// <summary>Holds a FIX 4.4 DontKnowTrade to the schema.</summary>
	public Func<FixContext, FixMessage.DontKnowTrade, bool> DontKnowTrade { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 4.4 Email to the schema.</summary>
	public Func<FixContext, FixMessage.Email, bool> Email { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 4.4 ExecutionReport to the schema.</summary>
	public Func<FixContext, FixMessage.ExecutionReport, bool> ExecutionReport { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 4.4 Heartbeat to the schema.</summary>
	public Func<FixContext, FixMessage.Heartbeat, bool> Heartbeat { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 4.4 IndicationOfInterest to the schema.</summary>
	public Func<FixContext, FixMessage.IndicationOfInterest, bool> IndicationOfInterest { get; set; } = ValidateIndicationOfInterest;

	/// <summary>Holds a FIX 4.4 ListCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ListCancelRequest, bool> ListCancelRequest { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 4.4 ListExecute to the schema.</summary>
	public Func<FixContext, FixMessage.ListExecute, bool> ListExecute { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 4.4 ListStatus to the schema.</summary>
	public Func<FixContext, FixMessage.ListStatus, bool> ListStatus { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 4.4 ListStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ListStatusRequest, bool> ListStatusRequest { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 4.4 ListStrikePrice to the schema.</summary>
	public Func<FixContext, FixMessage.ListStrikePrice, bool> ListStrikePrice { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 4.4 Logon to the schema.</summary>
	public Func<FixContext, FixMessage.Logon, bool> Logon { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 4.4 Logout to the schema.</summary>
	public Func<FixContext, FixMessage.Logout, bool> Logout { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 4.4 MarketDataIncrementalRefresh to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataIncrementalRefresh, bool> MarketDataIncrementalRefresh { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 4.4 MarketDataRequest to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataRequest, bool> MarketDataRequest { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 4.4 MarketDataRequestReject to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataRequestReject, bool> MarketDataRequestReject { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 4.4 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Func<FixContext, FixMessage.MarketDataSnapshotFullRefresh, bool> MarketDataSnapshotFullRefresh { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 4.4 MassQuote to the schema.</summary>
	public Func<FixContext, FixMessage.MassQuote, bool> MassQuote { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 4.4 MassQuoteAcknowledgement to the schema.</summary>
	public Func<FixContext, FixMessage.MassQuoteAcknowledgement, bool> MassQuoteAcknowledgement { get; set; } = ValidateMassQuoteAcknowledgement;

	/// <summary>Holds a FIX 4.4 MultilegOrderCancelReplaceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.MultilegOrderCancelReplaceRequest, bool> MultilegOrderCancelReplaceRequest { get; set; } = ValidateMultilegOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 NetworkStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.NetworkStatusRequest, bool> NetworkStatusRequest { get; set; } = ValidateNetworkStatusRequest;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponse to the schema.</summary>
	public Func<FixContext, FixMessage.NetworkStatusResponse, bool> NetworkStatusResponse { get; set; } = ValidateNetworkStatusResponse;

	/// <summary>Holds a FIX 4.4 NewOrderCross to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderCross, bool> NewOrderCross { get; set; } = ValidateNewOrderCross;

	/// <summary>Holds a FIX 4.4 NewOrderList to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderList, bool> NewOrderList { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 4.4 NewOrderMultileg to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderMultileg, bool> NewOrderMultileg { get; set; } = ValidateNewOrderMultileg;

	/// <summary>Holds a FIX 4.4 NewOrderSingle to the schema.</summary>
	public Func<FixContext, FixMessage.NewOrderSingle, bool> NewOrderSingle { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 4.4 News to the schema.</summary>
	public Func<FixContext, FixMessage.News, bool> News { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 4.4 OrderCancelReject to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelReject, bool> OrderCancelReject { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 4.4 OrderCancelReplaceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelReplaceRequest, bool> OrderCancelReplaceRequest { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.4 OrderCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderCancelRequest, bool> OrderCancelRequest { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassCancelReport to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassCancelReport, bool> OrderMassCancelReport { get; set; } = ValidateOrderMassCancelReport;

	/// <summary>Holds a FIX 4.4 OrderMassCancelRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassCancelRequest, bool> OrderMassCancelRequest { get; set; } = ValidateOrderMassCancelRequest;

	/// <summary>Holds a FIX 4.4 OrderMassStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderMassStatusRequest, bool> OrderMassStatusRequest { get; set; } = ValidateOrderMassStatusRequest;

	/// <summary>Holds a FIX 4.4 OrderStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.OrderStatusRequest, bool> OrderStatusRequest { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceReport to the schema.</summary>
	public Func<FixContext, FixMessage.PositionMaintenanceReport, bool> PositionMaintenanceReport { get; set; } = ValidatePositionMaintenanceReport;

	/// <summary>Holds a FIX 4.4 PositionMaintenanceRequest to the schema.</summary>
	public Func<FixContext, FixMessage.PositionMaintenanceRequest, bool> PositionMaintenanceRequest { get; set; } = ValidatePositionMaintenanceRequest;

	/// <summary>Holds a FIX 4.4 PositionReport to the schema.</summary>
	public Func<FixContext, FixMessage.PositionReport, bool> PositionReport { get; set; } = ValidatePositionReport;

	/// <summary>Holds a FIX 4.4 Quote to the schema.</summary>
	public Func<FixContext, FixMessage.Quote, bool> Quote { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 4.4 QuoteCancel to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteCancel, bool> QuoteCancel { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 4.4 QuoteRequest to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteRequest, bool> QuoteRequest { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 4.4 QuoteRequestReject to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteRequestReject, bool> QuoteRequestReject { get; set; } = ValidateQuoteRequestReject;

	/// <summary>Holds a FIX 4.4 QuoteResponse to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteResponse, bool> QuoteResponse { get; set; } = ValidateQuoteResponse;

	/// <summary>Holds a FIX 4.4 QuoteStatusReport to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteStatusReport, bool> QuoteStatusReport { get; set; } = ValidateQuoteStatusReport;

	/// <summary>Holds a FIX 4.4 QuoteStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.QuoteStatusRequest, bool> QuoteStatusRequest { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 4.4 RFQRequest to the schema.</summary>
	public Func<FixContext, FixMessage.RFQRequest, bool> RFQRequest { get; set; } = ValidateRFQRequest;

	/// <summary>Holds a FIX 4.4 RegistrationInstructions to the schema.</summary>
	public Func<FixContext, FixMessage.RegistrationInstructions, bool> RegistrationInstructions { get; set; } = ValidateRegistrationInstructions;

	/// <summary>Holds a FIX 4.4 RegistrationInstructionsResponse to the schema.</summary>
	public Func<FixContext, FixMessage.RegistrationInstructionsResponse, bool> RegistrationInstructionsResponse { get; set; } = ValidateRegistrationInstructionsResponse;

	/// <summary>Holds a FIX 4.4 Reject to the schema.</summary>
	public Func<FixContext, FixMessage.Reject, bool> Reject { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 4.4 RequestForPositions to the schema.</summary>
	public Func<FixContext, FixMessage.RequestForPositions, bool> RequestForPositions { get; set; } = ValidateRequestForPositions;

	/// <summary>Holds a FIX 4.4 RequestForPositionsAck to the schema.</summary>
	public Func<FixContext, FixMessage.RequestForPositionsAck, bool> RequestForPositionsAck { get; set; } = ValidateRequestForPositionsAck;

	/// <summary>Holds a FIX 4.4 ResendRequest to the schema.</summary>
	public Func<FixContext, FixMessage.ResendRequest, bool> ResendRequest { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 4.4 SecurityDefinition to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityDefinition, bool> SecurityDefinition { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 4.4 SecurityDefinitionRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityDefinitionRequest, bool> SecurityDefinitionRequest { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 4.4 SecurityList to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityList, bool> SecurityList { get; set; } = ValidateSecurityList;

	/// <summary>Holds a FIX 4.4 SecurityListRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityListRequest, bool> SecurityListRequest { get; set; } = ValidateSecurityListRequest;

	/// <summary>Holds a FIX 4.4 SecurityStatus to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityStatus, bool> SecurityStatus { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 4.4 SecurityStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityStatusRequest, bool> SecurityStatusRequest { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypeRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityTypeRequest, bool> SecurityTypeRequest { get; set; } = ValidateSecurityTypeRequest;

	/// <summary>Holds a FIX 4.4 SecurityTypes to the schema.</summary>
	public Func<FixContext, FixMessage.SecurityTypes, bool> SecurityTypes { get; set; } = ValidateSecurityTypes;

	/// <summary>Holds a FIX 4.4 SequenceReset to the schema.</summary>
	public Func<FixContext, FixMessage.SequenceReset, bool> SequenceReset { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 4.4 SettlementInstructionRequest to the schema.</summary>
	public Func<FixContext, FixMessage.SettlementInstructionRequest, bool> SettlementInstructionRequest { get; set; } = ValidateSettlementInstructionRequest;

	/// <summary>Holds a FIX 4.4 SettlementInstructions to the schema.</summary>
	public Func<FixContext, FixMessage.SettlementInstructions, bool> SettlementInstructions { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 4.4 TestRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TestRequest, bool> TestRequest { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReport to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReport, bool> TradeCaptureReport { get; set; } = ValidateTradeCaptureReport;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportAck to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportAck, bool> TradeCaptureReportAck { get; set; } = ValidateTradeCaptureReportAck;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportRequest, bool> TradeCaptureReportRequest { get; set; } = ValidateTradeCaptureReportRequest;

	/// <summary>Holds a FIX 4.4 TradeCaptureReportRequestAck to the schema.</summary>
	public Func<FixContext, FixMessage.TradeCaptureReportRequestAck, bool> TradeCaptureReportRequestAck { get; set; } = ValidateTradeCaptureReportRequestAck;

	/// <summary>Holds a FIX 4.4 TradingSessionStatus to the schema.</summary>
	public Func<FixContext, FixMessage.TradingSessionStatus, bool> TradingSessionStatus { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 4.4 TradingSessionStatusRequest to the schema.</summary>
	public Func<FixContext, FixMessage.TradingSessionStatusRequest, bool> TradingSessionStatusRequest { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds a FIX 4.4 UserRequest to the schema.</summary>
	public Func<FixContext, FixMessage.UserRequest, bool> UserRequest { get; set; } = ValidateUserRequest;

	/// <summary>Holds a FIX 4.4 UserResponse to the schema.</summary>
	public Func<FixContext, FixMessage.UserResponse, bool> UserResponse { get; set; } = ValidateUserResponse;

	/// <summary>Holds a FIX 4.4 XMLnonFIX to the schema.</summary>
	public Func<FixContext, FixMessage.XMLnonFIX, bool> XMLnonFIX { get; set; } = ValidateXMLnonFIX;

	/// <summary>Holds a message of a type FIX 4.4 does not describe to the schema.</summary>
	public Func<FixContext, FixMessage.Custom, bool> Custom { get; set; } = ValidateCustom;

	/// <summary>Holds a FIX 4.4 CommissionData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ICommissionData, bool> CommissionData { get; set; } = ValidateCommissionData;

	/// <summary>Holds a FIX 4.4 DiscretionInstructions to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IDiscretionInstructions, bool> DiscretionInstructions { get; set; } = ValidateDiscretionInstructions;

	/// <summary>Holds a FIX 4.4 FinancingDetails to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IFinancingDetails, bool> FinancingDetails { get; set; } = ValidateFinancingDetails;

	/// <summary>Holds a FIX 4.4 Instrument to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrument, bool> Instrument { get; set; } = ValidateInstrument;

	/// <summary>Holds a FIX 4.4 InstrumentExtension to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrumentExtension, bool> InstrumentExtension { get; set; } = ValidateInstrumentExtension;

	/// <summary>Holds a FIX 4.4 InstrumentLeg to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrumentLeg, bool> InstrumentLeg { get; set; } = ValidateInstrumentLeg;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ILegBenchmarkCurveData, bool> LegBenchmarkCurveData { get; set; } = ValidateLegBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 LegStipulations to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ILegStipulations, bool> LegStipulations { get; set; } = ValidateLegStipulations;

	/// <summary>Holds a FIX 4.4 NestedParties to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, INestedParties, bool> NestedParties { get; set; } = ValidateNestedParties;

	/// <summary>Holds a FIX 4.4 NestedParties2 to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, INestedParties2, bool> NestedParties2 { get; set; } = ValidateNestedParties2;

	/// <summary>Holds a FIX 4.4 NestedParties3 to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, INestedParties3, bool> NestedParties3 { get; set; } = ValidateNestedParties3;

	/// <summary>Holds a FIX 4.4 OrderQtyData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IOrderQtyData, bool> OrderQtyData { get; set; } = ValidateOrderQtyData;

	/// <summary>Holds a FIX 4.4 Parties to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IParties, bool> Parties { get; set; } = ValidateParties;

	/// <summary>Holds a FIX 4.4 PegInstructions to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IPegInstructions, bool> PegInstructions { get; set; } = ValidatePegInstructions;

	/// <summary>Holds a FIX 4.4 PositionAmountData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IPositionAmountData, bool> PositionAmountData { get; set; } = ValidatePositionAmountData;

	/// <summary>Holds a FIX 4.4 PositionQty to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IPositionQty, bool> PositionQty { get; set; } = ValidatePositionQty;

	/// <summary>Holds a FIX 4.4 SettlInstructionsData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ISettlInstructionsData, bool> SettlInstructionsData { get; set; } = ValidateSettlInstructionsData;

	/// <summary>Holds a FIX 4.4 SettlParties to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ISettlParties, bool> SettlParties { get; set; } = ValidateSettlParties;

	/// <summary>Holds a FIX 4.4 SpreadOrBenchmarkCurveData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ISpreadOrBenchmarkCurveData, bool> SpreadOrBenchmarkCurveData { get; set; } = ValidateSpreadOrBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 Stipulations to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IStipulations, bool> Stipulations { get; set; } = ValidateStipulations;

	/// <summary>Holds a FIX 4.4 TrdRegTimestamps to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ITrdRegTimestamps, bool> TrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrument to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IUnderlyingInstrument, bool> UnderlyingInstrument { get; set; } = ValidateUnderlyingInstrument;

	/// <summary>Holds a FIX 4.4 UnderlyingStipulations to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IUnderlyingStipulations, bool> UnderlyingStipulations { get; set; } = ValidateUnderlyingStipulations;

	/// <summary>Holds a FIX 4.4 YieldData to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IYieldData, bool> YieldData { get; set; } = ValidateYieldData;

	/// <summary>Holds one entry of IInstrument.NoSecurityAltIDGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IInstrument.NoSecurityAltIDGroup, int, bool> Instrument_NoSecurityAltID { get; set; } = ValidateInstrument_NoSecurityAltID;

	/// <summary>Holds one entry of IInstrument.NoEventsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IInstrument.NoEventsGroup, int, bool> Instrument_NoEvents { get; set; } = ValidateInstrument_NoEvents;

	/// <summary>Holds one entry of IInstrumentExtension.NoInstrAttribGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IInstrumentExtension.NoInstrAttribGroup, int, bool> InstrumentExtension_NoInstrAttrib { get; set; } = ValidateInstrumentExtension_NoInstrAttrib;

	/// <summary>Holds one entry of IInstrumentLeg.NoLegSecurityAltIDGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IInstrumentLeg.NoLegSecurityAltIDGroup, int, bool> InstrumentLeg_NoLegSecurityAltID { get; set; } = ValidateInstrumentLeg_NoLegSecurityAltID;

	/// <summary>Holds one entry of ILegStipulations.NoLegStipulationsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, ILegStipulations.NoLegStipulationsGroup, int, bool> LegStipulations_NoLegStipulations { get; set; } = ValidateLegStipulations_NoLegStipulations;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties.NoNestedPartyIDsGroup, int, bool> NestedParties_NoNestedPartyIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs;

	/// <summary>Holds one entry of INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup, int, bool> NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs { get; set; } = ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties2.NoNested2PartyIDsGroup, int, bool> NestedParties2_NoNested2PartyIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs;

	/// <summary>Holds one entry of INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup, int, bool> NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs { get; set; } = ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties3.NoNested3PartyIDsGroup, int, bool> NestedParties3_NoNested3PartyIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs;

	/// <summary>Holds one entry of INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup, int, bool> NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs { get; set; } = ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IParties.NoPartyIDsGroup, int, bool> Parties_NoPartyIDs { get; set; } = ValidateParties_NoPartyIDs;

	/// <summary>Holds one entry of IParties.NoPartyIDsGroup.NoPartySubIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IParties.NoPartyIDsGroup.NoPartySubIDsGroup, int, bool> Parties_NoPartyIDs_NoPartySubIDs { get; set; } = ValidateParties_NoPartyIDs_NoPartySubIDs;

	/// <summary>Holds one entry of IPositionAmountData.NoPosAmtGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IPositionAmountData.NoPosAmtGroup, int, bool> PositionAmountData_NoPosAmt { get; set; } = ValidatePositionAmountData_NoPosAmt;

	/// <summary>Holds one entry of IPositionQty.NoPositionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IPositionQty.NoPositionsGroup, int, bool> PositionQty_NoPositions { get; set; } = ValidatePositionQty_NoPositions;

	/// <summary>Holds one entry of ISettlInstructionsData.NoDlvyInstGroup to the schema.</summary>
	public Func<FixContext, FixMessage, ISettlInstructionsData.NoDlvyInstGroup, int, bool> SettlInstructionsData_NoDlvyInst { get; set; } = ValidateSettlInstructionsData_NoDlvyInst;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, ISettlParties.NoSettlPartyIDsGroup, int, bool> SettlParties_NoSettlPartyIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs;

	/// <summary>Holds one entry of ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup, int, bool> SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs { get; set; } = ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs;

	/// <summary>Holds one entry of IStipulations.NoStipulationsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IStipulations.NoStipulationsGroup, int, bool> Stipulations_NoStipulations { get; set; } = ValidateStipulations_NoStipulations;

	/// <summary>Holds one entry of ITrdRegTimestamps.NoTrdRegTimestampsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, ITrdRegTimestamps.NoTrdRegTimestampsGroup, int, bool> TrdRegTimestamps_NoTrdRegTimestamps { get; set; } = ValidateTrdRegTimestamps_NoTrdRegTimestamps;

	/// <summary>Holds one entry of IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup, int, bool> UnderlyingInstrument_NoUnderlyingSecurityAltID { get; set; } = ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID;

	/// <summary>Holds one entry of IUnderlyingStipulations.NoUnderlyingStipsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, IUnderlyingStipulations.NoUnderlyingStipsGroup, int, bool> UnderlyingStipulations_NoUnderlyingStips { get; set; } = ValidateUnderlyingStipulations_NoUnderlyingStips;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Advertisement.NoLegsGroup, int, bool> Advertisement_NoLegs { get; set; } = ValidateAdvertisement_NoLegs;

	/// <summary>Holds one entry of FixMessage.Advertisement.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Advertisement.NoUnderlyingsGroup, int, bool> Advertisement_NoUnderlyings { get; set; } = ValidateAdvertisement_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoOrdersGroup, int, bool> AllocationInstruction_NoOrders { get; set; } = ValidateAllocationInstruction_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoExecsGroup, int, bool> AllocationInstruction_NoExecs { get; set; } = ValidateAllocationInstruction_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoUnderlyingsGroup, int, bool> AllocationInstruction_NoUnderlyings { get; set; } = ValidateAllocationInstruction_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoLegsGroup, int, bool> AllocationInstruction_NoLegs { get; set; } = ValidateAllocationInstruction_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoAllocsGroup, int, bool> AllocationInstruction_NoAllocs { get; set; } = ValidateAllocationInstruction_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup, int, bool> AllocationInstruction_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationInstruction_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup, int, bool> AllocationInstruction_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationInstruction_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationInstructionAck.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationInstructionAck.NoAllocsGroup, int, bool> AllocationInstructionAck_NoAllocs { get; set; } = ValidateAllocationInstructionAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoOrdersGroup, int, bool> AllocationReport_NoOrders { get; set; } = ValidateAllocationReport_NoOrders;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoExecsGroup, int, bool> AllocationReport_NoExecs { get; set; } = ValidateAllocationReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoUnderlyingsGroup, int, bool> AllocationReport_NoUnderlyings { get; set; } = ValidateAllocationReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoLegsGroup, int, bool> AllocationReport_NoLegs { get; set; } = ValidateAllocationReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoAllocsGroup, int, bool> AllocationReport_NoAllocs { get; set; } = ValidateAllocationReport_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup, int, bool> AllocationReport_NoAllocs_NoMiscFees { get; set; } = ValidateAllocationReport_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup, int, bool> AllocationReport_NoAllocs_NoClearingInstructions { get; set; } = ValidateAllocationReport_NoAllocs_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.AllocationReportAck.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AllocationReportAck.NoAllocsGroup, int, bool> AllocationReportAck_NoAllocs { get; set; } = ValidateAllocationReportAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AssignmentReport.NoLegsGroup, int, bool> AssignmentReport_NoLegs { get; set; } = ValidateAssignmentReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.AssignmentReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.AssignmentReport.NoUnderlyingsGroup, int, bool> AssignmentReport_NoUnderlyings { get; set; } = ValidateAssignmentReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidDescriptorsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.BidRequest.NoBidDescriptorsGroup, int, bool> BidRequest_NoBidDescriptors { get; set; } = ValidateBidRequest_NoBidDescriptors;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidComponentsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.BidRequest.NoBidComponentsGroup, int, bool> BidRequest_NoBidComponents { get; set; } = ValidateBidRequest_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.BidResponse.NoBidComponentsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.BidResponse.NoBidComponentsGroup, int, bool> BidResponse_NoBidComponents { get; set; } = ValidateBidResponse_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralAssignment.NoExecsGroup, int, bool> CollateralAssignment_NoExecs { get; set; } = ValidateCollateralAssignment_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralAssignment.NoTradesGroup, int, bool> CollateralAssignment_NoTrades { get; set; } = ValidateCollateralAssignment_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralAssignment.NoLegsGroup, int, bool> CollateralAssignment_NoLegs { get; set; } = ValidateCollateralAssignment_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralAssignment.NoUnderlyingsGroup, int, bool> CollateralAssignment_NoUnderlyings { get; set; } = ValidateCollateralAssignment_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralAssignment.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralAssignment.NoMiscFeesGroup, int, bool> CollateralAssignment_NoMiscFees { get; set; } = ValidateCollateralAssignment_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup, int, bool> CollateralInquiry_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiry_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiry.NoExecsGroup, int, bool> CollateralInquiry_NoExecs { get; set; } = ValidateCollateralInquiry_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiry.NoTradesGroup, int, bool> CollateralInquiry_NoTrades { get; set; } = ValidateCollateralInquiry_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiry.NoLegsGroup, int, bool> CollateralInquiry_NoLegs { get; set; } = ValidateCollateralInquiry_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiry.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiry.NoUnderlyingsGroup, int, bool> CollateralInquiry_NoUnderlyings { get; set; } = ValidateCollateralInquiry_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup, int, bool> CollateralInquiryAck_NoCollInquiryQualifier { get; set; } = ValidateCollateralInquiryAck_NoCollInquiryQualifier;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiryAck.NoExecsGroup, int, bool> CollateralInquiryAck_NoExecs { get; set; } = ValidateCollateralInquiryAck_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiryAck.NoTradesGroup, int, bool> CollateralInquiryAck_NoTrades { get; set; } = ValidateCollateralInquiryAck_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiryAck.NoLegsGroup, int, bool> CollateralInquiryAck_NoLegs { get; set; } = ValidateCollateralInquiryAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralInquiryAck.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup, int, bool> CollateralInquiryAck_NoUnderlyings { get; set; } = ValidateCollateralInquiryAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralReport.NoExecsGroup, int, bool> CollateralReport_NoExecs { get; set; } = ValidateCollateralReport_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralReport.NoTradesGroup, int, bool> CollateralReport_NoTrades { get; set; } = ValidateCollateralReport_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralReport.NoLegsGroup, int, bool> CollateralReport_NoLegs { get; set; } = ValidateCollateralReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralReport.NoUnderlyingsGroup, int, bool> CollateralReport_NoUnderlyings { get; set; } = ValidateCollateralReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralReport.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralReport.NoMiscFeesGroup, int, bool> CollateralReport_NoMiscFees { get; set; } = ValidateCollateralReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralRequest.NoExecsGroup, int, bool> CollateralRequest_NoExecs { get; set; } = ValidateCollateralRequest_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralRequest.NoTradesGroup, int, bool> CollateralRequest_NoTrades { get; set; } = ValidateCollateralRequest_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralRequest.NoLegsGroup, int, bool> CollateralRequest_NoLegs { get; set; } = ValidateCollateralRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralRequest.NoUnderlyingsGroup, int, bool> CollateralRequest_NoUnderlyings { get; set; } = ValidateCollateralRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralRequest.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralRequest.NoMiscFeesGroup, int, bool> CollateralRequest_NoMiscFees { get; set; } = ValidateCollateralRequest_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoExecsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralResponse.NoExecsGroup, int, bool> CollateralResponse_NoExecs { get; set; } = ValidateCollateralResponse_NoExecs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoTradesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralResponse.NoTradesGroup, int, bool> CollateralResponse_NoTrades { get; set; } = ValidateCollateralResponse_NoTrades;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralResponse.NoLegsGroup, int, bool> CollateralResponse_NoLegs { get; set; } = ValidateCollateralResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralResponse.NoUnderlyingsGroup, int, bool> CollateralResponse_NoUnderlyings { get; set; } = ValidateCollateralResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CollateralResponse.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CollateralResponse.NoMiscFeesGroup, int, bool> CollateralResponse_NoMiscFees { get; set; } = ValidateCollateralResponse_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Confirmation.NoOrdersGroup, int, bool> Confirmation_NoOrders { get; set; } = ValidateConfirmation_NoOrders;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Confirmation.NoUnderlyingsGroup, int, bool> Confirmation_NoUnderlyings { get; set; } = ValidateConfirmation_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Confirmation.NoLegsGroup, int, bool> Confirmation_NoLegs { get; set; } = ValidateConfirmation_NoLegs;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoCapacitiesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Confirmation.NoCapacitiesGroup, int, bool> Confirmation_NoCapacities { get; set; } = ValidateConfirmation_NoCapacities;

	/// <summary>Holds one entry of FixMessage.Confirmation.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Confirmation.NoMiscFeesGroup, int, bool> Confirmation_NoMiscFees { get; set; } = ValidateConfirmation_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.ConfirmationRequest.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ConfirmationRequest.NoOrdersGroup, int, bool> ConfirmationRequest_NoOrders { get; set; } = ValidateConfirmationRequest_NoOrders;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup, int, bool> CrossOrderCancelReplaceRequest_NoSides { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup, int, bool> CrossOrderCancelReplaceRequest_NoSides_NoAllocs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup, int, bool> CrossOrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup, int, bool> CrossOrderCancelReplaceRequest_NoLegs { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup, int, bool> CrossOrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateCrossOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoSidesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelRequest.NoSidesGroup, int, bool> CrossOrderCancelRequest_NoSides { get; set; } = ValidateCrossOrderCancelRequest_NoSides;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup, int, bool> CrossOrderCancelRequest_NoUnderlyings { get; set; } = ValidateCrossOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.CrossOrderCancelRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.CrossOrderCancelRequest.NoLegsGroup, int, bool> CrossOrderCancelRequest_NoLegs { get; set; } = ValidateCrossOrderCancelRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.DerivativeSecurityList.NoRelatedSymGroup, int, bool> DerivativeSecurityList_NoRelatedSym { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup, int, bool> DerivativeSecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateDerivativeSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.DontKnowTrade.NoUnderlyingsGroup, int, bool> DontKnowTrade_NoUnderlyings { get; set; } = ValidateDontKnowTrade_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.DontKnowTrade.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.DontKnowTrade.NoLegsGroup, int, bool> DontKnowTrade_NoLegs { get; set; } = ValidateDontKnowTrade_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.NoRoutingIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Email.NoRoutingIDsGroup, int, bool> Email_NoRoutingIDs { get; set; } = ValidateEmail_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.Email.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Email.NoRelatedSymGroup, int, bool> Email_NoRelatedSym { get; set; } = ValidateEmail_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Email.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Email.NoUnderlyingsGroup, int, bool> Email_NoUnderlyings { get; set; } = ValidateEmail_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Email.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Email.NoLegsGroup, int, bool> Email_NoLegs { get; set; } = ValidateEmail_NoLegs;

	/// <summary>Holds one entry of FixMessage.Email.LinesOfTextGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Email.LinesOfTextGroup, int, bool> Email_LinesOfText { get; set; } = ValidateEmail_LinesOfText;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContraBrokersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ExecutionReport.NoContraBrokersGroup, int, bool> ExecutionReport_NoContraBrokers { get; set; } = ValidateExecutionReport_NoContraBrokers;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ExecutionReport.NoUnderlyingsGroup, int, bool> ExecutionReport_NoUnderlyings { get; set; } = ValidateExecutionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContAmtsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ExecutionReport.NoContAmtsGroup, int, bool> ExecutionReport_NoContAmts { get; set; } = ValidateExecutionReport_NoContAmts;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ExecutionReport.NoLegsGroup, int, bool> ExecutionReport_NoLegs { get; set; } = ValidateExecutionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ExecutionReport.NoMiscFeesGroup, int, bool> ExecutionReport_NoMiscFees { get; set; } = ValidateExecutionReport_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.IndicationOfInterest.NoUnderlyingsGroup, int, bool> IndicationOfInterest_NoUnderlyings { get; set; } = ValidateIndicationOfInterest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.IndicationOfInterest.NoLegsGroup, int, bool> IndicationOfInterest_NoLegs { get; set; } = ValidateIndicationOfInterest_NoLegs;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoIOIQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.IndicationOfInterest.NoIOIQualifiersGroup, int, bool> IndicationOfInterest_NoIOIQualifiers { get; set; } = ValidateIndicationOfInterest_NoIOIQualifiers;

	/// <summary>Holds one entry of FixMessage.IndicationOfInterest.NoRoutingIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.IndicationOfInterest.NoRoutingIDsGroup, int, bool> IndicationOfInterest_NoRoutingIDs { get; set; } = ValidateIndicationOfInterest_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.ListStatus.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ListStatus.NoOrdersGroup, int, bool> ListStatus_NoOrders { get; set; } = ValidateListStatus_NoOrders;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ListStrikePrice.NoStrikesGroup, int, bool> ListStrikePrice_NoStrikes { get; set; } = ValidateListStrikePrice_NoStrikes;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.ListStrikePrice.NoUnderlyingsGroup, int, bool> ListStrikePrice_NoUnderlyings { get; set; } = ValidateListStrikePrice_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Logon.NoMsgTypesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Logon.NoMsgTypesGroup, int, bool> Logon_NoMsgTypes { get; set; } = ValidateLogon_NoMsgTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup, int, bool> MarketDataIncrementalRefresh_NoMDEntries { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup, int, bool> MarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup, int, bool> MarketDataIncrementalRefresh_NoMDEntries_NoLegs { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoMDEntryTypesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequest.NoMDEntryTypesGroup, int, bool> MarketDataRequest_NoMDEntryTypes { get; set; } = ValidateMarketDataRequest_NoMDEntryTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequest.NoRelatedSymGroup, int, bool> MarketDataRequest_NoRelatedSym { get; set; } = ValidateMarketDataRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup, int, bool> MarketDataRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup, int, bool> MarketDataRequest_NoRelatedSym_NoLegs { get; set; } = ValidateMarketDataRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequest.NoTradingSessionsGroup, int, bool> MarketDataRequest_NoTradingSessions { get; set; } = ValidateMarketDataRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MarketDataRequestReject.NoAltMDSourceGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup, int, bool> MarketDataRequestReject_NoAltMDSource { get; set; } = ValidateMarketDataRequestReject_NoAltMDSource;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup, int, bool> MarketDataSnapshotFullRefresh_NoUnderlyings { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup, int, bool> MarketDataSnapshotFullRefresh_NoLegs { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoLegs;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup, int, bool> MarketDataSnapshotFullRefresh_NoMDEntries { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuote.NoQuoteSetsGroup, int, bool> MassQuote_NoQuoteSets { get; set; } = ValidateMassQuote_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup, int, bool> MassQuote_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup, int, bool> MassQuote_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup, int, bool> MassQuoteAcknowledgement_NoQuoteSets { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup, int, bool> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup, int, bool> MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs { get; set; } = ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup, int, bool> MultilegOrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup, int, bool> MultilegOrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup, int, bool> MultilegOrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup, int, bool> MultilegOrderCancelReplaceRequest_NoLegs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup, int, bool> MultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs { get; set; } = ValidateMultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.NetworkStatusRequest.NoCompIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NetworkStatusRequest.NoCompIDsGroup, int, bool> NetworkStatusRequest_NoCompIDs { get; set; } = ValidateNetworkStatusRequest_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NetworkStatusResponse.NoCompIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NetworkStatusResponse.NoCompIDsGroup, int, bool> NetworkStatusResponse_NoCompIDs { get; set; } = ValidateNetworkStatusResponse_NoCompIDs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderCross.NoSidesGroup, int, bool> NewOrderCross_NoSides { get; set; } = ValidateNewOrderCross_NoSides;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup, int, bool> NewOrderCross_NoSides_NoAllocs { get; set; } = ValidateNewOrderCross_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderCross.NoUnderlyingsGroup, int, bool> NewOrderCross_NoUnderlyings { get; set; } = ValidateNewOrderCross_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderCross.NoLegsGroup, int, bool> NewOrderCross_NoLegs { get; set; } = ValidateNewOrderCross_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderCross.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderCross.NoTradingSessionsGroup, int, bool> NewOrderCross_NoTradingSessions { get; set; } = ValidateNewOrderCross_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderList.NoOrdersGroup, int, bool> NewOrderList_NoOrders { get; set; } = ValidateNewOrderList_NoOrders;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup, int, bool> NewOrderList_NoOrders_NoAllocs { get; set; } = ValidateNewOrderList_NoOrders_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup, int, bool> NewOrderList_NoOrders_NoTradingSessions { get; set; } = ValidateNewOrderList_NoOrders_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup, int, bool> NewOrderList_NoOrders_NoUnderlyings { get; set; } = ValidateNewOrderList_NoOrders_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderMultileg.NoAllocsGroup, int, bool> NewOrderMultileg_NoAllocs { get; set; } = ValidateNewOrderMultileg_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderMultileg.NoTradingSessionsGroup, int, bool> NewOrderMultileg_NoTradingSessions { get; set; } = ValidateNewOrderMultileg_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderMultileg.NoUnderlyingsGroup, int, bool> NewOrderMultileg_NoUnderlyings { get; set; } = ValidateNewOrderMultileg_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderMultileg.NoLegsGroup, int, bool> NewOrderMultileg_NoLegs { get; set; } = ValidateNewOrderMultileg_NoLegs;

	/// <summary>Holds one entry of FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup, int, bool> NewOrderMultileg_NoLegs_NoLegAllocs { get; set; } = ValidateNewOrderMultileg_NoLegs_NoLegAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderSingle.NoAllocsGroup, int, bool> NewOrderSingle_NoAllocs { get; set; } = ValidateNewOrderSingle_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderSingle.NoTradingSessionsGroup, int, bool> NewOrderSingle_NoTradingSessions { get; set; } = ValidateNewOrderSingle_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.NewOrderSingle.NoUnderlyingsGroup, int, bool> NewOrderSingle_NoUnderlyings { get; set; } = ValidateNewOrderSingle_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.News.NoRoutingIDsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.News.NoRoutingIDsGroup, int, bool> News_NoRoutingIDs { get; set; } = ValidateNews_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.News.NoRelatedSymGroup, int, bool> News_NoRelatedSym { get; set; } = ValidateNews_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.News.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.News.NoLegsGroup, int, bool> News_NoLegs { get; set; } = ValidateNews_NoLegs;

	/// <summary>Holds one entry of FixMessage.News.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.News.NoUnderlyingsGroup, int, bool> News_NoUnderlyings { get; set; } = ValidateNews_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.News.LinesOfTextGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.News.LinesOfTextGroup, int, bool> News_LinesOfText { get; set; } = ValidateNews_LinesOfText;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup, int, bool> OrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup, int, bool> OrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup, int, bool> OrderCancelReplaceRequest_NoUnderlyings { get; set; } = ValidateOrderCancelReplaceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderCancelRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderCancelRequest.NoUnderlyingsGroup, int, bool> OrderCancelRequest_NoUnderlyings { get; set; } = ValidateOrderCancelRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup, int, bool> OrderMassCancelReport_NoAffectedOrders { get; set; } = ValidateOrderMassCancelReport_NoAffectedOrders;

	/// <summary>Holds one entry of FixMessage.OrderStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.OrderStatusRequest.NoUnderlyingsGroup, int, bool> OrderStatusRequest_NoUnderlyings { get; set; } = ValidateOrderStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceReport.NoLegsGroup, int, bool> PositionMaintenanceReport_NoLegs { get; set; } = ValidatePositionMaintenanceReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup, int, bool> PositionMaintenanceReport_NoUnderlyings { get; set; } = ValidatePositionMaintenanceReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup, int, bool> PositionMaintenanceReport_NoTradingSessions { get; set; } = ValidatePositionMaintenanceReport_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceRequest.NoLegsGroup, int, bool> PositionMaintenanceRequest_NoLegs { get; set; } = ValidatePositionMaintenanceRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup, int, bool> PositionMaintenanceRequest_NoUnderlyings { get; set; } = ValidatePositionMaintenanceRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup, int, bool> PositionMaintenanceRequest_NoTradingSessions { get; set; } = ValidatePositionMaintenanceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionReport.NoLegsGroup, int, bool> PositionReport_NoLegs { get; set; } = ValidatePositionReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.PositionReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.PositionReport.NoUnderlyingsGroup, int, bool> PositionReport_NoUnderlyings { get; set; } = ValidatePositionReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoQuoteQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Quote.NoQuoteQualifiersGroup, int, bool> Quote_NoQuoteQualifiers { get; set; } = ValidateQuote_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.Quote.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Quote.NoUnderlyingsGroup, int, bool> Quote_NoUnderlyings { get; set; } = ValidateQuote_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.Quote.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.Quote.NoLegsGroup, int, bool> Quote_NoLegs { get; set; } = ValidateQuote_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteCancel.NoQuoteEntriesGroup, int, bool> QuoteCancel_NoQuoteEntries { get; set; } = ValidateQuoteCancel_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup, int, bool> QuoteCancel_NoQuoteEntries_NoUnderlyings { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup, int, bool> QuoteCancel_NoQuoteEntries_NoLegs { get; set; } = ValidateQuoteCancel_NoQuoteEntries_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequest.NoRelatedSymGroup, int, bool> QuoteRequest_NoRelatedSym { get; set; } = ValidateQuoteRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup, int, bool> QuoteRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup, int, bool> QuoteRequest_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup, int, bool> QuoteRequest_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequestReject.NoRelatedSymGroup, int, bool> QuoteRequestReject_NoRelatedSym { get; set; } = ValidateQuoteRequestReject_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup, int, bool> QuoteRequestReject_NoRelatedSym_NoUnderlyings { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup, int, bool> QuoteRequestReject_NoRelatedSym_NoLegs { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup, int, bool> QuoteRequestReject_NoRelatedSym_NoQuoteQualifiers { get; set; } = ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoQuoteQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteResponse.NoQuoteQualifiersGroup, int, bool> QuoteResponse_NoQuoteQualifiers { get; set; } = ValidateQuoteResponse_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteResponse.NoUnderlyingsGroup, int, bool> QuoteResponse_NoUnderlyings { get; set; } = ValidateQuoteResponse_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteResponse.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteResponse.NoLegsGroup, int, bool> QuoteResponse_NoLegs { get; set; } = ValidateQuoteResponse_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteStatusReport.NoUnderlyingsGroup, int, bool> QuoteStatusReport_NoUnderlyings { get; set; } = ValidateQuoteStatusReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteStatusReport.NoLegsGroup, int, bool> QuoteStatusReport_NoLegs { get; set; } = ValidateQuoteStatusReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup, int, bool> QuoteStatusReport_NoQuoteQualifiers { get; set; } = ValidateQuoteStatusReport_NoQuoteQualifiers;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup, int, bool> QuoteStatusRequest_NoUnderlyings { get; set; } = ValidateQuoteStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.QuoteStatusRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.QuoteStatusRequest.NoLegsGroup, int, bool> QuoteStatusRequest_NoLegs { get; set; } = ValidateQuoteStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RFQRequest.NoRelatedSymGroup, int, bool> RFQRequest_NoRelatedSym { get; set; } = ValidateRFQRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup, int, bool> RFQRequest_NoRelatedSym_NoUnderlyings { get; set; } = ValidateRFQRequest_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup, int, bool> RFQRequest_NoRelatedSym_NoLegs { get; set; } = ValidateRFQRequest_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoRegistDtlsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RegistrationInstructions.NoRegistDtlsGroup, int, bool> RegistrationInstructions_NoRegistDtls { get; set; } = ValidateRegistrationInstructions_NoRegistDtls;

	/// <summary>Holds one entry of FixMessage.RegistrationInstructions.NoDistribInstsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RegistrationInstructions.NoDistribInstsGroup, int, bool> RegistrationInstructions_NoDistribInsts { get; set; } = ValidateRegistrationInstructions_NoDistribInsts;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RequestForPositions.NoLegsGroup, int, bool> RequestForPositions_NoLegs { get; set; } = ValidateRequestForPositions_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RequestForPositions.NoUnderlyingsGroup, int, bool> RequestForPositions_NoUnderlyings { get; set; } = ValidateRequestForPositions_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.RequestForPositions.NoTradingSessionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RequestForPositions.NoTradingSessionsGroup, int, bool> RequestForPositions_NoTradingSessions { get; set; } = ValidateRequestForPositions_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RequestForPositionsAck.NoLegsGroup, int, bool> RequestForPositionsAck_NoLegs { get; set; } = ValidateRequestForPositionsAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.RequestForPositionsAck.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup, int, bool> RequestForPositionsAck_NoUnderlyings { get; set; } = ValidateRequestForPositionsAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityDefinition.NoUnderlyingsGroup, int, bool> SecurityDefinition_NoUnderlyings { get; set; } = ValidateSecurityDefinition_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityDefinition.NoLegsGroup, int, bool> SecurityDefinition_NoLegs { get; set; } = ValidateSecurityDefinition_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup, int, bool> SecurityDefinitionRequest_NoUnderlyings { get; set; } = ValidateSecurityDefinitionRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityDefinitionRequest.NoLegsGroup, int, bool> SecurityDefinitionRequest_NoLegs { get; set; } = ValidateSecurityDefinitionRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityList.NoRelatedSymGroup, int, bool> SecurityList_NoRelatedSym { get; set; } = ValidateSecurityList_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup, int, bool> SecurityList_NoRelatedSym_NoUnderlyings { get; set; } = ValidateSecurityList_NoRelatedSym_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup, int, bool> SecurityList_NoRelatedSym_NoLegs { get; set; } = ValidateSecurityList_NoRelatedSym_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityListRequest.NoUnderlyingsGroup, int, bool> SecurityListRequest_NoUnderlyings { get; set; } = ValidateSecurityListRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityListRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityListRequest.NoLegsGroup, int, bool> SecurityListRequest_NoLegs { get; set; } = ValidateSecurityListRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatus.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityStatus.NoUnderlyingsGroup, int, bool> SecurityStatus_NoUnderlyings { get; set; } = ValidateSecurityStatus_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatus.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityStatus.NoLegsGroup, int, bool> SecurityStatus_NoLegs { get; set; } = ValidateSecurityStatus_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup, int, bool> SecurityStatusRequest_NoUnderlyings { get; set; } = ValidateSecurityStatusRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.SecurityStatusRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityStatusRequest.NoLegsGroup, int, bool> SecurityStatusRequest_NoLegs { get; set; } = ValidateSecurityStatusRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.SecurityTypes.NoSecurityTypesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SecurityTypes.NoSecurityTypesGroup, int, bool> SecurityTypes_NoSecurityTypes { get; set; } = ValidateSecurityTypes_NoSecurityTypes;

	/// <summary>Holds one entry of FixMessage.SettlementInstructions.NoSettlInstGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.SettlementInstructions.NoSettlInstGroup, int, bool> SettlementInstructions_NoSettlInst { get; set; } = ValidateSettlementInstructions_NoSettlInst;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoUnderlyingsGroup, int, bool> TradeCaptureReport_NoUnderlyings { get; set; } = ValidateTradeCaptureReport_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoLegsGroup, int, bool> TradeCaptureReport_NoLegs { get; set; } = ValidateTradeCaptureReport_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoSidesGroup, int, bool> TradeCaptureReport_NoSides { get; set; } = ValidateTradeCaptureReport_NoSides;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup, int, bool> TradeCaptureReport_NoSides_NoClearingInstructions { get; set; } = ValidateTradeCaptureReport_NoSides_NoClearingInstructions;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup, int, bool> TradeCaptureReport_NoSides_NoContAmts { get; set; } = ValidateTradeCaptureReport_NoSides_NoContAmts;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup, int, bool> TradeCaptureReport_NoSides_NoMiscFees { get; set; } = ValidateTradeCaptureReport_NoSides_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup, int, bool> TradeCaptureReport_NoSides_NoAllocs { get; set; } = ValidateTradeCaptureReport_NoSides_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportAck.NoLegsGroup, int, bool> TradeCaptureReportAck_NoLegs { get; set; } = ValidateTradeCaptureReportAck_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportAck.NoAllocsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportAck.NoAllocsGroup, int, bool> TradeCaptureReportAck_NoAllocs { get; set; } = ValidateTradeCaptureReportAck_NoAllocs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup, int, bool> TradeCaptureReportRequest_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequest_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportRequest.NoLegsGroup, int, bool> TradeCaptureReportRequest_NoLegs { get; set; } = ValidateTradeCaptureReportRequest_NoLegs;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequest.NoDatesGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportRequest.NoDatesGroup, int, bool> TradeCaptureReportRequest_NoDates { get; set; } = ValidateTradeCaptureReportRequest_NoDates;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup, int, bool> TradeCaptureReportRequestAck_NoUnderlyings { get; set; } = ValidateTradeCaptureReportRequestAck_NoUnderlyings;

	/// <summary>Holds one entry of FixMessage.TradeCaptureReportRequestAck.NoLegsGroup to the schema.</summary>
	public Func<FixContext, FixMessage, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup, int, bool> TradeCaptureReportRequestAck_NoLegs { get; set; } = ValidateTradeCaptureReportRequestAck_NoLegs;

	static bool ValidateAdvertisement(FixContext context, FixMessage.Advertisement message)
	{
		if (message.AdvId is null) Missing(message, 2);
		else context.Validators.AdvId(context, message, message.AdvId);
		if (message.AdvTransType is null) Missing(message, 5);
		else context.Validators.AdvTransType(context, message, message.AdvTransType);
		if (message.AdvRefID is not null) context.Validators.AdvRefID(context, message, message.AdvRefID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.Advertisement_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.Advertisement_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.AdvSide is null) Missing(message, 4);
		else context.Validators.AdvSide(context, message, message.AdvSide);
		if (message.Quantity is null) Missing(message, 53);
		else context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.URLLink is not null) context.Validators.URLLink(context, message, message.URLLink);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction(FixContext context, FixMessage.AllocationInstruction message)
	{
		if (message.AllocID is null) Missing(message, 70);
		else context.Validators.AllocID(context, message, message.AllocID);
		if (message.AllocTransType is null) Missing(message, 71);
		else context.Validators.AllocTransType(context, message, message.AllocTransType);
		if (message.AllocType is null) Missing(message, 626);
		else context.Validators.AllocType(context, message, message.AllocType);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, message, message.RefAllocID);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, message, message.AllocCancReplaceReason);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, message, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, message, message.AllocLinkType);
		if (message.BookingRefID is not null) context.Validators.BookingRefID(context, message, message.BookingRefID);
		if (message.AllocNoOrdersType is null) Missing(message, 857);
		else context.Validators.AllocNoOrdersType(context, message, message.AllocNoOrdersType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.AllocationInstruction_NoOrders(context, message, message.NoOrdersGroups[i], i);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.AllocationInstruction_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, message, message.PreviouslyReported);
		if (message.ReversalIndicator is not null) context.Validators.ReversalIndicator(context, message, message.ReversalIndicator);
		if (message.MatchType is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.AllocationInstruction_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.AllocationInstruction_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Quantity is null) Missing(message, 53);
		else context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AvgPx is null) Missing(message, 6);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, message, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, message, message.AvgPxPrecision);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeDate is null) Missing(message, 75);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.Concession is not null) context.Validators.Concession(context, message, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, message, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, message, message.NetMoney);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.AutoAcceptIndicator is not null) context.Validators.AutoAcceptIndicator(context, message, message.AutoAcceptIndicator);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.TotalAccruedInterestAmt is not null) context.Validators.TotalAccruedInterestAmt(context, message, message.TotalAccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, message, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, message, message.LegalConfirm);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, message, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.AllocationInstruction_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAllocationInstructionAck(FixContext context, FixMessage.AllocationInstructionAck message)
	{
		if (message.AllocID is null) Missing(message, 70);
		else context.Validators.AllocID(context, message, message.AllocID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.AllocStatus is null) Missing(message, 87);
		else context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.AllocType is not null) context.Validators.AllocType(context, message, message.AllocType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.Product is not null) context.Validators.Product(context, message, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.AllocationInstructionAck_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAllocationReport(FixContext context, FixMessage.AllocationReport message)
	{
		if (message.AllocReportID is null) Missing(message, 755);
		else context.Validators.AllocReportID(context, message, message.AllocReportID);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.AllocTransType is null) Missing(message, 71);
		else context.Validators.AllocTransType(context, message, message.AllocTransType);
		if (message.AllocReportRefID is not null) context.Validators.AllocReportRefID(context, message, message.AllocReportRefID);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, message, message.AllocCancReplaceReason);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.AllocReportType is null) Missing(message, 794);
		else context.Validators.AllocReportType(context, message, message.AllocReportType);
		if (message.AllocStatus is null) Missing(message, 87);
		else context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, message, message.RefAllocID);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, message, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, message, message.AllocLinkType);
		if (message.BookingRefID is not null) context.Validators.BookingRefID(context, message, message.BookingRefID);
		if (message.AllocNoOrdersType is null) Missing(message, 857);
		else context.Validators.AllocNoOrdersType(context, message, message.AllocNoOrdersType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.AllocationReport_NoOrders(context, message, message.NoOrdersGroups[i], i);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.AllocationReport_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.PreviouslyReported is not null) context.Validators.PreviouslyReported(context, message, message.PreviouslyReported);
		if (message.ReversalIndicator is not null) context.Validators.ReversalIndicator(context, message, message.ReversalIndicator);
		if (message.MatchType is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.AllocationReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.AllocationReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Quantity is null) Missing(message, 53);
		else context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AvgPx is null) Missing(message, 6);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, message, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, message, message.AvgPxPrecision);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeDate is null) Missing(message, 75);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.Concession is not null) context.Validators.Concession(context, message, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, message, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, message, message.NetMoney);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.AutoAcceptIndicator is not null) context.Validators.AutoAcceptIndicator(context, message, message.AutoAcceptIndicator);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.TotalAccruedInterestAmt is not null) context.Validators.TotalAccruedInterestAmt(context, message, message.TotalAccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, message, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, message, message.LegalConfirm);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.TotNoAllocs is not null) context.Validators.TotNoAllocs(context, message, message.TotNoAllocs);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.AllocationReport_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAllocationReportAck(FixContext context, FixMessage.AllocationReportAck message)
	{
		if (message.AllocReportID is null) Missing(message, 755);
		else context.Validators.AllocReportID(context, message, message.AllocReportID);
		if (message.AllocID is null) Missing(message, 70);
		else context.Validators.AllocID(context, message, message.AllocID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.AllocStatus is null) Missing(message, 87);
		else context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.AllocReportType is not null) context.Validators.AllocReportType(context, message, message.AllocReportType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.Product is not null) context.Validators.Product(context, message, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.AllocationReportAck_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAssignmentReport(FixContext context, FixMessage.AssignmentReport message)
	{
		if (message.AsgnRptID is null) Missing(message, 833);
		else context.Validators.AsgnRptID(context, message, message.AsgnRptID);
		if (message.TotNumAssignmentReports is not null) context.Validators.TotNumAssignmentReports(context, message, message.TotNumAssignmentReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, message, message.LastRptRequested);
		if (Empty((IParties)message)) Absent(message, 453);
		else context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.AssignmentReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.AssignmentReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (Empty((IPositionQty)message)) Absent(message, 702);
		else context.Validators.PositionQty(context, message, message);
		if (Empty((IPositionAmountData)message)) Absent(message, 753);
		else context.Validators.PositionAmountData(context, message, message);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, message, message.ThresholdAmount);
		if (message.SettlPrice is null) Missing(message, 730);
		else context.Validators.SettlPrice(context, message, message.SettlPrice);
		if (message.SettlPriceType is null) Missing(message, 731);
		else context.Validators.SettlPriceType(context, message, message.SettlPriceType);
		if (message.UnderlyingSettlPrice is null) Missing(message, 732);
		else context.Validators.UnderlyingSettlPrice(context, message, message.UnderlyingSettlPrice);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.AssignmentMethod is null) Missing(message, 744);
		else context.Validators.AssignmentMethod(context, message, message.AssignmentMethod);
		if (message.AssignmentUnit is not null) context.Validators.AssignmentUnit(context, message, message.AssignmentUnit);
		if (message.OpenInterest is null) Missing(message, 746);
		else context.Validators.OpenInterest(context, message, message.OpenInterest);
		if (message.ExerciseMethod is null) Missing(message, 747);
		else context.Validators.ExerciseMethod(context, message, message.ExerciseMethod);
		if (message.SettlSessID is null) Missing(message, 716);
		else context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is null) Missing(message, 717);
		else context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		else context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateBidRequest(FixContext context, FixMessage.BidRequest message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is null) Missing(message, 391);
		else context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.BidRequestTransType is null) Missing(message, 374);
		else context.Validators.BidRequestTransType(context, message, message.BidRequestTransType);
		if (message.ListName is not null) context.Validators.ListName(context, message, message.ListName);
		if (message.TotNoRelatedSym is null) Missing(message, 393);
		else context.Validators.TotNoRelatedSym(context, message, message.TotNoRelatedSym);
		if (message.BidType is null) Missing(message, 394);
		else context.Validators.BidType(context, message, message.BidType);
		if (message.NumTickets is not null) context.Validators.NumTickets(context, message, message.NumTickets);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.SideValue1 is not null) context.Validators.SideValue1(context, message, message.SideValue1);
		if (message.SideValue2 is not null) context.Validators.SideValue2(context, message, message.SideValue2);
		if (message.NoBidDescriptors is not null) context.Validators.NoBidDescriptors(context, message, message.NoBidDescriptors);
		Counted(message, message.NoBidDescriptors, message.NoBidDescriptorsGroups);
		if (message.NoBidDescriptorsGroups is not null)
			for (var i = 0; i < message.NoBidDescriptorsGroups.Count; i++)
				context.Validators.BidRequest_NoBidDescriptors(context, message, message.NoBidDescriptorsGroups[i], i);
		if (message.NoBidComponents is not null) context.Validators.NoBidComponents(context, message, message.NoBidComponents);
		Counted(message, message.NoBidComponents, message.NoBidComponentsGroups);
		if (message.NoBidComponentsGroups is not null)
			for (var i = 0; i < message.NoBidComponentsGroups.Count; i++)
				context.Validators.BidRequest_NoBidComponents(context, message, message.NoBidComponentsGroups[i], i);
		if (message.LiquidityIndType is not null) context.Validators.LiquidityIndType(context, message, message.LiquidityIndType);
		if (message.WtAverageLiquidity is not null) context.Validators.WtAverageLiquidity(context, message, message.WtAverageLiquidity);
		if (message.ExchangeForPhysical is not null) context.Validators.ExchangeForPhysical(context, message, message.ExchangeForPhysical);
		if (message.OutMainCntryUIndex is not null) context.Validators.OutMainCntryUIndex(context, message, message.OutMainCntryUIndex);
		if (message.CrossPercent is not null) context.Validators.CrossPercent(context, message, message.CrossPercent);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, message, message.ProgRptReqs);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, message, message.ProgPeriodInterval);
		if (message.IncTaxInd is not null) context.Validators.IncTaxInd(context, message, message.IncTaxInd);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.NumBidders is not null) context.Validators.NumBidders(context, message, message.NumBidders);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.BidTradeType is null) Missing(message, 418);
		else context.Validators.BidTradeType(context, message, message.BidTradeType);
		if (message.BasisPxType is null) Missing(message, 419);
		else context.Validators.BasisPxType(context, message, message.BasisPxType);
		if (message.StrikeTime is not null) context.Validators.StrikeTime(context, message, message.StrikeTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateBidResponse(FixContext context, FixMessage.BidResponse message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.NoBidComponents is null) Missing(message, 420);
		else context.Validators.NoBidComponents(context, message, message.NoBidComponents);
		Counted(message, message.NoBidComponents, message.NoBidComponentsGroups);
		if (message.NoBidComponentsGroups is not null)
			for (var i = 0; i < message.NoBidComponentsGroups.Count; i++)
				context.Validators.BidResponse_NoBidComponents(context, message, message.NoBidComponentsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateBusinessMessageReject(FixContext context, FixMessage.BusinessMessageReject message)
	{
		if (message.RefSeqNum is not null) context.Validators.RefSeqNum(context, message, message.RefSeqNum);
		if (message.RefMsgType is null) Missing(message, 372);
		else context.Validators.RefMsgType(context, message, message.RefMsgType);
		if (message.BusinessRejectRefID is not null) context.Validators.BusinessRejectRefID(context, message, message.BusinessRejectRefID);
		if (message.BusinessRejectReason is null) Missing(message, 380);
		else context.Validators.BusinessRejectReason(context, message, message.BusinessRejectReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment(FixContext context, FixMessage.CollateralAssignment message)
	{
		if (message.CollAsgnID is null) Missing(message, 902);
		else context.Validators.CollAsgnID(context, message, message.CollAsgnID);
		if (message.CollReqID is not null) context.Validators.CollReqID(context, message, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(message, 895);
		else context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.CollAsgnTransType is null) Missing(message, 903);
		else context.Validators.CollAsgnTransType(context, message, message.CollAsgnTransType);
		if (message.CollAsgnRefID is not null) context.Validators.CollAsgnRefID(context, message, message.CollAsgnRefID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralAssignment_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralAssignment_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralAssignment_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralAssignment_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, message, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, message, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, message, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.CollateralAssignment_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry(FixContext context, FixMessage.CollateralInquiry message)
	{
		if (message.CollInquiryID is not null) context.Validators.CollInquiryID(context, message, message.CollInquiryID);
		if (message.NoCollInquiryQualifier is not null) context.Validators.NoCollInquiryQualifier(context, message, message.NoCollInquiryQualifier);
		Counted(message, message.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups);
		if (message.NoCollInquiryQualifierGroups is not null)
			for (var i = 0; i < message.NoCollInquiryQualifierGroups.Count; i++)
				context.Validators.CollateralInquiry_NoCollInquiryQualifier(context, message, message.NoCollInquiryQualifierGroups[i], i);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralInquiry_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralInquiry_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralInquiry_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralInquiry_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, message, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, message, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, message, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck(FixContext context, FixMessage.CollateralInquiryAck message)
	{
		if (message.CollInquiryID is null) Missing(message, 909);
		else context.Validators.CollInquiryID(context, message, message.CollInquiryID);
		if (message.CollInquiryStatus is null) Missing(message, 945);
		else context.Validators.CollInquiryStatus(context, message, message.CollInquiryStatus);
		if (message.CollInquiryResult is not null) context.Validators.CollInquiryResult(context, message, message.CollInquiryResult);
		if (message.NoCollInquiryQualifier is not null) context.Validators.NoCollInquiryQualifier(context, message, message.NoCollInquiryQualifier);
		Counted(message, message.NoCollInquiryQualifier, message.NoCollInquiryQualifierGroups);
		if (message.NoCollInquiryQualifierGroups is not null)
			for (var i = 0; i < message.NoCollInquiryQualifierGroups.Count; i++)
				context.Validators.CollateralInquiryAck_NoCollInquiryQualifier(context, message, message.NoCollInquiryQualifierGroups[i], i);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, message, message.TotNumReports);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralInquiryAck_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralInquiryAck_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralInquiryAck_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralInquiryAck_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralReport(FixContext context, FixMessage.CollateralReport message)
	{
		if (message.CollRptID is null) Missing(message, 908);
		else context.Validators.CollRptID(context, message, message.CollRptID);
		if (message.CollInquiryID is not null) context.Validators.CollInquiryID(context, message, message.CollInquiryID);
		if (message.CollStatus is null) Missing(message, 910);
		else context.Validators.CollStatus(context, message, message.CollStatus);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, message, message.TotNumReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, message, message.LastRptRequested);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralReport_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralReport_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, message, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, message, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, message, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.CollateralReport_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest(FixContext context, FixMessage.CollateralRequest message)
	{
		if (message.CollReqID is null) Missing(message, 894);
		else context.Validators.CollReqID(context, message, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(message, 895);
		else context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralRequest_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralRequest_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, message, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, message, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, message, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.CollateralRequest_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse(FixContext context, FixMessage.CollateralResponse message)
	{
		if (message.CollRespID is null) Missing(message, 904);
		else context.Validators.CollRespID(context, message, message.CollRespID);
		if (message.CollAsgnID is null) Missing(message, 902);
		else context.Validators.CollAsgnID(context, message, message.CollAsgnID);
		if (message.CollReqID is not null) context.Validators.CollReqID(context, message, message.CollReqID);
		if (message.CollAsgnReason is null) Missing(message, 895);
		else context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.CollAsgnTransType is not null) context.Validators.CollAsgnTransType(context, message, message.CollAsgnTransType);
		if (message.CollAsgnRespType is null) Missing(message, 905);
		else context.Validators.CollAsgnRespType(context, message, message.CollAsgnRespType);
		if (message.CollAsgnRejectReason is not null) context.Validators.CollAsgnRejectReason(context, message, message.CollAsgnRejectReason);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.CollateralResponse_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.NoTrades is not null) context.Validators.NoTrades(context, message, message.NoTrades);
		Counted(message, message.NoTrades, message.NoTradesGroups);
		if (message.NoTradesGroups is not null)
			for (var i = 0; i < message.NoTradesGroups.Count; i++)
				context.Validators.CollateralResponse_NoTrades(context, message, message.NoTradesGroups[i], i);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.Quantity is not null) context.Validators.Quantity(context, message, message.Quantity);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CollateralResponse_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CollateralResponse_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.MarginExcess is not null) context.Validators.MarginExcess(context, message, message.MarginExcess);
		if (message.TotalNetValue is not null) context.Validators.TotalNetValue(context, message, message.TotalNetValue);
		if (message.CashOutstanding is not null) context.Validators.CashOutstanding(context, message, message.CashOutstanding);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.CollateralResponse_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateConfirmation(FixContext context, FixMessage.Confirmation message)
	{
		if (message.ConfirmID is null) Missing(message, 664);
		else context.Validators.ConfirmID(context, message, message.ConfirmID);
		if (message.ConfirmRefID is not null) context.Validators.ConfirmRefID(context, message, message.ConfirmRefID);
		if (message.ConfirmReqID is not null) context.Validators.ConfirmReqID(context, message, message.ConfirmReqID);
		if (message.ConfirmTransType is null) Missing(message, 666);
		else context.Validators.ConfirmTransType(context, message, message.ConfirmTransType);
		if (message.ConfirmType is null) Missing(message, 773);
		else context.Validators.ConfirmType(context, message, message.ConfirmType);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, message, message.CopyMsgIndicator);
		if (message.LegalConfirm is not null) context.Validators.LegalConfirm(context, message, message.LegalConfirm);
		if (message.ConfirmStatus is null) Missing(message, 665);
		else context.Validators.ConfirmStatus(context, message, message.ConfirmStatus);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.Confirmation_NoOrders(context, message, message.NoOrdersGroups[i], i);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, message.IndividualAllocID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.TradeDate is null) Missing(message, 75);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is null) Missing(message, 711);
		else context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.Confirmation_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is null) Missing(message, 555);
		else context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.Confirmation_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.AllocQty is null) Missing(message, 80);
		else context.Validators.AllocQty(context, message, message.AllocQty);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.NoCapacities is null) Missing(message, 862);
		else context.Validators.NoCapacities(context, message, message.NoCapacities);
		Counted(message, message.NoCapacities, message.NoCapacitiesGroups);
		if (message.NoCapacitiesGroups is not null)
			for (var i = 0; i < message.NoCapacitiesGroups.Count; i++)
				context.Validators.Confirmation_NoCapacities(context, message, message.NoCapacitiesGroups[i], i);
		if (message.AllocAccount is null) Missing(message, 79);
		else context.Validators.AllocAccount(context, message, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, message.AllocAcctIDSource);
		if (message.AllocAccountType is not null) context.Validators.AllocAccountType(context, message, message.AllocAccountType);
		if (message.AvgPx is null) Missing(message, 6);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.AvgPxPrecision is not null) context.Validators.AvgPxPrecision(context, message, message.AvgPxPrecision);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.AvgParPx is not null) context.Validators.AvgParPx(context, message, message.AvgParPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (message.ReportedPx is not null) context.Validators.ReportedPx(context, message, message.ReportedPx);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.GrossTradeAmt is null) Missing(message, 381);
		else context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, message.NumDaysInterest);
		if (message.ExDate is not null) context.Validators.ExDate(context, message, message.ExDate);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, message, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (message.Concession is not null) context.Validators.Concession(context, message, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, message, message.TotalTakedown);
		if (message.NetMoney is null) Missing(message, 118);
		else context.Validators.NetMoney(context, message, message.NetMoney);
		if (message.MaturityNetMoney is not null) context.Validators.MaturityNetMoney(context, message, message.MaturityNetMoney);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (message.SharedCommission is not null) context.Validators.SharedCommission(context, message, message.SharedCommission);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.Confirmation_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateConfirmationAck(FixContext context, FixMessage.ConfirmationAck message)
	{
		if (message.ConfirmID is null) Missing(message, 664);
		else context.Validators.ConfirmID(context, message, message.ConfirmID);
		if (message.TradeDate is null) Missing(message, 75);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.AffirmStatus is null) Missing(message, 940);
		else context.Validators.AffirmStatus(context, message, message.AffirmStatus);
		if (message.ConfirmRejReason is not null) context.Validators.ConfirmRejReason(context, message, message.ConfirmRejReason);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateConfirmationRequest(FixContext context, FixMessage.ConfirmationRequest message)
	{
		if (message.ConfirmReqID is null) Missing(message, 859);
		else context.Validators.ConfirmReqID(context, message, message.ConfirmReqID);
		if (message.ConfirmType is null) Missing(message, 773);
		else context.Validators.ConfirmType(context, message, message.ConfirmType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.ConfirmationRequest_NoOrders(context, message, message.NoOrdersGroups[i], i);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.SecondaryAllocID is not null) context.Validators.SecondaryAllocID(context, message, message.SecondaryAllocID);
		if (message.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, message.IndividualAllocID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.AllocAccount is not null) context.Validators.AllocAccount(context, message, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, message.AllocAcctIDSource);
		if (message.AllocAccountType is not null) context.Validators.AllocAccountType(context, message, message.AllocAccountType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest(FixContext context, FixMessage.CrossOrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.CrossID is null) Missing(message, 548);
		else context.Validators.CrossID(context, message, message.CrossID);
		if (message.OrigCrossID is null) Missing(message, 551);
		else context.Validators.OrigCrossID(context, message, message.OrigCrossID);
		if (message.CrossType is null) Missing(message, 549);
		else context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization is null) Missing(message, 550);
		else context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides is null) Missing(message, 552);
		else context.Validators.NoSides(context, message, message.NoSides);
		Counted(message, message.NoSides, message.NoSidesGroups);
		if (message.NoSidesGroups is not null)
			for (var i = 0; i < message.NoSidesGroups.Count; i++)
				context.Validators.CrossOrderCancelReplaceRequest_NoSides(context, message, message.NoSidesGroups[i], i);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CrossOrderCancelReplaceRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CrossOrderCancelReplaceRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.CrossOrderCancelReplaceRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelRequest(FixContext context, FixMessage.CrossOrderCancelRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.CrossID is null) Missing(message, 548);
		else context.Validators.CrossID(context, message, message.CrossID);
		if (message.OrigCrossID is null) Missing(message, 551);
		else context.Validators.OrigCrossID(context, message, message.OrigCrossID);
		if (message.CrossType is null) Missing(message, 549);
		else context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization is null) Missing(message, 550);
		else context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides is null) Missing(message, 552);
		else context.Validators.NoSides(context, message, message.NoSides);
		Counted(message, message.NoSides, message.NoSidesGroups);
		if (message.NoSidesGroups is not null)
			for (var i = 0; i < message.NoSidesGroups.Count; i++)
				context.Validators.CrossOrderCancelRequest_NoSides(context, message, message.NoSidesGroups[i], i);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.CrossOrderCancelRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.CrossOrderCancelRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityList(FixContext context, FixMessage.DerivativeSecurityList message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(message, 322);
		else context.Validators.SecurityResponseID(context, message, message.SecurityResponseID);
		if (message.SecurityRequestResult is null) Missing(message, 560);
		else context.Validators.SecurityRequestResult(context, message, message.SecurityRequestResult);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, message, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.DerivativeSecurityList_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityListRequest(FixContext context, FixMessage.DerivativeSecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(message, 559);
		else context.Validators.SecurityListRequestType(context, message, message.SecurityListRequestType);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, message, message.SecuritySubType);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateDontKnowTrade(FixContext context, FixMessage.DontKnowTrade message)
	{
		if (message.OrderID is null) Missing(message, 37);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.ExecID is null) Missing(message, 17);
		else context.Validators.ExecID(context, message, message.ExecID);
		if (message.DKReason is null) Missing(message, 127);
		else context.Validators.DKReason(context, message, message.DKReason);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.DontKnowTrade_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.DontKnowTrade_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.LastQty is not null) context.Validators.LastQty(context, message, message.LastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, message, message.LastPx);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateEmail(FixContext context, FixMessage.Email message)
	{
		if (message.EmailThreadID is null) Missing(message, 164);
		else context.Validators.EmailThreadID(context, message, message.EmailThreadID);
		if (message.EmailType is null) Missing(message, 94);
		else context.Validators.EmailType(context, message, message.EmailType);
		if (message.OrigTime is not null) context.Validators.OrigTime(context, message, message.OrigTime);
		if (message.Subject is null) Missing(message, 147);
		else context.Validators.Subject(context, message, message.Subject);
		if (message.EncodedSubjectLen is not null) context.Validators.EncodedSubjectLen(context, message, message.EncodedSubjectLen);
		if (message.EncodedSubject is not null) context.Validators.EncodedSubject(context, message, message.EncodedSubject);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, message, message.NoRoutingIDs);
		Counted(message, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		if (message.NoRoutingIDsGroups is not null)
			for (var i = 0; i < message.NoRoutingIDsGroups.Count; i++)
				context.Validators.Email_NoRoutingIDs(context, message, message.NoRoutingIDsGroups[i], i);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.Email_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.Email_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.Email_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.LinesOfText is null) Missing(message, 33);
		else context.Validators.LinesOfText(context, message, message.LinesOfText);
		Counted(message, message.LinesOfText, message.LinesOfTextGroups);
		if (message.LinesOfTextGroups is not null)
			for (var i = 0; i < message.LinesOfTextGroups.Count; i++)
				context.Validators.Email_LinesOfText(context, message, message.LinesOfTextGroups[i], i);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);

		return message.IsValid;
	}

	static bool ValidateExecutionReport(FixContext context, FixMessage.ExecutionReport message)
	{
		if (message.OrderID is null) Missing(message, 37);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, message, message.SecondaryExecID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, message, message.QuoteRespID);
		if (message.OrdStatusReqID is not null) context.Validators.OrdStatusReqID(context, message, message.OrdStatusReqID);
		if (message.MassStatusReqID is not null) context.Validators.MassStatusReqID(context, message, message.MassStatusReqID);
		if (message.TotNumReports is not null) context.Validators.TotNumReports(context, message, message.TotNumReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, message, message.LastRptRequested);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.NoContraBrokers is not null) context.Validators.NoContraBrokers(context, message, message.NoContraBrokers);
		Counted(message, message.NoContraBrokers, message.NoContraBrokersGroups);
		if (message.NoContraBrokersGroups is not null)
			for (var i = 0; i < message.NoContraBrokersGroups.Count; i++)
				context.Validators.ExecutionReport_NoContraBrokers(context, message, message.NoContraBrokersGroups[i], i);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.CrossID is not null) context.Validators.CrossID(context, message, message.CrossID);
		if (message.OrigCrossID is not null) context.Validators.OrigCrossID(context, message, message.OrigCrossID);
		if (message.CrossType is not null) context.Validators.CrossType(context, message, message.CrossType);
		if (message.ExecID is null) Missing(message, 17);
		else context.Validators.ExecID(context, message, message.ExecID);
		if (message.ExecRefID is not null) context.Validators.ExecRefID(context, message, message.ExecRefID);
		if (message.ExecType is null) Missing(message, 150);
		else context.Validators.ExecType(context, message, message.ExecType);
		if (message.OrdStatus is null) Missing(message, 39);
		else context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, message, message.WorkingIndicator);
		if (message.OrdRejReason is not null) context.Validators.OrdRejReason(context, message, message.OrdRejReason);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, message, message.ExecRestatementReason);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.ExecutionReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.PeggedPrice is not null) context.Validators.PeggedPrice(context, message, message.PeggedPrice);
		if (message.DiscretionPrice is not null) context.Validators.DiscretionPrice(context, message, message.DiscretionPrice);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.TargetStrategyPerformance is not null) context.Validators.TargetStrategyPerformance(context, message, message.TargetStrategyPerformance);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.LastQty is not null) context.Validators.LastQty(context, message, message.LastQty);
		if (message.UnderlyingLastQty is not null) context.Validators.UnderlyingLastQty(context, message, message.UnderlyingLastQty);
		if (message.LastPx is not null) context.Validators.LastPx(context, message, message.LastPx);
		if (message.UnderlyingLastPx is not null) context.Validators.UnderlyingLastPx(context, message, message.UnderlyingLastPx);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, message, message.LastParPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, message, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, message, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.TimeBracket is not null) context.Validators.TimeBracket(context, message, message.TimeBracket);
		if (message.LastCapacity is not null) context.Validators.LastCapacity(context, message, message.LastCapacity);
		if (message.LeavesQty is null) Missing(message, 151);
		else context.Validators.LeavesQty(context, message, message.LeavesQty);
		if (message.CumQty is null) Missing(message, 14);
		else context.Validators.CumQty(context, message, message.CumQty);
		if (message.AvgPx is null) Missing(message, 6);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.DayOrderQty is not null) context.Validators.DayOrderQty(context, message, message.DayOrderQty);
		if (message.DayCumQty is not null) context.Validators.DayCumQty(context, message, message.DayCumQty);
		if (message.DayAvgPx is not null) context.Validators.DayAvgPx(context, message, message.DayAvgPx);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ReportToExch is not null) context.Validators.ReportToExch(context, message, message.ReportToExch);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, message.NumDaysInterest);
		if (message.ExDate is not null) context.Validators.ExDate(context, message, message.ExDate);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, message.AccruedInterestRate);
		if (message.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, message.AccruedInterestAmt);
		if (message.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, message, message.InterestAtMaturity);
		if (message.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, message.EndAccruedInterestAmt);
		if (message.StartCash is not null) context.Validators.StartCash(context, message, message.StartCash);
		if (message.EndCash is not null) context.Validators.EndCash(context, message, message.EndCash);
		if (message.TradedFlatSwitch is not null) context.Validators.TradedFlatSwitch(context, message, message.TradedFlatSwitch);
		if (message.BasisFeatureDate is not null) context.Validators.BasisFeatureDate(context, message, message.BasisFeatureDate);
		if (message.BasisFeaturePrice is not null) context.Validators.BasisFeaturePrice(context, message, message.BasisFeaturePrice);
		if (message.Concession is not null) context.Validators.Concession(context, message, message.Concession);
		if (message.TotalTakedown is not null) context.Validators.TotalTakedown(context, message, message.TotalTakedown);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, message, message.NetMoney);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.LastForwardPoints2 is not null) context.Validators.LastForwardPoints2(context, message, message.LastForwardPoints2);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);
		if (message.TransBkdTime is not null) context.Validators.TransBkdTime(context, message, message.TransBkdTime);
		if (message.ExecValuationPoint is not null) context.Validators.ExecValuationPoint(context, message, message.ExecValuationPoint);
		if (message.ExecPriceType is not null) context.Validators.ExecPriceType(context, message, message.ExecPriceType);
		if (message.ExecPriceAdjustment is not null) context.Validators.ExecPriceAdjustment(context, message, message.ExecPriceAdjustment);
		if (message.PriorityIndicator is not null) context.Validators.PriorityIndicator(context, message, message.PriorityIndicator);
		if (message.PriceImprovement is not null) context.Validators.PriceImprovement(context, message, message.PriceImprovement);
		if (message.LastLiquidityInd is not null) context.Validators.LastLiquidityInd(context, message, message.LastLiquidityInd);
		if (message.NoContAmts is not null) context.Validators.NoContAmts(context, message, message.NoContAmts);
		Counted(message, message.NoContAmts, message.NoContAmtsGroups);
		if (message.NoContAmtsGroups is not null)
			for (var i = 0; i < message.NoContAmtsGroups.Count; i++)
				context.Validators.ExecutionReport_NoContAmts(context, message, message.NoContAmtsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.ExecutionReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, message, message.CopyMsgIndicator);
		if (message.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, message.NoMiscFees);
		Counted(message, message.NoMiscFees, message.NoMiscFeesGroups);
		if (message.NoMiscFeesGroups is not null)
			for (var i = 0; i < message.NoMiscFeesGroups.Count; i++)
				context.Validators.ExecutionReport_NoMiscFees(context, message, message.NoMiscFeesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateHeartbeat(FixContext context, FixMessage.Heartbeat message)
	{
		if (message.TestReqID is not null) context.Validators.TestReqID(context, message, message.TestReqID);

		return message.IsValid;
	}

	static bool ValidateIndicationOfInterest(FixContext context, FixMessage.IndicationOfInterest message)
	{
		if (message.IOIid is null) Missing(message, 23);
		else context.Validators.IOIid(context, message, message.IOIid);
		if (message.IOITransType is null) Missing(message, 28);
		else context.Validators.IOITransType(context, message, message.IOITransType);
		if (message.IOIRefID is not null) context.Validators.IOIRefID(context, message, message.IOIRefID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.IndicationOfInterest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.IOIQty is null) Missing(message, 27);
		else context.Validators.IOIQty(context, message, message.IOIQty);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.IndicationOfInterest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.IOIQltyInd is not null) context.Validators.IOIQltyInd(context, message, message.IOIQltyInd);
		if (message.IOINaturalFlag is not null) context.Validators.IOINaturalFlag(context, message, message.IOINaturalFlag);
		if (message.NoIOIQualifiers is not null) context.Validators.NoIOIQualifiers(context, message, message.NoIOIQualifiers);
		Counted(message, message.NoIOIQualifiers, message.NoIOIQualifiersGroups);
		if (message.NoIOIQualifiersGroups is not null)
			for (var i = 0; i < message.NoIOIQualifiersGroups.Count; i++)
				context.Validators.IndicationOfInterest_NoIOIQualifiers(context, message, message.NoIOIQualifiersGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.URLLink is not null) context.Validators.URLLink(context, message, message.URLLink);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, message, message.NoRoutingIDs);
		Counted(message, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		if (message.NoRoutingIDsGroups is not null)
			for (var i = 0; i < message.NoRoutingIDsGroups.Count; i++)
				context.Validators.IndicationOfInterest_NoRoutingIDs(context, message, message.NoRoutingIDsGroups[i], i);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		return message.IsValid;
	}

	static bool ValidateListCancelRequest(FixContext context, FixMessage.ListCancelRequest message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListExecute(FixContext context, FixMessage.ListExecute message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStatus(FixContext context, FixMessage.ListStatus message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.ListStatusType is null) Missing(message, 429);
		else context.Validators.ListStatusType(context, message, message.ListStatusType);
		if (message.NoRpts is null) Missing(message, 82);
		else context.Validators.NoRpts(context, message, message.NoRpts);
		if (message.ListOrderStatus is null) Missing(message, 431);
		else context.Validators.ListOrderStatus(context, message, message.ListOrderStatus);
		if (message.RptSeq is null) Missing(message, 83);
		else context.Validators.RptSeq(context, message, message.RptSeq);
		if (message.ListStatusText is not null) context.Validators.ListStatusText(context, message, message.ListStatusText);
		if (message.EncodedListStatusTextLen is not null) context.Validators.EncodedListStatusTextLen(context, message, message.EncodedListStatusTextLen);
		if (message.EncodedListStatusText is not null) context.Validators.EncodedListStatusText(context, message, message.EncodedListStatusText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.TotNoOrders is null) Missing(message, 68);
		else context.Validators.TotNoOrders(context, message, message.TotNoOrders);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoOrders is null) Missing(message, 73);
		else context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.ListStatus_NoOrders(context, message, message.NoOrdersGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateListStatusRequest(FixContext context, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice(FixContext context, FixMessage.ListStrikePrice message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.TotNoStrikes is null) Missing(message, 422);
		else context.Validators.TotNoStrikes(context, message, message.TotNoStrikes);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoStrikes is null) Missing(message, 428);
		else context.Validators.NoStrikes(context, message, message.NoStrikes);
		Counted(message, message.NoStrikes, message.NoStrikesGroups);
		if (message.NoStrikesGroups is not null)
			for (var i = 0; i < message.NoStrikesGroups.Count; i++)
				context.Validators.ListStrikePrice_NoStrikes(context, message, message.NoStrikesGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.ListStrikePrice_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateLogon(FixContext context, FixMessage.Logon message)
	{
		if (message.EncryptMethod is null) Missing(message, 98);
		else context.Validators.EncryptMethod(context, message, message.EncryptMethod);
		if (message.HeartBtInt is null) Missing(message, 108);
		else context.Validators.HeartBtInt(context, message, message.HeartBtInt);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);
		if (message.ResetSeqNumFlag is not null) context.Validators.ResetSeqNumFlag(context, message, message.ResetSeqNumFlag);
		if (message.NextExpectedMsgSeqNum is not null) context.Validators.NextExpectedMsgSeqNum(context, message, message.NextExpectedMsgSeqNum);
		if (message.MaxMessageSize is not null) context.Validators.MaxMessageSize(context, message, message.MaxMessageSize);
		if (message.NoMsgTypes is not null) context.Validators.NoMsgTypes(context, message, message.NoMsgTypes);
		Counted(message, message.NoMsgTypes, message.NoMsgTypesGroups);
		if (message.NoMsgTypesGroups is not null)
			for (var i = 0; i < message.NoMsgTypesGroups.Count; i++)
				context.Validators.Logon_NoMsgTypes(context, message, message.NoMsgTypesGroups[i], i);
		if (message.TestMessageIndicator is not null) context.Validators.TestMessageIndicator(context, message, message.TestMessageIndicator);
		if (message.Username is not null) context.Validators.Username(context, message, message.Username);
		if (message.Password is not null) context.Validators.Password(context, message, message.Password);

		return message.IsValid;
	}

	static bool ValidateLogout(FixContext context, FixMessage.Logout message)
	{
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh(FixContext context, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.NoMDEntries is null) Missing(message, 268);
		else context.Validators.NoMDEntries(context, message, message.NoMDEntries);
		Counted(message, message.NoMDEntries, message.NoMDEntriesGroups);
		if (message.NoMDEntriesGroups is not null)
			for (var i = 0; i < message.NoMDEntriesGroups.Count; i++)
				context.Validators.MarketDataIncrementalRefresh_NoMDEntries(context, message, message.NoMDEntriesGroups[i], i);
		if (message.ApplQueueDepth is not null) context.Validators.ApplQueueDepth(context, message, message.ApplQueueDepth);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, message, message.ApplQueueResolution);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest(FixContext context, FixMessage.MarketDataRequest message)
	{
		if (message.MDReqID is null) Missing(message, 262);
		else context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.MarketDepth is null) Missing(message, 264);
		else context.Validators.MarketDepth(context, message, message.MarketDepth);
		if (message.MDUpdateType is not null) context.Validators.MDUpdateType(context, message, message.MDUpdateType);
		if (message.AggregatedBook is not null) context.Validators.AggregatedBook(context, message, message.AggregatedBook);
		if (message.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, message, message.OpenCloseSettlFlag);
		if (message.Scope is not null) context.Validators.Scope(context, message, message.Scope);
		if (message.MDImplicitDelete is not null) context.Validators.MDImplicitDelete(context, message, message.MDImplicitDelete);
		if (message.NoMDEntryTypes is null) Missing(message, 267);
		else context.Validators.NoMDEntryTypes(context, message, message.NoMDEntryTypes);
		Counted(message, message.NoMDEntryTypes, message.NoMDEntryTypesGroups);
		if (message.NoMDEntryTypesGroups is not null)
			for (var i = 0; i < message.NoMDEntryTypesGroups.Count; i++)
				context.Validators.MarketDataRequest_NoMDEntryTypes(context, message, message.NoMDEntryTypesGroups[i], i);
		if (message.NoRelatedSym is null) Missing(message, 146);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.MarketDataRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.MarketDataRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ApplQueueAction is not null) context.Validators.ApplQueueAction(context, message, message.ApplQueueAction);
		if (message.ApplQueueMax is not null) context.Validators.ApplQueueMax(context, message, message.ApplQueueMax);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequestReject(FixContext context, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(message, 262);
		else context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, message, message.MDReqRejReason);
		if (message.NoAltMDSource is not null) context.Validators.NoAltMDSource(context, message, message.NoAltMDSource);
		Counted(message, message.NoAltMDSource, message.NoAltMDSourceGroups);
		if (message.NoAltMDSourceGroups is not null)
			for (var i = 0; i < message.NoAltMDSourceGroups.Count; i++)
				context.Validators.MarketDataRequestReject_NoAltMDSource(context, message, message.NoAltMDSourceGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh(FixContext context, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, message, message.MDReqID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.MarketDataSnapshotFullRefresh_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.MarketDataSnapshotFullRefresh_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, message, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, message, message.CorporateAction);
		if (message.NetChgPrevDay is not null) context.Validators.NetChgPrevDay(context, message, message.NetChgPrevDay);
		if (message.NoMDEntries is null) Missing(message, 268);
		else context.Validators.NoMDEntries(context, message, message.NoMDEntries);
		Counted(message, message.NoMDEntries, message.NoMDEntriesGroups);
		if (message.NoMDEntriesGroups is not null)
			for (var i = 0; i < message.NoMDEntriesGroups.Count; i++)
				context.Validators.MarketDataSnapshotFullRefresh_NoMDEntries(context, message, message.NoMDEntriesGroups[i], i);
		if (message.ApplQueueDepth is not null) context.Validators.ApplQueueDepth(context, message, message.ApplQueueDepth);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, message, message.ApplQueueResolution);

		return message.IsValid;
	}

	static bool ValidateMassQuote(FixContext context, FixMessage.MassQuote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, 117);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DefBidSize is not null) context.Validators.DefBidSize(context, message, message.DefBidSize);
		if (message.DefOfferSize is not null) context.Validators.DefOfferSize(context, message, message.DefOfferSize);
		if (message.NoQuoteSets is null) Missing(message, 296);
		else context.Validators.NoQuoteSets(context, message, message.NoQuoteSets);
		Counted(message, message.NoQuoteSets, message.NoQuoteSetsGroups);
		if (message.NoQuoteSetsGroups is not null)
			for (var i = 0; i < message.NoQuoteSetsGroups.Count; i++)
				context.Validators.MassQuote_NoQuoteSets(context, message, message.NoQuoteSetsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement(FixContext context, FixMessage.MassQuoteAcknowledgement message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteStatus is null) Missing(message, 297);
		else context.Validators.QuoteStatus(context, message, message.QuoteStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, message, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoQuoteSets is not null) context.Validators.NoQuoteSets(context, message, message.NoQuoteSets);
		Counted(message, message.NoQuoteSets, message.NoQuoteSetsGroups);
		if (message.NoQuoteSetsGroups is not null)
			for (var i = 0; i < message.NoQuoteSetsGroups.Count; i++)
				context.Validators.MassQuoteAcknowledgement_NoQuoteSets(context, message, message.NoQuoteSetsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest(FixContext context, FixMessage.MultilegOrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.OrigClOrdID is null) Missing(message, 41);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, message, message.OrigOrdModTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.MultilegOrderCancelReplaceRequest_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.MultilegOrderCancelReplaceRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.MultilegOrderCancelReplaceRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.NoLegs is null) Missing(message, 555);
		else context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.MultilegOrderCancelReplaceRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, message, message.MultiLegRptTypeReq);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusRequest(FixContext context, FixMessage.NetworkStatusRequest message)
	{
		if (message.NetworkRequestType is null) Missing(message, 935);
		else context.Validators.NetworkRequestType(context, message, message.NetworkRequestType);
		if (message.NetworkRequestID is null) Missing(message, 933);
		else context.Validators.NetworkRequestID(context, message, message.NetworkRequestID);
		if (message.NoCompIDs is not null) context.Validators.NoCompIDs(context, message, message.NoCompIDs);
		Counted(message, message.NoCompIDs, message.NoCompIDsGroups);
		if (message.NoCompIDsGroups is not null)
			for (var i = 0; i < message.NoCompIDsGroups.Count; i++)
				context.Validators.NetworkStatusRequest_NoCompIDs(context, message, message.NoCompIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponse(FixContext context, FixMessage.NetworkStatusResponse message)
	{
		if (message.NetworkStatusResponseType is null) Missing(message, 937);
		else context.Validators.NetworkStatusResponseType(context, message, message.NetworkStatusResponseType);
		if (message.NetworkRequestID is not null) context.Validators.NetworkRequestID(context, message, message.NetworkRequestID);
		if (message.NetworkResponseID is null) Missing(message, 932);
		else context.Validators.NetworkResponseID(context, message, message.NetworkResponseID);
		if (message.LastNetworkResponseID is not null) context.Validators.LastNetworkResponseID(context, message, message.LastNetworkResponseID);
		if (message.NoCompIDs is null) Missing(message, 936);
		else context.Validators.NoCompIDs(context, message, message.NoCompIDs);
		Counted(message, message.NoCompIDs, message.NoCompIDsGroups);
		if (message.NoCompIDsGroups is not null)
			for (var i = 0; i < message.NoCompIDsGroups.Count; i++)
				context.Validators.NetworkStatusResponse_NoCompIDs(context, message, message.NoCompIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross(FixContext context, FixMessage.NewOrderCross message)
	{
		if (message.CrossID is null) Missing(message, 548);
		else context.Validators.CrossID(context, message, message.CrossID);
		if (message.CrossType is null) Missing(message, 549);
		else context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization is null) Missing(message, 550);
		else context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides is null) Missing(message, 552);
		else context.Validators.NoSides(context, message, message.NoSides);
		Counted(message, message.NoSides, message.NoSidesGroups);
		if (message.NoSidesGroups is not null)
			for (var i = 0; i < message.NoSidesGroups.Count; i++)
				context.Validators.NewOrderCross_NoSides(context, message, message.NoSidesGroups[i], i);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.NewOrderCross_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.NewOrderCross_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.NewOrderCross_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);

		return message.IsValid;
	}

	static bool ValidateNewOrderList(FixContext context, FixMessage.NewOrderList message)
	{
		if (message.ListID is null) Missing(message, 66);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, message, message.ProgRptReqs);
		if (message.BidType is null) Missing(message, 394);
		else context.Validators.BidType(context, message, message.BidType);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, message, message.ProgPeriodInterval);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.ListExecInstType is not null) context.Validators.ListExecInstType(context, message, message.ListExecInstType);
		if (message.ListExecInst is not null) context.Validators.ListExecInst(context, message, message.ListExecInst);
		if (message.EncodedListExecInstLen is not null) context.Validators.EncodedListExecInstLen(context, message, message.EncodedListExecInstLen);
		if (message.EncodedListExecInst is not null) context.Validators.EncodedListExecInst(context, message, message.EncodedListExecInst);
		if (message.AllowableOneSidednessPct is not null) context.Validators.AllowableOneSidednessPct(context, message, message.AllowableOneSidednessPct);
		if (message.AllowableOneSidednessValue is not null) context.Validators.AllowableOneSidednessValue(context, message, message.AllowableOneSidednessValue);
		if (message.AllowableOneSidednessCurr is not null) context.Validators.AllowableOneSidednessCurr(context, message, message.AllowableOneSidednessCurr);
		if (message.TotNoOrders is null) Missing(message, 68);
		else context.Validators.TotNoOrders(context, message, message.TotNoOrders);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoOrders is null) Missing(message, 73);
		else context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders(context, message, message.NoOrdersGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg(FixContext context, FixMessage.NewOrderMultileg message)
	{
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderMultileg_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.NewOrderMultileg_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.NewOrderMultileg_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.NoLegs is null) Missing(message, 555);
		else context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.NewOrderMultileg_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);
		if (message.MultiLegRptTypeReq is not null) context.Validators.MultiLegRptTypeReq(context, message, message.MultiLegRptTypeReq);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle(FixContext context, FixMessage.NewOrderSingle message)
	{
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderSingle_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.NewOrderSingle_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.NewOrderSingle_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Price2 is not null) context.Validators.Price2(context, message, message.Price2);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);

		return message.IsValid;
	}

	static bool ValidateNews(FixContext context, FixMessage.News message)
	{
		if (message.OrigTime is not null) context.Validators.OrigTime(context, message, message.OrigTime);
		if (message.Urgency is not null) context.Validators.Urgency(context, message, message.Urgency);
		if (message.Headline is null) Missing(message, 148);
		else context.Validators.Headline(context, message, message.Headline);
		if (message.EncodedHeadlineLen is not null) context.Validators.EncodedHeadlineLen(context, message, message.EncodedHeadlineLen);
		if (message.EncodedHeadline is not null) context.Validators.EncodedHeadline(context, message, message.EncodedHeadline);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, message, message.NoRoutingIDs);
		Counted(message, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		if (message.NoRoutingIDsGroups is not null)
			for (var i = 0; i < message.NoRoutingIDsGroups.Count; i++)
				context.Validators.News_NoRoutingIDs(context, message, message.NoRoutingIDsGroups[i], i);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.News_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.News_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.News_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.LinesOfText is null) Missing(message, 33);
		else context.Validators.LinesOfText(context, message, message.LinesOfText);
		Counted(message, message.LinesOfText, message.LinesOfTextGroups);
		if (message.LinesOfTextGroups is not null)
			for (var i = 0; i < message.LinesOfTextGroups.Count; i++)
				context.Validators.News_LinesOfText(context, message, message.LinesOfTextGroups[i], i);
		if (message.URLLink is not null) context.Validators.URLLink(context, message, message.URLLink);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReject(FixContext context, FixMessage.OrderCancelReject message)
	{
		if (message.OrderID is null) Missing(message, 37);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (message.OrigClOrdID is null) Missing(message, 41);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.OrdStatus is null) Missing(message, 39);
		else context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, message, message.WorkingIndicator);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, message, message.OrigOrdModTime);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.CxlRejResponseTo is null) Missing(message, 434);
		else context.Validators.CxlRejResponseTo(context, message, message.CxlRejResponseTo);
		if (message.CxlRejReason is not null) context.Validators.CxlRejReason(context, message, message.CxlRejReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest(FixContext context, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, message.TradeOriginationDate);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.OrigClOrdID is null) Missing(message, 41);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, message, message.OrigOrdModTime);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.OrderCancelReplaceRequest_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.CashMargin is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.OrderCancelReplaceRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.OrderCancelReplaceRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.OrdType is null) Missing(message, 40);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (message.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, message.TargetStrategyParameters);
		if (message.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, message.ParticipationRate);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.BookingType is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Price2 is not null) context.Validators.Price2(context, message, message.Price2);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.CancellationRights is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.RegistID is not null) context.Validators.RegistID(context, message, message.RegistID);
		if (message.Designation is not null) context.Validators.Designation(context, message, message.Designation);

		return message.IsValid;
	}

	static bool ValidateOrderCancelRequest(FixContext context, FixMessage.OrderCancelRequest message)
	{
		if (message.OrigClOrdID is null) Missing(message, 41);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, message, message.OrigOrdModTime);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.OrderCancelRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderMassCancelReport(FixContext context, FixMessage.OrderMassCancelReport message)
	{
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.OrderID is null) Missing(message, 37);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.MassCancelRequestType is null) Missing(message, 530);
		else context.Validators.MassCancelRequestType(context, message, message.MassCancelRequestType);
		if (message.MassCancelResponse is null) Missing(message, 531);
		else context.Validators.MassCancelResponse(context, message, message.MassCancelResponse);
		if (message.MassCancelRejectReason is not null) context.Validators.MassCancelRejectReason(context, message, message.MassCancelRejectReason);
		if (message.TotalAffectedOrders is not null) context.Validators.TotalAffectedOrders(context, message, message.TotalAffectedOrders);
		if (message.NoAffectedOrders is not null) context.Validators.NoAffectedOrders(context, message, message.NoAffectedOrders);
		Counted(message, message.NoAffectedOrders, message.NoAffectedOrdersGroups);
		if (message.NoAffectedOrdersGroups is not null)
			for (var i = 0; i < message.NoAffectedOrdersGroups.Count; i++)
				context.Validators.OrderMassCancelReport_NoAffectedOrders(context, message, message.NoAffectedOrdersGroups[i], i);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderMassCancelRequest(FixContext context, FixMessage.OrderMassCancelRequest message)
	{
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.MassCancelRequestType is null) Missing(message, 530);
		else context.Validators.MassCancelRequestType(context, message, message.MassCancelRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderMassStatusRequest(FixContext context, FixMessage.OrderMassStatusRequest message)
	{
		if (message.MassStatusReqID is null) Missing(message, 584);
		else context.Validators.MassStatusReqID(context, message, message.MassStatusReqID);
		if (message.MassStatusReqType is null) Missing(message, 585);
		else context.Validators.MassStatusReqType(context, message, message.MassStatusReqType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);

		return message.IsValid;
	}

	static bool ValidateOrderStatusRequest(FixContext context, FixMessage.OrderStatusRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is null) Missing(message, 11);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, message.SecondaryClOrdID);
		if (message.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, message.ClOrdLinkID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.OrdStatusReqID is not null) context.Validators.OrdStatusReqID(context, message, message.OrdStatusReqID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.OrderStatusRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is null) Missing(message, 54);
		else context.Validators.Side(context, message, message.Side);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceReport(FixContext context, FixMessage.PositionMaintenanceReport message)
	{
		if (message.PosMaintRptID is null) Missing(message, 721);
		else context.Validators.PosMaintRptID(context, message, message.PosMaintRptID);
		if (message.PosTransType is null) Missing(message, 709);
		else context.Validators.PosTransType(context, message, message.PosTransType);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, message, message.PosReqID);
		if (message.PosMaintAction is null) Missing(message, 712);
		else context.Validators.PosMaintAction(context, message, message.PosMaintAction);
		if (message.OrigPosReqRefID is null) Missing(message, 713);
		else context.Validators.OrigPosReqRefID(context, message, message.OrigPosReqRefID);
		if (message.PosMaintStatus is null) Missing(message, 722);
		else context.Validators.PosMaintStatus(context, message, message.PosMaintStatus);
		if (message.PosMaintResult is not null) context.Validators.PosMaintResult(context, message, message.PosMaintResult);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		else context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is null) Missing(message, 1);
		else context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.PositionMaintenanceReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.PositionMaintenanceReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.PositionMaintenanceReport_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(message, 702);
		else context.Validators.PositionQty(context, message, message);
		if (Empty((IPositionAmountData)message)) Absent(message, 753);
		else context.Validators.PositionAmountData(context, message, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, message, message.AdjustmentType);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, message, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceRequest(FixContext context, FixMessage.PositionMaintenanceRequest message)
	{
		if (message.PosReqID is null) Missing(message, 710);
		else context.Validators.PosReqID(context, message, message.PosReqID);
		if (message.PosTransType is null) Missing(message, 709);
		else context.Validators.PosTransType(context, message, message.PosTransType);
		if (message.PosMaintAction is null) Missing(message, 712);
		else context.Validators.PosMaintAction(context, message, message.PosMaintAction);
		if (message.OrigPosReqRefID is not null) context.Validators.OrigPosReqRefID(context, message, message.OrigPosReqRefID);
		if (message.PosMaintRptRefID is not null) context.Validators.PosMaintRptRefID(context, message, message.PosMaintRptRefID);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		else context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (Empty((IParties)message)) Absent(message, 453);
		else context.Validators.Parties(context, message, message);
		if (message.Account is null) Missing(message, 1);
		else context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.PositionMaintenanceRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.PositionMaintenanceRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.PositionMaintenanceRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (Empty((IPositionQty)message)) Absent(message, 702);
		else context.Validators.PositionQty(context, message, message);
		if (message.AdjustmentType is not null) context.Validators.AdjustmentType(context, message, message.AdjustmentType);
		if (message.ContraryInstructionIndicator is not null) context.Validators.ContraryInstructionIndicator(context, message, message.ContraryInstructionIndicator);
		if (message.PriorSpreadIndicator is not null) context.Validators.PriorSpreadIndicator(context, message, message.PriorSpreadIndicator);
		if (message.ThresholdAmount is not null) context.Validators.ThresholdAmount(context, message, message.ThresholdAmount);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidatePositionReport(FixContext context, FixMessage.PositionReport message)
	{
		if (message.PosMaintRptID is null) Missing(message, 721);
		else context.Validators.PosMaintRptID(context, message, message.PosMaintRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, message, message.PosReqID);
		if (message.PosReqType is not null) context.Validators.PosReqType(context, message, message.PosReqType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TotalNumPosReports is not null) context.Validators.TotalNumPosReports(context, message, message.TotalNumPosReports);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.PosReqResult is null) Missing(message, 728);
		else context.Validators.PosReqResult(context, message, message.PosReqResult);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		else context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (Empty((IParties)message)) Absent(message, 453);
		else context.Validators.Parties(context, message, message);
		if (message.Account is null) Missing(message, 1);
		else context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.SettlPrice is null) Missing(message, 730);
		else context.Validators.SettlPrice(context, message, message.SettlPrice);
		if (message.SettlPriceType is null) Missing(message, 731);
		else context.Validators.SettlPriceType(context, message, message.SettlPriceType);
		if (message.PriorSettlPrice is null) Missing(message, 734);
		else context.Validators.PriorSettlPrice(context, message, message.PriorSettlPrice);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.PositionReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.PositionReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (Empty((IPositionQty)message)) Absent(message, 702);
		else context.Validators.PositionQty(context, message, message);
		if (Empty((IPositionAmountData)message)) Absent(message, 753);
		else context.Validators.PositionAmountData(context, message, message);
		if (message.RegistStatus is not null) context.Validators.RegistStatus(context, message, message.RegistStatus);
		if (message.DeliveryDate is not null) context.Validators.DeliveryDate(context, message, message.DeliveryDate);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateQuote(FixContext context, FixMessage.Quote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, 117);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, message, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, message, message.NoQuoteQualifiers);
		Counted(message, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		if (message.NoQuoteQualifiersGroups is not null)
			for (var i = 0; i < message.NoQuoteQualifiersGroups.Count; i++)
				context.Validators.Quote_NoQuoteQualifiers(context, message, message.NoQuoteQualifiersGroups[i], i);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.Quote_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.Quote_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.BidPx is not null) context.Validators.BidPx(context, message, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, message, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, message, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, message, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, message, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, message, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, message, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, message, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, message.OfferForwardPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, message, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, message, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, message, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, message, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, message, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, message, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, message, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, message, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel(FixContext context, FixMessage.QuoteCancel message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, 117);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteCancelType is null) Missing(message, 298);
		else context.Validators.QuoteCancelType(context, message, message.QuoteCancelType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, message, message.NoQuoteEntries);
		Counted(message, message.NoQuoteEntries, message.NoQuoteEntriesGroups);
		if (message.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < message.NoQuoteEntriesGroups.Count; i++)
				context.Validators.QuoteCancel_NoQuoteEntries(context, message, message.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest(FixContext context, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID is null) Missing(message, 131);
		else context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, message, message.RFQReqID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.NoRelatedSym is null) Missing(message, 146);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.QuoteRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject(FixContext context, FixMessage.QuoteRequestReject message)
	{
		if (message.QuoteReqID is null) Missing(message, 131);
		else context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.RFQReqID is not null) context.Validators.RFQReqID(context, message, message.RFQReqID);
		if (message.QuoteRequestRejectReason is null) Missing(message, 658);
		else context.Validators.QuoteRequestRejectReason(context, message, message.QuoteRequestRejectReason);
		if (message.NoRelatedSym is null) Missing(message, 146);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.QuoteRequestReject_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse(FixContext context, FixMessage.QuoteResponse message)
	{
		if (message.QuoteRespID is null) Missing(message, 693);
		else context.Validators.QuoteRespID(context, message, message.QuoteRespID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteRespType is null) Missing(message, 694);
		else context.Validators.QuoteRespType(context, message, message.QuoteRespType);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.IOIid is not null) context.Validators.IOIid(context, message, message.IOIid);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, message, message.NoQuoteQualifiers);
		Counted(message, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		if (message.NoQuoteQualifiersGroups is not null)
			for (var i = 0; i < message.NoQuoteQualifiersGroups.Count; i++)
				context.Validators.QuoteResponse_NoQuoteQualifiers(context, message, message.NoQuoteQualifiersGroups[i], i);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteResponse_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.QuoteResponse_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.BidPx is not null) context.Validators.BidPx(context, message, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, message, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, message, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, message, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, message, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, message, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, message, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, message, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, message.OfferForwardPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, message, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, message, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, message, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, message, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, message, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, message, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, message, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, message, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReport(FixContext context, FixMessage.QuoteStatusReport message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, message, message.QuoteStatusReqID);
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, 117);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteRespID is not null) context.Validators.QuoteRespID(context, message, message.QuoteRespID);
		if (message.QuoteType is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteStatusReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.SettlDate2 is not null) context.Validators.SettlDate2(context, message, message.SettlDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (!Empty((IStipulations)message)) context.Validators.Stipulations(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.QuoteStatusReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, message, message.NoQuoteQualifiers);
		Counted(message, message.NoQuoteQualifiers, message.NoQuoteQualifiersGroups);
		if (message.NoQuoteQualifiersGroups is not null)
			for (var i = 0; i < message.NoQuoteQualifiersGroups.Count; i++)
				context.Validators.QuoteStatusReport_NoQuoteQualifiers(context, message, message.NoQuoteQualifiersGroups[i], i);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.BidPx is not null) context.Validators.BidPx(context, message, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, message, message.OfferPx);
		if (message.MktBidPx is not null) context.Validators.MktBidPx(context, message, message.MktBidPx);
		if (message.MktOfferPx is not null) context.Validators.MktOfferPx(context, message, message.MktOfferPx);
		if (message.MinBidSize is not null) context.Validators.MinBidSize(context, message, message.MinBidSize);
		if (message.BidSize is not null) context.Validators.BidSize(context, message, message.BidSize);
		if (message.MinOfferSize is not null) context.Validators.MinOfferSize(context, message, message.MinOfferSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, message, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, message.OfferForwardPoints);
		if (message.MidPx is not null) context.Validators.MidPx(context, message, message.MidPx);
		if (message.BidYield is not null) context.Validators.BidYield(context, message, message.BidYield);
		if (message.MidYield is not null) context.Validators.MidYield(context, message, message.MidYield);
		if (message.OfferYield is not null) context.Validators.OfferYield(context, message, message.OfferYield);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, message, message.BidForwardPoints2);
		if (message.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, message, message.OfferForwardPoints2);
		if (message.SettlCurrBidFxRate is not null) context.Validators.SettlCurrBidFxRate(context, message, message.SettlCurrBidFxRate);
		if (message.SettlCurrOfferFxRate is not null) context.Validators.SettlCurrOfferFxRate(context, message, message.SettlCurrOfferFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.QuoteStatus is not null) context.Validators.QuoteStatus(context, message, message.QuoteStatus);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusRequest(FixContext context, FixMessage.QuoteStatusRequest message)
	{
		if (message.QuoteStatusReqID is not null) context.Validators.QuoteStatusReqID(context, message, message.QuoteStatusReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteStatusRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.QuoteStatusRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateRFQRequest(FixContext context, FixMessage.RFQRequest message)
	{
		if (message.RFQReqID is null) Missing(message, 644);
		else context.Validators.RFQReqID(context, message, message.RFQReqID);
		if (message.NoRelatedSym is null) Missing(message, 146);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.RFQRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructions(FixContext context, FixMessage.RegistrationInstructions message)
	{
		if (message.RegistID is null) Missing(message, 513);
		else context.Validators.RegistID(context, message, message.RegistID);
		if (message.RegistTransType is null) Missing(message, 514);
		else context.Validators.RegistTransType(context, message, message.RegistTransType);
		if (message.RegistRefID is null) Missing(message, 508);
		else context.Validators.RegistRefID(context, message, message.RegistRefID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.RegistAcctType is not null) context.Validators.RegistAcctType(context, message, message.RegistAcctType);
		if (message.TaxAdvantageType is not null) context.Validators.TaxAdvantageType(context, message, message.TaxAdvantageType);
		if (message.OwnershipType is not null) context.Validators.OwnershipType(context, message, message.OwnershipType);
		if (message.NoRegistDtls is not null) context.Validators.NoRegistDtls(context, message, message.NoRegistDtls);
		Counted(message, message.NoRegistDtls, message.NoRegistDtlsGroups);
		if (message.NoRegistDtlsGroups is not null)
			for (var i = 0; i < message.NoRegistDtlsGroups.Count; i++)
				context.Validators.RegistrationInstructions_NoRegistDtls(context, message, message.NoRegistDtlsGroups[i], i);
		if (message.NoDistribInsts is not null) context.Validators.NoDistribInsts(context, message, message.NoDistribInsts);
		Counted(message, message.NoDistribInsts, message.NoDistribInstsGroups);
		if (message.NoDistribInstsGroups is not null)
			for (var i = 0; i < message.NoDistribInstsGroups.Count; i++)
				context.Validators.RegistrationInstructions_NoDistribInsts(context, message, message.NoDistribInstsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructionsResponse(FixContext context, FixMessage.RegistrationInstructionsResponse message)
	{
		if (message.RegistID is null) Missing(message, 513);
		else context.Validators.RegistID(context, message, message.RegistID);
		if (message.RegistTransType is null) Missing(message, 514);
		else context.Validators.RegistTransType(context, message, message.RegistTransType);
		if (message.RegistRefID is null) Missing(message, 508);
		else context.Validators.RegistRefID(context, message, message.RegistRefID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.RegistStatus is null) Missing(message, 506);
		else context.Validators.RegistStatus(context, message, message.RegistStatus);
		if (message.RegistRejReasonCode is not null) context.Validators.RegistRejReasonCode(context, message, message.RegistRejReasonCode);
		if (message.RegistRejReasonText is not null) context.Validators.RegistRejReasonText(context, message, message.RegistRejReasonText);

		return message.IsValid;
	}

	static bool ValidateReject(FixContext context, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(message, 45);
		else context.Validators.RefSeqNum(context, message, message.RefSeqNum);
		if (message.RefTagID is not null) context.Validators.RefTagID(context, message, message.RefTagID);
		if (message.RefMsgType is not null) context.Validators.RefMsgType(context, message, message.RefMsgType);
		if (message.SessionRejectReason is not null) context.Validators.SessionRejectReason(context, message, message.SessionRejectReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateRequestForPositions(FixContext context, FixMessage.RequestForPositions message)
	{
		if (message.PosReqID is null) Missing(message, 710);
		else context.Validators.PosReqID(context, message, message.PosReqID);
		if (message.PosReqType is null) Missing(message, 724);
		else context.Validators.PosReqType(context, message, message.PosReqType);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (Empty((IParties)message)) Absent(message, 453);
		else context.Validators.Parties(context, message, message);
		if (message.Account is null) Missing(message, 1);
		else context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.RequestForPositions_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.RequestForPositions_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.ClearingBusinessDate is null) Missing(message, 715);
		else context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.SettlSessID is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlSessSubID is not null) context.Validators.SettlSessSubID(context, message, message.SettlSessSubID);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.RequestForPositions_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateRequestForPositionsAck(FixContext context, FixMessage.RequestForPositionsAck message)
	{
		if (message.PosMaintRptID is null) Missing(message, 721);
		else context.Validators.PosMaintRptID(context, message, message.PosMaintRptID);
		if (message.PosReqID is not null) context.Validators.PosReqID(context, message, message.PosReqID);
		if (message.TotalNumPosReports is not null) context.Validators.TotalNumPosReports(context, message, message.TotalNumPosReports);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.PosReqResult is null) Missing(message, 728);
		else context.Validators.PosReqResult(context, message, message.PosReqResult);
		if (message.PosReqStatus is null) Missing(message, 729);
		else context.Validators.PosReqStatus(context, message, message.PosReqStatus);
		if (Empty((IParties)message)) Absent(message, 453);
		else context.Validators.Parties(context, message, message);
		if (message.Account is null) Missing(message, 1);
		else context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is null) Missing(message, 581);
		else context.Validators.AccountType(context, message, message.AccountType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.RequestForPositionsAck_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.RequestForPositionsAck_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateResendRequest(FixContext context, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(message, 7);
		else context.Validators.BeginSeqNo(context, message, message.BeginSeqNo);
		if (message.EndSeqNo is null) Missing(message, 16);
		else context.Validators.EndSeqNo(context, message, message.EndSeqNo);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition(FixContext context, FixMessage.SecurityDefinition message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(message, 322);
		else context.Validators.SecurityResponseID(context, message, message.SecurityResponseID);
		if (message.SecurityResponseType is null) Missing(message, 323);
		else context.Validators.SecurityResponseType(context, message, message.SecurityResponseType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityDefinition_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.SecurityDefinition_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, message, message.ExpirationCycle);
		if (message.RoundLot is not null) context.Validators.RoundLot(context, message, message.RoundLot);
		if (message.MinTradeVol is not null) context.Validators.MinTradeVol(context, message, message.MinTradeVol);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest(FixContext context, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityRequestType is null) Missing(message, 321);
		else context.Validators.SecurityRequestType(context, message, message.SecurityRequestType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityDefinitionRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.SecurityDefinitionRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, message, message.ExpirationCycle);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateSecurityList(FixContext context, FixMessage.SecurityList message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(message, 322);
		else context.Validators.SecurityResponseID(context, message, message.SecurityResponseID);
		if (message.SecurityRequestResult is null) Missing(message, 560);
		else context.Validators.SecurityRequestResult(context, message, message.SecurityRequestResult);
		if (message.TotNoRelatedSym is not null) context.Validators.TotNoRelatedSym(context, message, message.TotNoRelatedSym);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.SecurityList_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequest(FixContext context, FixMessage.SecurityListRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityListRequestType is null) Missing(message, 559);
		else context.Validators.SecurityListRequestType(context, message, message.SecurityListRequestType);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityListRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.SecurityListRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus(FixContext context, FixMessage.SecurityStatus message)
	{
		if (message.SecurityStatusReqID is not null) context.Validators.SecurityStatusReqID(context, message, message.SecurityStatusReqID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityStatus_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.SecurityStatus_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, message, message.SecurityTradingStatus);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, message, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, message, message.CorporateAction);
		if (message.HaltReason is not null) context.Validators.HaltReason(context, message, message.HaltReason);
		if (message.InViewOfCommon is not null) context.Validators.InViewOfCommon(context, message, message.InViewOfCommon);
		if (message.DueToRelated is not null) context.Validators.DueToRelated(context, message, message.DueToRelated);
		if (message.BuyVolume is not null) context.Validators.BuyVolume(context, message, message.BuyVolume);
		if (message.SellVolume is not null) context.Validators.SellVolume(context, message, message.SellVolume);
		if (message.HighPx is not null) context.Validators.HighPx(context, message, message.HighPx);
		if (message.LowPx is not null) context.Validators.LowPx(context, message, message.LowPx);
		if (message.LastPx is not null) context.Validators.LastPx(context, message, message.LastPx);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Adjustment is not null) context.Validators.Adjustment(context, message, message.Adjustment);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusRequest(FixContext context, FixMessage.SecurityStatusRequest message)
	{
		if (message.SecurityStatusReqID is null) Missing(message, 324);
		else context.Validators.SecurityStatusReqID(context, message, message.SecurityStatusReqID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityStatusRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.SecurityStatusRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateSecurityTypeRequest(FixContext context, FixMessage.SecurityTypeRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.Product is not null) context.Validators.Product(context, message, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.SecuritySubType is not null) context.Validators.SecuritySubType(context, message, message.SecuritySubType);

		return message.IsValid;
	}

	static bool ValidateSecurityTypes(FixContext context, FixMessage.SecurityTypes message)
	{
		if (message.SecurityReqID is null) Missing(message, 320);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(message, 322);
		else context.Validators.SecurityResponseID(context, message, message.SecurityResponseID);
		if (message.SecurityResponseType is null) Missing(message, 323);
		else context.Validators.SecurityResponseType(context, message, message.SecurityResponseType);
		if (message.TotNoSecurityTypes is not null) context.Validators.TotNoSecurityTypes(context, message, message.TotNoSecurityTypes);
		if (message.LastFragment is not null) context.Validators.LastFragment(context, message, message.LastFragment);
		if (message.NoSecurityTypes is not null) context.Validators.NoSecurityTypes(context, message, message.NoSecurityTypes);
		Counted(message, message.NoSecurityTypes, message.NoSecurityTypesGroups);
		if (message.NoSecurityTypesGroups is not null)
			for (var i = 0; i < message.NoSecurityTypesGroups.Count; i++)
				context.Validators.SecurityTypes_NoSecurityTypes(context, message, message.NoSecurityTypesGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateSequenceReset(FixContext context, FixMessage.SequenceReset message)
	{
		if (message.GapFillFlag is not null) context.Validators.GapFillFlag(context, message, message.GapFillFlag);
		if (message.NewSeqNo is null) Missing(message, 36);
		else context.Validators.NewSeqNo(context, message, message.NewSeqNo);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructionRequest(FixContext context, FixMessage.SettlementInstructionRequest message)
	{
		if (message.SettlInstReqID is null) Missing(message, 791);
		else context.Validators.SettlInstReqID(context, message, message.SettlInstReqID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (message.AllocAccount is not null) context.Validators.AllocAccount(context, message, message.AllocAccount);
		if (message.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, message.AllocAcctIDSource);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.Product is not null) context.Validators.Product(context, message, message.Product);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.CFICode is not null) context.Validators.CFICode(context, message, message.CFICode);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, message, message.LastUpdateTime);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.StandInstDbName is not null) context.Validators.StandInstDbName(context, message, message.StandInstDbName);
		if (message.StandInstDbID is not null) context.Validators.StandInstDbID(context, message, message.StandInstDbID);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructions(FixContext context, FixMessage.SettlementInstructions message)
	{
		if (message.SettlInstMsgID is null) Missing(message, 777);
		else context.Validators.SettlInstMsgID(context, message, message.SettlInstMsgID);
		if (message.SettlInstReqID is not null) context.Validators.SettlInstReqID(context, message, message.SettlInstReqID);
		if (message.SettlInstMode is null) Missing(message, 160);
		else context.Validators.SettlInstMode(context, message, message.SettlInstMode);
		if (message.SettlInstReqRejCode is not null) context.Validators.SettlInstReqRejCode(context, message, message.SettlInstReqRejCode);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.NoSettlInst is not null) context.Validators.NoSettlInst(context, message, message.NoSettlInst);
		Counted(message, message.NoSettlInst, message.NoSettlInstGroups);
		if (message.NoSettlInstGroups is not null)
			for (var i = 0; i < message.NoSettlInstGroups.Count; i++)
				context.Validators.SettlementInstructions_NoSettlInst(context, message, message.NoSettlInstGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateTestRequest(FixContext context, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(message, 112);
		else context.Validators.TestReqID(context, message, message.TestReqID);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport(FixContext context, FixMessage.TradeCaptureReport message)
	{
		if (message.TradeReportID is null) Missing(message, 571);
		else context.Validators.TradeReportID(context, message, message.TradeReportID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, message, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, message, message.TradeReportType);
		if (message.TradeRequestID is not null) context.Validators.TradeRequestID(context, message, message.TradeRequestID);
		if (message.TrdType is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, message, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, message, message.SecondaryTrdType);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, message, message.TransferReason);
		if (message.ExecType is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.TotNumTradeReports is not null) context.Validators.TotNumTradeReports(context, message, message.TotNumTradeReports);
		if (message.LastRptRequested is not null) context.Validators.LastRptRequested(context, message, message.LastRptRequested);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradeReportRefID is not null) context.Validators.TradeReportRefID(context, message, message.TradeReportRefID);
		if (message.SecondaryTradeReportRefID is not null) context.Validators.SecondaryTradeReportRefID(context, message, message.SecondaryTradeReportRefID);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, message.SecondaryTradeReportID);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, message, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, message, message.TrdMatchID);
		if (message.ExecID is not null) context.Validators.ExecID(context, message, message.ExecID);
		if (message.OrdStatus is not null) context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, message, message.SecondaryExecID);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, message, message.ExecRestatementReason);
		if (message.PreviouslyReported is null) Missing(message, 570);
		else context.Validators.PreviouslyReported(context, message, message.PreviouslyReported);
		if (message.PriceType is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (message.QtyType is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.UnderlyingTradingSessionID is not null) context.Validators.UnderlyingTradingSessionID(context, message, message.UnderlyingTradingSessionID);
		if (message.UnderlyingTradingSessionSubID is not null) context.Validators.UnderlyingTradingSessionSubID(context, message, message.UnderlyingTradingSessionSubID);
		if (message.LastQty is null) Missing(message, 32);
		else context.Validators.LastQty(context, message, message.LastQty);
		if (message.LastPx is null) Missing(message, 31);
		else context.Validators.LastPx(context, message, message.LastPx);
		if (message.LastParPx is not null) context.Validators.LastParPx(context, message, message.LastParPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, message, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, message, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradeDate is null) Missing(message, 75);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.AvgPx is not null) context.Validators.AvgPx(context, message, message.AvgPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (message.AvgPxIndicator is not null) context.Validators.AvgPxIndicator(context, message, message.AvgPxIndicator);
		if (!Empty((IPositionAmountData)message)) context.Validators.PositionAmountData(context, message, message);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.TradeLegRefID is not null) context.Validators.TradeLegRefID(context, message, message.TradeLegRefID);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.TransactTime is null) Missing(message, 60);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.SettlType is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlDate is not null) context.Validators.SettlDate(context, message, message.SettlDate);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.MatchType is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.NoSides is null) Missing(message, 552);
		else context.Validators.NoSides(context, message, message.NoSides);
		Counted(message, message.NoSides, message.NoSidesGroups);
		if (message.NoSidesGroups is not null)
			for (var i = 0; i < message.NoSidesGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoSides(context, message, message.NoSidesGroups[i], i);
		if (message.CopyMsgIndicator is not null) context.Validators.CopyMsgIndicator(context, message, message.CopyMsgIndicator);
		if (message.PublishTrdIndicator is not null) context.Validators.PublishTrdIndicator(context, message, message.PublishTrdIndicator);
		if (message.ShortSaleReason is not null) context.Validators.ShortSaleReason(context, message, message.ShortSaleReason);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportAck(FixContext context, FixMessage.TradeCaptureReportAck message)
	{
		if (message.TradeReportID is null) Missing(message, 571);
		else context.Validators.TradeReportID(context, message, message.TradeReportID);
		if (message.TradeReportTransType is not null) context.Validators.TradeReportTransType(context, message, message.TradeReportTransType);
		if (message.TradeReportType is not null) context.Validators.TradeReportType(context, message, message.TradeReportType);
		if (message.TrdType is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, message, message.TrdSubType);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, message, message.SecondaryTrdType);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, message, message.TransferReason);
		if (message.ExecType is null) Missing(message, 150);
		else context.Validators.ExecType(context, message, message.ExecType);
		if (message.TradeReportRefID is not null) context.Validators.TradeReportRefID(context, message, message.TradeReportRefID);
		if (message.SecondaryTradeReportRefID is not null) context.Validators.SecondaryTradeReportRefID(context, message, message.SecondaryTradeReportRefID);
		if (message.TrdRptStatus is not null) context.Validators.TrdRptStatus(context, message, message.TrdRptStatus);
		if (message.TradeReportRejectReason is not null) context.Validators.TradeReportRejectReason(context, message, message.TradeReportRejectReason);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, message.SecondaryTradeReportID);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, message, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, message, message.TrdMatchID);
		if (message.ExecID is not null) context.Validators.ExecID(context, message, message.ExecID);
		if (message.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, message, message.SecondaryExecID);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (!Empty((ITrdRegTimestamps)message)) context.Validators.TrdRegTimestamps(context, message, message);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.TradeCaptureReportAck_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.AccountType is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.PositionEffect is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.TradeCaptureReportAck_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequest(FixContext context, FixMessage.TradeCaptureReportRequest message)
	{
		if (message.TradeRequestID is null) Missing(message, 568);
		else context.Validators.TradeRequestID(context, message, message.TradeRequestID);
		if (message.TradeRequestType is null) Missing(message, 569);
		else context.Validators.TradeRequestType(context, message, message.TradeRequestType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradeReportID is not null) context.Validators.TradeReportID(context, message, message.TradeReportID);
		if (message.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, message.SecondaryTradeReportID);
		if (message.ExecID is not null) context.Validators.ExecID(context, message, message.ExecID);
		if (message.ExecType is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.MatchStatus is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.TrdType is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.TrdSubType is not null) context.Validators.TrdSubType(context, message, message.TrdSubType);
		if (message.TransferReason is not null) context.Validators.TransferReason(context, message, message.TransferReason);
		if (message.SecondaryTrdType is not null) context.Validators.SecondaryTrdType(context, message, message.SecondaryTrdType);
		if (message.TradeLinkID is not null) context.Validators.TradeLinkID(context, message, message.TradeLinkID);
		if (message.TrdMatchID is not null) context.Validators.TrdMatchID(context, message, message.TrdMatchID);
		if (!Empty((IParties)message)) context.Validators.Parties(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.TradeCaptureReportRequest_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.TradeCaptureReportRequest_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.NoDates is not null) context.Validators.NoDates(context, message, message.NoDates);
		Counted(message, message.NoDates, message.NoDatesGroups);
		if (message.NoDatesGroups is not null)
			for (var i = 0; i < message.NoDatesGroups.Count; i++)
				context.Validators.TradeCaptureReportRequest_NoDates(context, message, message.NoDatesGroups[i], i);
		if (message.ClearingBusinessDate is not null) context.Validators.ClearingBusinessDate(context, message, message.ClearingBusinessDate);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.TimeBracket is not null) context.Validators.TimeBracket(context, message, message.TimeBracket);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.TradeInputSource is not null) context.Validators.TradeInputSource(context, message, message.TradeInputSource);
		if (message.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, message, message.TradeInputDevice);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequestAck(FixContext context, FixMessage.TradeCaptureReportRequestAck message)
	{
		if (message.TradeRequestID is null) Missing(message, 568);
		else context.Validators.TradeRequestID(context, message, message.TradeRequestID);
		if (message.TradeRequestType is null) Missing(message, 569);
		else context.Validators.TradeRequestType(context, message, message.TradeRequestType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TotNumTradeReports is not null) context.Validators.TotNumTradeReports(context, message, message.TotNumTradeReports);
		if (message.TradeRequestResult is null) Missing(message, 749);
		else context.Validators.TradeRequestResult(context, message, message.TradeRequestResult);
		if (message.TradeRequestStatus is null) Missing(message, 750);
		else context.Validators.TradeRequestStatus(context, message, message.TradeRequestStatus);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (message.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, message.NoUnderlyings);
		Counted(message, message.NoUnderlyings, message.NoUnderlyingsGroups);
		if (message.NoUnderlyingsGroups is not null)
			for (var i = 0; i < message.NoUnderlyingsGroups.Count; i++)
				context.Validators.TradeCaptureReportRequestAck_NoUnderlyings(context, message, message.NoUnderlyingsGroups[i], i);
		if (message.NoLegs is not null) context.Validators.NoLegs(context, message, message.NoLegs);
		Counted(message, message.NoLegs, message.NoLegsGroups);
		if (message.NoLegsGroups is not null)
			for (var i = 0; i < message.NoLegsGroups.Count; i++)
				context.Validators.TradeCaptureReportRequestAck_NoLegs(context, message, message.NoLegsGroups[i], i);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.ResponseDestination is not null) context.Validators.ResponseDestination(context, message, message.ResponseDestination);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatus(FixContext context, FixMessage.TradingSessionStatus message)
	{
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, message, message.TradSesReqID);
		if (message.TradingSessionID is null) Missing(message, 336);
		else context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.TradSesStatus is null) Missing(message, 340);
		else context.Validators.TradSesStatus(context, message, message.TradSesStatus);
		if (message.TradSesStatusRejReason is not null) context.Validators.TradSesStatusRejReason(context, message, message.TradSesStatusRejReason);
		if (message.TradSesStartTime is not null) context.Validators.TradSesStartTime(context, message, message.TradSesStartTime);
		if (message.TradSesOpenTime is not null) context.Validators.TradSesOpenTime(context, message, message.TradSesOpenTime);
		if (message.TradSesPreCloseTime is not null) context.Validators.TradSesPreCloseTime(context, message, message.TradSesPreCloseTime);
		if (message.TradSesCloseTime is not null) context.Validators.TradSesCloseTime(context, message, message.TradSesCloseTime);
		if (message.TradSesEndTime is not null) context.Validators.TradSesEndTime(context, message, message.TradSesEndTime);
		if (message.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, message, message.TotalVolumeTraded);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatusRequest(FixContext context, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.TradSesReqID is null) Missing(message, 335);
		else context.Validators.TradSesReqID(context, message, message.TradSesReqID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, message.TradingSessionSubID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateUserRequest(FixContext context, FixMessage.UserRequest message)
	{
		if (message.UserRequestID is null) Missing(message, 923);
		else context.Validators.UserRequestID(context, message, message.UserRequestID);
		if (message.UserRequestType is null) Missing(message, 924);
		else context.Validators.UserRequestType(context, message, message.UserRequestType);
		if (message.Username is null) Missing(message, 553);
		else context.Validators.Username(context, message, message.Username);
		if (message.Password is not null) context.Validators.Password(context, message, message.Password);
		if (message.NewPassword is not null) context.Validators.NewPassword(context, message, message.NewPassword);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);

		return message.IsValid;
	}

	static bool ValidateUserResponse(FixContext context, FixMessage.UserResponse message)
	{
		if (message.UserRequestID is null) Missing(message, 923);
		else context.Validators.UserRequestID(context, message, message.UserRequestID);
		if (message.Username is null) Missing(message, 553);
		else context.Validators.Username(context, message, message.Username);
		if (message.UserStatus is not null) context.Validators.UserStatus(context, message, message.UserStatus);
		if (message.UserStatusText is not null) context.Validators.UserStatusText(context, message, message.UserStatusText);

		return message.IsValid;
	}

	static bool ValidateXMLnonFIX(FixContext context, FixMessage.XMLnonFIX message)
	{

		return message.IsValid;
	}

	static bool ValidateCustom(FixContext context, FixMessage.Custom message)
	{
		// The type is not one the schema describes, which is the whole of what can be said about it.
		message.AddFinding(new FixFinding(FixRule.UnknownMessageType, 35, message.MsgType?.Position ?? 0, message.MsgType, -1));

		return message.IsValid;
	}

	static bool ValidateCommissionData(FixContext context, FixMessage message, ICommissionData block)
	{
		if (block.Commission is not null) context.Validators.Commission(context, message, block.Commission);
		if (block.CommType is not null) context.Validators.CommType(context, message, block.CommType);
		if (block.CommCurrency is not null) context.Validators.CommCurrency(context, message, block.CommCurrency);
		if (block.FundRenewWaiv is not null) context.Validators.FundRenewWaiv(context, message, block.FundRenewWaiv);

		return message.IsValid;
	}

	static bool ValidateDiscretionInstructions(FixContext context, FixMessage message, IDiscretionInstructions block)
	{
		if (block.DiscretionInst is not null) context.Validators.DiscretionInst(context, message, block.DiscretionInst);
		if (block.DiscretionOffsetValue is not null) context.Validators.DiscretionOffsetValue(context, message, block.DiscretionOffsetValue);
		if (block.DiscretionMoveType is not null) context.Validators.DiscretionMoveType(context, message, block.DiscretionMoveType);
		if (block.DiscretionOffsetType is not null) context.Validators.DiscretionOffsetType(context, message, block.DiscretionOffsetType);
		if (block.DiscretionLimitType is not null) context.Validators.DiscretionLimitType(context, message, block.DiscretionLimitType);
		if (block.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, block.DiscretionRoundDirection);
		if (block.DiscretionScope is not null) context.Validators.DiscretionScope(context, message, block.DiscretionScope);

		return message.IsValid;
	}

	static bool ValidateFinancingDetails(FixContext context, FixMessage message, IFinancingDetails block)
	{
		if (block.AgreementDesc is not null) context.Validators.AgreementDesc(context, message, block.AgreementDesc);
		if (block.AgreementID is not null) context.Validators.AgreementID(context, message, block.AgreementID);
		if (block.AgreementDate is not null) context.Validators.AgreementDate(context, message, block.AgreementDate);
		if (block.AgreementCurrency is not null) context.Validators.AgreementCurrency(context, message, block.AgreementCurrency);
		if (block.TerminationType is not null) context.Validators.TerminationType(context, message, block.TerminationType);
		if (block.StartDate is not null) context.Validators.StartDate(context, message, block.StartDate);
		if (block.EndDate is not null) context.Validators.EndDate(context, message, block.EndDate);
		if (block.DeliveryType is not null) context.Validators.DeliveryType(context, message, block.DeliveryType);
		if (block.MarginRatio is not null) context.Validators.MarginRatio(context, message, block.MarginRatio);

		return message.IsValid;
	}

	static bool ValidateInstrument(FixContext context, FixMessage message, IInstrument block)
	{
		if (block.Symbol is not null) context.Validators.Symbol(context, message, block.Symbol);
		if (block.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, block.SymbolSfx);
		if (block.SecurityID is not null) context.Validators.SecurityID(context, message, block.SecurityID);
		if (block.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, block.SecurityIDSource);
		if (block.NoSecurityAltID is not null) context.Validators.NoSecurityAltID(context, message, block.NoSecurityAltID);
		Counted(message, block.NoSecurityAltID, block.NoSecurityAltIDGroups);
		if (block.NoSecurityAltIDGroups is not null)
			for (var i = 0; i < block.NoSecurityAltIDGroups.Count; i++)
				context.Validators.Instrument_NoSecurityAltID(context, message, block.NoSecurityAltIDGroups[i], i);
		if (block.Product is not null) context.Validators.Product(context, message, block.Product);
		if (block.CFICode is not null) context.Validators.CFICode(context, message, block.CFICode);
		if (block.SecurityType is not null) context.Validators.SecurityType(context, message, block.SecurityType);
		if (block.SecuritySubType is not null) context.Validators.SecuritySubType(context, message, block.SecuritySubType);
		if (block.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, block.MaturityMonthYear);
		if (block.MaturityDate is not null) context.Validators.MaturityDate(context, message, block.MaturityDate);
		if (block.PutOrCall is not null) context.Validators.PutOrCall(context, message, block.PutOrCall);
		if (block.CouponPaymentDate is not null) context.Validators.CouponPaymentDate(context, message, block.CouponPaymentDate);
		if (block.IssueDate is not null) context.Validators.IssueDate(context, message, block.IssueDate);
		if (block.RepoCollateralSecurityType is not null) context.Validators.RepoCollateralSecurityType(context, message, block.RepoCollateralSecurityType);
		if (block.RepurchaseTerm is not null) context.Validators.RepurchaseTerm(context, message, block.RepurchaseTerm);
		if (block.RepurchaseRate is not null) context.Validators.RepurchaseRate(context, message, block.RepurchaseRate);
		if (block.Factor is not null) context.Validators.Factor(context, message, block.Factor);
		if (block.CreditRating is not null) context.Validators.CreditRating(context, message, block.CreditRating);
		if (block.InstrRegistry is not null) context.Validators.InstrRegistry(context, message, block.InstrRegistry);
		if (block.CountryOfIssue is not null) context.Validators.CountryOfIssue(context, message, block.CountryOfIssue);
		if (block.StateOrProvinceOfIssue is not null) context.Validators.StateOrProvinceOfIssue(context, message, block.StateOrProvinceOfIssue);
		if (block.LocaleOfIssue is not null) context.Validators.LocaleOfIssue(context, message, block.LocaleOfIssue);
		if (block.RedemptionDate is not null) context.Validators.RedemptionDate(context, message, block.RedemptionDate);
		if (block.StrikePrice is not null) context.Validators.StrikePrice(context, message, block.StrikePrice);
		if (block.StrikeCurrency is not null) context.Validators.StrikeCurrency(context, message, block.StrikeCurrency);
		if (block.OptAttribute is not null) context.Validators.OptAttribute(context, message, block.OptAttribute);
		if (block.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, block.ContractMultiplier);
		if (block.CouponRate is not null) context.Validators.CouponRate(context, message, block.CouponRate);
		if (block.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, block.SecurityExchange);
		if (block.Issuer is not null) context.Validators.Issuer(context, message, block.Issuer);
		if (block.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, block.EncodedIssuerLen);
		if (block.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, block.EncodedIssuer);
		if (block.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, block.SecurityDesc);
		if (block.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, block.EncodedSecurityDescLen);
		if (block.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, block.EncodedSecurityDesc);
		if (block.Pool is not null) context.Validators.Pool(context, message, block.Pool);
		if (block.ContractSettlMonth is not null) context.Validators.ContractSettlMonth(context, message, block.ContractSettlMonth);
		if (block.CPProgram is not null) context.Validators.CPProgram(context, message, block.CPProgram);
		if (block.CPRegType is not null) context.Validators.CPRegType(context, message, block.CPRegType);
		if (block.NoEvents is not null) context.Validators.NoEvents(context, message, block.NoEvents);
		Counted(message, block.NoEvents, block.NoEventsGroups);
		if (block.NoEventsGroups is not null)
			for (var i = 0; i < block.NoEventsGroups.Count; i++)
				context.Validators.Instrument_NoEvents(context, message, block.NoEventsGroups[i], i);
		if (block.DatedDate is not null) context.Validators.DatedDate(context, message, block.DatedDate);
		if (block.InterestAccrualDate is not null) context.Validators.InterestAccrualDate(context, message, block.InterestAccrualDate);

		return message.IsValid;
	}

	static bool ValidateInstrumentExtension(FixContext context, FixMessage message, IInstrumentExtension block)
	{
		if (block.DeliveryForm is not null) context.Validators.DeliveryForm(context, message, block.DeliveryForm);
		if (block.PctAtRisk is not null) context.Validators.PctAtRisk(context, message, block.PctAtRisk);
		if (block.NoInstrAttrib is not null) context.Validators.NoInstrAttrib(context, message, block.NoInstrAttrib);
		Counted(message, block.NoInstrAttrib, block.NoInstrAttribGroups);
		if (block.NoInstrAttribGroups is not null)
			for (var i = 0; i < block.NoInstrAttribGroups.Count; i++)
				context.Validators.InstrumentExtension_NoInstrAttrib(context, message, block.NoInstrAttribGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateInstrumentLeg(FixContext context, FixMessage message, IInstrumentLeg block)
	{
		if (block.LegSymbol is not null) context.Validators.LegSymbol(context, message, block.LegSymbol);
		if (block.LegSymbolSfx is not null) context.Validators.LegSymbolSfx(context, message, block.LegSymbolSfx);
		if (block.LegSecurityID is not null) context.Validators.LegSecurityID(context, message, block.LegSecurityID);
		if (block.LegSecurityIDSource is not null) context.Validators.LegSecurityIDSource(context, message, block.LegSecurityIDSource);
		if (block.NoLegSecurityAltID is not null) context.Validators.NoLegSecurityAltID(context, message, block.NoLegSecurityAltID);
		Counted(message, block.NoLegSecurityAltID, block.NoLegSecurityAltIDGroups);
		if (block.NoLegSecurityAltIDGroups is not null)
			for (var i = 0; i < block.NoLegSecurityAltIDGroups.Count; i++)
				context.Validators.InstrumentLeg_NoLegSecurityAltID(context, message, block.NoLegSecurityAltIDGroups[i], i);
		if (block.LegProduct is not null) context.Validators.LegProduct(context, message, block.LegProduct);
		if (block.LegCFICode is not null) context.Validators.LegCFICode(context, message, block.LegCFICode);
		if (block.LegSecurityType is not null) context.Validators.LegSecurityType(context, message, block.LegSecurityType);
		if (block.LegSecuritySubType is not null) context.Validators.LegSecuritySubType(context, message, block.LegSecuritySubType);
		if (block.LegMaturityMonthYear is not null) context.Validators.LegMaturityMonthYear(context, message, block.LegMaturityMonthYear);
		if (block.LegMaturityDate is not null) context.Validators.LegMaturityDate(context, message, block.LegMaturityDate);
		if (block.LegCouponPaymentDate is not null) context.Validators.LegCouponPaymentDate(context, message, block.LegCouponPaymentDate);
		if (block.LegIssueDate is not null) context.Validators.LegIssueDate(context, message, block.LegIssueDate);
		if (block.LegRepoCollateralSecurityType is not null) context.Validators.LegRepoCollateralSecurityType(context, message, block.LegRepoCollateralSecurityType);
		if (block.LegRepurchaseTerm is not null) context.Validators.LegRepurchaseTerm(context, message, block.LegRepurchaseTerm);
		if (block.LegRepurchaseRate is not null) context.Validators.LegRepurchaseRate(context, message, block.LegRepurchaseRate);
		if (block.LegFactor is not null) context.Validators.LegFactor(context, message, block.LegFactor);
		if (block.LegCreditRating is not null) context.Validators.LegCreditRating(context, message, block.LegCreditRating);
		if (block.LegInstrRegistry is not null) context.Validators.LegInstrRegistry(context, message, block.LegInstrRegistry);
		if (block.LegCountryOfIssue is not null) context.Validators.LegCountryOfIssue(context, message, block.LegCountryOfIssue);
		if (block.LegStateOrProvinceOfIssue is not null) context.Validators.LegStateOrProvinceOfIssue(context, message, block.LegStateOrProvinceOfIssue);
		if (block.LegLocaleOfIssue is not null) context.Validators.LegLocaleOfIssue(context, message, block.LegLocaleOfIssue);
		if (block.LegRedemptionDate is not null) context.Validators.LegRedemptionDate(context, message, block.LegRedemptionDate);
		if (block.LegStrikePrice is not null) context.Validators.LegStrikePrice(context, message, block.LegStrikePrice);
		if (block.LegStrikeCurrency is not null) context.Validators.LegStrikeCurrency(context, message, block.LegStrikeCurrency);
		if (block.LegOptAttribute is not null) context.Validators.LegOptAttribute(context, message, block.LegOptAttribute);
		if (block.LegContractMultiplier is not null) context.Validators.LegContractMultiplier(context, message, block.LegContractMultiplier);
		if (block.LegCouponRate is not null) context.Validators.LegCouponRate(context, message, block.LegCouponRate);
		if (block.LegSecurityExchange is not null) context.Validators.LegSecurityExchange(context, message, block.LegSecurityExchange);
		if (block.LegIssuer is not null) context.Validators.LegIssuer(context, message, block.LegIssuer);
		if (block.EncodedLegIssuerLen is not null) context.Validators.EncodedLegIssuerLen(context, message, block.EncodedLegIssuerLen);
		if (block.EncodedLegIssuer is not null) context.Validators.EncodedLegIssuer(context, message, block.EncodedLegIssuer);
		if (block.LegSecurityDesc is not null) context.Validators.LegSecurityDesc(context, message, block.LegSecurityDesc);
		if (block.EncodedLegSecurityDescLen is not null) context.Validators.EncodedLegSecurityDescLen(context, message, block.EncodedLegSecurityDescLen);
		if (block.EncodedLegSecurityDesc is not null) context.Validators.EncodedLegSecurityDesc(context, message, block.EncodedLegSecurityDesc);
		if (block.LegRatioQty is not null) context.Validators.LegRatioQty(context, message, block.LegRatioQty);
		if (block.LegSide is not null) context.Validators.LegSide(context, message, block.LegSide);
		if (block.LegCurrency is not null) context.Validators.LegCurrency(context, message, block.LegCurrency);
		if (block.LegPool is not null) context.Validators.LegPool(context, message, block.LegPool);
		if (block.LegDatedDate is not null) context.Validators.LegDatedDate(context, message, block.LegDatedDate);
		if (block.LegContractSettlMonth is not null) context.Validators.LegContractSettlMonth(context, message, block.LegContractSettlMonth);
		if (block.LegInterestAccrualDate is not null) context.Validators.LegInterestAccrualDate(context, message, block.LegInterestAccrualDate);

		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveData(FixContext context, FixMessage message, ILegBenchmarkCurveData block)
	{
		if (block.LegBenchmarkCurveCurrency is not null) context.Validators.LegBenchmarkCurveCurrency(context, message, block.LegBenchmarkCurveCurrency);
		if (block.LegBenchmarkCurveName is not null) context.Validators.LegBenchmarkCurveName(context, message, block.LegBenchmarkCurveName);
		if (block.LegBenchmarkCurvePoint is not null) context.Validators.LegBenchmarkCurvePoint(context, message, block.LegBenchmarkCurvePoint);
		if (block.LegBenchmarkPrice is not null) context.Validators.LegBenchmarkPrice(context, message, block.LegBenchmarkPrice);
		if (block.LegBenchmarkPriceType is not null) context.Validators.LegBenchmarkPriceType(context, message, block.LegBenchmarkPriceType);

		return message.IsValid;
	}

	static bool ValidateLegStipulations(FixContext context, FixMessage message, ILegStipulations block)
	{
		if (block.NoLegStipulations is not null) context.Validators.NoLegStipulations(context, message, block.NoLegStipulations);
		Counted(message, block.NoLegStipulations, block.NoLegStipulationsGroups);
		if (block.NoLegStipulationsGroups is not null)
			for (var i = 0; i < block.NoLegStipulationsGroups.Count; i++)
				context.Validators.LegStipulations_NoLegStipulations(context, message, block.NoLegStipulationsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties(FixContext context, FixMessage message, INestedParties block)
	{
		if (block.NoNestedPartyIDs is not null) context.Validators.NoNestedPartyIDs(context, message, block.NoNestedPartyIDs);
		Counted(message, block.NoNestedPartyIDs, block.NoNestedPartyIDsGroups);
		if (block.NoNestedPartyIDsGroups is not null)
			for (var i = 0; i < block.NoNestedPartyIDsGroups.Count; i++)
				context.Validators.NestedParties_NoNestedPartyIDs(context, message, block.NoNestedPartyIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties2(FixContext context, FixMessage message, INestedParties2 block)
	{
		if (block.NoNested2PartyIDs is not null) context.Validators.NoNested2PartyIDs(context, message, block.NoNested2PartyIDs);
		Counted(message, block.NoNested2PartyIDs, block.NoNested2PartyIDsGroups);
		if (block.NoNested2PartyIDsGroups is not null)
			for (var i = 0; i < block.NoNested2PartyIDsGroups.Count; i++)
				context.Validators.NestedParties2_NoNested2PartyIDs(context, message, block.NoNested2PartyIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties3(FixContext context, FixMessage message, INestedParties3 block)
	{
		if (block.NoNested3PartyIDs is not null) context.Validators.NoNested3PartyIDs(context, message, block.NoNested3PartyIDs);
		Counted(message, block.NoNested3PartyIDs, block.NoNested3PartyIDsGroups);
		if (block.NoNested3PartyIDsGroups is not null)
			for (var i = 0; i < block.NoNested3PartyIDsGroups.Count; i++)
				context.Validators.NestedParties3_NoNested3PartyIDs(context, message, block.NoNested3PartyIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateOrderQtyData(FixContext context, FixMessage message, IOrderQtyData block)
	{
		if (block.OrderQty is not null) context.Validators.OrderQty(context, message, block.OrderQty);
		if (block.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, block.CashOrderQty);
		if (block.OrderPercent is not null) context.Validators.OrderPercent(context, message, block.OrderPercent);
		if (block.RoundingDirection is not null) context.Validators.RoundingDirection(context, message, block.RoundingDirection);
		if (block.RoundingModulus is not null) context.Validators.RoundingModulus(context, message, block.RoundingModulus);

		return message.IsValid;
	}

	static bool ValidateParties(FixContext context, FixMessage message, IParties block)
	{
		if (block.NoPartyIDs is not null) context.Validators.NoPartyIDs(context, message, block.NoPartyIDs);
		Counted(message, block.NoPartyIDs, block.NoPartyIDsGroups);
		if (block.NoPartyIDsGroups is not null)
			for (var i = 0; i < block.NoPartyIDsGroups.Count; i++)
				context.Validators.Parties_NoPartyIDs(context, message, block.NoPartyIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidatePegInstructions(FixContext context, FixMessage message, IPegInstructions block)
	{
		if (block.PegOffsetValue is not null) context.Validators.PegOffsetValue(context, message, block.PegOffsetValue);
		if (block.PegMoveType is not null) context.Validators.PegMoveType(context, message, block.PegMoveType);
		if (block.PegOffsetType is not null) context.Validators.PegOffsetType(context, message, block.PegOffsetType);
		if (block.PegLimitType is not null) context.Validators.PegLimitType(context, message, block.PegLimitType);
		if (block.PegRoundDirection is not null) context.Validators.PegRoundDirection(context, message, block.PegRoundDirection);
		if (block.PegScope is not null) context.Validators.PegScope(context, message, block.PegScope);

		return message.IsValid;
	}

	static bool ValidatePositionAmountData(FixContext context, FixMessage message, IPositionAmountData block)
	{
		if (block.NoPosAmt is not null) context.Validators.NoPosAmt(context, message, block.NoPosAmt);
		Counted(message, block.NoPosAmt, block.NoPosAmtGroups);
		if (block.NoPosAmtGroups is not null)
			for (var i = 0; i < block.NoPosAmtGroups.Count; i++)
				context.Validators.PositionAmountData_NoPosAmt(context, message, block.NoPosAmtGroups[i], i);

		return message.IsValid;
	}

	static bool ValidatePositionQty(FixContext context, FixMessage message, IPositionQty block)
	{
		if (block.NoPositions is not null) context.Validators.NoPositions(context, message, block.NoPositions);
		Counted(message, block.NoPositions, block.NoPositionsGroups);
		if (block.NoPositionsGroups is not null)
			for (var i = 0; i < block.NoPositionsGroups.Count; i++)
				context.Validators.PositionQty_NoPositions(context, message, block.NoPositionsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSettlInstructionsData(FixContext context, FixMessage message, ISettlInstructionsData block)
	{
		if (block.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, message, block.SettlDeliveryType);
		if (block.StandInstDbType is not null) context.Validators.StandInstDbType(context, message, block.StandInstDbType);
		if (block.StandInstDbName is not null) context.Validators.StandInstDbName(context, message, block.StandInstDbName);
		if (block.StandInstDbID is not null) context.Validators.StandInstDbID(context, message, block.StandInstDbID);
		if (block.NoDlvyInst is not null) context.Validators.NoDlvyInst(context, message, block.NoDlvyInst);
		Counted(message, block.NoDlvyInst, block.NoDlvyInstGroups);
		if (block.NoDlvyInstGroups is not null)
			for (var i = 0; i < block.NoDlvyInstGroups.Count; i++)
				context.Validators.SettlInstructionsData_NoDlvyInst(context, message, block.NoDlvyInstGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSettlParties(FixContext context, FixMessage message, ISettlParties block)
	{
		if (block.NoSettlPartyIDs is not null) context.Validators.NoSettlPartyIDs(context, message, block.NoSettlPartyIDs);
		Counted(message, block.NoSettlPartyIDs, block.NoSettlPartyIDsGroups);
		if (block.NoSettlPartyIDsGroups is not null)
			for (var i = 0; i < block.NoSettlPartyIDsGroups.Count; i++)
				context.Validators.SettlParties_NoSettlPartyIDs(context, message, block.NoSettlPartyIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSpreadOrBenchmarkCurveData(FixContext context, FixMessage message, ISpreadOrBenchmarkCurveData block)
	{
		if (block.Spread is not null) context.Validators.Spread(context, message, block.Spread);
		if (block.BenchmarkCurveCurrency is not null) context.Validators.BenchmarkCurveCurrency(context, message, block.BenchmarkCurveCurrency);
		if (block.BenchmarkCurveName is not null) context.Validators.BenchmarkCurveName(context, message, block.BenchmarkCurveName);
		if (block.BenchmarkCurvePoint is not null) context.Validators.BenchmarkCurvePoint(context, message, block.BenchmarkCurvePoint);
		if (block.BenchmarkPrice is not null) context.Validators.BenchmarkPrice(context, message, block.BenchmarkPrice);
		if (block.BenchmarkPriceType is not null) context.Validators.BenchmarkPriceType(context, message, block.BenchmarkPriceType);
		if (block.BenchmarkSecurityID is not null) context.Validators.BenchmarkSecurityID(context, message, block.BenchmarkSecurityID);
		if (block.BenchmarkSecurityIDSource is not null) context.Validators.BenchmarkSecurityIDSource(context, message, block.BenchmarkSecurityIDSource);

		return message.IsValid;
	}

	static bool ValidateStipulations(FixContext context, FixMessage message, IStipulations block)
	{
		if (block.NoStipulations is not null) context.Validators.NoStipulations(context, message, block.NoStipulations);
		Counted(message, block.NoStipulations, block.NoStipulationsGroups);
		if (block.NoStipulationsGroups is not null)
			for (var i = 0; i < block.NoStipulationsGroups.Count; i++)
				context.Validators.Stipulations_NoStipulations(context, message, block.NoStipulationsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestamps(FixContext context, FixMessage message, ITrdRegTimestamps block)
	{
		if (block.NoTrdRegTimestamps is not null) context.Validators.NoTrdRegTimestamps(context, message, block.NoTrdRegTimestamps);
		Counted(message, block.NoTrdRegTimestamps, block.NoTrdRegTimestampsGroups);
		if (block.NoTrdRegTimestampsGroups is not null)
			for (var i = 0; i < block.NoTrdRegTimestampsGroups.Count; i++)
				context.Validators.TrdRegTimestamps_NoTrdRegTimestamps(context, message, block.NoTrdRegTimestampsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrument(FixContext context, FixMessage message, IUnderlyingInstrument block)
	{
		if (block.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, message, block.UnderlyingSymbol);
		if (block.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, message, block.UnderlyingSymbolSfx);
		if (block.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, message, block.UnderlyingSecurityID);
		if (block.UnderlyingSecurityIDSource is not null) context.Validators.UnderlyingSecurityIDSource(context, message, block.UnderlyingSecurityIDSource);
		if (block.NoUnderlyingSecurityAltID is not null) context.Validators.NoUnderlyingSecurityAltID(context, message, block.NoUnderlyingSecurityAltID);
		Counted(message, block.NoUnderlyingSecurityAltID, block.NoUnderlyingSecurityAltIDGroups);
		if (block.NoUnderlyingSecurityAltIDGroups is not null)
			for (var i = 0; i < block.NoUnderlyingSecurityAltIDGroups.Count; i++)
				context.Validators.UnderlyingInstrument_NoUnderlyingSecurityAltID(context, message, block.NoUnderlyingSecurityAltIDGroups[i], i);
		if (block.UnderlyingProduct is not null) context.Validators.UnderlyingProduct(context, message, block.UnderlyingProduct);
		if (block.UnderlyingCFICode is not null) context.Validators.UnderlyingCFICode(context, message, block.UnderlyingCFICode);
		if (block.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, message, block.UnderlyingSecurityType);
		if (block.UnderlyingSecuritySubType is not null) context.Validators.UnderlyingSecuritySubType(context, message, block.UnderlyingSecuritySubType);
		if (block.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, message, block.UnderlyingMaturityMonthYear);
		if (block.UnderlyingMaturityDate is not null) context.Validators.UnderlyingMaturityDate(context, message, block.UnderlyingMaturityDate);
		if (block.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, message, block.UnderlyingPutOrCall);
		if (block.UnderlyingCouponPaymentDate is not null) context.Validators.UnderlyingCouponPaymentDate(context, message, block.UnderlyingCouponPaymentDate);
		if (block.UnderlyingIssueDate is not null) context.Validators.UnderlyingIssueDate(context, message, block.UnderlyingIssueDate);
		if (block.UnderlyingRepoCollateralSecurityType is not null) context.Validators.UnderlyingRepoCollateralSecurityType(context, message, block.UnderlyingRepoCollateralSecurityType);
		if (block.UnderlyingRepurchaseTerm is not null) context.Validators.UnderlyingRepurchaseTerm(context, message, block.UnderlyingRepurchaseTerm);
		if (block.UnderlyingRepurchaseRate is not null) context.Validators.UnderlyingRepurchaseRate(context, message, block.UnderlyingRepurchaseRate);
		if (block.UnderlyingFactor is not null) context.Validators.UnderlyingFactor(context, message, block.UnderlyingFactor);
		if (block.UnderlyingCreditRating is not null) context.Validators.UnderlyingCreditRating(context, message, block.UnderlyingCreditRating);
		if (block.UnderlyingInstrRegistry is not null) context.Validators.UnderlyingInstrRegistry(context, message, block.UnderlyingInstrRegistry);
		if (block.UnderlyingCountryOfIssue is not null) context.Validators.UnderlyingCountryOfIssue(context, message, block.UnderlyingCountryOfIssue);
		if (block.UnderlyingStateOrProvinceOfIssue is not null) context.Validators.UnderlyingStateOrProvinceOfIssue(context, message, block.UnderlyingStateOrProvinceOfIssue);
		if (block.UnderlyingLocaleOfIssue is not null) context.Validators.UnderlyingLocaleOfIssue(context, message, block.UnderlyingLocaleOfIssue);
		if (block.UnderlyingRedemptionDate is not null) context.Validators.UnderlyingRedemptionDate(context, message, block.UnderlyingRedemptionDate);
		if (block.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, message, block.UnderlyingStrikePrice);
		if (block.UnderlyingStrikeCurrency is not null) context.Validators.UnderlyingStrikeCurrency(context, message, block.UnderlyingStrikeCurrency);
		if (block.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, message, block.UnderlyingOptAttribute);
		if (block.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, message, block.UnderlyingContractMultiplier);
		if (block.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, message, block.UnderlyingCouponRate);
		if (block.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, message, block.UnderlyingSecurityExchange);
		if (block.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, message, block.UnderlyingIssuer);
		if (block.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, message, block.EncodedUnderlyingIssuerLen);
		if (block.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, message, block.EncodedUnderlyingIssuer);
		if (block.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, message, block.UnderlyingSecurityDesc);
		if (block.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, message, block.EncodedUnderlyingSecurityDescLen);
		if (block.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, message, block.EncodedUnderlyingSecurityDesc);
		if (block.UnderlyingCPProgram is not null) context.Validators.UnderlyingCPProgram(context, message, block.UnderlyingCPProgram);
		if (block.UnderlyingCPRegType is not null) context.Validators.UnderlyingCPRegType(context, message, block.UnderlyingCPRegType);
		if (block.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, message, block.UnderlyingCurrency);
		if (block.UnderlyingQty is not null) context.Validators.UnderlyingQty(context, message, block.UnderlyingQty);
		if (block.UnderlyingPx is not null) context.Validators.UnderlyingPx(context, message, block.UnderlyingPx);
		if (block.UnderlyingDirtyPrice is not null) context.Validators.UnderlyingDirtyPrice(context, message, block.UnderlyingDirtyPrice);
		if (block.UnderlyingEndPrice is not null) context.Validators.UnderlyingEndPrice(context, message, block.UnderlyingEndPrice);
		if (block.UnderlyingStartValue is not null) context.Validators.UnderlyingStartValue(context, message, block.UnderlyingStartValue);
		if (block.UnderlyingCurrentValue is not null) context.Validators.UnderlyingCurrentValue(context, message, block.UnderlyingCurrentValue);
		if (block.UnderlyingEndValue is not null) context.Validators.UnderlyingEndValue(context, message, block.UnderlyingEndValue);
		if (!Empty((IUnderlyingStipulations)block)) context.Validators.UnderlyingStipulations(context, message, block);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipulations(FixContext context, FixMessage message, IUnderlyingStipulations block)
	{
		if (block.NoUnderlyingStips is not null) context.Validators.NoUnderlyingStips(context, message, block.NoUnderlyingStips);
		Counted(message, block.NoUnderlyingStips, block.NoUnderlyingStipsGroups);
		if (block.NoUnderlyingStipsGroups is not null)
			for (var i = 0; i < block.NoUnderlyingStipsGroups.Count; i++)
				context.Validators.UnderlyingStipulations_NoUnderlyingStips(context, message, block.NoUnderlyingStipsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateYieldData(FixContext context, FixMessage message, IYieldData block)
	{
		if (block.YieldType is not null) context.Validators.YieldType(context, message, block.YieldType);
		if (block.Yield is not null) context.Validators.Yield(context, message, block.Yield);
		if (block.YieldCalcDate is not null) context.Validators.YieldCalcDate(context, message, block.YieldCalcDate);
		if (block.YieldRedemptionDate is not null) context.Validators.YieldRedemptionDate(context, message, block.YieldRedemptionDate);
		if (block.YieldRedemptionPrice is not null) context.Validators.YieldRedemptionPrice(context, message, block.YieldRedemptionPrice);
		if (block.YieldRedemptionPriceType is not null) context.Validators.YieldRedemptionPriceType(context, message, block.YieldRedemptionPriceType);

		return message.IsValid;
	}

	static bool ValidateInstrument_NoSecurityAltID(FixContext context, FixMessage message, IInstrument.NoSecurityAltIDGroup entry, int index)
	{
		context.Validators.SecurityAltID(context, message, entry.SecurityAltID);
		if (entry.SecurityAltIDSource is not null) context.Validators.SecurityAltIDSource(context, message, entry.SecurityAltIDSource);

		return message.IsValid;
	}

	static bool ValidateInstrument_NoEvents(FixContext context, FixMessage message, IInstrument.NoEventsGroup entry, int index)
	{
		context.Validators.EventType(context, message, entry.EventType);
		if (entry.EventDate is not null) context.Validators.EventDate(context, message, entry.EventDate);
		if (entry.EventPx is not null) context.Validators.EventPx(context, message, entry.EventPx);
		if (entry.EventText is not null) context.Validators.EventText(context, message, entry.EventText);

		return message.IsValid;
	}

	static bool ValidateInstrumentExtension_NoInstrAttrib(FixContext context, FixMessage message, IInstrumentExtension.NoInstrAttribGroup entry, int index)
	{
		context.Validators.InstrAttribType(context, message, entry.InstrAttribType);
		if (entry.InstrAttribValue is not null) context.Validators.InstrAttribValue(context, message, entry.InstrAttribValue);

		return message.IsValid;
	}

	static bool ValidateInstrumentLeg_NoLegSecurityAltID(FixContext context, FixMessage message, IInstrumentLeg.NoLegSecurityAltIDGroup entry, int index)
	{
		context.Validators.LegSecurityAltID(context, message, entry.LegSecurityAltID);
		if (entry.LegSecurityAltIDSource is not null) context.Validators.LegSecurityAltIDSource(context, message, entry.LegSecurityAltIDSource);

		return message.IsValid;
	}

	static bool ValidateLegStipulations_NoLegStipulations(FixContext context, FixMessage message, ILegStipulations.NoLegStipulationsGroup entry, int index)
	{
		context.Validators.LegStipulationType(context, message, entry.LegStipulationType);
		if (entry.LegStipulationValue is not null) context.Validators.LegStipulationValue(context, message, entry.LegStipulationValue);

		return message.IsValid;
	}

	static bool ValidateNestedParties_NoNestedPartyIDs(FixContext context, FixMessage message, INestedParties.NoNestedPartyIDsGroup entry, int index)
	{
		context.Validators.NestedPartyID(context, message, entry.NestedPartyID);
		if (entry.NestedPartyIDSource is not null) context.Validators.NestedPartyIDSource(context, message, entry.NestedPartyIDSource);
		if (entry.NestedPartyRole is not null) context.Validators.NestedPartyRole(context, message, entry.NestedPartyRole);
		if (entry.NoNestedPartySubIDs is not null) context.Validators.NoNestedPartySubIDs(context, message, entry.NoNestedPartySubIDs);
		Counted(message, entry.NoNestedPartySubIDs, entry.NoNestedPartySubIDsGroups);
		if (entry.NoNestedPartySubIDsGroups is not null)
			for (var i = 0; i < entry.NoNestedPartySubIDsGroups.Count; i++)
				context.Validators.NestedParties_NoNestedPartyIDs_NoNestedPartySubIDs(context, message, entry.NoNestedPartySubIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties_NoNestedPartyIDs_NoNestedPartySubIDs(FixContext context, FixMessage message, INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup entry, int index)
	{
		context.Validators.NestedPartySubID(context, message, entry.NestedPartySubID);
		if (entry.NestedPartySubIDType is not null) context.Validators.NestedPartySubIDType(context, message, entry.NestedPartySubIDType);

		return message.IsValid;
	}

	static bool ValidateNestedParties2_NoNested2PartyIDs(FixContext context, FixMessage message, INestedParties2.NoNested2PartyIDsGroup entry, int index)
	{
		context.Validators.Nested2PartyID(context, message, entry.Nested2PartyID);
		if (entry.Nested2PartyIDSource is not null) context.Validators.Nested2PartyIDSource(context, message, entry.Nested2PartyIDSource);
		if (entry.Nested2PartyRole is not null) context.Validators.Nested2PartyRole(context, message, entry.Nested2PartyRole);
		if (entry.NoNested2PartySubIDs is not null) context.Validators.NoNested2PartySubIDs(context, message, entry.NoNested2PartySubIDs);
		Counted(message, entry.NoNested2PartySubIDs, entry.NoNested2PartySubIDsGroups);
		if (entry.NoNested2PartySubIDsGroups is not null)
			for (var i = 0; i < entry.NoNested2PartySubIDsGroups.Count; i++)
				context.Validators.NestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs(context, message, entry.NoNested2PartySubIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties2_NoNested2PartyIDs_NoNested2PartySubIDs(FixContext context, FixMessage message, INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup entry, int index)
	{
		context.Validators.Nested2PartySubID(context, message, entry.Nested2PartySubID);
		if (entry.Nested2PartySubIDType is not null) context.Validators.Nested2PartySubIDType(context, message, entry.Nested2PartySubIDType);

		return message.IsValid;
	}

	static bool ValidateNestedParties3_NoNested3PartyIDs(FixContext context, FixMessage message, INestedParties3.NoNested3PartyIDsGroup entry, int index)
	{
		context.Validators.Nested3PartyID(context, message, entry.Nested3PartyID);
		if (entry.Nested3PartyIDSource is not null) context.Validators.Nested3PartyIDSource(context, message, entry.Nested3PartyIDSource);
		if (entry.Nested3PartyRole is not null) context.Validators.Nested3PartyRole(context, message, entry.Nested3PartyRole);
		if (entry.NoNested3PartySubIDs is not null) context.Validators.NoNested3PartySubIDs(context, message, entry.NoNested3PartySubIDs);
		Counted(message, entry.NoNested3PartySubIDs, entry.NoNested3PartySubIDsGroups);
		if (entry.NoNested3PartySubIDsGroups is not null)
			for (var i = 0; i < entry.NoNested3PartySubIDsGroups.Count; i++)
				context.Validators.NestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs(context, message, entry.NoNested3PartySubIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNestedParties3_NoNested3PartyIDs_NoNested3PartySubIDs(FixContext context, FixMessage message, INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup entry, int index)
	{
		context.Validators.Nested3PartySubID(context, message, entry.Nested3PartySubID);
		if (entry.Nested3PartySubIDType is not null) context.Validators.Nested3PartySubIDType(context, message, entry.Nested3PartySubIDType);

		return message.IsValid;
	}

	static bool ValidateParties_NoPartyIDs(FixContext context, FixMessage message, IParties.NoPartyIDsGroup entry, int index)
	{
		context.Validators.PartyID(context, message, entry.PartyID);
		if (entry.PartyIDSource is not null) context.Validators.PartyIDSource(context, message, entry.PartyIDSource);
		if (entry.PartyRole is not null) context.Validators.PartyRole(context, message, entry.PartyRole);
		if (entry.NoPartySubIDs is not null) context.Validators.NoPartySubIDs(context, message, entry.NoPartySubIDs);
		Counted(message, entry.NoPartySubIDs, entry.NoPartySubIDsGroups);
		if (entry.NoPartySubIDsGroups is not null)
			for (var i = 0; i < entry.NoPartySubIDsGroups.Count; i++)
				context.Validators.Parties_NoPartyIDs_NoPartySubIDs(context, message, entry.NoPartySubIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateParties_NoPartyIDs_NoPartySubIDs(FixContext context, FixMessage message, IParties.NoPartyIDsGroup.NoPartySubIDsGroup entry, int index)
	{
		context.Validators.PartySubID(context, message, entry.PartySubID);
		if (entry.PartySubIDType is not null) context.Validators.PartySubIDType(context, message, entry.PartySubIDType);

		return message.IsValid;
	}

	static bool ValidatePositionAmountData_NoPosAmt(FixContext context, FixMessage message, IPositionAmountData.NoPosAmtGroup entry, int index)
	{
		context.Validators.PosAmtType(context, message, entry.PosAmtType);
		if (entry.PosAmt is not null) context.Validators.PosAmt(context, message, entry.PosAmt);

		return message.IsValid;
	}

	static bool ValidatePositionQty_NoPositions(FixContext context, FixMessage message, IPositionQty.NoPositionsGroup entry, int index)
	{
		context.Validators.PosType(context, message, entry.PosType);
		if (entry.LongQty is not null) context.Validators.LongQty(context, message, entry.LongQty);
		if (entry.ShortQty is not null) context.Validators.ShortQty(context, message, entry.ShortQty);
		if (entry.PosQtyStatus is not null) context.Validators.PosQtyStatus(context, message, entry.PosQtyStatus);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSettlInstructionsData_NoDlvyInst(FixContext context, FixMessage message, ISettlInstructionsData.NoDlvyInstGroup entry, int index)
	{
		context.Validators.SettlInstSource(context, message, entry.SettlInstSource);
		if (entry.DlvyInstType is not null) context.Validators.DlvyInstType(context, message, entry.DlvyInstType);
		if (!Empty((ISettlParties)entry)) context.Validators.SettlParties(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSettlParties_NoSettlPartyIDs(FixContext context, FixMessage message, ISettlParties.NoSettlPartyIDsGroup entry, int index)
	{
		context.Validators.SettlPartyID(context, message, entry.SettlPartyID);
		if (entry.SettlPartyIDSource is not null) context.Validators.SettlPartyIDSource(context, message, entry.SettlPartyIDSource);
		if (entry.SettlPartyRole is not null) context.Validators.SettlPartyRole(context, message, entry.SettlPartyRole);
		if (entry.NoSettlPartySubIDs is not null) context.Validators.NoSettlPartySubIDs(context, message, entry.NoSettlPartySubIDs);
		Counted(message, entry.NoSettlPartySubIDs, entry.NoSettlPartySubIDsGroups);
		if (entry.NoSettlPartySubIDsGroups is not null)
			for (var i = 0; i < entry.NoSettlPartySubIDsGroups.Count; i++)
				context.Validators.SettlParties_NoSettlPartyIDs_NoSettlPartySubIDs(context, message, entry.NoSettlPartySubIDsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSettlParties_NoSettlPartyIDs_NoSettlPartySubIDs(FixContext context, FixMessage message, ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup entry, int index)
	{
		context.Validators.SettlPartySubID(context, message, entry.SettlPartySubID);
		if (entry.SettlPartySubIDType is not null) context.Validators.SettlPartySubIDType(context, message, entry.SettlPartySubIDType);

		return message.IsValid;
	}

	static bool ValidateStipulations_NoStipulations(FixContext context, FixMessage message, IStipulations.NoStipulationsGroup entry, int index)
	{
		context.Validators.StipulationType(context, message, entry.StipulationType);
		if (entry.StipulationValue is not null) context.Validators.StipulationValue(context, message, entry.StipulationValue);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestamps_NoTrdRegTimestamps(FixContext context, FixMessage message, ITrdRegTimestamps.NoTrdRegTimestampsGroup entry, int index)
	{
		context.Validators.TrdRegTimestamp(context, message, entry.TrdRegTimestamp);
		if (entry.TrdRegTimestampType is not null) context.Validators.TrdRegTimestampType(context, message, entry.TrdRegTimestampType);
		if (entry.TrdRegTimestampOrigin is not null) context.Validators.TrdRegTimestampOrigin(context, message, entry.TrdRegTimestampOrigin);

		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrument_NoUnderlyingSecurityAltID(FixContext context, FixMessage message, IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup entry, int index)
	{
		context.Validators.UnderlyingSecurityAltID(context, message, entry.UnderlyingSecurityAltID);
		if (entry.UnderlyingSecurityAltIDSource is not null) context.Validators.UnderlyingSecurityAltIDSource(context, message, entry.UnderlyingSecurityAltIDSource);

		return message.IsValid;
	}

	static bool ValidateUnderlyingStipulations_NoUnderlyingStips(FixContext context, FixMessage message, IUnderlyingStipulations.NoUnderlyingStipsGroup entry, int index)
	{
		context.Validators.UnderlyingStipType(context, message, entry.UnderlyingStipType);
		if (entry.UnderlyingStipValue is not null) context.Validators.UnderlyingStipValue(context, message, entry.UnderlyingStipValue);

		return message.IsValid;
	}

	static bool ValidateAdvertisement_NoLegs(FixContext context, FixMessage message, FixMessage.Advertisement.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAdvertisement_NoUnderlyings(FixContext context, FixMessage message, FixMessage.Advertisement.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoOrders(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, message, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, message, entry.OrderBookingQty);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoExecs(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoExecsGroup entry, int index)
	{
		context.Validators.LastQty(context, message, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, message, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, message, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, message, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, message, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, message, entry.LastCapacity);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoUnderlyings(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoLegs(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoAllocs(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, message, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, message, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, message, entry.ProcessCode);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, message, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, message, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, message, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, message, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, message, entry.EncodedAllocText);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, message, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, message, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, entry.SettlCurrAmt);
		if (entry.AllocSettlCurrAmt is not null) context.Validators.AllocSettlCurrAmt(context, message, entry.AllocSettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, entry.SettlCurrFxRateCalc);
		if (entry.AllocAccruedInterestAmt is not null) context.Validators.AllocAccruedInterestAmt(context, message, entry.AllocAccruedInterestAmt);
		if (entry.AllocInterestAtMaturity is not null) context.Validators.AllocInterestAtMaturity(context, message, entry.AllocInterestAtMaturity);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, entry.NoMiscFees);
		Counted(message, entry.NoMiscFees, entry.NoMiscFeesGroups);
		if (entry.NoMiscFeesGroups is not null)
			for (var i = 0; i < entry.NoMiscFeesGroups.Count; i++)
				context.Validators.AllocationInstruction_NoAllocs_NoMiscFees(context, message, entry.NoMiscFeesGroups[i], i);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, message, entry.NoClearingInstructions);
		Counted(message, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		if (entry.NoClearingInstructionsGroups is not null)
			for (var i = 0; i < entry.NoClearingInstructionsGroups.Count; i++)
				context.Validators.AllocationInstruction_NoAllocs_NoClearingInstructions(context, message, entry.NoClearingInstructionsGroups[i], i);
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, message, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoAllocs_NoMiscFees(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoAllocsGroup.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateAllocationInstruction_NoAllocs_NoClearingInstructions(FixContext context, FixMessage message, FixMessage.AllocationInstruction.NoAllocsGroup.NoClearingInstructionsGroup entry, int index)
	{
		context.Validators.ClearingInstruction(context, message, entry.ClearingInstruction);

		return message.IsValid;
	}

	static bool ValidateAllocationInstructionAck_NoAllocs(FixContext context, FixMessage message, FixMessage.AllocationInstructionAck.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, message, entry.AllocPrice);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, message, entry.IndividualAllocRejCode);
		if (entry.AllocText is not null) context.Validators.AllocText(context, message, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, message, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, message, entry.EncodedAllocText);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoOrders(FixContext context, FixMessage message, FixMessage.AllocationReport.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, message, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, message, entry.OrderBookingQty);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoExecs(FixContext context, FixMessage message, FixMessage.AllocationReport.NoExecsGroup entry, int index)
	{
		context.Validators.LastQty(context, message, entry.LastQty);
		if (entry.ExecID is not null) context.Validators.ExecID(context, message, entry.ExecID);
		if (entry.SecondaryExecID is not null) context.Validators.SecondaryExecID(context, message, entry.SecondaryExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, message, entry.LastPx);
		if (entry.LastParPx is not null) context.Validators.LastParPx(context, message, entry.LastParPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, message, entry.LastCapacity);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.AllocationReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoLegs(FixContext context, FixMessage message, FixMessage.AllocationReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoAllocs(FixContext context, FixMessage message, FixMessage.AllocationReport.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.MatchStatus is not null) context.Validators.MatchStatus(context, message, entry.MatchStatus);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, message, entry.AllocPrice);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, message, entry.ProcessCode);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, message, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, message, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, message, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, message, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, message, entry.EncodedAllocText);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, message, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, message, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, entry.SettlCurrAmt);
		if (entry.AllocSettlCurrAmt is not null) context.Validators.AllocSettlCurrAmt(context, message, entry.AllocSettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, entry.SettlCurrFxRateCalc);
		if (entry.AllocAccruedInterestAmt is not null) context.Validators.AllocAccruedInterestAmt(context, message, entry.AllocAccruedInterestAmt);
		if (entry.AllocInterestAtMaturity is not null) context.Validators.AllocInterestAtMaturity(context, message, entry.AllocInterestAtMaturity);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, entry.NoMiscFees);
		Counted(message, entry.NoMiscFees, entry.NoMiscFeesGroups);
		if (entry.NoMiscFeesGroups is not null)
			for (var i = 0; i < entry.NoMiscFeesGroups.Count; i++)
				context.Validators.AllocationReport_NoAllocs_NoMiscFees(context, message, entry.NoMiscFeesGroups[i], i);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, message, entry.NoClearingInstructions);
		Counted(message, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		if (entry.NoClearingInstructionsGroups is not null)
			for (var i = 0; i < entry.NoClearingInstructionsGroups.Count; i++)
				context.Validators.AllocationReport_NoAllocs_NoClearingInstructions(context, message, entry.NoClearingInstructionsGroups[i], i);
		if (entry.AllocSettlInstType is not null) context.Validators.AllocSettlInstType(context, message, entry.AllocSettlInstType);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoAllocs_NoMiscFees(FixContext context, FixMessage message, FixMessage.AllocationReport.NoAllocsGroup.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateAllocationReport_NoAllocs_NoClearingInstructions(FixContext context, FixMessage message, FixMessage.AllocationReport.NoAllocsGroup.NoClearingInstructionsGroup entry, int index)
	{
		context.Validators.ClearingInstruction(context, message, entry.ClearingInstruction);

		return message.IsValid;
	}

	static bool ValidateAllocationReportAck_NoAllocs(FixContext context, FixMessage message, FixMessage.AllocationReportAck.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, message, entry.AllocPrice);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (entry.IndividualAllocRejCode is not null) context.Validators.IndividualAllocRejCode(context, message, entry.IndividualAllocRejCode);
		if (entry.AllocText is not null) context.Validators.AllocText(context, message, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, message, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, message, entry.EncodedAllocText);

		return message.IsValid;
	}

	static bool ValidateAssignmentReport_NoLegs(FixContext context, FixMessage message, FixMessage.AssignmentReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateAssignmentReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.AssignmentReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateBidRequest_NoBidDescriptors(FixContext context, FixMessage message, FixMessage.BidRequest.NoBidDescriptorsGroup entry, int index)
	{
		context.Validators.BidDescriptorType(context, message, entry.BidDescriptorType);
		if (entry.BidDescriptor is not null) context.Validators.BidDescriptor(context, message, entry.BidDescriptor);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, message, entry.SideValueInd);
		if (entry.LiquidityValue is not null) context.Validators.LiquidityValue(context, message, entry.LiquidityValue);
		if (entry.LiquidityNumSecurities is not null) context.Validators.LiquidityNumSecurities(context, message, entry.LiquidityNumSecurities);
		if (entry.LiquidityPctLow is not null) context.Validators.LiquidityPctLow(context, message, entry.LiquidityPctLow);
		if (entry.LiquidityPctHigh is not null) context.Validators.LiquidityPctHigh(context, message, entry.LiquidityPctHigh);
		if (entry.EFPTrackingError is not null) context.Validators.EFPTrackingError(context, message, entry.EFPTrackingError);
		if (entry.FairValue is not null) context.Validators.FairValue(context, message, entry.FairValue);
		if (entry.OutsideIndexPct is not null) context.Validators.OutsideIndexPct(context, message, entry.OutsideIndexPct);
		if (entry.ValueOfFutures is not null) context.Validators.ValueOfFutures(context, message, entry.ValueOfFutures);

		return message.IsValid;
	}

	static bool ValidateBidRequest_NoBidComponents(FixContext context, FixMessage message, FixMessage.BidRequest.NoBidComponentsGroup entry, int index)
	{
		context.Validators.ListID(context, message, entry.ListID);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, message, entry.NetGrossInd);
		if (entry.SettlType is not null) context.Validators.SettlType(context, message, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);

		return message.IsValid;
	}

	static bool ValidateBidResponse_NoBidComponents(FixContext context, FixMessage message, FixMessage.BidResponse.NoBidComponentsGroup entry, int index)
	{
		if (Empty((ICommissionData)entry)) Absent(message, 12);
		else context.Validators.CommissionData(context, message, entry);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (entry.Country is not null) context.Validators.Country(context, message, entry.Country);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.PriceType is not null) context.Validators.PriceType(context, message, entry.PriceType);
		if (entry.FairValue is not null) context.Validators.FairValue(context, message, entry.FairValue);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, message, entry.NetGrossInd);
		if (entry.SettlType is not null) context.Validators.SettlType(context, message, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralAssignment.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralAssignment.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralAssignment.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralAssignment.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, message, entry.CollAction);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment_NoMiscFees(FixContext context, FixMessage message, FixMessage.CollateralAssignment.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry_NoCollInquiryQualifier(FixContext context, FixMessage message, FixMessage.CollateralInquiry.NoCollInquiryQualifierGroup entry, int index)
	{
		context.Validators.CollInquiryQualifier(context, message, entry.CollInquiryQualifier);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralInquiry.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralInquiry.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralInquiry.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiry_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralInquiry.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck_NoCollInquiryQualifier(FixContext context, FixMessage message, FixMessage.CollateralInquiryAck.NoCollInquiryQualifierGroup entry, int index)
	{
		context.Validators.CollInquiryQualifier(context, message, entry.CollInquiryQualifier);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralInquiryAck.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralInquiryAck.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralInquiryAck.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralInquiryAck_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralInquiryAck.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralReport_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralReport.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralReport_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralReport.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralReport_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralReport_NoMiscFees(FixContext context, FixMessage message, FixMessage.CollateralReport.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralRequest.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralRequest.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, message, entry.CollAction);

		return message.IsValid;
	}

	static bool ValidateCollateralRequest_NoMiscFees(FixContext context, FixMessage message, FixMessage.CollateralRequest.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse_NoExecs(FixContext context, FixMessage message, FixMessage.CollateralResponse.NoExecsGroup entry, int index)
	{
		context.Validators.ExecID(context, message, entry.ExecID);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse_NoTrades(FixContext context, FixMessage message, FixMessage.CollateralResponse.NoTradesGroup entry, int index)
	{
		context.Validators.TradeReportID(context, message, entry.TradeReportID);
		if (entry.SecondaryTradeReportID is not null) context.Validators.SecondaryTradeReportID(context, message, entry.SecondaryTradeReportID);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse_NoLegs(FixContext context, FixMessage message, FixMessage.CollateralResponse.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CollateralResponse.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.CollAction is not null) context.Validators.CollAction(context, message, entry.CollAction);

		return message.IsValid;
	}

	static bool ValidateCollateralResponse_NoMiscFees(FixContext context, FixMessage message, FixMessage.CollateralResponse.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateConfirmation_NoOrders(FixContext context, FixMessage message, FixMessage.Confirmation.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, message, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, message, entry.OrderBookingQty);

		return message.IsValid;
	}

	static bool ValidateConfirmation_NoUnderlyings(FixContext context, FixMessage message, FixMessage.Confirmation.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateConfirmation_NoLegs(FixContext context, FixMessage message, FixMessage.Confirmation.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateConfirmation_NoCapacities(FixContext context, FixMessage message, FixMessage.Confirmation.NoCapacitiesGroup entry, int index)
	{
		context.Validators.OrderCapacity(context, message, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, entry.OrderRestrictions);
		if (entry.OrderCapacityQty is null) Missing(message, 863, entry.OrderCapacity.Position, index);
		else context.Validators.OrderCapacityQty(context, message, entry.OrderCapacityQty);

		return message.IsValid;
	}

	static bool ValidateConfirmation_NoMiscFees(FixContext context, FixMessage message, FixMessage.Confirmation.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateConfirmationRequest_NoOrders(FixContext context, FixMessage message, FixMessage.ConfirmationRequest.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.OrderAvgPx is not null) context.Validators.OrderAvgPx(context, message, entry.OrderAvgPx);
		if (entry.OrderBookingQty is not null) context.Validators.OrderBookingQty(context, message, entry.OrderBookingQty);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest_NoSides(FixContext context, FixMessage message, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup entry, int index)
	{
		context.Validators.Side(context, message, entry.Side);
		if (entry.ClOrdID is null) Missing(message, 11, entry.Side.Position, index);
		else context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, entry.ClOrdLinkID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, message, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, message, entry.BookingUnit);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, message, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, message, entry.NoAllocs);
		Counted(message, entry.NoAllocs, entry.NoAllocsGroups);
		if (entry.NoAllocsGroups is not null)
			for (var i = 0; i < entry.NoAllocsGroups.Count; i++)
				context.Validators.CrossOrderCancelReplaceRequest_NoSides_NoAllocs(context, message, entry.NoAllocsGroups[i], i);
		if (entry.QtyType is not null) context.Validators.QtyType(context, message, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, entry);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, entry.OrderRestrictions);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, message, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, message, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, message, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, entry.CoveredOrUncovered);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, message, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, entry.ClearingFeeIndicator);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, entry.SolicitedFlag);
		if (entry.SideComplianceID is not null) context.Validators.SideComplianceID(context, message, entry.SideComplianceID);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest_NoSides_NoAllocs(FixContext context, FixMessage message, FixMessage.CrossOrderCancelReplaceRequest.NoSidesGroup.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CrossOrderCancelReplaceRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest_NoLegs(FixContext context, FixMessage message, FixMessage.CrossOrderCancelReplaceRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelReplaceRequest_NoTradingSessions(FixContext context, FixMessage message, FixMessage.CrossOrderCancelReplaceRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelRequest_NoSides(FixContext context, FixMessage message, FixMessage.CrossOrderCancelRequest.NoSidesGroup entry, int index)
	{
		context.Validators.Side(context, message, entry.Side);
		if (entry.OrigClOrdID is null) Missing(message, 41, entry.Side.Position, index);
		else context.Validators.OrigClOrdID(context, message, entry.OrigClOrdID);
		if (entry.ClOrdID is null) Missing(message, 11, entry.Side.Position, index);
		else context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, entry.ClOrdLinkID);
		if (entry.OrigOrdModTime is not null) context.Validators.OrigOrdModTime(context, message, entry.OrigOrdModTime);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, message, entry.TradeDate);
		if (Empty((IOrderQtyData)entry)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, entry);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, message, entry.ComplianceID);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.CrossOrderCancelRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateCrossOrderCancelRequest_NoLegs(FixContext context, FixMessage message, FixMessage.CrossOrderCancelRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityList_NoRelatedSym(FixContext context, FixMessage message, FixMessage.DerivativeSecurityList.NoRelatedSymGroup entry, int index)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, message, entry.ExpirationCycle);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, message, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.DerivativeSecurityList_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityList_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.DerivativeSecurityList.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateDontKnowTrade_NoUnderlyings(FixContext context, FixMessage message, FixMessage.DontKnowTrade.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateDontKnowTrade_NoLegs(FixContext context, FixMessage message, FixMessage.DontKnowTrade.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateEmail_NoRoutingIDs(FixContext context, FixMessage message, FixMessage.Email.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateEmail_NoRelatedSym(FixContext context, FixMessage message, FixMessage.Email.NoRelatedSymGroup entry, int index)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateEmail_NoUnderlyings(FixContext context, FixMessage message, FixMessage.Email.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateEmail_NoLegs(FixContext context, FixMessage message, FixMessage.Email.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateEmail_LinesOfText(FixContext context, FixMessage message, FixMessage.Email.LinesOfTextGroup entry, int index)
	{
		context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoContraBrokers(FixContext context, FixMessage message, FixMessage.ExecutionReport.NoContraBrokersGroup entry, int index)
	{
		context.Validators.ContraBroker(context, message, entry.ContraBroker);
		if (entry.ContraTrader is not null) context.Validators.ContraTrader(context, message, entry.ContraTrader);
		if (entry.ContraTradeQty is not null) context.Validators.ContraTradeQty(context, message, entry.ContraTradeQty);
		if (entry.ContraTradeTime is not null) context.Validators.ContraTradeTime(context, message, entry.ContraTradeTime);
		if (entry.ContraLegRefID is not null) context.Validators.ContraLegRefID(context, message, entry.ContraLegRefID);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.ExecutionReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoContAmts(FixContext context, FixMessage message, FixMessage.ExecutionReport.NoContAmtsGroup entry, int index)
	{
		context.Validators.ContAmtType(context, message, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, message, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, message, entry.ContAmtCurr);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoLegs(FixContext context, FixMessage message, FixMessage.ExecutionReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, message, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, message, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, message, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, message, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, message, entry.LegLastPx);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoMiscFees(FixContext context, FixMessage message, FixMessage.ExecutionReport.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateIndicationOfInterest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.IndicationOfInterest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateIndicationOfInterest_NoLegs(FixContext context, FixMessage message, FixMessage.IndicationOfInterest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegIOIQty is not null) context.Validators.LegIOIQty(context, message, entry.LegIOIQty);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateIndicationOfInterest_NoIOIQualifiers(FixContext context, FixMessage message, FixMessage.IndicationOfInterest.NoIOIQualifiersGroup entry, int index)
	{
		context.Validators.IOIQualifier(context, message, entry.IOIQualifier);

		return message.IsValid;
	}

	static bool ValidateIndicationOfInterest_NoRoutingIDs(FixContext context, FixMessage message, FixMessage.IndicationOfInterest.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateListStatus_NoOrders(FixContext context, FixMessage message, FixMessage.ListStatus.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.CumQty is null) Missing(message, 14, entry.ClOrdID.Position, index);
		else context.Validators.CumQty(context, message, entry.CumQty);
		if (entry.OrdStatus is null) Missing(message, 39, entry.ClOrdID.Position, index);
		else context.Validators.OrdStatus(context, message, entry.OrdStatus);
		if (entry.WorkingIndicator is not null) context.Validators.WorkingIndicator(context, message, entry.WorkingIndicator);
		if (entry.LeavesQty is null) Missing(message, 151, entry.ClOrdID.Position, index);
		else context.Validators.LeavesQty(context, message, entry.LeavesQty);
		if (entry.CxlQty is null) Missing(message, 84, entry.ClOrdID.Position, index);
		else context.Validators.CxlQty(context, message, entry.CxlQty);
		if (entry.AvgPx is null) Missing(message, 6, entry.ClOrdID.Position, index);
		else context.Validators.AvgPx(context, message, entry.AvgPx);
		if (entry.OrdRejReason is not null) context.Validators.OrdRejReason(context, message, entry.OrdRejReason);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice_NoStrikes(FixContext context, FixMessage message, FixMessage.ListStrikePrice.NoStrikesGroup entry, int index)
	{
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice_NoUnderlyings(FixContext context, FixMessage message, FixMessage.ListStrikePrice.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.Price is null) Missing(message, 44, entry.UnderlyingSymbol.Position, index);
		else context.Validators.Price(context, message, entry.Price);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateLogon_NoMsgTypes(FixContext context, FixMessage message, FixMessage.Logon.NoMsgTypesGroup entry, int index)
	{
		context.Validators.RefMsgType(context, message, entry.RefMsgType);
		if (entry.MsgDirection is not null) context.Validators.MsgDirection(context, message, entry.MsgDirection);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh_NoMDEntries(FixContext context, FixMessage message, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup entry, int index)
	{
		context.Validators.MDUpdateAction(context, message, entry.MDUpdateAction);
		if (entry.DeleteReason is not null) context.Validators.DeleteReason(context, message, entry.DeleteReason);
		if (entry.MDEntryType is not null) context.Validators.MDEntryType(context, message, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, message, entry.MDEntryID);
		if (entry.MDEntryRefID is not null) context.Validators.MDEntryRefID(context, message, entry.MDEntryRefID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.MarketDataIncrementalRefresh_NoMDEntries_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.FinancialStatus is not null) context.Validators.FinancialStatus(context, message, entry.FinancialStatus);
		if (entry.CorporateAction is not null) context.Validators.CorporateAction(context, message, entry.CorporateAction);
		if (entry.MDEntryPx is not null) context.Validators.MDEntryPx(context, message, entry.MDEntryPx);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, message, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, message, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, message, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, message, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, message, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, message, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, message, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, message, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);
		if (entry.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, message, entry.OpenCloseSettlFlag);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, message, entry.TimeInForce);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, message, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.MinQty is not null) context.Validators.MinQty(context, message, entry.MinQty);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, message, entry.ExecInst);
		if (entry.SellerDays is not null) context.Validators.SellerDays(context, message, entry.SellerDays);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (entry.MDEntryBuyer is not null) context.Validators.MDEntryBuyer(context, message, entry.MDEntryBuyer);
		if (entry.MDEntrySeller is not null) context.Validators.MDEntrySeller(context, message, entry.MDEntrySeller);
		if (entry.NumberOfOrders is not null) context.Validators.NumberOfOrders(context, message, entry.NumberOfOrders);
		if (entry.MDEntryPositionNo is not null) context.Validators.MDEntryPositionNo(context, message, entry.MDEntryPositionNo);
		if (entry.Scope is not null) context.Validators.Scope(context, message, entry.Scope);
		if (entry.PriceDelta is not null) context.Validators.PriceDelta(context, message, entry.PriceDelta);
		if (entry.NetChgPrevDay is not null) context.Validators.NetChgPrevDay(context, message, entry.NetChgPrevDay);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh_NoMDEntries_NoUnderlyings(FixContext context, FixMessage message, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh_NoMDEntries_NoLegs(FixContext context, FixMessage message, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoMDEntryTypes(FixContext context, FixMessage message, FixMessage.MarketDataRequest.NoMDEntryTypesGroup entry, int index)
	{
		context.Validators.MDEntryType(context, message, entry.MDEntryType);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoRelatedSym(FixContext context, FixMessage message, FixMessage.MarketDataRequest.NoRelatedSymGroup entry, int index)
	{
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.MarketDataRequest_NoRelatedSym_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.MarketDataRequest_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoRelatedSym_NoUnderlyings(FixContext context, FixMessage message, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.MarketDataRequest.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoTradingSessions(FixContext context, FixMessage message, FixMessage.MarketDataRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequestReject_NoAltMDSource(FixContext context, FixMessage message, FixMessage.MarketDataRequestReject.NoAltMDSourceGroup entry, int index)
	{
		context.Validators.AltMDSourceID(context, message, entry.AltMDSourceID);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh_NoUnderlyings(FixContext context, FixMessage message, FixMessage.MarketDataSnapshotFullRefresh.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh_NoLegs(FixContext context, FixMessage message, FixMessage.MarketDataSnapshotFullRefresh.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh_NoMDEntries(FixContext context, FixMessage message, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup entry, int index)
	{
		context.Validators.MDEntryType(context, message, entry.MDEntryType);
		if (entry.MDEntryPx is not null) context.Validators.MDEntryPx(context, message, entry.MDEntryPx);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, message, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, message, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, message, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, message, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, message, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, message, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, message, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, message, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);
		if (entry.OpenCloseSettlFlag is not null) context.Validators.OpenCloseSettlFlag(context, message, entry.OpenCloseSettlFlag);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, message, entry.TimeInForce);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, message, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.MinQty is not null) context.Validators.MinQty(context, message, entry.MinQty);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, message, entry.ExecInst);
		if (entry.SellerDays is not null) context.Validators.SellerDays(context, message, entry.SellerDays);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.QuoteEntryID is not null) context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (entry.MDEntryBuyer is not null) context.Validators.MDEntryBuyer(context, message, entry.MDEntryBuyer);
		if (entry.MDEntrySeller is not null) context.Validators.MDEntrySeller(context, message, entry.MDEntrySeller);
		if (entry.NumberOfOrders is not null) context.Validators.NumberOfOrders(context, message, entry.NumberOfOrders);
		if (entry.MDEntryPositionNo is not null) context.Validators.MDEntryPositionNo(context, message, entry.MDEntryPositionNo);
		if (entry.Scope is not null) context.Validators.Scope(context, message, entry.Scope);
		if (entry.PriceDelta is not null) context.Validators.PriceDelta(context, message, entry.PriceDelta);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMassQuote_NoQuoteSets(FixContext context, FixMessage message, FixMessage.MassQuote.NoQuoteSetsGroup entry, int index)
	{
		context.Validators.QuoteSetID(context, message, entry.QuoteSetID);
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.QuoteSetValidUntilTime is not null) context.Validators.QuoteSetValidUntilTime(context, message, entry.QuoteSetValidUntilTime);
		if (entry.TotNoQuoteEntries is null) Missing(message, 304, entry.QuoteSetID.Position, index);
		else context.Validators.TotNoQuoteEntries(context, message, entry.TotNoQuoteEntries);
		if (entry.LastFragment is not null) context.Validators.LastFragment(context, message, entry.LastFragment);
		if (entry.NoQuoteEntries is null) Missing(message, 295, entry.QuoteSetID.Position, index);
		else context.Validators.NoQuoteEntries(context, message, entry.NoQuoteEntries);
		Counted(message, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		if (entry.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < entry.NoQuoteEntriesGroups.Count; i++)
				context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries(context, message, entry.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMassQuote_NoQuoteSets_NoQuoteEntries(FixContext context, FixMessage message, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup entry, int index)
	{
		context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.BidPx is not null) context.Validators.BidPx(context, message, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, message, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, message, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, message, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, entry.OfferForwardPoints);
		if (entry.MidPx is not null) context.Validators.MidPx(context, message, entry.MidPx);
		if (entry.BidYield is not null) context.Validators.BidYield(context, message, entry.BidYield);
		if (entry.MidYield is not null) context.Validators.MidYield(context, message, entry.MidYield);
		if (entry.OfferYield is not null) context.Validators.OfferYield(context, message, entry.OfferYield);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, message, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, message, entry.BidForwardPoints2);
		if (entry.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, message, entry.OfferForwardPoints2);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);

		return message.IsValid;
	}

	static bool ValidateMassQuote_NoQuoteSets_NoQuoteEntries_NoLegs(FixContext context, FixMessage message, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement_NoQuoteSets(FixContext context, FixMessage message, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup entry, int index)
	{
		context.Validators.QuoteSetID(context, message, entry.QuoteSetID);
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.TotNoQuoteEntries is not null) context.Validators.TotNoQuoteEntries(context, message, entry.TotNoQuoteEntries);
		if (entry.LastFragment is not null) context.Validators.LastFragment(context, message, entry.LastFragment);
		if (entry.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, message, entry.NoQuoteEntries);
		Counted(message, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		if (entry.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < entry.NoQuoteEntriesGroups.Count; i++)
				context.Validators.MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(context, message, entry.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(FixContext context, FixMessage message, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup entry, int index)
	{
		context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.MassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.BidPx is not null) context.Validators.BidPx(context, message, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, message, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, message, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, message, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, entry.OfferForwardPoints);
		if (entry.MidPx is not null) context.Validators.MidPx(context, message, entry.MidPx);
		if (entry.BidYield is not null) context.Validators.BidYield(context, message, entry.BidYield);
		if (entry.MidYield is not null) context.Validators.MidYield(context, message, entry.MidYield);
		if (entry.OfferYield is not null) context.Validators.OfferYield(context, message, entry.OfferYield);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, message, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.BidForwardPoints2 is not null) context.Validators.BidForwardPoints2(context, message, entry.BidForwardPoints2);
		if (entry.OfferForwardPoints2 is not null) context.Validators.OfferForwardPoints2(context, message, entry.OfferForwardPoints2);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.QuoteEntryRejectReason is not null) context.Validators.QuoteEntryRejectReason(context, message, entry.QuoteEntryRejectReason);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries_NoLegs(FixContext context, FixMessage message, FixMessage.MassQuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest_NoAllocs(FixContext context, FixMessage message, FixMessage.MultilegOrderCancelReplaceRequest.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest_NoTradingSessions(FixContext context, FixMessage message, FixMessage.MultilegOrderCancelReplaceRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.MultilegOrderCancelReplaceRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest_NoLegs(FixContext context, FixMessage message, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, message, entry.NoLegAllocs);
		Counted(message, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		if (entry.NoLegAllocsGroups is not null)
			for (var i = 0; i < entry.NoLegAllocsGroups.Count; i++)
				context.Validators.MultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs(context, message, entry.NoLegAllocsGroups[i], i);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, message, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, message, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, message, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, message, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);

		return message.IsValid;
	}

	static bool ValidateMultilegOrderCancelReplaceRequest_NoLegs_NoLegAllocs(FixContext context, FixMessage message, FixMessage.MultilegOrderCancelReplaceRequest.NoLegsGroup.NoLegAllocsGroup entry, int index)
	{
		context.Validators.LegAllocAccount(context, message, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, message, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, message, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, message, entry.LegAllocAcctIDSource);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, message, entry.LegSettlCurrency);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusRequest_NoCompIDs(FixContext context, FixMessage message, FixMessage.NetworkStatusRequest.NoCompIDsGroup entry, int index)
	{
		context.Validators.RefCompID(context, message, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, message, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponse_NoCompIDs(FixContext context, FixMessage message, FixMessage.NetworkStatusResponse.NoCompIDsGroup entry, int index)
	{
		context.Validators.RefCompID(context, message, entry.RefCompID);
		if (entry.RefSubID is not null) context.Validators.RefSubID(context, message, entry.RefSubID);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);
		if (entry.StatusValue is not null) context.Validators.StatusValue(context, message, entry.StatusValue);
		if (entry.StatusText is not null) context.Validators.StatusText(context, message, entry.StatusText);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross_NoSides(FixContext context, FixMessage message, FixMessage.NewOrderCross.NoSidesGroup entry, int index)
	{
		context.Validators.Side(context, message, entry.Side);
		if (entry.ClOrdID is null) Missing(message, 11, entry.Side.Position, index);
		else context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, entry.ClOrdLinkID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, message, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, message, entry.BookingUnit);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, message, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, message, entry.NoAllocs);
		Counted(message, entry.NoAllocs, entry.NoAllocsGroups);
		if (entry.NoAllocsGroups is not null)
			for (var i = 0; i < entry.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderCross_NoSides_NoAllocs(context, message, entry.NoAllocsGroups[i], i);
		if (entry.QtyType is not null) context.Validators.QtyType(context, message, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, entry);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, entry.OrderRestrictions);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, message, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, message, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, message, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, entry.CoveredOrUncovered);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, message, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, entry.ClearingFeeIndicator);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, entry.SolicitedFlag);
		if (entry.SideComplianceID is not null) context.Validators.SideComplianceID(context, message, entry.SideComplianceID);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross_NoSides_NoAllocs(FixContext context, FixMessage message, FixMessage.NewOrderCross.NoSidesGroup.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross_NoUnderlyings(FixContext context, FixMessage message, FixMessage.NewOrderCross.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross_NoLegs(FixContext context, FixMessage message, FixMessage.NewOrderCross.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNewOrderCross_NoTradingSessions(FixContext context, FixMessage message, FixMessage.NewOrderCross.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders(FixContext context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListSeqNo is null) Missing(message, 67, entry.ClOrdID.Position, index);
		else context.Validators.ListSeqNo(context, message, entry.ListSeqNo);
		if (entry.ClOrdLinkID is not null) context.Validators.ClOrdLinkID(context, message, entry.ClOrdLinkID);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, message, entry.SettlInstMode);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.TradeDate is not null) context.Validators.TradeDate(context, message, entry.TradeDate);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.DayBookingInst is not null) context.Validators.DayBookingInst(context, message, entry.DayBookingInst);
		if (entry.BookingUnit is not null) context.Validators.BookingUnit(context, message, entry.BookingUnit);
		if (entry.AllocID is not null) context.Validators.AllocID(context, message, entry.AllocID);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, entry.PreallocMethod);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, message, entry.NoAllocs);
		Counted(message, entry.NoAllocs, entry.NoAllocsGroups);
		if (entry.NoAllocsGroups is not null)
			for (var i = 0; i < entry.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders_NoAllocs(context, message, entry.NoAllocsGroups[i], i);
		if (entry.SettlType is not null) context.Validators.SettlType(context, message, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.CashMargin is not null) context.Validators.CashMargin(context, message, entry.CashMargin);
		if (entry.ClearingFeeIndicator is not null) context.Validators.ClearingFeeIndicator(context, message, entry.ClearingFeeIndicator);
		if (entry.HandlInst is not null) context.Validators.HandlInst(context, message, entry.HandlInst);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, message, entry.ExecInst);
		if (entry.MinQty is not null) context.Validators.MinQty(context, message, entry.MinQty);
		if (entry.MaxFloor is not null) context.Validators.MaxFloor(context, message, entry.MaxFloor);
		if (entry.ExDestination is not null) context.Validators.ExDestination(context, message, entry.ExDestination);
		if (entry.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, entry.NoTradingSessions);
		Counted(message, entry.NoTradingSessions, entry.NoTradingSessionsGroups);
		if (entry.NoTradingSessionsGroups is not null)
			for (var i = 0; i < entry.NoTradingSessionsGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders_NoTradingSessions(context, message, entry.NoTradingSessionsGroups[i], i);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, message, entry.ProcessCode);
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.Side is null) Missing(message, 54, entry.ClOrdID.Position, index);
		else context.Validators.Side(context, message, entry.Side);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, message, entry.SideValueInd);
		if (entry.LocateReqd is not null) context.Validators.LocateReqd(context, message, entry.LocateReqd);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, message, entry);
		if (entry.QtyType is not null) context.Validators.QtyType(context, message, entry.QtyType);
		if (Empty((IOrderQtyData)entry)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, entry);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.PriceType is not null) context.Validators.PriceType(context, message, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.StopPx is not null) context.Validators.StopPx(context, message, entry.StopPx);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, message, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, message, entry);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, message, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, entry.SolicitedFlag);
		if (entry.IOIid is not null) context.Validators.IOIid(context, message, entry.IOIid);
		if (entry.QuoteID is not null) context.Validators.QuoteID(context, message, entry.QuoteID);
		if (entry.TimeInForce is not null) context.Validators.TimeInForce(context, message, entry.TimeInForce);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, entry.EffectiveTime);
		if (entry.ExpireDate is not null) context.Validators.ExpireDate(context, message, entry.ExpireDate);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, entry.GTBookingInst);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, entry.OrderRestrictions);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, entry.CustOrderCapacity);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, message, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.BookingType is not null) context.Validators.BookingType(context, message, entry.BookingType);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, message, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.Price2 is not null) context.Validators.Price2(context, message, entry.Price2);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, message, entry.PositionEffect);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, entry.CoveredOrUncovered);
		if (entry.MaxShow is not null) context.Validators.MaxShow(context, message, entry.MaxShow);
		if (!Empty((IPegInstructions)entry)) context.Validators.PegInstructions(context, message, entry);
		if (!Empty((IDiscretionInstructions)entry)) context.Validators.DiscretionInstructions(context, message, entry);
		if (entry.TargetStrategy is not null) context.Validators.TargetStrategy(context, message, entry.TargetStrategy);
		if (entry.TargetStrategyParameters is not null) context.Validators.TargetStrategyParameters(context, message, entry.TargetStrategyParameters);
		if (entry.ParticipationRate is not null) context.Validators.ParticipationRate(context, message, entry.ParticipationRate);
		if (entry.Designation is not null) context.Validators.Designation(context, message, entry.Designation);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders_NoAllocs(FixContext context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders_NoTradingSessions(FixContext context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders_NoUnderlyings(FixContext context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg_NoAllocs(FixContext context, FixMessage message, FixMessage.NewOrderMultileg.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties3)entry)) context.Validators.NestedParties3(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg_NoTradingSessions(FixContext context, FixMessage message, FixMessage.NewOrderMultileg.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg_NoUnderlyings(FixContext context, FixMessage message, FixMessage.NewOrderMultileg.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg_NoLegs(FixContext context, FixMessage message, FixMessage.NewOrderMultileg.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (entry.NoLegAllocs is not null) context.Validators.NoLegAllocs(context, message, entry.NoLegAllocs);
		Counted(message, entry.NoLegAllocs, entry.NoLegAllocsGroups);
		if (entry.NoLegAllocsGroups is not null)
			for (var i = 0; i < entry.NoLegAllocsGroups.Count; i++)
				context.Validators.NewOrderMultileg_NoLegs_NoLegAllocs(context, message, entry.NoLegAllocsGroups[i], i);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, message, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, message, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, message, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, message, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);

		return message.IsValid;
	}

	static bool ValidateNewOrderMultileg_NoLegs_NoLegAllocs(FixContext context, FixMessage message, FixMessage.NewOrderMultileg.NoLegsGroup.NoLegAllocsGroup entry, int index)
	{
		context.Validators.LegAllocAccount(context, message, entry.LegAllocAccount);
		if (entry.LegIndividualAllocID is not null) context.Validators.LegIndividualAllocID(context, message, entry.LegIndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.LegAllocQty is not null) context.Validators.LegAllocQty(context, message, entry.LegAllocQty);
		if (entry.LegAllocAcctIDSource is not null) context.Validators.LegAllocAcctIDSource(context, message, entry.LegAllocAcctIDSource);
		if (entry.LegSettlCurrency is not null) context.Validators.LegSettlCurrency(context, message, entry.LegSettlCurrency);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle_NoAllocs(FixContext context, FixMessage message, FixMessage.NewOrderSingle.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle_NoTradingSessions(FixContext context, FixMessage message, FixMessage.NewOrderSingle.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle_NoUnderlyings(FixContext context, FixMessage message, FixMessage.NewOrderSingle.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNews_NoRoutingIDs(FixContext context, FixMessage message, FixMessage.News.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateNews_NoRelatedSym(FixContext context, FixMessage message, FixMessage.News.NoRelatedSymGroup entry, int index)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNews_NoLegs(FixContext context, FixMessage message, FixMessage.News.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNews_NoUnderlyings(FixContext context, FixMessage message, FixMessage.News.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateNews_LinesOfText(FixContext context, FixMessage message, FixMessage.News.LinesOfTextGroup entry, int index)
	{
		context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest_NoAllocs(FixContext context, FixMessage message, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest_NoTradingSessions(FixContext context, FixMessage message, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.OrderCancelReplaceRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateOrderCancelRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.OrderCancelRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateOrderMassCancelReport_NoAffectedOrders(FixContext context, FixMessage message, FixMessage.OrderMassCancelReport.NoAffectedOrdersGroup entry, int index)
	{
		context.Validators.OrigClOrdID(context, message, entry.OrigClOrdID);
		if (entry.AffectedOrderID is not null) context.Validators.AffectedOrderID(context, message, entry.AffectedOrderID);
		if (entry.AffectedSecondaryOrderID is not null) context.Validators.AffectedSecondaryOrderID(context, message, entry.AffectedSecondaryOrderID);

		return message.IsValid;
	}

	static bool ValidateOrderStatusRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.OrderStatusRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceReport_NoLegs(FixContext context, FixMessage message, FixMessage.PositionMaintenanceReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.PositionMaintenanceReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceReport_NoTradingSessions(FixContext context, FixMessage message, FixMessage.PositionMaintenanceReport.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceRequest_NoLegs(FixContext context, FixMessage message, FixMessage.PositionMaintenanceRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.PositionMaintenanceRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionMaintenanceRequest_NoTradingSessions(FixContext context, FixMessage message, FixMessage.PositionMaintenanceRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidatePositionReport_NoLegs(FixContext context, FixMessage message, FixMessage.PositionReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidatePositionReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.PositionReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);
		if (entry.UnderlyingSettlPrice is null) Missing(message, 732, entry.UnderlyingSymbol.Position, index);
		else context.Validators.UnderlyingSettlPrice(context, message, entry.UnderlyingSettlPrice);
		if (entry.UnderlyingSettlPriceType is null) Missing(message, 733, entry.UnderlyingSymbol.Position, index);
		else context.Validators.UnderlyingSettlPriceType(context, message, entry.UnderlyingSettlPriceType);

		return message.IsValid;
	}

	static bool ValidateQuote_NoQuoteQualifiers(FixContext context, FixMessage message, FixMessage.Quote.NoQuoteQualifiersGroup entry, int index)
	{
		context.Validators.QuoteQualifier(context, message, entry.QuoteQualifier);

		return message.IsValid;
	}

	static bool ValidateQuote_NoUnderlyings(FixContext context, FixMessage message, FixMessage.Quote.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuote_NoLegs(FixContext context, FixMessage message, FixMessage.Quote.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, message, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, message, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, message, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel_NoQuoteEntries(FixContext context, FixMessage message, FixMessage.QuoteCancel.NoQuoteEntriesGroup entry, int index)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteCancel_NoQuoteEntries_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.QuoteCancel_NoQuoteEntries_NoLegs(context, message, entry.NoLegsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel_NoQuoteEntries_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel_NoQuoteEntries_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteCancel.NoQuoteEntriesGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest_NoRelatedSym(FixContext context, FixMessage message, FixMessage.QuoteRequest.NoRelatedSymGroup entry, int index)
	{
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteRequest_NoRelatedSym_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, message, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, message, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.QtyType is not null) context.Validators.QtyType(context, message, entry.QtyType);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, message, entry);
		if (entry.SettlType is not null) context.Validators.SettlType(context, message, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, message, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, message, entry);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.QuoteRequest_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, message, entry.NoQuoteQualifiers);
		Counted(message, entry.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups);
		if (entry.NoQuoteQualifiersGroups is not null)
			for (var i = 0; i < entry.NoQuoteQualifiersGroups.Count; i++)
				context.Validators.QuoteRequest_NoRelatedSym_NoQuoteQualifiers(context, message, entry.NoQuoteQualifiersGroups[i], i);
		if (entry.QuotePriceType is not null) context.Validators.QuotePriceType(context, message, entry.QuotePriceType);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, entry.ValidUntilTime);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, message, entry);
		if (entry.PriceType is not null) context.Validators.PriceType(context, message, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.Price2 is not null) context.Validators.Price2(context, message, entry.Price2);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, message, entry);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest_NoRelatedSym_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteRequest.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest_NoRelatedSym_NoQuoteQualifiers(FixContext context, FixMessage message, FixMessage.QuoteRequest.NoRelatedSymGroup.NoQuoteQualifiersGroup entry, int index)
	{
		context.Validators.QuoteQualifier(context, message, entry.QuoteQualifier);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject_NoRelatedSym(FixContext context, FixMessage message, FixMessage.QuoteRequestReject.NoRelatedSymGroup entry, int index)
	{
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.QuoteRequestReject_NoRelatedSym_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, message, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, message, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.TradeOriginationDate is not null) context.Validators.TradeOriginationDate(context, message, entry.TradeOriginationDate);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.QtyType is not null) context.Validators.QtyType(context, message, entry.QtyType);
		if (!Empty((IOrderQtyData)entry)) context.Validators.OrderQtyData(context, message, entry);
		if (entry.SettlType is not null) context.Validators.SettlType(context, message, entry.SettlType);
		if (entry.SettlDate is not null) context.Validators.SettlDate(context, message, entry.SettlDate);
		if (entry.SettlDate2 is not null) context.Validators.SettlDate2(context, message, entry.SettlDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, message, entry);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.QuoteRequestReject_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.NoQuoteQualifiers is not null) context.Validators.NoQuoteQualifiers(context, message, entry.NoQuoteQualifiers);
		Counted(message, entry.NoQuoteQualifiers, entry.NoQuoteQualifiersGroups);
		if (entry.NoQuoteQualifiersGroups is not null)
			for (var i = 0; i < entry.NoQuoteQualifiersGroups.Count; i++)
				context.Validators.QuoteRequestReject_NoRelatedSym_NoQuoteQualifiers(context, message, entry.NoQuoteQualifiersGroups[i], i);
		if (entry.QuotePriceType is not null) context.Validators.QuotePriceType(context, message, entry.QuotePriceType);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, message, entry);
		if (entry.PriceType is not null) context.Validators.PriceType(context, message, entry.PriceType);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.Price2 is not null) context.Validators.Price2(context, message, entry.Price2);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, message, entry);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject_NoRelatedSym_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject_NoRelatedSym_NoQuoteQualifiers(FixContext context, FixMessage message, FixMessage.QuoteRequestReject.NoRelatedSymGroup.NoQuoteQualifiersGroup entry, int index)
	{
		context.Validators.QuoteQualifier(context, message, entry.QuoteQualifier);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse_NoQuoteQualifiers(FixContext context, FixMessage message, FixMessage.QuoteResponse.NoQuoteQualifiersGroup entry, int index)
	{
		context.Validators.QuoteQualifier(context, message, entry.QuoteQualifier);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteResponse.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteResponse.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegPriceType is not null) context.Validators.LegPriceType(context, message, entry.LegPriceType);
		if (entry.LegBidPx is not null) context.Validators.LegBidPx(context, message, entry.LegBidPx);
		if (entry.LegOfferPx is not null) context.Validators.LegOfferPx(context, message, entry.LegOfferPx);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteStatusReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReport_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteStatusReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusReport_NoQuoteQualifiers(FixContext context, FixMessage message, FixMessage.QuoteStatusReport.NoQuoteQualifiersGroup entry, int index)
	{
		context.Validators.QuoteQualifier(context, message, entry.QuoteQualifier);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.QuoteStatusRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusRequest_NoLegs(FixContext context, FixMessage message, FixMessage.QuoteStatusRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRFQRequest_NoRelatedSym(FixContext context, FixMessage message, FixMessage.RFQRequest.NoRelatedSymGroup entry, int index)
	{
		if (Empty((IInstrument)entry)) Absent(message, 55);
		else context.Validators.Instrument(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.RFQRequest_NoRelatedSym_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.RFQRequest_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, message, entry.QuoteRequestType);
		if (entry.QuoteType is not null) context.Validators.QuoteType(context, message, entry.QuoteType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateRFQRequest_NoRelatedSym_NoUnderlyings(FixContext context, FixMessage message, FixMessage.RFQRequest.NoRelatedSymGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRFQRequest_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.RFQRequest.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructions_NoRegistDtls(FixContext context, FixMessage message, FixMessage.RegistrationInstructions.NoRegistDtlsGroup entry, int index)
	{
		context.Validators.RegistDtls(context, message, entry.RegistDtls);
		if (entry.RegistEmail is not null) context.Validators.RegistEmail(context, message, entry.RegistEmail);
		if (entry.MailingDtls is not null) context.Validators.MailingDtls(context, message, entry.MailingDtls);
		if (entry.MailingInst is not null) context.Validators.MailingInst(context, message, entry.MailingInst);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.OwnerType is not null) context.Validators.OwnerType(context, message, entry.OwnerType);
		if (entry.DateOfBirth is not null) context.Validators.DateOfBirth(context, message, entry.DateOfBirth);
		if (entry.InvestorCountryOfResidence is not null) context.Validators.InvestorCountryOfResidence(context, message, entry.InvestorCountryOfResidence);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructions_NoDistribInsts(FixContext context, FixMessage message, FixMessage.RegistrationInstructions.NoDistribInstsGroup entry, int index)
	{
		context.Validators.DistribPaymentMethod(context, message, entry.DistribPaymentMethod);
		if (entry.DistribPercentage is not null) context.Validators.DistribPercentage(context, message, entry.DistribPercentage);
		if (entry.CashDistribCurr is not null) context.Validators.CashDistribCurr(context, message, entry.CashDistribCurr);
		if (entry.CashDistribAgentName is not null) context.Validators.CashDistribAgentName(context, message, entry.CashDistribAgentName);
		if (entry.CashDistribAgentCode is not null) context.Validators.CashDistribAgentCode(context, message, entry.CashDistribAgentCode);
		if (entry.CashDistribAgentAcctNumber is not null) context.Validators.CashDistribAgentAcctNumber(context, message, entry.CashDistribAgentAcctNumber);
		if (entry.CashDistribPayRef is not null) context.Validators.CashDistribPayRef(context, message, entry.CashDistribPayRef);
		if (entry.CashDistribAgentAcctName is not null) context.Validators.CashDistribAgentAcctName(context, message, entry.CashDistribAgentAcctName);

		return message.IsValid;
	}

	static bool ValidateRequestForPositions_NoLegs(FixContext context, FixMessage message, FixMessage.RequestForPositions.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRequestForPositions_NoUnderlyings(FixContext context, FixMessage message, FixMessage.RequestForPositions.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRequestForPositions_NoTradingSessions(FixContext context, FixMessage message, FixMessage.RequestForPositions.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);

		return message.IsValid;
	}

	static bool ValidateRequestForPositionsAck_NoLegs(FixContext context, FixMessage message, FixMessage.RequestForPositionsAck.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateRequestForPositionsAck_NoUnderlyings(FixContext context, FixMessage message, FixMessage.RequestForPositionsAck.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityDefinition.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityDefinition.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityDefinitionRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityDefinitionRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityList_NoRelatedSym(FixContext context, FixMessage message, FixMessage.SecurityList.NoRelatedSymGroup entry, int index)
	{
		if (!Empty((IInstrument)entry)) context.Validators.Instrument(context, message, entry);
		if (!Empty((IInstrumentExtension)entry)) context.Validators.InstrumentExtension(context, message, entry);
		if (!Empty((IFinancingDetails)entry)) context.Validators.FinancingDetails(context, message, entry);
		if (entry.NoUnderlyings is not null) context.Validators.NoUnderlyings(context, message, entry.NoUnderlyings);
		Counted(message, entry.NoUnderlyings, entry.NoUnderlyingsGroups);
		if (entry.NoUnderlyingsGroups is not null)
			for (var i = 0; i < entry.NoUnderlyingsGroups.Count; i++)
				context.Validators.SecurityList_NoRelatedSym_NoUnderlyings(context, message, entry.NoUnderlyingsGroups[i], i);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, message, entry);
		if (entry.NoLegs is not null) context.Validators.NoLegs(context, message, entry.NoLegs);
		Counted(message, entry.NoLegs, entry.NoLegsGroups);
		if (entry.NoLegsGroups is not null)
			for (var i = 0; i < entry.NoLegsGroups.Count; i++)
				context.Validators.SecurityList_NoRelatedSym_NoLegs(context, message, entry.NoLegsGroups[i], i);
		if (!Empty((ISpreadOrBenchmarkCurveData)entry)) context.Validators.SpreadOrBenchmarkCurveData(context, message, entry);
		if (!Empty((IYieldData)entry)) context.Validators.YieldData(context, message, entry);
		if (entry.RoundLot is not null) context.Validators.RoundLot(context, message, entry.RoundLot);
		if (entry.MinTradeVol is not null) context.Validators.MinTradeVol(context, message, entry.MinTradeVol);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.ExpirationCycle is not null) context.Validators.ExpirationCycle(context, message, entry.ExpirationCycle);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateSecurityList_NoRelatedSym_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityList.NoRelatedSymGroup.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityList_NoRelatedSym_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityList.NoRelatedSymGroup.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (!Empty((ILegBenchmarkCurveData)entry)) context.Validators.LegBenchmarkCurveData(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityListRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequest_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityListRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityStatus.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityStatus.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.SecurityStatusRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityStatusRequest_NoLegs(FixContext context, FixMessage message, FixMessage.SecurityStatusRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateSecurityTypes_NoSecurityTypes(FixContext context, FixMessage message, FixMessage.SecurityTypes.NoSecurityTypesGroup entry, int index)
	{
		context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.SecuritySubType is not null) context.Validators.SecuritySubType(context, message, entry.SecuritySubType);
		if (entry.Product is not null) context.Validators.Product(context, message, entry.Product);
		if (entry.CFICode is not null) context.Validators.CFICode(context, message, entry.CFICode);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructions_NoSettlInst(FixContext context, FixMessage message, FixMessage.SettlementInstructions.NoSettlInstGroup entry, int index)
	{
		context.Validators.SettlInstID(context, message, entry.SettlInstID);
		if (entry.SettlInstTransType is not null) context.Validators.SettlInstTransType(context, message, entry.SettlInstTransType);
		if (entry.SettlInstRefID is not null) context.Validators.SettlInstRefID(context, message, entry.SettlInstRefID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.Product is not null) context.Validators.Product(context, message, entry.Product);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.CFICode is not null) context.Validators.CFICode(context, message, entry.CFICode);
		if (entry.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, entry.EffectiveTime);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.LastUpdateTime is not null) context.Validators.LastUpdateTime(context, message, entry.LastUpdateTime);
		if (!Empty((ISettlInstructionsData)entry)) context.Validators.SettlInstructionsData(context, message, entry);
		if (entry.PaymentMethod is not null) context.Validators.PaymentMethod(context, message, entry.PaymentMethod);
		if (entry.PaymentRef is not null) context.Validators.PaymentRef(context, message, entry.PaymentRef);
		if (entry.CardHolderName is not null) context.Validators.CardHolderName(context, message, entry.CardHolderName);
		if (entry.CardNumber is not null) context.Validators.CardNumber(context, message, entry.CardNumber);
		if (entry.CardStartDate is not null) context.Validators.CardStartDate(context, message, entry.CardStartDate);
		if (entry.CardExpDate is not null) context.Validators.CardExpDate(context, message, entry.CardExpDate);
		if (entry.CardIssNum is not null) context.Validators.CardIssNum(context, message, entry.CardIssNum);
		if (entry.PaymentDate is not null) context.Validators.PaymentDate(context, message, entry.PaymentDate);
		if (entry.PaymentRemitterID is not null) context.Validators.PaymentRemitterID(context, message, entry.PaymentRemitterID);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoUnderlyings(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoLegs(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, message, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, message, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, message, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, message, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, message, entry.LegLastPx);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoSides(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoSidesGroup entry, int index)
	{
		context.Validators.Side(context, message, entry.Side);
		if (entry.OrderID is null) Missing(message, 37, entry.Side.Position, index);
		else context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.SecondaryClOrdID is not null) context.Validators.SecondaryClOrdID(context, message, entry.SecondaryClOrdID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (!Empty((IParties)entry)) context.Validators.Parties(context, message, entry);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.AcctIDSource is not null) context.Validators.AcctIDSource(context, message, entry.AcctIDSource);
		if (entry.AccountType is not null) context.Validators.AccountType(context, message, entry.AccountType);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, message, entry.ProcessCode);
		if (entry.OddLot is not null) context.Validators.OddLot(context, message, entry.OddLot);
		if (entry.NoClearingInstructions is not null) context.Validators.NoClearingInstructions(context, message, entry.NoClearingInstructions);
		Counted(message, entry.NoClearingInstructions, entry.NoClearingInstructionsGroups);
		if (entry.NoClearingInstructionsGroups is not null)
			for (var i = 0; i < entry.NoClearingInstructionsGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoSides_NoClearingInstructions(context, message, entry.NoClearingInstructionsGroups[i], i);
		if (entry.TradeInputSource is not null) context.Validators.TradeInputSource(context, message, entry.TradeInputSource);
		if (entry.TradeInputDevice is not null) context.Validators.TradeInputDevice(context, message, entry.TradeInputDevice);
		if (entry.OrderInputDevice is not null) context.Validators.OrderInputDevice(context, message, entry.OrderInputDevice);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.ComplianceID is not null) context.Validators.ComplianceID(context, message, entry.ComplianceID);
		if (entry.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, entry.SolicitedFlag);
		if (entry.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, entry.OrderCapacity);
		if (entry.OrderRestrictions is not null) context.Validators.OrderRestrictions(context, message, entry.OrderRestrictions);
		if (entry.CustOrderCapacity is not null) context.Validators.CustOrderCapacity(context, message, entry.CustOrderCapacity);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, message, entry.ExecInst);
		if (entry.TransBkdTime is not null) context.Validators.TransBkdTime(context, message, entry.TransBkdTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.TradingSessionSubID is not null) context.Validators.TradingSessionSubID(context, message, entry.TradingSessionSubID);
		if (entry.TimeBracket is not null) context.Validators.TimeBracket(context, message, entry.TimeBracket);
		if (!Empty((ICommissionData)entry)) context.Validators.CommissionData(context, message, entry);
		if (entry.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, entry.GrossTradeAmt);
		if (entry.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, entry.NumDaysInterest);
		if (entry.ExDate is not null) context.Validators.ExDate(context, message, entry.ExDate);
		if (entry.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, entry.AccruedInterestRate);
		if (entry.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, entry.AccruedInterestAmt);
		if (entry.InterestAtMaturity is not null) context.Validators.InterestAtMaturity(context, message, entry.InterestAtMaturity);
		if (entry.EndAccruedInterestAmt is not null) context.Validators.EndAccruedInterestAmt(context, message, entry.EndAccruedInterestAmt);
		if (entry.StartCash is not null) context.Validators.StartCash(context, message, entry.StartCash);
		if (entry.EndCash is not null) context.Validators.EndCash(context, message, entry.EndCash);
		if (entry.Concession is not null) context.Validators.Concession(context, message, entry.Concession);
		if (entry.TotalTakedown is not null) context.Validators.TotalTakedown(context, message, entry.TotalTakedown);
		if (entry.NetMoney is not null) context.Validators.NetMoney(context, message, entry.NetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, entry.SettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, entry.SettlCurrFxRateCalc);
		if (entry.PositionEffect is not null) context.Validators.PositionEffect(context, message, entry.PositionEffect);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);
		if (entry.SideMultiLegReportingType is not null) context.Validators.SideMultiLegReportingType(context, message, entry.SideMultiLegReportingType);
		if (entry.NoContAmts is not null) context.Validators.NoContAmts(context, message, entry.NoContAmts);
		Counted(message, entry.NoContAmts, entry.NoContAmtsGroups);
		if (entry.NoContAmtsGroups is not null)
			for (var i = 0; i < entry.NoContAmtsGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoSides_NoContAmts(context, message, entry.NoContAmtsGroups[i], i);
		if (!Empty((IStipulations)entry)) context.Validators.Stipulations(context, message, entry);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, entry.NoMiscFees);
		Counted(message, entry.NoMiscFees, entry.NoMiscFeesGroups);
		if (entry.NoMiscFeesGroups is not null)
			for (var i = 0; i < entry.NoMiscFeesGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoSides_NoMiscFees(context, message, entry.NoMiscFeesGroups[i], i);
		if (entry.ExchangeRule is not null) context.Validators.ExchangeRule(context, message, entry.ExchangeRule);
		if (entry.TradeAllocIndicator is not null) context.Validators.TradeAllocIndicator(context, message, entry.TradeAllocIndicator);
		if (entry.PreallocMethod is not null) context.Validators.PreallocMethod(context, message, entry.PreallocMethod);
		if (entry.AllocID is not null) context.Validators.AllocID(context, message, entry.AllocID);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, message, entry.NoAllocs);
		Counted(message, entry.NoAllocs, entry.NoAllocsGroups);
		if (entry.NoAllocsGroups is not null)
			for (var i = 0; i < entry.NoAllocsGroups.Count; i++)
				context.Validators.TradeCaptureReport_NoSides_NoAllocs(context, message, entry.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoSides_NoClearingInstructions(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup entry, int index)
	{
		context.Validators.ClearingInstruction(context, message, entry.ClearingInstruction);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoSides_NoContAmts(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoSidesGroup.NoContAmtsGroup entry, int index)
	{
		context.Validators.ContAmtType(context, message, entry.ContAmtType);
		if (entry.ContAmtValue is not null) context.Validators.ContAmtValue(context, message, entry.ContAmtValue);
		if (entry.ContAmtCurr is not null) context.Validators.ContAmtCurr(context, message, entry.ContAmtCurr);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoSides_NoMiscFees(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoSidesGroup.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);
		if (entry.MiscFeeBasis is not null) context.Validators.MiscFeeBasis(context, message, entry.MiscFeeBasis);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReport_NoSides_NoAllocs(FixContext context, FixMessage message, FixMessage.TradeCaptureReport.NoSidesGroup.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportAck_NoLegs(FixContext context, FixMessage message, FixMessage.TradeCaptureReportAck.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);
		if (entry.LegQty is not null) context.Validators.LegQty(context, message, entry.LegQty);
		if (entry.LegSwapType is not null) context.Validators.LegSwapType(context, message, entry.LegSwapType);
		if (!Empty((ILegStipulations)entry)) context.Validators.LegStipulations(context, message, entry);
		if (entry.LegPositionEffect is not null) context.Validators.LegPositionEffect(context, message, entry.LegPositionEffect);
		if (entry.LegCoveredOrUncovered is not null) context.Validators.LegCoveredOrUncovered(context, message, entry.LegCoveredOrUncovered);
		if (!Empty((INestedParties)entry)) context.Validators.NestedParties(context, message, entry);
		if (entry.LegRefID is not null) context.Validators.LegRefID(context, message, entry.LegRefID);
		if (entry.LegPrice is not null) context.Validators.LegPrice(context, message, entry.LegPrice);
		if (entry.LegSettlType is not null) context.Validators.LegSettlType(context, message, entry.LegSettlType);
		if (entry.LegSettlDate is not null) context.Validators.LegSettlDate(context, message, entry.LegSettlDate);
		if (entry.LegLastPx is not null) context.Validators.LegLastPx(context, message, entry.LegLastPx);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportAck_NoAllocs(FixContext context, FixMessage message, FixMessage.TradeCaptureReportAck.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocAcctIDSource is not null) context.Validators.AllocAcctIDSource(context, message, entry.AllocAcctIDSource);
		if (entry.AllocSettlCurrency is not null) context.Validators.AllocSettlCurrency(context, message, entry.AllocSettlCurrency);
		if (entry.IndividualAllocID is not null) context.Validators.IndividualAllocID(context, message, entry.IndividualAllocID);
		if (!Empty((INestedParties2)entry)) context.Validators.NestedParties2(context, message, entry);
		if (entry.AllocQty is not null) context.Validators.AllocQty(context, message, entry.AllocQty);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequest_NoUnderlyings(FixContext context, FixMessage message, FixMessage.TradeCaptureReportRequest.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequest_NoLegs(FixContext context, FixMessage message, FixMessage.TradeCaptureReportRequest.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequest_NoDates(FixContext context, FixMessage message, FixMessage.TradeCaptureReportRequest.NoDatesGroup entry, int index)
	{
		context.Validators.TradeDate(context, message, entry.TradeDate);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequestAck_NoUnderlyings(FixContext context, FixMessage message, FixMessage.TradeCaptureReportRequestAck.NoUnderlyingsGroup entry, int index)
	{
		if (!Empty((IUnderlyingInstrument)entry)) context.Validators.UnderlyingInstrument(context, message, entry);

		return message.IsValid;
	}

	static bool ValidateTradeCaptureReportRequestAck_NoLegs(FixContext context, FixMessage message, FixMessage.TradeCaptureReportRequestAck.NoLegsGroup entry, int index)
	{
		if (!Empty((IInstrumentLeg)entry)) context.Validators.InstrumentLeg(context, message, entry);

		return message.IsValid;
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
