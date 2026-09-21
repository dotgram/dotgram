using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// Which class holds the rule for which <c>MsgType</c>, and what this package compiles in for it.
/// </summary>
/// <remarks>
/// <para>
/// Validating a message asks no table: a message type and a class are the same thing, so a message
/// reads its own class's field. Loading a dictionary is the other direction — a file names types
/// and something has to find the field each one belongs to — and that is what this is for. It is
/// read once a load, never once a message.
/// </para>
/// <para>
/// <strong>One row a type, carrying both the way in and the way back.</strong> This was two lists
/// once: one mapping MsgType to a field, one putting the compiled rules back. They were written
/// separately, and they disagreed — the second held NewOrderSingle twice and Heartbeat not at all,
/// so a restore left Heartbeat holding a dictionary's rule and nothing said so. Two lists of one
/// set diverge in silence; one list cannot.
/// </para>
/// <para>
/// Internal, because a consumer reaching a rule reaches it by name: <c>FixMessage.NewOrderSingle.Rule</c>.
/// </para>
/// </remarks>
static class FixRuleFields
{
	/// <summary>Every type this package models: its MsgType, its field, and its compiled rule.</summary>
	static readonly (string Type, Action<FixMessageRule> Set, FixMessageRule Compiled)[] _rules =
	[
		("0",   rule => FixMessage.Heartbeat.Rule = rule, FixValidator.ValidateHeartbeat),
		("1",   rule => FixMessage.TestRequest.Rule = rule, FixValidator.ValidateTestRequest),
		("2",   rule => FixMessage.ResendRequest.Rule = rule, FixValidator.ValidateResendRequest),
		("3",   rule => FixMessage.Reject.Rule = rule, FixValidator.ValidateReject),
		("4",   rule => FixMessage.SequenceReset.Rule = rule, FixValidator.ValidateSequenceReset),
		("5",   rule => FixMessage.Logout.Rule = rule, FixValidator.ValidateLogout),
		("6",   rule => FixMessage.IOI.Rule = rule, FixValidator.ValidateIOI),
		("7",   rule => FixMessage.Advertisement.Rule = rule, FixValidator.ValidateAdvertisement),
		("8",   rule => FixMessage.ExecutionReport.Rule = rule, FixValidator.ValidateExecutionReport),
		("9",   rule => FixMessage.OrderCancelReject.Rule = rule, FixValidator.ValidateOrderCancelReject),
		("A",   rule => FixMessage.Logon.Rule = rule, FixValidator.ValidateLogon),
		("B",   rule => FixMessage.News.Rule = rule, FixValidator.ValidateNews),
		("C",   rule => FixMessage.Email.Rule = rule, FixValidator.ValidateEmail),
		("D",   rule => FixMessage.NewOrderSingle.Rule = rule, FixValidator.ValidateNewOrderSingle),
		("E",   rule => FixMessage.NewOrderList.Rule = rule, FixValidator.ValidateNewOrderList),
		("F",   rule => FixMessage.OrderCancelRequest.Rule = rule, FixValidator.ValidateOrderCancelRequest),
		("G",   rule => FixMessage.OrderCancelReplaceRequest.Rule = rule, FixValidator.ValidateOrderCancelReplaceRequest),
		("H",   rule => FixMessage.OrderStatusRequest.Rule = rule, FixValidator.ValidateOrderStatusRequest),
		("J",   rule => FixMessage.AllocationInstruction.Rule = rule, FixValidator.ValidateAllocationInstruction),
		("K",   rule => FixMessage.ListCancelRequest.Rule = rule, FixValidator.ValidateListCancelRequest),
		("L",   rule => FixMessage.ListExecute.Rule = rule, FixValidator.ValidateListExecute),
		("M",   rule => FixMessage.ListStatusRequest.Rule = rule, FixValidator.ValidateListStatusRequest),
		("N",   rule => FixMessage.ListStatus.Rule = rule, FixValidator.ValidateListStatus),
		("P",   rule => FixMessage.AllocationInstructionAck.Rule = rule, FixValidator.ValidateAllocationInstructionAck),
		("Q",   rule => FixMessage.DontKnowTrade.Rule = rule, FixValidator.ValidateDontKnowTrade),
		("R",   rule => FixMessage.QuoteRequest.Rule = rule, FixValidator.ValidateQuoteRequest),
		("S",   rule => FixMessage.Quote.Rule = rule, FixValidator.ValidateQuote),
		("T",   rule => FixMessage.SettlementInstructions.Rule = rule, FixValidator.ValidateSettlementInstructions),
		("V",   rule => FixMessage.MarketDataRequest.Rule = rule, FixValidator.ValidateMarketDataRequest),
		("W",   rule => FixMessage.MarketDataSnapshotFullRefresh.Rule = rule, FixValidator.ValidateMarketDataSnapshotFullRefresh),
		("X",   rule => FixMessage.MarketDataIncrementalRefresh.Rule = rule, FixValidator.ValidateMarketDataIncrementalRefresh),
		("Y",   rule => FixMessage.MarketDataRequestReject.Rule = rule, FixValidator.ValidateMarketDataRequestReject),
		("Z",   rule => FixMessage.QuoteCancel.Rule = rule, FixValidator.ValidateQuoteCancel),
		("a",   rule => FixMessage.QuoteStatusRequest.Rule = rule, FixValidator.ValidateQuoteStatusRequest),
		("b",   rule => FixMessage.MassQuoteAcknowledgement.Rule = rule, FixValidator.ValidateMassQuoteAcknowledgement),
		("c",   rule => FixMessage.SecurityDefinitionRequest.Rule = rule, FixValidator.ValidateSecurityDefinitionRequest),
		("d",   rule => FixMessage.SecurityDefinition.Rule = rule, FixValidator.ValidateSecurityDefinition),
		("e",   rule => FixMessage.SecurityStatusRequest.Rule = rule, FixValidator.ValidateSecurityStatusRequest),
		("f",   rule => FixMessage.SecurityStatus.Rule = rule, FixValidator.ValidateSecurityStatus),
		("g",   rule => FixMessage.TradingSessionStatusRequest.Rule = rule, FixValidator.ValidateTradingSessionStatusRequest),
		("h",   rule => FixMessage.TradingSessionStatus.Rule = rule, FixValidator.ValidateTradingSessionStatus),
		("i",   rule => FixMessage.MassQuote.Rule = rule, FixValidator.ValidateMassQuote),
		("j",   rule => FixMessage.BusinessMessageReject.Rule = rule, FixValidator.ValidateBusinessMessageReject),
		("k",   rule => FixMessage.BidRequest.Rule = rule, FixValidator.ValidateBidRequest),
		("l",   rule => FixMessage.BidResponse.Rule = rule, FixValidator.ValidateBidResponse),
		("m",   rule => FixMessage.ListStrikePrice.Rule = rule, FixValidator.ValidateListStrikePrice),
		("n",   rule => FixMessage.XMLnonFIX.Rule = rule, FixValidator.ValidateXMLnonFIX),
		("o",   rule => FixMessage.RegistrationInstructions.Rule = rule, FixValidator.ValidateRegistrationInstructions),
		("p",   rule => FixMessage.RegistrationInstructionsResponse.Rule = rule, FixValidator.ValidateRegistrationInstructionsResponse),
		("q",   rule => FixMessage.OrderMassCancelRequest.Rule = rule, FixValidator.ValidateOrderMassCancelRequest),
		("r",   rule => FixMessage.OrderMassCancelReport.Rule = rule, FixValidator.ValidateOrderMassCancelReport),
		("s",   rule => FixMessage.NewOrderCross.Rule = rule, FixValidator.ValidateNewOrderCross),
		("t",   rule => FixMessage.CrossOrderCancelReplaceRequest.Rule = rule, FixValidator.ValidateCrossOrderCancelReplaceRequest),
		("u",   rule => FixMessage.CrossOrderCancelRequest.Rule = rule, FixValidator.ValidateCrossOrderCancelRequest),
		("v",   rule => FixMessage.SecurityTypeRequest.Rule = rule, FixValidator.ValidateSecurityTypeRequest),
		("w",   rule => FixMessage.SecurityTypes.Rule = rule, FixValidator.ValidateSecurityTypes),
		("x",   rule => FixMessage.SecurityListRequest.Rule = rule, FixValidator.ValidateSecurityListRequest),
		("y",   rule => FixMessage.SecurityList.Rule = rule, FixValidator.ValidateSecurityList),
		("z",   rule => FixMessage.DerivativeSecurityListRequest.Rule = rule, FixValidator.ValidateDerivativeSecurityListRequest),
		("AA",  rule => FixMessage.DerivativeSecurityList.Rule = rule, FixValidator.ValidateDerivativeSecurityList),
		("AB",  rule => FixMessage.NewOrderMultileg.Rule = rule, FixValidator.ValidateNewOrderMultileg),
		("AC",  rule => FixMessage.MultilegOrderCancelReplace.Rule = rule, FixValidator.ValidateMultilegOrderCancelReplace),
		("AD",  rule => FixMessage.TradeCaptureReportRequest.Rule = rule, FixValidator.ValidateTradeCaptureReportRequest),
		("AE",  rule => FixMessage.TradeCaptureReport.Rule = rule, FixValidator.ValidateTradeCaptureReport),
		("AF",  rule => FixMessage.OrderMassStatusRequest.Rule = rule, FixValidator.ValidateOrderMassStatusRequest),
		("AG",  rule => FixMessage.QuoteRequestReject.Rule = rule, FixValidator.ValidateQuoteRequestReject),
		("AH",  rule => FixMessage.RFQRequest.Rule = rule, FixValidator.ValidateRFQRequest),
		("AI",  rule => FixMessage.QuoteStatusReport.Rule = rule, FixValidator.ValidateQuoteStatusReport),
		("AJ",  rule => FixMessage.QuoteResponse.Rule = rule, FixValidator.ValidateQuoteResponse),
		("AK",  rule => FixMessage.Confirmation.Rule = rule, FixValidator.ValidateConfirmation),
		("AL",  rule => FixMessage.PositionMaintenanceRequest.Rule = rule, FixValidator.ValidatePositionMaintenanceRequest),
		("AM",  rule => FixMessage.PositionMaintenanceReport.Rule = rule, FixValidator.ValidatePositionMaintenanceReport),
		("AN",  rule => FixMessage.RequestForPositions.Rule = rule, FixValidator.ValidateRequestForPositions),
		("AO",  rule => FixMessage.RequestForPositionsAck.Rule = rule, FixValidator.ValidateRequestForPositionsAck),
		("AP",  rule => FixMessage.PositionReport.Rule = rule, FixValidator.ValidatePositionReport),
		("AQ",  rule => FixMessage.TradeCaptureReportRequestAck.Rule = rule, FixValidator.ValidateTradeCaptureReportRequestAck),
		("AR",  rule => FixMessage.TradeCaptureReportAck.Rule = rule, FixValidator.ValidateTradeCaptureReportAck),
		("AS",  rule => FixMessage.AllocationReport.Rule = rule, FixValidator.ValidateAllocationReport),
		("AT",  rule => FixMessage.AllocationReportAck.Rule = rule, FixValidator.ValidateAllocationReportAck),
		("AU",  rule => FixMessage.ConfirmationAck.Rule = rule, FixValidator.ValidateConfirmationAck),
		("AV",  rule => FixMessage.SettlementInstructionRequest.Rule = rule, FixValidator.ValidateSettlementInstructionRequest),
		("AW",  rule => FixMessage.AssignmentReport.Rule = rule, FixValidator.ValidateAssignmentReport),
		("AX",  rule => FixMessage.CollateralRequest.Rule = rule, FixValidator.ValidateCollateralRequest),
		("AY",  rule => FixMessage.CollateralAssignment.Rule = rule, FixValidator.ValidateCollateralAssignment),
		("AZ",  rule => FixMessage.CollateralResponse.Rule = rule, FixValidator.ValidateCollateralResponse),
		("BA",  rule => FixMessage.CollateralReport.Rule = rule, FixValidator.ValidateCollateralReport),
		("BB",  rule => FixMessage.CollateralInquiry.Rule = rule, FixValidator.ValidateCollateralInquiry),
		("BC",  rule => FixMessage.NetworkCounterpartySystemStatusRequest.Rule = rule, FixValidator.ValidateNetworkCounterpartySystemStatusRequest),
		("BD",  rule => FixMessage.NetworkCounterpartySystemStatusResponse.Rule = rule, FixValidator.ValidateNetworkCounterpartySystemStatusResponse),
		("BE",  rule => FixMessage.UserRequest.Rule = rule, FixValidator.ValidateUserRequest),
		("BF",  rule => FixMessage.UserResponse.Rule = rule, FixValidator.ValidateUserResponse),
		("BG",  rule => FixMessage.CollateralInquiryAck.Rule = rule, FixValidator.ValidateCollateralInquiryAck),
		("BH",  rule => FixMessage.ConfirmationRequest.Rule = rule, FixValidator.ValidateConfirmationRequest),
	];

	/// <summary>The field each <c>MsgType</c> writes to, which is what a load walks.</summary>
	public static readonly Dictionary<string, Action<FixMessageRule>> Fields = Build();

	static Dictionary<string, Action<FixMessageRule>> Build()
	{
		var fields = new Dictionary<string, Action<FixMessageRule>>(StringComparer.Ordinal);

		foreach (var one in _rules)
		{
			fields.Add(one.Type, one.Set);
		}

		return fields;
	}

	/// <summary>Puts every class back to the rule this package compiles in.</summary>
	/// <remarks>
	/// Not offered outside: a consumer undoes a replacement by assigning the name back, one class
	/// at a time, which is the whole of the seam. This exists because a test that loads somebody's
	/// dictionary has to leave the process as it found it.
	/// </remarks>
	public static void Compiled()
	{
		foreach (var one in _rules)
		{
			one.Set(one.Compiled);
		}

		FixMessage.Custom.Rule = FixValidator.ValidateCustom;
	}
}
