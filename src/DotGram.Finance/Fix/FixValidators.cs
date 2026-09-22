using System;
using System.Collections.Generic;
using System.Numerics;

namespace DotGram.Finance.Fix;

/// <summary>
/// The check of every FIX 4.4 message type, of every block it reuses, and of every field whose
/// value the specification limits.
/// </summary>
/// <remarks>
/// <para>
/// Straight-line checks written against the fields of the class they are about, and nothing
/// else: no tables, no schema read at run time, no question asked of anything but the message.
/// What each one asks is what the published repository says — a field it marks required, the
/// counter of a repeating component it marks required, a block it marks required and the message
/// left empty, and the values it lists for a field that has a set of them.
/// </para>
/// <para>
/// Three kinds of slot, every one typed on what it checks, so a check cannot be put in the wrong
/// one: ninety-four message types, thirteen blocks through the interface their carriers
/// implement, and two hundred and twenty-one fields. A carrier asks the block slot; a message
/// asks the field slot of every limited field it holds. That is what makes a loaded dictionary
/// small: a venue that adds a value to OrdType replaces one slot, not the forty-one message
/// types that carry an OrdType.
/// </para>
/// <para>
/// The repository requires nothing inside any of the thirteen blocks, so those thirteen checks
/// find nothing today. They are here because the question belongs to the block.
/// </para>
/// <para>
/// One instance a context, and a context takes <see cref="Default"/> unless it is given another.
/// </para>
/// <para>
/// What is not here yet: what a group entry requires of itself, and the limited fields an entry
/// carries, which are the same two questions one level down.
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

	// The blocks. A carrier asks these of the block it has, and the message it belongs to is where
	// a finding goes, since a block is not a thing a consumer holds on its own.

	/// <summary>Holds a FIX 4.4 CommissionData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ICommissionData, bool> CommissionData                          { get; set; } = ValidateCommissionData;

	/// <summary>Holds a FIX 4.4 DiscretionInstructions block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IDiscretionInstructions, bool> DiscretionInstructions                  { get; set; } = ValidateDiscretionInstructions;

	/// <summary>Holds a FIX 4.4 FinancingDetails block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IFinancingDetails, bool> FinancingDetails                        { get; set; } = ValidateFinancingDetails;

	/// <summary>Holds a FIX 4.4 Instrument block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrument, bool> Instrument                              { get; set; } = ValidateInstrument;

	/// <summary>Holds a FIX 4.4 InstrumentExtension block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrumentExtension, bool> InstrumentExtension                     { get; set; } = ValidateInstrumentExtension;

	/// <summary>Holds a FIX 4.4 InstrumentLeg block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IInstrumentLeg, bool> InstrumentLeg                           { get; set; } = ValidateInstrumentLeg;

	/// <summary>Holds a FIX 4.4 LegBenchmarkCurveData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ILegBenchmarkCurveData, bool> LegBenchmarkCurveData                   { get; set; } = ValidateLegBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 OrderQtyData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IOrderQtyData, bool> OrderQtyData                            { get; set; } = ValidateOrderQtyData;

	/// <summary>Holds a FIX 4.4 PegInstructions block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IPegInstructions, bool> PegInstructions                         { get; set; } = ValidatePegInstructions;

	/// <summary>Holds a FIX 4.4 SettlInstructionsData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ISettlInstructionsData, bool> SettlInstructionsData                   { get; set; } = ValidateSettlInstructionsData;

	/// <summary>Holds a FIX 4.4 SpreadOrBenchmarkCurveData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, ISpreadOrBenchmarkCurveData, bool> SpreadOrBenchmarkCurveData              { get; set; } = ValidateSpreadOrBenchmarkCurveData;

	/// <summary>Holds a FIX 4.4 UnderlyingInstrument block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IUnderlyingInstrument, bool> UnderlyingInstrument                    { get; set; } = ValidateUnderlyingInstrument;

	/// <summary>Holds a FIX 4.4 YieldData block to the schema, wherever it is carried.</summary>
	public Func<FixContext, FixMessage, IYieldData, bool> YieldData                               { get; set; } = ValidateYieldData;

	// The fields the specification gives a set of values. A field that has none is not here:
	// whether its characters fitted its type, the field answered when it was read.

	/// <summary>Holds a FIX 4.4 AdvSide, tag 4, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdvSide, bool> AdvSide                                 { get; set; } = ValidateAdvSide;

	/// <summary>Holds a FIX 4.4 AdvTransType, tag 5, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdvTransType, bool> AdvTransType                            { get; set; } = ValidateAdvTransType;

	/// <summary>Holds a FIX 4.4 CommType, tag 13, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CommType, bool> CommType                                { get; set; } = ValidateCommType;

	/// <summary>Holds a FIX 4.4 ExecInst, tag 18, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecInst, bool> ExecInst                                { get; set; } = ValidateExecInst;

	/// <summary>Holds a FIX 4.4 HandlInst, tag 21, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.HandlInst, bool> HandlInst                               { get; set; } = ValidateHandlInst;

	/// <summary>Holds a FIX 4.4 SecurityIDSource, tag 22, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityIDSource, bool> SecurityIDSource                        { get; set; } = ValidateSecurityIDSource;

	/// <summary>Holds a FIX 4.4 IOIQltyInd, tag 25, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQltyInd, bool> IOIQltyInd                              { get; set; } = ValidateIOIQltyInd;

	/// <summary>Holds a FIX 4.4 IOIQty, tag 27, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQty, bool> IOIQty                                  { get; set; } = ValidateIOIQty;

	/// <summary>Holds a FIX 4.4 IOITransType, tag 28, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOITransType, bool> IOITransType                            { get; set; } = ValidateIOITransType;

	/// <summary>Holds a FIX 4.4 LastCapacity, tag 29, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LastCapacity, bool> LastCapacity                            { get; set; } = ValidateLastCapacity;

	/// <summary>Holds a FIX 4.4 MsgType, tag 35, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MsgType, bool> MsgType                                 { get; set; } = ValidateMsgType;

	/// <summary>Holds a FIX 4.4 OrdStatus, tag 39, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdStatus, bool> OrdStatus                               { get; set; } = ValidateOrdStatus;

	/// <summary>Holds a FIX 4.4 OrdType, tag 40, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdType, bool> OrdType                                 { get; set; } = ValidateOrdType;

	/// <summary>Holds a FIX 4.4 Side, tag 54, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Side, bool> Side                                    { get; set; } = ValidateSide;

	/// <summary>Holds a FIX 4.4 TimeInForce, tag 59, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TimeInForce, bool> TimeInForce                             { get; set; } = ValidateTimeInForce;

	/// <summary>Holds a FIX 4.4 Urgency, tag 61, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Urgency, bool> Urgency                                 { get; set; } = ValidateUrgency;

	/// <summary>Holds a FIX 4.4 SettlType, tag 63, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlType, bool> SettlType                               { get; set; } = ValidateSettlType;

	/// <summary>Holds a FIX 4.4 AllocTransType, tag 71, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocTransType, bool> AllocTransType                          { get; set; } = ValidateAllocTransType;

	/// <summary>Holds a FIX 4.4 PositionEffect, tag 77, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PositionEffect, bool> PositionEffect                          { get; set; } = ValidatePositionEffect;

	/// <summary>Holds a FIX 4.4 ProcessCode, tag 81, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ProcessCode, bool> ProcessCode                             { get; set; } = ValidateProcessCode;

	/// <summary>Holds a FIX 4.4 AllocStatus, tag 87, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocStatus, bool> AllocStatus                             { get; set; } = ValidateAllocStatus;

	/// <summary>Holds a FIX 4.4 AllocRejCode, tag 88, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocRejCode, bool> AllocRejCode                            { get; set; } = ValidateAllocRejCode;

	/// <summary>Holds a FIX 4.4 EmailType, tag 94, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EmailType, bool> EmailType                               { get; set; } = ValidateEmailType;

	/// <summary>Holds a FIX 4.4 EncryptMethod, tag 98, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EncryptMethod, bool> EncryptMethod                           { get; set; } = ValidateEncryptMethod;

	/// <summary>Holds a FIX 4.4 CxlRejReason, tag 102, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CxlRejReason, bool> CxlRejReason                            { get; set; } = ValidateCxlRejReason;

	/// <summary>Holds a FIX 4.4 OrdRejReason, tag 103, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OrdRejReason, bool> OrdRejReason                            { get; set; } = ValidateOrdRejReason;

	/// <summary>Holds a FIX 4.4 IOIQualifier, tag 104, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IOIQualifier, bool> IOIQualifier                            { get; set; } = ValidateIOIQualifier;

	/// <summary>Holds a FIX 4.4 DKReason, tag 127, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DKReason, bool> DKReason                                { get; set; } = ValidateDKReason;

	/// <summary>Holds a FIX 4.4 MiscFeeType, tag 139, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeType, bool> MiscFeeType                             { get; set; } = ValidateMiscFeeType;

	/// <summary>Holds a FIX 4.4 ExecType, tag 150, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecType, bool> ExecType                                { get; set; } = ValidateExecType;

	/// <summary>Holds a FIX 4.4 SettlCurrFxRateCalc, tag 156, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlCurrFxRateCalc, bool> SettlCurrFxRateCalc                     { get; set; } = ValidateSettlCurrFxRateCalc;

	/// <summary>Holds a FIX 4.4 SettlInstMode, tag 160, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstMode, bool> SettlInstMode                           { get; set; } = ValidateSettlInstMode;

	/// <summary>Holds a FIX 4.4 SettlInstTransType, tag 163, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstTransType, bool> SettlInstTransType                      { get; set; } = ValidateSettlInstTransType;

	/// <summary>Holds a FIX 4.4 SettlInstSource, tag 165, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstSource, bool> SettlInstSource                         { get; set; } = ValidateSettlInstSource;

	/// <summary>Holds a FIX 4.4 SecurityType, tag 167, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityType, bool> SecurityType                            { get; set; } = ValidateSecurityType;

	/// <summary>Holds a FIX 4.4 StandInstDbType, tag 169, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StandInstDbType, bool> StandInstDbType                         { get; set; } = ValidateStandInstDbType;

	/// <summary>Holds a FIX 4.4 SettlDeliveryType, tag 172, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlDeliveryType, bool> SettlDeliveryType                       { get; set; } = ValidateSettlDeliveryType;

	/// <summary>Holds a FIX 4.4 AllocLinkType, tag 197, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocLinkType, bool> AllocLinkType                           { get; set; } = ValidateAllocLinkType;

	/// <summary>Holds a FIX 4.4 PutOrCall, tag 201, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PutOrCall, bool> PutOrCall                               { get; set; } = ValidatePutOrCall;

	/// <summary>Holds a FIX 4.4 CoveredOrUncovered, tag 203, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CoveredOrUncovered, bool> CoveredOrUncovered                      { get; set; } = ValidateCoveredOrUncovered;

	/// <summary>Holds a FIX 4.4 AllocHandlInst, tag 209, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocHandlInst, bool> AllocHandlInst                          { get; set; } = ValidateAllocHandlInst;

	/// <summary>Holds a FIX 4.4 RoutingType, tag 216, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RoutingType, bool> RoutingType                             { get; set; } = ValidateRoutingType;

	/// <summary>Holds a FIX 4.4 StipulationType, tag 233, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StipulationType, bool> StipulationType                         { get; set; } = ValidateStipulationType;

	/// <summary>Holds a FIX 4.4 YieldType, tag 235, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.YieldType, bool> YieldType                               { get; set; } = ValidateYieldType;

	/// <summary>Holds a FIX 4.4 SubscriptionRequestType, tag 263, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SubscriptionRequestType, bool> SubscriptionRequestType                 { get; set; } = ValidateSubscriptionRequestType;

	/// <summary>Holds a FIX 4.4 MDUpdateType, tag 265, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDUpdateType, bool> MDUpdateType                            { get; set; } = ValidateMDUpdateType;

	/// <summary>Holds a FIX 4.4 MDEntryType, tag 269, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDEntryType, bool> MDEntryType                             { get; set; } = ValidateMDEntryType;

	/// <summary>Holds a FIX 4.4 TickDirection, tag 274, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TickDirection, bool> TickDirection                           { get; set; } = ValidateTickDirection;

	/// <summary>Holds a FIX 4.4 QuoteCondition, tag 276, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteCondition, bool> QuoteCondition                          { get; set; } = ValidateQuoteCondition;

	/// <summary>Holds a FIX 4.4 TradeCondition, tag 277, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeCondition, bool> TradeCondition                          { get; set; } = ValidateTradeCondition;

	/// <summary>Holds a FIX 4.4 MDUpdateAction, tag 279, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDUpdateAction, bool> MDUpdateAction                          { get; set; } = ValidateMDUpdateAction;

	/// <summary>Holds a FIX 4.4 MDReqRejReason, tag 281, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MDReqRejReason, bool> MDReqRejReason                          { get; set; } = ValidateMDReqRejReason;

	/// <summary>Holds a FIX 4.4 DeleteReason, tag 285, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeleteReason, bool> DeleteReason                            { get; set; } = ValidateDeleteReason;

	/// <summary>Holds a FIX 4.4 OpenCloseSettlFlag, tag 286, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OpenCloseSettlFlag, bool> OpenCloseSettlFlag                      { get; set; } = ValidateOpenCloseSettlFlag;

	/// <summary>Holds a FIX 4.4 FinancialStatus, tag 291, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.FinancialStatus, bool> FinancialStatus                         { get; set; } = ValidateFinancialStatus;

	/// <summary>Holds a FIX 4.4 CorporateAction, tag 292, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CorporateAction, bool> CorporateAction                         { get; set; } = ValidateCorporateAction;

	/// <summary>Holds a FIX 4.4 QuoteStatus, tag 297, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteStatus, bool> QuoteStatus                             { get; set; } = ValidateQuoteStatus;

	/// <summary>Holds a FIX 4.4 QuoteCancelType, tag 298, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteCancelType, bool> QuoteCancelType                         { get; set; } = ValidateQuoteCancelType;

	/// <summary>Holds a FIX 4.4 QuoteRejectReason, tag 300, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRejectReason, bool> QuoteRejectReason                       { get; set; } = ValidateQuoteRejectReason;

	/// <summary>Holds a FIX 4.4 QuoteResponseLevel, tag 301, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteResponseLevel, bool> QuoteResponseLevel                      { get; set; } = ValidateQuoteResponseLevel;

	/// <summary>Holds a FIX 4.4 QuoteRequestType, tag 303, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRequestType, bool> QuoteRequestType                        { get; set; } = ValidateQuoteRequestType;

	/// <summary>Holds a FIX 4.4 SecurityRequestType, tag 321, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityRequestType, bool> SecurityRequestType                     { get; set; } = ValidateSecurityRequestType;

	/// <summary>Holds a FIX 4.4 SecurityResponseType, tag 323, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityResponseType, bool> SecurityResponseType                    { get; set; } = ValidateSecurityResponseType;

	/// <summary>Holds a FIX 4.4 SecurityTradingStatus, tag 326, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityTradingStatus, bool> SecurityTradingStatus                   { get; set; } = ValidateSecurityTradingStatus;

	/// <summary>Holds a FIX 4.4 HaltReason, tag 327, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.HaltReason, bool> HaltReason                              { get; set; } = ValidateHaltReason;

	/// <summary>Holds a FIX 4.4 Adjustment, tag 334, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Adjustment, bool> Adjustment                              { get; set; } = ValidateAdjustment;

	/// <summary>Holds a FIX 4.4 TradSesMethod, tag 338, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesMethod, bool> TradSesMethod                           { get; set; } = ValidateTradSesMethod;

	/// <summary>Holds a FIX 4.4 TradSesMode, tag 339, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesMode, bool> TradSesMode                             { get; set; } = ValidateTradSesMode;

	/// <summary>Holds a FIX 4.4 TradSesStatus, tag 340, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesStatus, bool> TradSesStatus                           { get; set; } = ValidateTradSesStatus;

	/// <summary>Holds a FIX 4.4 MessageEncoding, tag 347, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MessageEncoding, bool> MessageEncoding                         { get; set; } = ValidateMessageEncoding;

	/// <summary>Holds a FIX 4.4 SessionRejectReason, tag 373, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SessionRejectReason, bool> SessionRejectReason                     { get; set; } = ValidateSessionRejectReason;

	/// <summary>Holds a FIX 4.4 BidRequestTransType, tag 374, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidRequestTransType, bool> BidRequestTransType                     { get; set; } = ValidateBidRequestTransType;

	/// <summary>Holds a FIX 4.4 ExecRestatementReason, tag 378, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecRestatementReason, bool> ExecRestatementReason                   { get; set; } = ValidateExecRestatementReason;

	/// <summary>Holds a FIX 4.4 BusinessRejectReason, tag 380, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BusinessRejectReason, bool> BusinessRejectReason                    { get; set; } = ValidateBusinessRejectReason;

	/// <summary>Holds a FIX 4.4 MsgDirection, tag 385, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MsgDirection, bool> MsgDirection                            { get; set; } = ValidateMsgDirection;

	/// <summary>Holds a FIX 4.4 DiscretionInst, tag 388, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionInst, bool> DiscretionInst                          { get; set; } = ValidateDiscretionInst;

	/// <summary>Holds a FIX 4.4 BidType, tag 394, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidType, bool> BidType                                 { get; set; } = ValidateBidType;

	/// <summary>Holds a FIX 4.4 BidDescriptorType, tag 399, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidDescriptorType, bool> BidDescriptorType                       { get; set; } = ValidateBidDescriptorType;

	/// <summary>Holds a FIX 4.4 SideValueInd, tag 401, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SideValueInd, bool> SideValueInd                            { get; set; } = ValidateSideValueInd;

	/// <summary>Holds a FIX 4.4 LiquidityIndType, tag 409, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LiquidityIndType, bool> LiquidityIndType                        { get; set; } = ValidateLiquidityIndType;

	/// <summary>Holds a FIX 4.4 ProgRptReqs, tag 414, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ProgRptReqs, bool> ProgRptReqs                             { get; set; } = ValidateProgRptReqs;

	/// <summary>Holds a FIX 4.4 IncTaxInd, tag 416, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.IncTaxInd, bool> IncTaxInd                               { get; set; } = ValidateIncTaxInd;

	/// <summary>Holds a FIX 4.4 BidTradeType, tag 418, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BidTradeType, bool> BidTradeType                            { get; set; } = ValidateBidTradeType;

	/// <summary>Holds a FIX 4.4 BasisPxType, tag 419, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BasisPxType, bool> BasisPxType                             { get; set; } = ValidateBasisPxType;

	/// <summary>Holds a FIX 4.4 PriceType, tag 423, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PriceType, bool> PriceType                               { get; set; } = ValidatePriceType;

	/// <summary>Holds a FIX 4.4 GTBookingInst, tag 427, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.GTBookingInst, bool> GTBookingInst                           { get; set; } = ValidateGTBookingInst;

	/// <summary>Holds a FIX 4.4 ListStatusType, tag 429, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListStatusType, bool> ListStatusType                          { get; set; } = ValidateListStatusType;

	/// <summary>Holds a FIX 4.4 NetGrossInd, tag 430, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetGrossInd, bool> NetGrossInd                             { get; set; } = ValidateNetGrossInd;

	/// <summary>Holds a FIX 4.4 ListOrderStatus, tag 431, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListOrderStatus, bool> ListOrderStatus                         { get; set; } = ValidateListOrderStatus;

	/// <summary>Holds a FIX 4.4 ListExecInstType, tag 433, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ListExecInstType, bool> ListExecInstType                        { get; set; } = ValidateListExecInstType;

	/// <summary>Holds a FIX 4.4 CxlRejResponseTo, tag 434, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CxlRejResponseTo, bool> CxlRejResponseTo                        { get; set; } = ValidateCxlRejResponseTo;

	/// <summary>Holds a FIX 4.4 MultiLegReportingType, tag 442, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MultiLegReportingType, bool> MultiLegReportingType                   { get; set; } = ValidateMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 PartyIDSource, tag 447, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartyIDSource, bool> PartyIDSource                           { get; set; } = ValidatePartyIDSource;

	/// <summary>Holds a FIX 4.4 PartyRole, tag 452, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartyRole, bool> PartyRole                               { get; set; } = ValidatePartyRole;

	/// <summary>Holds a FIX 4.4 Product, tag 460, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Product, bool> Product                                 { get; set; } = ValidateProduct;

	/// <summary>Holds a FIX 4.4 RoundingDirection, tag 468, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RoundingDirection, bool> RoundingDirection                       { get; set; } = ValidateRoundingDirection;

	/// <summary>Holds a FIX 4.4 DistribPaymentMethod, tag 477, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DistribPaymentMethod, bool> DistribPaymentMethod                    { get; set; } = ValidateDistribPaymentMethod;

	/// <summary>Holds a FIX 4.4 CancellationRights, tag 480, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CancellationRights, bool> CancellationRights                      { get; set; } = ValidateCancellationRights;

	/// <summary>Holds a FIX 4.4 MoneyLaunderingStatus, tag 481, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MoneyLaunderingStatus, bool> MoneyLaunderingStatus                   { get; set; } = ValidateMoneyLaunderingStatus;

	/// <summary>Holds a FIX 4.4 ExecPriceType, tag 484, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExecPriceType, bool> ExecPriceType                           { get; set; } = ValidateExecPriceType;

	/// <summary>Holds a FIX 4.4 PaymentMethod, tag 492, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PaymentMethod, bool> PaymentMethod                           { get; set; } = ValidatePaymentMethod;

	/// <summary>Holds a FIX 4.4 TaxAdvantageType, tag 495, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TaxAdvantageType, bool> TaxAdvantageType                        { get; set; } = ValidateTaxAdvantageType;

	/// <summary>Holds a FIX 4.4 FundRenewWaiv, tag 497, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.FundRenewWaiv, bool> FundRenewWaiv                           { get; set; } = ValidateFundRenewWaiv;

	/// <summary>Holds a FIX 4.4 RegistStatus, tag 506, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistStatus, bool> RegistStatus                            { get; set; } = ValidateRegistStatus;

	/// <summary>Holds a FIX 4.4 RegistRejReasonCode, tag 507, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistRejReasonCode, bool> RegistRejReasonCode                     { get; set; } = ValidateRegistRejReasonCode;

	/// <summary>Holds a FIX 4.4 RegistTransType, tag 514, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.RegistTransType, bool> RegistTransType                         { get; set; } = ValidateRegistTransType;

	/// <summary>Holds a FIX 4.4 OwnershipType, tag 517, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OwnershipType, bool> OwnershipType                           { get; set; } = ValidateOwnershipType;

	/// <summary>Holds a FIX 4.4 ContAmtType, tag 519, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ContAmtType, bool> ContAmtType                             { get; set; } = ValidateContAmtType;

	/// <summary>Holds a FIX 4.4 OwnerType, tag 522, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.OwnerType, bool> OwnerType                               { get; set; } = ValidateOwnerType;

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

	/// <summary>Holds a FIX 4.4 QuoteType, tag 537, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteType, bool> QuoteType                               { get; set; } = ValidateQuoteType;

	/// <summary>Holds a FIX 4.4 CashMargin, tag 544, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CashMargin, bool> CashMargin                              { get; set; } = ValidateCashMargin;

	/// <summary>Holds a FIX 4.4 Scope, tag 546, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.Scope, bool> Scope                                   { get; set; } = ValidateScope;

	/// <summary>Holds a FIX 4.4 CrossType, tag 549, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CrossType, bool> CrossType                               { get; set; } = ValidateCrossType;

	/// <summary>Holds a FIX 4.4 CrossPrioritization, tag 550, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CrossPrioritization, bool> CrossPrioritization                     { get; set; } = ValidateCrossPrioritization;

	/// <summary>Holds a FIX 4.4 NoSides, tag 552, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NoSides, bool> NoSides                                 { get; set; } = ValidateNoSides;

	/// <summary>Holds a FIX 4.4 SecurityListRequestType, tag 559, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityListRequestType, bool> SecurityListRequestType                 { get; set; } = ValidateSecurityListRequestType;

	/// <summary>Holds a FIX 4.4 SecurityRequestResult, tag 560, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SecurityRequestResult, bool> SecurityRequestResult                   { get; set; } = ValidateSecurityRequestResult;

	/// <summary>Holds a FIX 4.4 MultiLegRptTypeReq, tag 563, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MultiLegRptTypeReq, bool> MultiLegRptTypeReq                      { get; set; } = ValidateMultiLegRptTypeReq;

	/// <summary>Holds a FIX 4.4 TradSesStatusRejReason, tag 567, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradSesStatusRejReason, bool> TradSesStatusRejReason                  { get; set; } = ValidateTradSesStatusRejReason;

	/// <summary>Holds a FIX 4.4 TradeRequestType, tag 569, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestType, bool> TradeRequestType                        { get; set; } = ValidateTradeRequestType;

	/// <summary>Holds a FIX 4.4 MatchStatus, tag 573, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MatchStatus, bool> MatchStatus                             { get; set; } = ValidateMatchStatus;

	/// <summary>Holds a FIX 4.4 MatchType, tag 574, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MatchType, bool> MatchType                               { get; set; } = ValidateMatchType;

	/// <summary>Holds a FIX 4.4 ClearingInstruction, tag 577, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ClearingInstruction, bool> ClearingInstruction                     { get; set; } = ValidateClearingInstruction;

	/// <summary>Holds a FIX 4.4 AccountType, tag 581, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AccountType, bool> AccountType                             { get; set; } = ValidateAccountType;

	/// <summary>Holds a FIX 4.4 CustOrderCapacity, tag 582, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CustOrderCapacity, bool> CustOrderCapacity                       { get; set; } = ValidateCustOrderCapacity;

	/// <summary>Holds a FIX 4.4 MassStatusReqType, tag 585, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MassStatusReqType, bool> MassStatusReqType                       { get; set; } = ValidateMassStatusReqType;

	/// <summary>Holds a FIX 4.4 DayBookingInst, tag 589, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DayBookingInst, bool> DayBookingInst                          { get; set; } = ValidateDayBookingInst;

	/// <summary>Holds a FIX 4.4 BookingUnit, tag 590, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BookingUnit, bool> BookingUnit                             { get; set; } = ValidateBookingUnit;

	/// <summary>Holds a FIX 4.4 PreallocMethod, tag 591, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PreallocMethod, bool> PreallocMethod                          { get; set; } = ValidatePreallocMethod;

	/// <summary>Holds a FIX 4.4 AllocType, tag 626, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocType, bool> AllocType                               { get; set; } = ValidateAllocType;

	/// <summary>Holds a FIX 4.4 ClearingFeeIndicator, tag 635, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ClearingFeeIndicator, bool> ClearingFeeIndicator                    { get; set; } = ValidateClearingFeeIndicator;

	/// <summary>Holds a FIX 4.4 PriorityIndicator, tag 638, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PriorityIndicator, bool> PriorityIndicator                       { get; set; } = ValidatePriorityIndicator;

	/// <summary>Holds a FIX 4.4 QuoteRequestRejectReason, tag 658, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRequestRejectReason, bool> QuoteRequestRejectReason                { get; set; } = ValidateQuoteRequestRejectReason;

	/// <summary>Holds a FIX 4.4 AcctIDSource, tag 660, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AcctIDSource, bool> AcctIDSource                            { get; set; } = ValidateAcctIDSource;

	/// <summary>Holds a FIX 4.4 ConfirmStatus, tag 665, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmStatus, bool> ConfirmStatus                           { get; set; } = ValidateConfirmStatus;

	/// <summary>Holds a FIX 4.4 ConfirmTransType, tag 666, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmTransType, bool> ConfirmTransType                        { get; set; } = ValidateConfirmTransType;

	/// <summary>Holds a FIX 4.4 DeliveryForm, tag 668, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeliveryForm, bool> DeliveryForm                            { get; set; } = ValidateDeliveryForm;

	/// <summary>Holds a FIX 4.4 LegSwapType, tag 690, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LegSwapType, bool> LegSwapType                             { get; set; } = ValidateLegSwapType;

	/// <summary>Holds a FIX 4.4 QuotePriceType, tag 692, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuotePriceType, bool> QuotePriceType                          { get; set; } = ValidateQuotePriceType;

	/// <summary>Holds a FIX 4.4 QuoteRespType, tag 694, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QuoteRespType, bool> QuoteRespType                           { get; set; } = ValidateQuoteRespType;

	/// <summary>Holds a FIX 4.4 PosType, tag 703, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosType, bool> PosType                                 { get; set; } = ValidatePosType;

	/// <summary>Holds a FIX 4.4 PosQtyStatus, tag 706, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosQtyStatus, bool> PosQtyStatus                            { get; set; } = ValidatePosQtyStatus;

	/// <summary>Holds a FIX 4.4 PosAmtType, tag 707, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosAmtType, bool> PosAmtType                              { get; set; } = ValidatePosAmtType;

	/// <summary>Holds a FIX 4.4 PosTransType, tag 709, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosTransType, bool> PosTransType                            { get; set; } = ValidatePosTransType;

	/// <summary>Holds a FIX 4.4 PosMaintAction, tag 712, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintAction, bool> PosMaintAction                          { get; set; } = ValidatePosMaintAction;

	/// <summary>Holds a FIX 4.4 SettlSessID, tag 716, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlSessID, bool> SettlSessID                             { get; set; } = ValidateSettlSessID;

	/// <summary>Holds a FIX 4.4 AdjustmentType, tag 718, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AdjustmentType, bool> AdjustmentType                          { get; set; } = ValidateAdjustmentType;

	/// <summary>Holds a FIX 4.4 PosMaintStatus, tag 722, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintStatus, bool> PosMaintStatus                          { get; set; } = ValidatePosMaintStatus;

	/// <summary>Holds a FIX 4.4 PosMaintResult, tag 723, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosMaintResult, bool> PosMaintResult                          { get; set; } = ValidatePosMaintResult;

	/// <summary>Holds a FIX 4.4 PosReqType, tag 724, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqType, bool> PosReqType                              { get; set; } = ValidatePosReqType;

	/// <summary>Holds a FIX 4.4 ResponseTransportType, tag 725, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ResponseTransportType, bool> ResponseTransportType                   { get; set; } = ValidateResponseTransportType;

	/// <summary>Holds a FIX 4.4 PosReqResult, tag 728, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqResult, bool> PosReqResult                            { get; set; } = ValidatePosReqResult;

	/// <summary>Holds a FIX 4.4 PosReqStatus, tag 729, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PosReqStatus, bool> PosReqStatus                            { get; set; } = ValidatePosReqStatus;

	/// <summary>Holds a FIX 4.4 SettlPriceType, tag 731, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlPriceType, bool> SettlPriceType                          { get; set; } = ValidateSettlPriceType;

	/// <summary>Holds a FIX 4.4 AssignmentMethod, tag 744, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AssignmentMethod, bool> AssignmentMethod                        { get; set; } = ValidateAssignmentMethod;

	/// <summary>Holds a FIX 4.4 ExerciseMethod, tag 747, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExerciseMethod, bool> ExerciseMethod                          { get; set; } = ValidateExerciseMethod;

	/// <summary>Holds a FIX 4.4 TradeRequestResult, tag 749, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestResult, bool> TradeRequestResult                      { get; set; } = ValidateTradeRequestResult;

	/// <summary>Holds a FIX 4.4 TradeRequestStatus, tag 750, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeRequestStatus, bool> TradeRequestStatus                      { get; set; } = ValidateTradeRequestStatus;

	/// <summary>Holds a FIX 4.4 TradeReportRejectReason, tag 751, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportRejectReason, bool> TradeReportRejectReason                 { get; set; } = ValidateTradeReportRejectReason;

	/// <summary>Holds a FIX 4.4 SideMultiLegReportingType, tag 752, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SideMultiLegReportingType, bool> SideMultiLegReportingType               { get; set; } = ValidateSideMultiLegReportingType;

	/// <summary>Holds a FIX 4.4 TrdRegTimestampType, tag 770, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRegTimestampType, bool> TrdRegTimestampType                     { get; set; } = ValidateTrdRegTimestampType;

	/// <summary>Holds a FIX 4.4 ConfirmType, tag 773, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmType, bool> ConfirmType                             { get; set; } = ValidateConfirmType;

	/// <summary>Holds a FIX 4.4 ConfirmRejReason, tag 774, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ConfirmRejReason, bool> ConfirmRejReason                        { get; set; } = ValidateConfirmRejReason;

	/// <summary>Holds a FIX 4.4 BookingType, tag 775, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.BookingType, bool> BookingType                             { get; set; } = ValidateBookingType;

	/// <summary>Holds a FIX 4.4 AllocSettlInstType, tag 780, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocSettlInstType, bool> AllocSettlInstType                      { get; set; } = ValidateAllocSettlInstType;

	/// <summary>Holds a FIX 4.4 DlvyInstType, tag 787, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DlvyInstType, bool> DlvyInstType                            { get; set; } = ValidateDlvyInstType;

	/// <summary>Holds a FIX 4.4 TerminationType, tag 788, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TerminationType, bool> TerminationType                         { get; set; } = ValidateTerminationType;

	/// <summary>Holds a FIX 4.4 SettlInstReqRejCode, tag 792, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.SettlInstReqRejCode, bool> SettlInstReqRejCode                     { get; set; } = ValidateSettlInstReqRejCode;

	/// <summary>Holds a FIX 4.4 AllocReportType, tag 794, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocReportType, bool> AllocReportType                         { get; set; } = ValidateAllocReportType;

	/// <summary>Holds a FIX 4.4 AllocCancReplaceReason, tag 796, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocCancReplaceReason, bool> AllocCancReplaceReason                  { get; set; } = ValidateAllocCancReplaceReason;

	/// <summary>Holds a FIX 4.4 AllocAccountType, tag 798, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocAccountType, bool> AllocAccountType                        { get; set; } = ValidateAllocAccountType;

	/// <summary>Holds a FIX 4.4 PartySubIDType, tag 803, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PartySubIDType, bool> PartySubIDType                          { get; set; } = ValidatePartySubIDType;

	/// <summary>Holds a FIX 4.4 AllocIntermedReqType, tag 808, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocIntermedReqType, bool> AllocIntermedReqType                    { get; set; } = ValidateAllocIntermedReqType;

	/// <summary>Holds a FIX 4.4 ApplQueueResolution, tag 814, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueResolution, bool> ApplQueueResolution                     { get; set; } = ValidateApplQueueResolution;

	/// <summary>Holds a FIX 4.4 ApplQueueAction, tag 815, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ApplQueueAction, bool> ApplQueueAction                         { get; set; } = ValidateApplQueueAction;

	/// <summary>Holds a FIX 4.4 AvgPxIndicator, tag 819, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AvgPxIndicator, bool> AvgPxIndicator                          { get; set; } = ValidateAvgPxIndicator;

	/// <summary>Holds a FIX 4.4 TradeAllocIndicator, tag 826, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeAllocIndicator, bool> TradeAllocIndicator                     { get; set; } = ValidateTradeAllocIndicator;

	/// <summary>Holds a FIX 4.4 ExpirationCycle, tag 827, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ExpirationCycle, bool> ExpirationCycle                         { get; set; } = ValidateExpirationCycle;

	/// <summary>Holds a FIX 4.4 TrdType, tag 828, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdType, bool> TrdType                                 { get; set; } = ValidateTrdType;

	/// <summary>Holds a FIX 4.4 PegMoveType, tag 835, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegMoveType, bool> PegMoveType                             { get; set; } = ValidatePegMoveType;

	/// <summary>Holds a FIX 4.4 PegOffsetType, tag 836, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegOffsetType, bool> PegOffsetType                           { get; set; } = ValidatePegOffsetType;

	/// <summary>Holds a FIX 4.4 PegLimitType, tag 837, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegLimitType, bool> PegLimitType                            { get; set; } = ValidatePegLimitType;

	/// <summary>Holds a FIX 4.4 PegRoundDirection, tag 838, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.PegRoundDirection, bool> PegRoundDirection                       { get; set; } = ValidatePegRoundDirection;

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

	/// <summary>Holds a FIX 4.4 DiscretionScope, tag 846, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DiscretionScope, bool> DiscretionScope                         { get; set; } = ValidateDiscretionScope;

	/// <summary>Holds a FIX 4.4 TargetStrategy, tag 847, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TargetStrategy, bool> TargetStrategy                          { get; set; } = ValidateTargetStrategy;

	/// <summary>Holds a FIX 4.4 LastLiquidityInd, tag 851, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.LastLiquidityInd, bool> LastLiquidityInd                        { get; set; } = ValidateLastLiquidityInd;

	/// <summary>Holds a FIX 4.4 ShortSaleReason, tag 853, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.ShortSaleReason, bool> ShortSaleReason                         { get; set; } = ValidateShortSaleReason;

	/// <summary>Holds a FIX 4.4 QtyType, tag 854, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.QtyType, bool> QtyType                                 { get; set; } = ValidateQtyType;

	/// <summary>Holds a FIX 4.4 TradeReportType, tag 856, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TradeReportType, bool> TradeReportType                         { get; set; } = ValidateTradeReportType;

	/// <summary>Holds a FIX 4.4 AllocNoOrdersType, tag 857, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AllocNoOrdersType, bool> AllocNoOrdersType                       { get; set; } = ValidateAllocNoOrdersType;

	/// <summary>Holds a FIX 4.4 EventType, tag 865, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.EventType, bool> EventType                               { get; set; } = ValidateEventType;

	/// <summary>Holds a FIX 4.4 InstrAttribType, tag 871, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.InstrAttribType, bool> InstrAttribType                         { get; set; } = ValidateInstrAttribType;

	/// <summary>Holds a FIX 4.4 CPProgram, tag 875, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CPProgram, bool> CPProgram                               { get; set; } = ValidateCPProgram;

	/// <summary>Holds a FIX 4.4 MiscFeeBasis, tag 891, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.MiscFeeBasis, bool> MiscFeeBasis                            { get; set; } = ValidateMiscFeeBasis;

	/// <summary>Holds a FIX 4.4 CollAsgnReason, tag 895, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnReason, bool> CollAsgnReason                          { get; set; } = ValidateCollAsgnReason;

	/// <summary>Holds a FIX 4.4 CollInquiryQualifier, tag 896, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryQualifier, bool> CollInquiryQualifier                    { get; set; } = ValidateCollInquiryQualifier;

	/// <summary>Holds a FIX 4.4 CollAsgnTransType, tag 903, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnTransType, bool> CollAsgnTransType                       { get; set; } = ValidateCollAsgnTransType;

	/// <summary>Holds a FIX 4.4 CollAsgnRespType, tag 905, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnRespType, bool> CollAsgnRespType                        { get; set; } = ValidateCollAsgnRespType;

	/// <summary>Holds a FIX 4.4 CollAsgnRejectReason, tag 906, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAsgnRejectReason, bool> CollAsgnRejectReason                    { get; set; } = ValidateCollAsgnRejectReason;

	/// <summary>Holds a FIX 4.4 CollStatus, tag 910, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollStatus, bool> CollStatus                              { get; set; } = ValidateCollStatus;

	/// <summary>Holds a FIX 4.4 DeliveryType, tag 919, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.DeliveryType, bool> DeliveryType                            { get; set; } = ValidateDeliveryType;

	/// <summary>Holds a FIX 4.4 UserRequestType, tag 924, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.UserRequestType, bool> UserRequestType                         { get; set; } = ValidateUserRequestType;

	/// <summary>Holds a FIX 4.4 UserStatus, tag 926, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.UserStatus, bool> UserStatus                              { get; set; } = ValidateUserStatus;

	/// <summary>Holds a FIX 4.4 StatusValue, tag 928, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.StatusValue, bool> StatusValue                             { get; set; } = ValidateStatusValue;

	/// <summary>Holds a FIX 4.4 NetworkRequestType, tag 935, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkRequestType, bool> NetworkRequestType                      { get; set; } = ValidateNetworkRequestType;

	/// <summary>Holds a FIX 4.4 NetworkStatusResponseType, tag 937, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.NetworkStatusResponseType, bool> NetworkStatusResponseType               { get; set; } = ValidateNetworkStatusResponseType;

	/// <summary>Holds a FIX 4.4 TrdRptStatus, tag 939, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.TrdRptStatus, bool> TrdRptStatus                            { get; set; } = ValidateTrdRptStatus;

	/// <summary>Holds a FIX 4.4 AffirmStatus, tag 940, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.AffirmStatus, bool> AffirmStatus                            { get; set; } = ValidateAffirmStatus;

	/// <summary>Holds a FIX 4.4 CollAction, tag 944, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollAction, bool> CollAction                              { get; set; } = ValidateCollAction;

	/// <summary>Holds a FIX 4.4 CollInquiryStatus, tag 945, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryStatus, bool> CollInquiryStatus                       { get; set; } = ValidateCollInquiryStatus;

	/// <summary>Holds a FIX 4.4 CollInquiryResult, tag 946, to the values the specification lists.</summary>
	public Func<FixContext, FixMessage, FixField.CollInquiryResult, bool> CollInquiryResult                       { get; set; } = ValidateCollInquiryResult;

	static bool ValidateAdvertisement(FixContext context, FixMessage.Advertisement message)
	{
		if (message.AdvId        is null) Missing(message, 2);
		if (message.AdvSide      is null) Missing(message, 4);
		if (message.AdvTransType is null) Missing(message, 5);
		if (message.Quantity     is null) Missing(message, 53);

		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);

		if (message.AdvSide          is not null) context.Validators.AdvSide(context, message, message.AdvSide);
		if (message.AdvTransType     is not null) context.Validators.AdvTransType(context, message, message.AdvTransType);
		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.QtyType          is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.SecurityIDSource       is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                   is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType              is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.AllocTransType         is not null) context.Validators.AllocTransType(context, message, message.AllocTransType);
		if (message.PositionEffect         is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.SecurityType           is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.AllocLinkType          is not null) context.Validators.AllocLinkType(context, message, message.AllocLinkType);
		if (message.PutOrCall              is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType              is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.PriceType              is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product                is not null) context.Validators.Product(context, message, message.Product);
		if (message.MatchType              is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.AllocType              is not null) context.Validators.AllocType(context, message, message.AllocType);
		if (message.DeliveryForm           is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.BookingType            is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.TerminationType        is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, message, message.AllocCancReplaceReason);
		if (message.AllocIntermedReqType   is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.QtyType                is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.AllocNoOrdersType      is not null) context.Validators.AllocNoOrdersType(context, message, message.AllocNoOrdersType);
		if (message.CPProgram              is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType           is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.AllocStatus          is not null) context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode         is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.SecurityType         is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.Product              is not null) context.Validators.Product(context, message, message.Product);
		if (message.MatchStatus          is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.AllocType            is not null) context.Validators.AllocType(context, message, message.AllocType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.SecurityIDSource       is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                   is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType              is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.AllocTransType         is not null) context.Validators.AllocTransType(context, message, message.AllocTransType);
		if (message.PositionEffect         is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.AllocStatus            is not null) context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode           is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.SecurityType           is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.AllocLinkType          is not null) context.Validators.AllocLinkType(context, message, message.AllocLinkType);
		if (message.PutOrCall              is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType              is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.PriceType              is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product                is not null) context.Validators.Product(context, message, message.Product);
		if (message.MatchType              is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.DeliveryForm           is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.BookingType            is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.TerminationType        is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.AllocReportType        is not null) context.Validators.AllocReportType(context, message, message.AllocReportType);
		if (message.AllocCancReplaceReason is not null) context.Validators.AllocCancReplaceReason(context, message, message.AllocCancReplaceReason);
		if (message.AllocIntermedReqType   is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);
		if (message.QtyType                is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.AllocNoOrdersType      is not null) context.Validators.AllocNoOrdersType(context, message, message.AllocNoOrdersType);
		if (message.CPProgram              is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType           is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.AllocStatus          is not null) context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode         is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.SecurityType         is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.Product              is not null) context.Validators.Product(context, message, message.Product);
		if (message.MatchStatus          is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.AllocReportType      is not null) context.Validators.AllocReportType(context, message, message.AllocReportType);
		if (message.AllocIntermedReqType is not null) context.Validators.AllocIntermedReqType(context, message, message.AllocIntermedReqType);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType      is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID      is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.SettlPriceType   is not null) context.Validators.SettlPriceType(context, message, message.SettlPriceType);
		if (message.AssignmentMethod is not null) context.Validators.AssignmentMethod(context, message, message.AssignmentMethod);
		if (message.ExerciseMethod   is not null) context.Validators.ExerciseMethod(context, message, message.ExerciseMethod);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.BidRequestTransType is not null) context.Validators.BidRequestTransType(context, message, message.BidRequestTransType);
		if (message.BidType             is not null) context.Validators.BidType(context, message, message.BidType);
		if (message.LiquidityIndType    is not null) context.Validators.LiquidityIndType(context, message, message.LiquidityIndType);
		if (message.ProgRptReqs         is not null) context.Validators.ProgRptReqs(context, message, message.ProgRptReqs);
		if (message.IncTaxInd           is not null) context.Validators.IncTaxInd(context, message, message.IncTaxInd);
		if (message.BidTradeType        is not null) context.Validators.BidTradeType(context, message, message.BidTradeType);
		if (message.BasisPxType         is not null) context.Validators.BasisPxType(context, message, message.BasisPxType);

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

		if (message.BusinessRejectReason is not null) context.Validators.BusinessRejectReason(context, message, message.BusinessRejectReason);

		return message.IsValid;
	}

	static bool ValidateCollateralAssignment(FixContext context, FixMessage.CollateralAssignment message)
	{
		if (message.TransactTime      is null) Missing(message, 60);
		if (message.CollAsgnReason    is null) Missing(message, 895);
		if (message.CollAsgnID        is null) Missing(message, 902);
		if (message.CollAsgnTransType is null) Missing(message, 903);

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.StandInstDbType   is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, message, message.SettlDeliveryType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.PriceType         is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType       is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID       is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.TerminationType   is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType           is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.CollAsgnReason    is not null) context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.CollAsgnTransType is not null) context.Validators.CollAsgnTransType(context, message, message.CollAsgnTransType);
		if (message.DeliveryType      is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                    is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.StandInstDbType         is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.SettlDeliveryType       is not null) context.Validators.SettlDeliveryType(context, message, message.SettlDeliveryType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.PriceType               is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType             is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID             is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.ResponseTransportType   is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.TerminationType         is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType                 is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType            is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource      is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType          is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall             is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product               is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType           is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID           is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.TerminationType       is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType               is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram             is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType          is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);
		if (message.CollInquiryStatus     is not null) context.Validators.CollInquiryStatus(context, message, message.CollInquiryStatus);
		if (message.CollInquiryResult     is not null) context.Validators.CollInquiryResult(context, message, message.CollInquiryResult);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.StandInstDbType   is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, message, message.SettlDeliveryType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.PriceType         is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType       is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID       is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.TerminationType   is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType           is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.CollStatus        is not null) context.Validators.CollStatus(context, message, message.CollStatus);
		if (message.DeliveryType      is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);

		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side             is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.PriceType        is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType      is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.SettlSessID      is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.TerminationType  is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType          is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.CollAsgnReason   is not null) context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.DeliveryType     is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);

		if (message.SecurityIDSource     is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                 is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType         is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall            is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.PriceType            is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product              is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType          is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.TerminationType      is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType              is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram            is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.CollAsgnReason       is not null) context.Validators.CollAsgnReason(context, message, message.CollAsgnReason);
		if (message.CollAsgnTransType    is not null) context.Validators.CollAsgnTransType(context, message, message.CollAsgnTransType);
		if (message.CollAsgnRespType     is not null) context.Validators.CollAsgnRespType(context, message, message.CollAsgnRespType);
		if (message.CollAsgnRejectReason is not null) context.Validators.CollAsgnRejectReason(context, message, message.CollAsgnRejectReason);
		if (message.DeliveryType         is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);
		if (!Empty((ISettlInstructionsData)message)) context.Validators.SettlInstructionsData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType            is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType           is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.ProcessCode         is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.StandInstDbType     is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.SettlDeliveryType   is not null) context.Validators.SettlDeliveryType(context, message, message.SettlDeliveryType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType           is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.PriceType           is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.FundRenewWaiv       is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.ConfirmStatus       is not null) context.Validators.ConfirmStatus(context, message, message.ConfirmStatus);
		if (message.ConfirmTransType    is not null) context.Validators.ConfirmTransType(context, message, message.ConfirmTransType);
		if (message.DeliveryForm        is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.ConfirmType         is not null) context.Validators.ConfirmType(context, message, message.ConfirmType);
		if (message.TerminationType     is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.AllocAccountType    is not null) context.Validators.AllocAccountType(context, message, message.AllocAccountType);
		if (message.QtyType             is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType        is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.MatchStatus      is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.ConfirmRejReason is not null) context.Validators.ConfirmRejReason(context, message, message.ConfirmRejReason);
		if (message.AffirmStatus     is not null) context.Validators.AffirmStatus(context, message, message.AffirmStatus);

		return message.IsValid;
	}

	static bool ValidateConfirmationRequest(FixContext context, FixMessage.ConfirmationRequest message)
	{
		if (message.TransactTime is null) Missing(message, 60);
		if (message.ConfirmType  is null) Missing(message, 773);
		if (message.ConfirmReqID is null) Missing(message, 859);

		if (message.ConfirmType      is not null) context.Validators.ConfirmType(context, message, message.ConfirmType);
		if (message.AllocAccountType is not null) context.Validators.AllocAccountType(context, message, message.AllocAccountType);

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

		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.ProcessCode              is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType                is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.CrossType                is not null) context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization      is not null) context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides                  is not null) context.Validators.NoSides(context, message, message.NoSides);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.CrossType           is not null) context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization is not null) context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides             is not null) context.Validators.NoSides(context, message, message.NoSides);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);

		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, message, message.SecurityRequestResult);

		Counted(message, message.NoRelatedSym, message.RelSymDerivSecGrp);
		Counted(message, message.NoUnderlyingSecurityAltID, message.UndSecAltIDGrp);
		Counted(message, message.NoUnderlyingStips, message.UnderlyingStipulations);

		return message.IsValid;
	}

	static bool ValidateDerivativeSecurityListRequest(FixContext context, FixMessage.DerivativeSecurityListRequest message)
	{
		if (message.SecurityReqID           is null) Missing(message, 320);
		if (message.SecurityListRequestType is null) Missing(message, 559);

		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);

		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.SecurityListRequestType is not null) context.Validators.SecurityListRequestType(context, message, message.SecurityListRequestType);

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
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.DKReason          is not null) context.Validators.DKReason(context, message, message.DKReason);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.EmailType is not null) context.Validators.EmailType(context, message, message.EmailType);

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

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType                 is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.LastCapacity             is not null) context.Validators.LastCapacity(context, message, message.LastCapacity);
		if (message.OrdStatus                is not null) context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                     is not null) context.Validators.Side(context, message, message.Side);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.PositionEffect           is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.OrdRejReason             is not null) context.Validators.OrdRejReason(context, message, message.OrdRejReason);
		if (message.ExecType                 is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.SettlCurrFxRateCalc      is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType                is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.ExecRestatementReason    is not null) context.Validators.ExecRestatementReason(context, message, message.ExecRestatementReason);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.MultiLegReportingType    is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection        is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.ExecPriceType            is not null) context.Validators.ExecPriceType(context, message, message.ExecPriceType);
		if (message.FundRenewWaiv            is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.OrderCapacity            is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions        is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CashMargin               is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.CrossType                is not null) context.Validators.CrossType(context, message, message.CrossType);
		if (message.AccountType              is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity        is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.DayBookingInst           is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit              is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod           is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator     is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.PriorityIndicator        is not null) context.Validators.PriorityIndicator(context, message, message.PriorityIndicator);
		if (message.AcctIDSource             is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.BookingType              is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.TerminationType          is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.LastLiquidityInd         is not null) context.Validators.LastLiquidityInd(context, message, message.LastLiquidityInd);
		if (message.QtyType                  is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType             is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.IOIQltyInd        is not null) context.Validators.IOIQltyInd(context, message, message.IOIQltyInd);
		if (message.IOIQty            is not null) context.Validators.IOIQty(context, message, message.IOIQty);
		if (message.IOITransType      is not null) context.Validators.IOITransType(context, message, message.IOITransType);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType         is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.PriceType         is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.TerminationType   is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.QtyType           is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType      is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.ListStatusType  is not null) context.Validators.ListStatusType(context, message, message.ListStatusType);
		if (message.ListOrderStatus is not null) context.Validators.ListOrderStatus(context, message, message.ListOrderStatus);

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

		if (message.EncryptMethod is not null) context.Validators.EncryptMethod(context, message, message.EncryptMethod);

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

		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, message, message.ApplQueueResolution);

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

		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.MDUpdateType            is not null) context.Validators.MDUpdateType(context, message, message.MDUpdateType);
		if (message.OpenCloseSettlFlag      is not null) context.Validators.OpenCloseSettlFlag(context, message, message.OpenCloseSettlFlag);
		if (message.Scope                   is not null) context.Validators.Scope(context, message, message.Scope);
		if (message.ApplQueueAction         is not null) context.Validators.ApplQueueAction(context, message, message.ApplQueueAction);

		Counted(message, message.NoRelatedSym, message.InstrmtMDReqGrp);
		Counted(message, message.NoMDEntryTypes, message.MDReqGrp);
		Counted(message, message.NoTradingSessions, message.TrdgSesGrp);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequestReject(FixContext context, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(message, 262);

		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, message, message.MDReqRejReason);

		Counted(message, message.NoAltMDSource, message.MDRjctGrp);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh(FixContext context, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.NoMDEntries is null) Missing(message, 268);

		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.FinancialStatus     is not null) context.Validators.FinancialStatus(context, message, message.FinancialStatus);
		if (message.CorporateAction     is not null) context.Validators.CorporateAction(context, message, message.CorporateAction);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.ApplQueueResolution is not null) context.Validators.ApplQueueResolution(context, message, message.ApplQueueResolution);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.QuoteType          is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.AccountType        is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource       is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteSets, message.QuotSetGrp);

		return message.IsValid;
	}

	static bool ValidateMassQuoteAcknowledgement(FixContext context, FixMessage.MassQuoteAcknowledgement message)
	{
		if (message.QuoteStatus is null) Missing(message, 297);

		if (message.QuoteStatus        is not null) context.Validators.QuoteStatus(context, message, message.QuoteStatus);
		if (message.QuoteRejectReason  is not null) context.Validators.QuoteRejectReason(context, message, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.QuoteType          is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.AccountType        is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource       is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

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

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);

		if (message.CommType                 is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                     is not null) context.Validators.Side(context, message, message.Side);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.PositionEffect           is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.ProcessCode              is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.CoveredOrUncovered       is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection        is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.FundRenewWaiv            is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.OrderCapacity            is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions        is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CashMargin               is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.MultiLegRptTypeReq       is not null) context.Validators.MultiLegRptTypeReq(context, message, message.MultiLegRptTypeReq);
		if (message.AccountType              is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity        is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.DayBookingInst           is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit              is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod           is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator     is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.AcctIDSource             is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.BookingType              is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.QtyType                  is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.NetworkRequestType is not null) context.Validators.NetworkRequestType(context, message, message.NetworkRequestType);

		Counted(message, message.NoCompIDs, message.CompIDReqGrp);

		return message.IsValid;
	}

	static bool ValidateNetworkCounterpartySystemStatusResponse(FixContext context, FixMessage.NetworkCounterpartySystemStatusResponse message)
	{
		if (message.NetworkResponseID         is null) Missing(message, 932);
		if (message.NoCompIDs                 is null) Missing(message, 936);
		if (message.NetworkStatusResponseType is null) Missing(message, 937);

		if (message.NetworkStatusResponseType is not null) context.Validators.NetworkStatusResponseType(context, message, message.NetworkStatusResponseType);

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

		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.ProcessCode              is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType                is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.CrossType                is not null) context.Validators.CrossType(context, message, message.CrossType);
		if (message.CrossPrioritization      is not null) context.Validators.CrossPrioritization(context, message, message.CrossPrioritization);
		if (message.NoSides                  is not null) context.Validators.NoSides(context, message, message.NoSides);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.BidType               is not null) context.Validators.BidType(context, message, message.BidType);
		if (message.ProgRptReqs           is not null) context.Validators.ProgRptReqs(context, message, message.ProgRptReqs);
		if (message.ListExecInstType      is not null) context.Validators.ListExecInstType(context, message, message.ListExecInstType);
		if (message.CancellationRights    is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);

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

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);

		if (message.CommType                 is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                     is not null) context.Validators.Side(context, message, message.Side);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.PositionEffect           is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.ProcessCode              is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.CoveredOrUncovered       is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection        is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.FundRenewWaiv            is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.OrderCapacity            is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions        is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CashMargin               is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.MultiLegRptTypeReq       is not null) context.Validators.MultiLegRptTypeReq(context, message, message.MultiLegRptTypeReq);
		if (message.AccountType              is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity        is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.DayBookingInst           is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit              is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod           is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator     is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.AcctIDSource             is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.BookingType              is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.QtyType                  is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType                 is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                     is not null) context.Validators.Side(context, message, message.Side);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.PositionEffect           is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.ProcessCode              is not null) context.Validators.ProcessCode(context, message, message.ProcessCode);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.CoveredOrUncovered       is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.YieldType                is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection        is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.FundRenewWaiv            is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.OrderCapacity            is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions        is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CashMargin               is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.AccountType              is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity        is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.DayBookingInst           is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit              is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod           is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator     is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.AcctIDSource             is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.BookingType              is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.TerminationType          is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.QtyType                  is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType             is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.Urgency is not null) context.Validators.Urgency(context, message, message.Urgency);

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

		if (message.OrdStatus        is not null) context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.CxlRejReason     is not null) context.Validators.CxlRejReason(context, message, message.CxlRejReason);
		if (message.CxlRejResponseTo is not null) context.Validators.CxlRejResponseTo(context, message, message.CxlRejResponseTo);
		if (message.AccountType      is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource     is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest(FixContext context, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.ClOrdID      is null) Missing(message, 11);
		if (message.OrdType      is null) Missing(message, 40);
		if (message.OrigClOrdID  is null) Missing(message, 41);
		if (message.Side         is null) Missing(message, 54);
		if (message.TransactTime is null) Missing(message, 60);

		if (!Empty((ICommissionData)message)) context.Validators.CommissionData(context, message, message);
		if (!Empty((IDiscretionInstructions)message)) context.Validators.DiscretionInstructions(context, message, message);
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);
		if (!Empty((IPegInstructions)message)) context.Validators.PegInstructions(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType                 is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.ExecInst                 is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.HandlInst                is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.SecurityIDSource         is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType                  is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                     is not null) context.Validators.Side(context, message, message.Side);
		if (message.TimeInForce              is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.SettlType                is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.PositionEffect           is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.SecurityType             is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall                is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.CoveredOrUncovered       is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.YieldType                is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.DiscretionInst           is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.PriceType                is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.GTBookingInst            is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Product                  is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection        is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.CancellationRights       is not null) context.Validators.CancellationRights(context, message, message.CancellationRights);
		if (message.MoneyLaunderingStatus    is not null) context.Validators.MoneyLaunderingStatus(context, message, message.MoneyLaunderingStatus);
		if (message.FundRenewWaiv            is not null) context.Validators.FundRenewWaiv(context, message, message.FundRenewWaiv);
		if (message.OrderCapacity            is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions        is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.CashMargin               is not null) context.Validators.CashMargin(context, message, message.CashMargin);
		if (message.AccountType              is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity        is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.DayBookingInst           is not null) context.Validators.DayBookingInst(context, message, message.DayBookingInst);
		if (message.BookingUnit              is not null) context.Validators.BookingUnit(context, message, message.BookingUnit);
		if (message.PreallocMethod           is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator     is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.AcctIDSource             is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.BookingType              is not null) context.Validators.BookingType(context, message, message.BookingType);
		if (message.TerminationType          is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.PegMoveType              is not null) context.Validators.PegMoveType(context, message, message.PegMoveType);
		if (message.PegOffsetType            is not null) context.Validators.PegOffsetType(context, message, message.PegOffsetType);
		if (message.PegLimitType             is not null) context.Validators.PegLimitType(context, message, message.PegLimitType);
		if (message.PegRoundDirection        is not null) context.Validators.PegRoundDirection(context, message, message.PegRoundDirection);
		if (message.PegScope                 is not null) context.Validators.PegScope(context, message, message.PegScope);
		if (message.DiscretionMoveType       is not null) context.Validators.DiscretionMoveType(context, message, message.DiscretionMoveType);
		if (message.DiscretionOffsetType     is not null) context.Validators.DiscretionOffsetType(context, message, message.DiscretionOffsetType);
		if (message.DiscretionLimitType      is not null) context.Validators.DiscretionLimitType(context, message, message.DiscretionLimitType);
		if (message.DiscretionRoundDirection is not null) context.Validators.DiscretionRoundDirection(context, message, message.DiscretionRoundDirection);
		if (message.DiscretionScope          is not null) context.Validators.DiscretionScope(context, message, message.DiscretionScope);
		if (message.TargetStrategy           is not null) context.Validators.TargetStrategy(context, message, message.TargetStrategy);
		if (message.QtyType                  is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.CPProgram                is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType             is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (Empty((IOrderQtyData)message)) Absent(message, 38);
		else context.Validators.OrderQtyData(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.AccountType       is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource      is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TerminationType   is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType      is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);

		if (message.SecurityIDSource       is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                   is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType           is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall              is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product                is not null) context.Validators.Product(context, message, message.Product);
		if (message.MassCancelRequestType  is not null) context.Validators.MassCancelRequestType(context, message, message.MassCancelRequestType);
		if (message.MassCancelResponse     is not null) context.Validators.MassCancelResponse(context, message, message.MassCancelResponse);
		if (message.MassCancelRejectReason is not null) context.Validators.MassCancelRejectReason(context, message, message.MassCancelRejectReason);
		if (message.CPProgram              is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);

		if (message.SecurityIDSource      is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                  is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType          is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall             is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product               is not null) context.Validators.Product(context, message, message.Product);
		if (message.MassCancelRequestType is not null) context.Validators.MassCancelRequestType(context, message, message.MassCancelRequestType);
		if (message.CPProgram             is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IUnderlyingInstrument)message)) context.Validators.UnderlyingInstrument(context, message, message);

		if (message.SecurityIDSource  is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side              is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType      is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall         is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product           is not null) context.Validators.Product(context, message, message.Product);
		if (message.MassStatusReqType is not null) context.Validators.MassStatusReqType(context, message, message.MassStatusReqType);
		if (message.AcctIDSource      is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.CPProgram         is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side             is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.AcctIDSource     is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TerminationType  is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType     is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType      is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource     is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.PosTransType     is not null) context.Validators.PosTransType(context, message, message.PosTransType);
		if (message.PosMaintAction   is not null) context.Validators.PosMaintAction(context, message, message.PosMaintAction);
		if (message.SettlSessID      is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.AdjustmentType   is not null) context.Validators.AdjustmentType(context, message, message.AdjustmentType);
		if (message.PosMaintStatus   is not null) context.Validators.PosMaintStatus(context, message, message.PosMaintStatus);
		if (message.PosMaintResult   is not null) context.Validators.PosMaintResult(context, message, message.PosMaintResult);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType     is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall        is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product          is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType      is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource     is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.PosTransType     is not null) context.Validators.PosTransType(context, message, message.PosTransType);
		if (message.PosMaintAction   is not null) context.Validators.PosMaintAction(context, message, message.PosMaintAction);
		if (message.SettlSessID      is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.AdjustmentType   is not null) context.Validators.AdjustmentType(context, message, message.AdjustmentType);
		if (message.CPProgram        is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.RegistStatus            is not null) context.Validators.RegistStatus(context, message, message.RegistStatus);
		if (message.AccountType             is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource            is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.SettlSessID             is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.PosReqType              is not null) context.Validators.PosReqType(context, message, message.PosReqType);
		if (message.PosReqResult            is not null) context.Validators.PosReqResult(context, message, message.PosReqResult);
		if (message.SettlPriceType          is not null) context.Validators.SettlPriceType(context, message, message.SettlPriceType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType            is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType             is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType           is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType           is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.QuoteResponseLevel  is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.PriceType           is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection   is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.OrderCapacity       is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.QuoteType           is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.AccountType         is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity   is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.AcctIDSource        is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TerminationType     is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType        is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.QuoteCancelType    is not null) context.Validators.QuoteCancelType(context, message, message.QuoteCancelType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.AccountType        is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource       is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

		Counted(message, message.NoPartyIDs, message.Parties);
		Counted(message, message.NoQuoteEntries, message.QuotCxlEntriesGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest(FixContext context, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID   is null) Missing(message, 131);
		if (message.NoRelatedSym is null) Missing(message, 146);

		if (message.OrderCapacity is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);

		Counted(message, message.NoRelatedSym, message.QuotReqGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestReject(FixContext context, FixMessage.QuoteRequestReject message)
	{
		if (message.QuoteReqID               is null) Missing(message, 131);
		if (message.NoRelatedSym             is null) Missing(message, 146);
		if (message.QuoteRequestRejectReason is null) Missing(message, 658);

		if (message.QuoteRequestRejectReason is not null) context.Validators.QuoteRequestRejectReason(context, message, message.QuoteRequestRejectReason);

		Counted(message, message.NoRelatedSym, message.QuotReqRjctGrp);

		return message.IsValid;
	}

	static bool ValidateQuoteResponse(FixContext context, FixMessage.QuoteResponse message)
	{
		if (message.QuoteRespID   is null) Missing(message, 693);
		if (message.QuoteRespType is null) Missing(message, 694);

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType            is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType             is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType           is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType           is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.PriceType           is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection   is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.OrderCapacity       is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.QuoteType           is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.AccountType         is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity   is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.AcctIDSource        is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.QuoteRespType       is not null) context.Validators.QuoteRespType(context, message, message.QuoteRespType);
		if (message.TerminationType     is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType        is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.CommType            is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.SecurityIDSource    is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdType             is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Side                is not null) context.Validators.Side(context, message, message.Side);
		if (message.SettlType           is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.SecurityType        is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall           is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType           is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.QuoteStatus         is not null) context.Validators.QuoteStatus(context, message, message.QuoteStatus);
		if (message.PriceType           is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.Product             is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection   is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.QuoteType           is not null) context.Validators.QuoteType(context, message, message.QuoteType);
		if (message.AccountType         is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity   is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.AcctIDSource        is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TerminationType     is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram           is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType        is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType             is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource            is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.TerminationType         is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType            is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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

		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		Counted(message, message.NoRelatedSym, message.RFQReqGrp);

		return message.IsValid;
	}

	static bool ValidateRegistrationInstructions(FixContext context, FixMessage.RegistrationInstructions message)
	{
		if (message.RegistRefID     is null) Missing(message, 508);
		if (message.RegistID        is null) Missing(message, 513);
		if (message.RegistTransType is null) Missing(message, 514);

		if (message.TaxAdvantageType is not null) context.Validators.TaxAdvantageType(context, message, message.TaxAdvantageType);
		if (message.RegistTransType  is not null) context.Validators.RegistTransType(context, message, message.RegistTransType);
		if (message.OwnershipType    is not null) context.Validators.OwnershipType(context, message, message.OwnershipType);
		if (message.AcctIDSource     is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

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

		if (message.RegistStatus        is not null) context.Validators.RegistStatus(context, message, message.RegistStatus);
		if (message.RegistRejReasonCode is not null) context.Validators.RegistRejReasonCode(context, message, message.RegistRejReasonCode);
		if (message.RegistTransType     is not null) context.Validators.RegistTransType(context, message, message.RegistTransType);
		if (message.AcctIDSource        is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);

		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateReject(FixContext context, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(message, 45);

		if (message.SessionRejectReason is not null) context.Validators.SessionRejectReason(context, message, message.SessionRejectReason);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.MatchStatus             is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.AccountType             is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource            is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.SettlSessID             is not null) context.Validators.SettlSessID(context, message, message.SettlSessID);
		if (message.PosReqType              is not null) context.Validators.PosReqType(context, message, message.PosReqType);
		if (message.ResponseTransportType   is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource      is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType          is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall             is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.Product               is not null) context.Validators.Product(context, message, message.Product);
		if (message.AccountType           is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.AcctIDSource          is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.ResponseTransportType is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.PosReqResult          is not null) context.Validators.PosReqResult(context, message, message.PosReqResult);
		if (message.PosReqStatus          is not null) context.Validators.PosReqStatus(context, message, message.PosReqStatus);
		if (message.CPProgram             is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource     is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType         is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall            is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SecurityResponseType is not null) context.Validators.SecurityResponseType(context, message, message.SecurityResponseType);
		if (message.Product              is not null) context.Validators.Product(context, message, message.Product);
		if (message.DeliveryForm         is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.ExpirationCycle      is not null) context.Validators.ExpirationCycle(context, message, message.ExpirationCycle);
		if (message.CPProgram            is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.SecurityRequestType     is not null) context.Validators.SecurityRequestType(context, message, message.SecurityRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.DeliveryForm            is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.ExpirationCycle         is not null) context.Validators.ExpirationCycle(context, message, message.ExpirationCycle);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.SecurityRequestResult is not null) context.Validators.SecurityRequestResult(context, message, message.SecurityRequestResult);

		Counted(message, message.NoRelatedSym, message.SecListGrp);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequest(FixContext context, FixMessage.SecurityListRequest message)
	{
		if (message.SecurityReqID           is null) Missing(message, 320);
		if (message.SecurityListRequestType is null) Missing(message, 559);

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.SecurityListRequestType is not null) context.Validators.SecurityListRequestType(context, message, message.SecurityListRequestType);
		if (message.DeliveryForm            is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.TerminationType         is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType            is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource      is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType          is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall             is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.FinancialStatus       is not null) context.Validators.FinancialStatus(context, message, message.FinancialStatus);
		if (message.CorporateAction       is not null) context.Validators.CorporateAction(context, message, message.CorporateAction);
		if (message.SecurityTradingStatus is not null) context.Validators.SecurityTradingStatus(context, message, message.SecurityTradingStatus);
		if (message.HaltReason            is not null) context.Validators.HaltReason(context, message, message.HaltReason);
		if (message.Adjustment            is not null) context.Validators.Adjustment(context, message, message.Adjustment);
		if (message.Product               is not null) context.Validators.Product(context, message, message.Product);
		if (message.DeliveryForm          is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.CPProgram             is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.DeliveryForm            is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.Product      is not null) context.Validators.Product(context, message, message.Product);

		return message.IsValid;
	}

	static bool ValidateSecurityTypes(FixContext context, FixMessage.SecurityTypes message)
	{
		if (message.SecurityReqID        is null) Missing(message, 320);
		if (message.SecurityResponseID   is null) Missing(message, 322);
		if (message.SecurityResponseType is null) Missing(message, 323);

		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.SecurityResponseType    is not null) context.Validators.SecurityResponseType(context, message, message.SecurityResponseType);

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

		if (message.Side            is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType    is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.Product         is not null) context.Validators.Product(context, message, message.Product);

		Counted(message, message.NoPartyIDs, message.Parties);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructions(FixContext context, FixMessage.SettlementInstructions message)
	{
		if (message.TransactTime   is null) Missing(message, 60);
		if (message.SettlInstMode  is null) Missing(message, 160);
		if (message.SettlInstMsgID is null) Missing(message, 777);

		if (message.SettlInstMode       is not null) context.Validators.SettlInstMode(context, message, message.SettlInstMode);
		if (message.SettlInstReqRejCode is not null) context.Validators.SettlInstReqRejCode(context, message, message.SettlInstReqRejCode);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (Empty((IInstrument)message)) Absent(message, 55);
		else context.Validators.Instrument(context, message, message);
		if (!Empty((IOrderQtyData)message)) context.Validators.OrderQtyData(context, message, message);
		if (!Empty((ISpreadOrBenchmarkCurveData)message)) context.Validators.SpreadOrBenchmarkCurveData(context, message, message);
		if (!Empty((IYieldData)message)) context.Validators.YieldData(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.OrdStatus               is not null) context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.SettlType               is not null) context.Validators.SettlType(context, message, message.SettlType);
		if (message.ExecType                is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.YieldType               is not null) context.Validators.YieldType(context, message, message.YieldType);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.ExecRestatementReason   is not null) context.Validators.ExecRestatementReason(context, message, message.ExecRestatementReason);
		if (message.PriceType               is not null) context.Validators.PriceType(context, message, message.PriceType);
		if (message.MultiLegReportingType   is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.RoundingDirection       is not null) context.Validators.RoundingDirection(context, message, message.RoundingDirection);
		if (message.NoSides                 is not null) context.Validators.NoSides(context, message, message.NoSides);
		if (message.MatchStatus             is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.MatchType               is not null) context.Validators.MatchType(context, message, message.MatchType);
		if (message.TerminationType         is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.AvgPxIndicator          is not null) context.Validators.AvgPxIndicator(context, message, message.AvgPxIndicator);
		if (message.TrdType                 is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.ShortSaleReason         is not null) context.Validators.ShortSaleReason(context, message, message.ShortSaleReason);
		if (message.QtyType                 is not null) context.Validators.QtyType(context, message, message.QtyType);
		if (message.TradeReportType         is not null) context.Validators.TradeReportType(context, message, message.TradeReportType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType            is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.PositionEffect          is not null) context.Validators.PositionEffect(context, message, message.PositionEffect);
		if (message.ExecType                is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.OrderCapacity           is not null) context.Validators.OrderCapacity(context, message, message.OrderCapacity);
		if (message.OrderRestrictions       is not null) context.Validators.OrderRestrictions(context, message, message.OrderRestrictions);
		if (message.AccountType             is not null) context.Validators.AccountType(context, message, message.AccountType);
		if (message.CustOrderCapacity       is not null) context.Validators.CustOrderCapacity(context, message, message.CustOrderCapacity);
		if (message.PreallocMethod          is not null) context.Validators.PreallocMethod(context, message, message.PreallocMethod);
		if (message.ClearingFeeIndicator    is not null) context.Validators.ClearingFeeIndicator(context, message, message.ClearingFeeIndicator);
		if (message.AcctIDSource            is not null) context.Validators.AcctIDSource(context, message, message.AcctIDSource);
		if (message.ResponseTransportType   is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.TradeReportRejectReason is not null) context.Validators.TradeReportRejectReason(context, message, message.TradeReportRejectReason);
		if (message.TrdType                 is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.TradeReportType         is not null) context.Validators.TradeReportType(context, message, message.TradeReportType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.TrdRptStatus            is not null) context.Validators.TrdRptStatus(context, message, message.TrdRptStatus);

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

		if (!Empty((IFinancingDetails)message)) context.Validators.FinancingDetails(context, message, message);
		if (!Empty((IInstrument)message)) context.Validators.Instrument(context, message, message);
		if (!Empty((IInstrumentExtension)message)) context.Validators.InstrumentExtension(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.Side                    is not null) context.Validators.Side(context, message, message.Side);
		if (message.ExecType                is not null) context.Validators.ExecType(context, message, message.ExecType);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.MultiLegReportingType   is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.TradeRequestType        is not null) context.Validators.TradeRequestType(context, message, message.TradeRequestType);
		if (message.MatchStatus             is not null) context.Validators.MatchStatus(context, message, message.MatchStatus);
		if (message.DeliveryForm            is not null) context.Validators.DeliveryForm(context, message, message.DeliveryForm);
		if (message.ResponseTransportType   is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.TerminationType         is not null) context.Validators.TerminationType(context, message, message.TerminationType);
		if (message.TrdType                 is not null) context.Validators.TrdType(context, message, message.TrdType);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);
		if (message.DeliveryType            is not null) context.Validators.DeliveryType(context, message, message.DeliveryType);

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
		else context.Validators.Instrument(context, message, message);

		if (message.SecurityIDSource        is not null) context.Validators.SecurityIDSource(context, message, message.SecurityIDSource);
		if (message.SecurityType            is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.PutOrCall               is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.MultiLegReportingType   is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);
		if (message.Product                 is not null) context.Validators.Product(context, message, message.Product);
		if (message.TradeRequestType        is not null) context.Validators.TradeRequestType(context, message, message.TradeRequestType);
		if (message.ResponseTransportType   is not null) context.Validators.ResponseTransportType(context, message, message.ResponseTransportType);
		if (message.TradeRequestResult      is not null) context.Validators.TradeRequestResult(context, message, message.TradeRequestResult);
		if (message.TradeRequestStatus      is not null) context.Validators.TradeRequestStatus(context, message, message.TradeRequestStatus);
		if (message.CPProgram               is not null) context.Validators.CPProgram(context, message, message.CPProgram);

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

		if (message.TradSesMethod          is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode            is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);
		if (message.TradSesStatus          is not null) context.Validators.TradSesStatus(context, message, message.TradSesStatus);
		if (message.TradSesStatusRejReason is not null) context.Validators.TradSesStatusRejReason(context, message, message.TradSesStatusRejReason);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatusRequest(FixContext context, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.SubscriptionRequestType is null) Missing(message, 263);
		if (message.TradSesReqID            is null) Missing(message, 335);

		if (message.SubscriptionRequestType is not null) context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradSesMethod           is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode             is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);

		return message.IsValid;
	}

	static bool ValidateUserRequest(FixContext context, FixMessage.UserRequest message)
	{
		if (message.Username        is null) Missing(message, 553);
		if (message.UserRequestID   is null) Missing(message, 923);
		if (message.UserRequestType is null) Missing(message, 924);

		if (message.UserRequestType is not null) context.Validators.UserRequestType(context, message, message.UserRequestType);

		return message.IsValid;
	}

	static bool ValidateUserResponse(FixContext context, FixMessage.UserResponse message)
	{
		if (message.Username      is null) Missing(message, 553);
		if (message.UserRequestID is null) Missing(message, 923);

		if (message.UserStatus is not null) context.Validators.UserStatus(context, message, message.UserStatus);

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

	static bool ValidateCommissionData(FixContext context, FixMessage message, ICommissionData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateDiscretionInstructions(FixContext context, FixMessage message, IDiscretionInstructions block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateFinancingDetails(FixContext context, FixMessage message, IFinancingDetails block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateInstrument(FixContext context, FixMessage message, IInstrument block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateInstrumentExtension(FixContext context, FixMessage message, IInstrumentExtension block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateInstrumentLeg(FixContext context, FixMessage message, IInstrumentLeg block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateLegBenchmarkCurveData(FixContext context, FixMessage message, ILegBenchmarkCurveData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateOrderQtyData(FixContext context, FixMessage message, IOrderQtyData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidatePegInstructions(FixContext context, FixMessage message, IPegInstructions block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateSettlInstructionsData(FixContext context, FixMessage message, ISettlInstructionsData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateSpreadOrBenchmarkCurveData(FixContext context, FixMessage message, ISpreadOrBenchmarkCurveData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateUnderlyingInstrument(FixContext context, FixMessage message, IUnderlyingInstrument block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
		return message.IsValid;
	}

	static bool ValidateYieldData(FixContext context, FixMessage message, IYieldData block)
	{
		// The repository requires no field of this block. A dictionary that does replaces this slot.
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

	static bool ValidateCommType(FixContext context, FixMessage message, FixField.CommType field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6'))
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

	static bool ValidateIOIQltyInd(FixContext context, FixMessage message, FixField.IOIQltyInd field)
	{
		if (field.Value is not ('H' or 'L' or 'M'))
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

	static bool ValidateSide(FixContext context, FixMessage message, FixField.Side field)
	{
		if (field.Value is not ('1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or 'A' or 'B' or 'C' or
			'D' or 'E' or 'F' or 'G'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTimeInForce(FixContext context, FixMessage message, FixField.TimeInForce field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUrgency(FixContext context, FixMessage message, FixField.Urgency field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlType(FixContext context, FixMessage message, FixField.SettlType field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocTransType(FixContext context, FixMessage message, FixField.AllocTransType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePositionEffect(FixContext context, FixMessage message, FixField.PositionEffect field)
	{
		if (field.Value is not ('C' or 'F' or 'O' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProcessCode(FixContext context, FixMessage message, FixField.ProcessCode field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocStatus(FixContext context, FixMessage message, FixField.AllocStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocRejCode(FixContext context, FixMessage message, FixField.AllocRejCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 12, 13, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEmailType(FixContext context, FixMessage message, FixField.EmailType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEncryptMethod(FixContext context, FixMessage message, FixField.EncryptMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCxlRejReason(FixContext context, FixMessage message, FixField.CxlRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOrdRejReason(FixContext context, FixMessage message, FixField.OrdRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
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

	static bool ValidateDKReason(FixContext context, FixMessage message, FixField.DKReason field)
	{
		if (field.Value is not ('A' or 'B' or 'C' or 'D' or 'E' or 'F' or 'Z'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeType(FixContext context, FixMessage message, FixField.MiscFeeType field)
	{
		if (field.Value is not ("1" or "10" or "11" or "12" or "2" or "3" or "4" or "5" or "6" or "7" or "8" or "9"))
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

	static bool ValidateSettlCurrFxRateCalc(FixContext context, FixMessage message, FixField.SettlCurrFxRateCalc field)
	{
		if (field.Value is not ('D' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstMode(FixContext context, FixMessage message, FixField.SettlInstMode field)
	{
		if (field.Value is not ('1' or '4' or '5'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstTransType(FixContext context, FixMessage message, FixField.SettlInstTransType field)
	{
		if (field.Value is not ('C' or 'N' or 'R' or 'T'))
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

	static bool ValidateStandInstDbType(FixContext context, FixMessage message, FixField.StandInstDbType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlDeliveryType(FixContext context, FixMessage message, FixField.SettlDeliveryType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocLinkType(FixContext context, FixMessage message, FixField.AllocLinkType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePutOrCall(FixContext context, FixMessage message, FixField.PutOrCall field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCoveredOrUncovered(FixContext context, FixMessage message, FixField.CoveredOrUncovered field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocHandlInst(FixContext context, FixMessage message, FixField.AllocHandlInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoutingType(FixContext context, FixMessage message, FixField.RoutingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
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

	static bool ValidateSubscriptionRequestType(FixContext context, FixMessage message, FixField.SubscriptionRequestType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMDUpdateType(FixContext context, FixMessage message, FixField.MDUpdateType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
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

	static bool ValidateTickDirection(FixContext context, FixMessage message, FixField.TickDirection field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3'))
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

	static bool ValidateMDUpdateAction(FixContext context, FixMessage message, FixField.MDUpdateAction field)
	{
		if (field.Value is not ('0' or '1' or '2'))
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

	static bool ValidateQuoteStatus(FixContext context, FixMessage message, FixField.QuoteStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteCancelType(FixContext context, FixMessage message, FixField.QuoteCancelType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRejectReason(FixContext context, FixMessage message, FixField.QuoteRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteResponseLevel(FixContext context, FixMessage message, FixField.QuoteResponseLevel field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestType(FixContext context, FixMessage message, FixField.QuoteRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestType(FixContext context, FixMessage message, FixField.SecurityRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityResponseType(FixContext context, FixMessage message, FixField.SecurityResponseType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityTradingStatus(FixContext context, FixMessage message, FixField.SecurityTradingStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 23, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateHaltReason(FixContext context, FixMessage message, FixField.HaltReason field)
	{
		if (field.Value is not ('D' or 'E' or 'I' or 'M' or 'P' or 'X'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustment(FixContext context, FixMessage message, FixField.Adjustment field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMethod(FixContext context, FixMessage message, FixField.TradSesMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesMode(FixContext context, FixMessage message, FixField.TradSesMode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatus(FixContext context, FixMessage message, FixField.TradSesStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMessageEncoding(FixContext context, FixMessage message, FixField.MessageEncoding field)
	{
		if (field.Value is not ("EUC-JP" or "ISO-2022-JP" or "Shift_JIS" or "UTF-8"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSessionRejectReason(FixContext context, FixMessage message, FixField.SessionRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 12, 13, 14, 15, 16, 17, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidRequestTransType(FixContext context, FixMessage message, FixField.BidRequestTransType field)
	{
		if (field.Value is not ('C' or 'N'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExecRestatementReason(FixContext context, FixMessage message, FixField.ExecRestatementReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBusinessRejectReason(FixContext context, FixMessage message, FixField.BusinessRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 7]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMsgDirection(FixContext context, FixMessage message, FixField.MsgDirection field)
	{
		if (field.Value is not ('R' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionInst(FixContext context, FixMessage message, FixField.DiscretionInst field)
	{
		if (field.Value is not ('0' or '1' or '2' or '3' or '4' or '5' or '6'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidType(FixContext context, FixMessage message, FixField.BidType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBidDescriptorType(FixContext context, FixMessage message, FixField.BidDescriptorType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideValueInd(FixContext context, FixMessage message, FixField.SideValueInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLiquidityIndType(FixContext context, FixMessage message, FixField.LiquidityIndType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProgRptReqs(FixContext context, FixMessage message, FixField.ProgRptReqs field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateIncTaxInd(FixContext context, FixMessage message, FixField.IncTaxInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
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

	static bool ValidatePriceType(FixContext context, FixMessage message, FixField.PriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateGTBookingInst(FixContext context, FixMessage message, FixField.GTBookingInst field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListStatusType(FixContext context, FixMessage message, FixField.ListStatusType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetGrossInd(FixContext context, FixMessage message, FixField.NetGrossInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateListOrderStatus(FixContext context, FixMessage message, FixField.ListOrderStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6, 7]))
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

	static bool ValidateMultiLegReportingType(FixContext context, FixMessage message, FixField.MultiLegReportingType field)
	{
		if (field.Value is not ('1' or '2' or '3'))
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

	static bool ValidatePartyRole(FixContext context, FixMessage message, FixField.PartyRole field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 24, 25, 26, 27, 28,
			29, 3, 30, 31, 32, 33, 34, 35, 36, 37, 38, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateProduct(FixContext context, FixMessage message, FixField.Product field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRoundingDirection(FixContext context, FixMessage message, FixField.RoundingDirection field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDistribPaymentMethod(FixContext context, FixMessage message, FixField.DistribPaymentMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 2, 3, 4, 5, 6, 7, 8, 9]))
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

	static bool ValidateExecPriceType(FixContext context, FixMessage message, FixField.ExecPriceType field)
	{
		if (field.Value is not ('B' or 'C' or 'D' or 'E' or 'O' or 'P' or 'Q' or 'S'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePaymentMethod(FixContext context, FixMessage message, FixField.PaymentMethod field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTaxAdvantageType(FixContext context, FixMessage message, FixField.TaxAdvantageType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 23, 24, 25, 26,
			27, 28, 29, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateFundRenewWaiv(FixContext context, FixMessage message, FixField.FundRenewWaiv field)
	{
		if (field.Value is not ('N' or 'Y'))
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
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateRegistTransType(FixContext context, FixMessage message, FixField.RegistTransType field)
	{
		if (field.Value is not ('0' or '1' or '2'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnershipType(FixContext context, FixMessage message, FixField.OwnershipType field)
	{
		if (field.Value is not ('2' or 'J' or 'T'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateContAmtType(FixContext context, FixMessage message, FixField.ContAmtType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateOwnerType(FixContext context, FixMessage message, FixField.OwnerType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 2, 3, 4, 5, 6, 7, 8, 9]))
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

	static bool ValidateQuoteType(FixContext context, FixMessage message, FixField.QuoteType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCashMargin(FixContext context, FixMessage message, FixField.CashMargin field)
	{
		if (field.Value is not ('1' or '2' or '3'))
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

	static bool ValidateCrossType(FixContext context, FixMessage message, FixField.CrossType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCrossPrioritization(FixContext context, FixMessage message, FixField.CrossPrioritization field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNoSides(FixContext context, FixMessage message, FixField.NoSides field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityListRequestType(FixContext context, FixMessage message, FixField.SecurityListRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSecurityRequestResult(FixContext context, FixMessage message, FixField.SecurityRequestResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMultiLegRptTypeReq(FixContext context, FixMessage message, FixField.MultiLegRptTypeReq field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradSesStatusRejReason(FixContext context, FixMessage message, FixField.TradSesStatusRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestType(FixContext context, FixMessage message, FixField.TradeRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
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

	static bool ValidateClearingInstruction(FixContext context, FixMessage message, FixField.ClearingInstruction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 11, 12, 13, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAccountType(FixContext context, FixMessage message, FixField.AccountType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 6, 7, 8]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCustOrderCapacity(FixContext context, FixMessage message, FixField.CustOrderCapacity field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMassStatusReqType(FixContext context, FixMessage message, FixField.MassStatusReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6, 7, 8]))
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

	static bool ValidateAllocType(FixContext context, FixMessage message, FixField.AllocType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 5, 7, 8]))
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

	static bool ValidatePriorityIndicator(FixContext context, FixMessage message, FixField.PriorityIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRequestRejectReason(FixContext context, FixMessage message, FixField.QuoteRequestRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAcctIDSource(FixContext context, FixMessage message, FixField.AcctIDSource field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmStatus(FixContext context, FixMessage message, FixField.ConfirmStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmTransType(FixContext context, FixMessage message, FixField.ConfirmTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryForm(FixContext context, FixMessage message, FixField.DeliveryForm field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLegSwapType(FixContext context, FixMessage message, FixField.LegSwapType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuotePriceType(FixContext context, FixMessage message, FixField.QuotePriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQuoteRespType(FixContext context, FixMessage message, FixField.QuoteRespType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6]))
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

	static bool ValidatePosQtyStatus(FixContext context, FixMessage message, FixField.PosQtyStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosAmtType(FixContext context, FixMessage message, FixField.PosAmtType field)
	{
		if (field.Value is not ("CASH" or "CRES" or "FMTM" or "IMTM" or "PREM" or "SMTM" or "TVAR" or "VADJ"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosTransType(FixContext context, FixMessage message, FixField.PosTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintAction(FixContext context, FixMessage message, FixField.PosMaintAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlSessID(FixContext context, FixMessage message, FixField.SettlSessID field)
	{
		if (field.Value is not ("ETH" or "ITD" or "RTH"))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAdjustmentType(FixContext context, FixMessage message, FixField.AdjustmentType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintStatus(FixContext context, FixMessage message, FixField.PosMaintStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosMaintResult(FixContext context, FixMessage message, FixField.PosMaintResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqType(FixContext context, FixMessage message, FixField.PosReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateResponseTransportType(FixContext context, FixMessage message, FixField.ResponseTransportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqResult(FixContext context, FixMessage message, FixField.PosReqResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePosReqStatus(FixContext context, FixMessage message, FixField.PosReqStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlPriceType(FixContext context, FixMessage message, FixField.SettlPriceType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAssignmentMethod(FixContext context, FixMessage message, FixField.AssignmentMethod field)
	{
		if (field.Value is not ('P' or 'R'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExerciseMethod(FixContext context, FixMessage message, FixField.ExerciseMethod field)
	{
		if (field.Value is not ('A' or 'M'))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestResult(FixContext context, FixMessage message, FixField.TradeRequestResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeRequestStatus(FixContext context, FixMessage message, FixField.TradeRequestStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportRejectReason(FixContext context, FixMessage message, FixField.TradeReportRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSideMultiLegReportingType(FixContext context, FixMessage message, FixField.SideMultiLegReportingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRegTimestampType(FixContext context, FixMessage message, FixField.TrdRegTimestampType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmType(FixContext context, FixMessage message, FixField.ConfirmType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateConfirmRejReason(FixContext context, FixMessage message, FixField.ConfirmRejReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateBookingType(FixContext context, FixMessage message, FixField.BookingType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocSettlInstType(FixContext context, FixMessage message, FixField.AllocSettlInstType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
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
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateSettlInstReqRejCode(FixContext context, FixMessage message, FixField.SettlInstReqRejCode field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocReportType(FixContext context, FixMessage message, FixField.AllocReportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [3, 4, 5, 8]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocCancReplaceReason(FixContext context, FixMessage message, FixField.AllocCancReplaceReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocAccountType(FixContext context, FixMessage message, FixField.AllocAccountType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 6, 7, 8]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePartySubIDType(FixContext context, FixMessage message, FixField.PartySubIDType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 23, 24, 25, 26, 3,
			4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocIntermedReqType(FixContext context, FixMessage message, FixField.AllocIntermedReqType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueResolution(FixContext context, FixMessage message, FixField.ApplQueueResolution field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateApplQueueAction(FixContext context, FixMessage message, FixField.ApplQueueAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAvgPxIndicator(FixContext context, FixMessage message, FixField.AvgPxIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeAllocIndicator(FixContext context, FixMessage message, FixField.TradeAllocIndicator field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateExpirationCycle(FixContext context, FixMessage message, FixField.ExpirationCycle field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdType(FixContext context, FixMessage message, FixField.TrdType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 10, 2, 3, 4, 5, 6, 7, 8, 9]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegMoveType(FixContext context, FixMessage message, FixField.PegMoveType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegOffsetType(FixContext context, FixMessage message, FixField.PegOffsetType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegLimitType(FixContext context, FixMessage message, FixField.PegLimitType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegRoundDirection(FixContext context, FixMessage message, FixField.PegRoundDirection field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidatePegScope(FixContext context, FixMessage message, FixField.PegScope field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionMoveType(FixContext context, FixMessage message, FixField.DiscretionMoveType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionOffsetType(FixContext context, FixMessage message, FixField.DiscretionOffsetType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionLimitType(FixContext context, FixMessage message, FixField.DiscretionLimitType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionRoundDirection(FixContext context, FixMessage message, FixField.DiscretionRoundDirection field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDiscretionScope(FixContext context, FixMessage message, FixField.DiscretionScope field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTargetStrategy(FixContext context, FixMessage message, FixField.TargetStrategy field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateLastLiquidityInd(FixContext context, FixMessage message, FixField.LastLiquidityInd field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateShortSaleReason(FixContext context, FixMessage message, FixField.ShortSaleReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateQtyType(FixContext context, FixMessage message, FixField.QtyType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTradeReportType(FixContext context, FixMessage message, FixField.TradeReportType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 7]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAllocNoOrdersType(FixContext context, FixMessage message, FixField.AllocNoOrdersType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateEventType(FixContext context, FixMessage message, FixField.EventType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateInstrAttribType(FixContext context, FixMessage message, FixField.InstrAttribType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 2, 20, 21, 22, 3, 4, 5, 6, 7, 8, 9,
			99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCPProgram(FixContext context, FixMessage message, FixField.CPProgram field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateMiscFeeBasis(FixContext context, FixMessage message, FixField.MiscFeeBasis field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnReason(FixContext context, FixMessage message, FixField.CollAsgnReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 7]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryQualifier(FixContext context, FixMessage message, FixField.CollInquiryQualifier field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 7]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnTransType(FixContext context, FixMessage message, FixField.CollAsgnTransType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRespType(FixContext context, FixMessage message, FixField.CollAsgnRespType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAsgnRejectReason(FixContext context, FixMessage message, FixField.CollAsgnRejectReason field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 99]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollStatus(FixContext context, FixMessage message, FixField.CollStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateDeliveryType(FixContext context, FixMessage message, FixField.DeliveryType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserRequestType(FixContext context, FixMessage message, FixField.UserRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateUserStatus(FixContext context, FixMessage message, FixField.UserStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4, 5, 6]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateStatusValue(FixContext context, FixMessage message, FixField.StatusValue field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkRequestType(FixContext context, FixMessage message, FixField.NetworkRequestType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 4, 8]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateNetworkStatusResponseType(FixContext context, FixMessage message, FixField.NetworkStatusResponseType field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateTrdRptStatus(FixContext context, FixMessage message, FixField.TrdRptStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateAffirmStatus(FixContext context, FixMessage message, FixField.AffirmStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [1, 2, 3]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollAction(FixContext context, FixMessage message, FixField.CollAction field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryStatus(FixContext context, FixMessage message, FixField.CollInquiryStatus field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4]))
			Invalid(message, field);

		return message.IsValid;
	}

	static bool ValidateCollInquiryResult(FixContext context, FixMessage message, FixField.CollInquiryResult field)
	{
		if (!field.IsValid)
			Invalid(message, field);
		else if (!Listed(field.Value, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 99]))
			Invalid(message, field);

		return message.IsValid;
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

	/// <summary>A value the specification does not list for its field.</summary>
	static void Invalid(FixMessage message, FixField field)
	{
		message.AddFinding(new FixFinding(FixRule.InvalidValue, field.Tag, field.Position, field, -1));
	}

	/// <summary>Whether a whole number is one of the values listed for its field.</summary>
	/// <remarks>The codes are read from static data rather than from an array built for the call.</remarks>
	static bool Listed(BigInteger value, ReadOnlySpan<int> codes)
	{
		foreach (var code in codes)
			if (value == code)
				return true;

		return false;
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
