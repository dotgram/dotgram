using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.cs from the FIX 4.2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The check of every FIX 4.2 message type, of every component it reuses, and of every entry of
/// every repeating group; the fields' own are in FixValidator42.Fields.cs.
/// </summary>
/// <remarks>
/// Straight-line checks written against the fields of the class they are about, and nothing else:
/// a field the repository marks required, a component it marks required and the carrier left empty,
/// every field handed to its own slot, and every count held to the entries that follow it. Every
/// slot is typed on what it checks and named for it — a message type, a component, and a group
/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run
/// time replaces the slots it describes by name and leaves the rest.
/// </remarks>
partial class FixValidator42
{
	/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>
	public static readonly FixValidator42 Default = new();

	/// <summary>The tag of a field this version names otherwise than FixTag does.</summary>
	private protected override int FieldTag(string name)
	{
		return name switch
		{
			"AllocShares"         => 80,
			"AvgPrxPrecision"     => 74,
			"DiscretionOffset"    => 389,
			"FutSettDate"         => 64,
			"FutSettDate2"        => 193,
			"IDSource"            => 22,
			"IOIShares"           => 27,
			"IOIid"               => 23,
			"LastShares"          => 32,
			"LinesOfText"         => 33,
			"OpenClose"           => 77,
			"OpenCloseSettleFlag" => 286,
			"PegDifference"       => 211,
			"QuoteAckStatus"      => 297,
			"SettlmntTyp"         => 63,
			"Shares"              => 53,
			"SpreadToBenchmark"   => 218,
			"TotQuoteEntries"     => 304,
			"TotalNumSecurities"  => 393,
			"TradeType"           => 418,
			"UnderlyingIDSource"  => 305,
			_                     => 0,
		};
	}

	/// <summary>Holds a FIX 4.2 Advertisement to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Advertisement> Advertisement { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 4.2 Allocation to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Allocation> Allocation { get; set; } = ValidateAllocation;

	/// <summary>Holds a FIX 4.2 AllocationACK to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.AllocationACK> AllocationACK { get; set; } = ValidateAllocationACK;

	/// <summary>Holds a FIX 4.2 BidRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BidRequest> BidRequest { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 4.2 BidResponse to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BidResponse> BidResponse { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 4.2 BusinessMessageReject to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BusinessMessageReject> BusinessMessageReject { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 4.2 DontKnowTrade to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.DontKnowTrade> DontKnowTrade { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 4.2 Email to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Email> Email { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 4.2 ExecutionReport to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ExecutionReport> ExecutionReport { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 4.2 Heartbeat to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Heartbeat> Heartbeat { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 4.2 IndicationofInterest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.IndicationofInterest> IndicationofInterest { get; set; } = ValidateIndicationofInterest;

	/// <summary>Holds a FIX 4.2 ListCancelRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListCancelRequest> ListCancelRequest { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 4.2 ListExecute to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListExecute> ListExecute { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 4.2 ListStatus to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListStatus> ListStatus { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 4.2 ListStatusRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListStatusRequest> ListStatusRequest { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 4.2 ListStrikePrice to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListStrikePrice> ListStrikePrice { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 4.2 Logon to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Logon> Logon { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 4.2 Logout to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Logout> Logout { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 4.2 MarketDataIncrementalRefresh to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataIncrementalRefresh> MarketDataIncrementalRefresh { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 4.2 MarketDataRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataRequest> MarketDataRequest { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 4.2 MarketDataRequestReject to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataRequestReject> MarketDataRequestReject { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 4.2 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh> MarketDataSnapshotFullRefresh { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 4.2 MassQuote to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MassQuote> MassQuote { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 4.2 NewOrderList to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderList> NewOrderList { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 4.2 NewOrderSingle to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderSingle> NewOrderSingle { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 4.2 News to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.News> News { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 4.2 OrderCancelReject to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderCancelReject> OrderCancelReject { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 4.2 OrderCancelReplaceRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderCancelReplaceRequest> OrderCancelReplaceRequest { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.2 OrderCancelRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderCancelRequest> OrderCancelRequest { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 4.2 OrderStatusRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderStatusRequest> OrderStatusRequest { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 4.2 Quote to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Quote> Quote { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 4.2 QuoteAcknowledgement to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteAcknowledgement> QuoteAcknowledgement { get; set; } = ValidateQuoteAcknowledgement;

	/// <summary>Holds a FIX 4.2 QuoteCancel to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteCancel> QuoteCancel { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 4.2 QuoteRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteRequest> QuoteRequest { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 4.2 QuoteStatusRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteStatusRequest> QuoteStatusRequest { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 4.2 Reject to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Reject> Reject { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 4.2 ResendRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ResendRequest> ResendRequest { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 4.2 SecurityDefinition to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityDefinition> SecurityDefinition { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 4.2 SecurityDefinitionRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityDefinitionRequest> SecurityDefinitionRequest { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 4.2 SecurityStatus to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityStatus> SecurityStatus { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 4.2 SecurityStatusRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityStatusRequest> SecurityStatusRequest { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 4.2 SequenceReset to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SequenceReset> SequenceReset { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 4.2 SettlementInstructions to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SettlementInstructions> SettlementInstructions { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 4.2 TestRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.TestRequest> TestRequest { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 4.2 TradingSessionStatus to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.TradingSessionStatus> TradingSessionStatus { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 4.2 TradingSessionStatusRequest to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.TradingSessionStatusRequest> TradingSessionStatusRequest { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds one entry of FixMessage.Allocation.NoOrdersGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Allocation.NoOrdersGroup> Allocation_NoOrders { get; set; } = ValidateAllocation_NoOrders;

	/// <summary>Holds one entry of FixMessage.Allocation.NoExecsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Allocation.NoExecsGroup> Allocation_NoExecs { get; set; } = ValidateAllocation_NoExecs;

	/// <summary>Holds one entry of FixMessage.Allocation.NoAllocsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Allocation.NoAllocsGroup> Allocation_NoAllocs { get; set; } = ValidateAllocation_NoAllocs;

	/// <summary>Holds one entry of FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup> Allocation_NoAllocs_NoMiscFees { get; set; } = ValidateAllocation_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidDescriptorsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BidRequest.NoBidDescriptorsGroup> BidRequest_NoBidDescriptors { get; set; } = ValidateBidRequest_NoBidDescriptors;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BidRequest.NoBidComponentsGroup> BidRequest_NoBidComponents { get; set; } = ValidateBidRequest_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.BidResponse.NoBidComponentsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.BidResponse.NoBidComponentsGroup> BidResponse_NoBidComponents { get; set; } = ValidateBidResponse_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.Email.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Email.NoRoutingIDsGroup> Email_NoRoutingIDs { get; set; } = ValidateEmail_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.Email.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Email.NoRelatedSymGroup> Email_NoRelatedSym { get; set; } = ValidateEmail_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Email.LinesOfTextGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Email.LinesOfTextGroup> Email_LinesOfText { get; set; } = ValidateEmail_LinesOfText;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContraBrokersGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ExecutionReport.NoContraBrokersGroup> ExecutionReport_NoContraBrokers { get; set; } = ValidateExecutionReport_NoContraBrokers;

	/// <summary>Holds one entry of FixMessage.IndicationofInterest.NoIOIQualifiersGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.IndicationofInterest.NoIOIQualifiersGroup> IndicationofInterest_NoIOIQualifiers { get; set; } = ValidateIndicationofInterest_NoIOIQualifiers;

	/// <summary>Holds one entry of FixMessage.IndicationofInterest.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.IndicationofInterest.NoRoutingIDsGroup> IndicationofInterest_NoRoutingIDs { get; set; } = ValidateIndicationofInterest_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.ListStatus.NoOrdersGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListStatus.NoOrdersGroup> ListStatus_NoOrders { get; set; } = ValidateListStatus_NoOrders;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.ListStrikePrice.NoStrikesGroup> ListStrikePrice_NoStrikes { get; set; } = ValidateListStrikePrice_NoStrikes;

	/// <summary>Holds one entry of FixMessage.Logon.NoMsgTypesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.Logon.NoMsgTypesGroup> Logon_NoMsgTypes { get; set; } = ValidateLogon_NoMsgTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup> MarketDataIncrementalRefresh_NoMDEntries { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoMDEntryTypesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataRequest.NoMDEntryTypesGroup> MarketDataRequest_NoMDEntryTypes { get; set; } = ValidateMarketDataRequest_NoMDEntryTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataRequest.NoRelatedSymGroup> MarketDataRequest_NoRelatedSym { get; set; } = ValidateMarketDataRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup> MarketDataSnapshotFullRefresh_NoMDEntries { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup> MassQuote_NoQuoteSets { get; set; } = ValidateMassQuote_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup> MassQuote_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup> NewOrderList_NoOrders { get; set; } = ValidateNewOrderList_NoOrders;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup> NewOrderList_NoOrders_NoAllocs { get; set; } = ValidateNewOrderList_NoOrders_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup> NewOrderList_NoOrders_NoTradingSessions { get; set; } = ValidateNewOrderList_NoOrders_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoAllocsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderSingle.NoAllocsGroup> NewOrderSingle_NoAllocs { get; set; } = ValidateNewOrderSingle_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.NewOrderSingle.NoTradingSessionsGroup> NewOrderSingle_NoTradingSessions { get; set; } = ValidateNewOrderSingle_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.News.NoRoutingIDsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.News.NoRoutingIDsGroup> News_NoRoutingIDs { get; set; } = ValidateNews_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.News.NoRelatedSymGroup> News_NoRelatedSym { get; set; } = ValidateNews_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.News.LinesOfTextGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.News.LinesOfTextGroup> News_LinesOfText { get; set; } = ValidateNews_LinesOfText;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup> OrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup> OrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup> QuoteAcknowledgement_NoQuoteSets { get; set; } = ValidateQuoteAcknowledgement_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup> QuoteAcknowledgement_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteCancel.NoQuoteEntriesGroup> QuoteCancel_NoQuoteEntries { get; set; } = ValidateQuoteCancel_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.QuoteRequest.NoRelatedSymGroup> QuoteRequest_NoRelatedSym { get; set; } = ValidateQuoteRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityDefinition.NoRelatedSymGroup> SecurityDefinition_NoRelatedSym { get; set; } = ValidateSecurityDefinition_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup to the schema.</summary>
	public Action<Fix42Context, FixCheck, FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup> SecurityDefinitionRequest_NoRelatedSym { get; set; } = ValidateSecurityDefinitionRequest_NoRelatedSym;

	static void ValidateAdvertisement(Fix42Context context, FixCheck check, FixMessage.Advertisement message)
	{
		if (message.AdvId is null) Missing(check, FixTag.AdvId);
		else context.Validators.AdvId(context, check, message.AdvId);
		if (message.AdvTransType is null) Missing(check, FixTag.AdvTransType);
		else context.Validators.AdvTransType(context, check, message.AdvTransType);
		if (message.AdvRefID is not null) context.Validators.AdvRefID(context, check, message.AdvRefID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.AdvSide is null) Missing(check, FixTag.AdvSide);
		else context.Validators.AdvSide(context, check, message.AdvSide);
		if (message.Shares is null) Missing(check, FixTag.Quantity);
		else context.Validators.Shares(context, check, message.Shares);
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
	}

	static void ValidateAllocation(Fix42Context context, FixCheck check, FixMessage.Allocation message)
	{
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (message.AllocTransType is null) Missing(check, FixTag.AllocTransType);
		else context.Validators.AllocTransType(context, check, message.AllocTransType);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, check, message.RefAllocID);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, check, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, check, message.AllocLinkType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.Allocation_NoOrders);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, check, message.NoExecs);
		Counted(check, message.NoExecs, message.NoExecsGroups);
		Each(context, check, FixTag.NoExecs, message.NoExecsGroups, context.Validators.Allocation_NoExecs);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Shares is null) Missing(check, FixTag.Quantity);
		else context.Validators.Shares(context, check, message.Shares);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.AvgPx is null) Missing(check, FixTag.AvgPx);
		else context.Validators.AvgPx(context, check, message.AvgPx);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.AvgPrxPrecision is not null) context.Validators.AvgPrxPrecision(context, check, message.AvgPrxPrecision);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, check, message.FutSettDate);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, check, message.NetMoney);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, check, message.OpenClose);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, check, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, check, message.AccruedInterestRate);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.Allocation_NoAllocs);
	}

	static void ValidateAllocationACK(Fix42Context context, FixCheck check, FixMessage.AllocationACK message)
	{
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.AllocID is null) Missing(check, FixTag.AllocID);
		else context.Validators.AllocID(context, check, message.AllocID);
		if (message.TradeDate is null) Missing(check, FixTag.TradeDate);
		else context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.AllocStatus is null) Missing(check, FixTag.AllocStatus);
		else context.Validators.AllocStatus(context, check, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, check, message.AllocRejCode);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateBidRequest(Fix42Context context, FixCheck check, FixMessage.BidRequest message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is null) Missing(check, FixTag.ClientBidID);
		else context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.BidRequestTransType is null) Missing(check, FixTag.BidRequestTransType);
		else context.Validators.BidRequestTransType(context, check, message.BidRequestTransType);
		if (message.ListName is not null) context.Validators.ListName(context, check, message.ListName);
		if (message.TotalNumSecurities is null) Missing(check, FixTag.TotNoRelatedSym);
		else context.Validators.TotalNumSecurities(context, check, message.TotalNumSecurities);
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
		if (message.TradeType is null) Missing(check, FixTag.BidTradeType);
		else context.Validators.TradeType(context, check, message.TradeType);
		if (message.BasisPxType is null) Missing(check, FixTag.BasisPxType);
		else context.Validators.BasisPxType(context, check, message.BasisPxType);
		if (message.StrikeTime is not null) context.Validators.StrikeTime(context, check, message.StrikeTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateBidResponse(Fix42Context context, FixCheck check, FixMessage.BidResponse message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.NoBidComponents is null) Missing(check, FixTag.NoBidComponents);
		else context.Validators.NoBidComponents(context, check, message.NoBidComponents);
		Counted(check, message.NoBidComponents, message.NoBidComponentsGroups);
		Each(context, check, FixTag.NoBidComponents, message.NoBidComponentsGroups, context.Validators.BidResponse_NoBidComponents);
	}

	static void ValidateBusinessMessageReject(Fix42Context context, FixCheck check, FixMessage.BusinessMessageReject message)
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

	static void ValidateDontKnowTrade(Fix42Context context, FixCheck check, FixMessage.DontKnowTrade message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.ExecID is null) Missing(check, FixTag.ExecID);
		else context.Validators.ExecID(context, check, message.ExecID);
		if (message.DKReason is null) Missing(check, FixTag.DKReason);
		else context.Validators.DKReason(context, check, message.DKReason);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, check, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, message.CashOrderQty);
		if (message.LastShares is not null) context.Validators.LastShares(context, check, message.LastShares);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateEmail(Fix42Context context, FixCheck check, FixMessage.Email message)
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
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.LinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.LinesOfText(context, check, message.LinesOfText);
		Counted(check, message.LinesOfText, message.LinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.LinesOfTextGroups, context.Validators.Email_LinesOfText);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateExecutionReport(Fix42Context context, FixCheck check, FixMessage.ExecutionReport message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.NoContraBrokers is not null) context.Validators.NoContraBrokers(context, check, message.NoContraBrokers);
		Counted(check, message.NoContraBrokers, message.NoContraBrokersGroups);
		Each(context, check, FixTag.NoContraBrokers, message.NoContraBrokersGroups, context.Validators.ExecutionReport_NoContraBrokers);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.ExecID is null) Missing(check, FixTag.ExecID);
		else context.Validators.ExecID(context, check, message.ExecID);
		if (message.ExecTransType is null) Missing(check, FixTag.ExecTransType);
		else context.Validators.ExecTransType(context, check, message.ExecTransType);
		if (message.ExecRefID is not null) context.Validators.ExecRefID(context, check, message.ExecRefID);
		if (message.ExecType is null) Missing(check, FixTag.ExecType);
		else context.Validators.ExecType(context, check, message.ExecType);
		if (message.OrdStatus is null) Missing(check, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, check, message.OrdStatus);
		if (message.OrdRejReason is not null) context.Validators.OrdRejReason(context, check, message.OrdRejReason);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, check, message.ExecRestatementReason);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, check, message.FutSettDate);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, check, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, message.CashOrderQty);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, check, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, check, message.DiscretionOffset);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, check, message.Rule80A);
		if (message.LastShares is not null) context.Validators.LastShares(context, check, message.LastShares);
		if (message.LastPx is not null) context.Validators.LastPx(context, check, message.LastPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, check, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, check, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
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
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, check, message.GrossTradeAmt);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, message.SettlCurrFxRateCalc);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, check, message.OpenClose);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, check, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, check, message.ClearingAccount);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, check, message.MultiLegReportingType);
	}

	static void ValidateHeartbeat(Fix42Context context, FixCheck check, FixMessage.Heartbeat message)
	{
		if (message.TestReqID is not null) context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateIndicationofInterest(Fix42Context context, FixCheck check, FixMessage.IndicationofInterest message)
	{
		if (message.IOIid is null) Missing(check, FixTag.IOIID);
		else context.Validators.IOIid(context, check, message.IOIid);
		if (message.IOITransType is null) Missing(check, FixTag.IOITransType);
		else context.Validators.IOITransType(context, check, message.IOITransType);
		if (message.IOIRefID is not null) context.Validators.IOIRefID(context, check, message.IOIRefID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.IOIShares is null) Missing(check, FixTag.IOIQty);
		else context.Validators.IOIShares(context, check, message.IOIShares);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.IOIQltyInd is not null) context.Validators.IOIQltyInd(context, check, message.IOIQltyInd);
		if (message.IOINaturalFlag is not null) context.Validators.IOINaturalFlag(context, check, message.IOINaturalFlag);
		if (message.NoIOIQualifiers is not null) context.Validators.NoIOIQualifiers(context, check, message.NoIOIQualifiers);
		Counted(check, message.NoIOIQualifiers, message.NoIOIQualifiersGroups);
		Each(context, check, FixTag.NoIOIQualifiers, message.NoIOIQualifiersGroups, context.Validators.IndicationofInterest_NoIOIQualifiers);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, check, message.NoRoutingIDs);
		Counted(check, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		Each(context, check, FixTag.NoRoutingIDs, message.NoRoutingIDsGroups, context.Validators.IndicationofInterest_NoRoutingIDs);
		if (message.SpreadToBenchmark is not null) context.Validators.SpreadToBenchmark(context, check, message.SpreadToBenchmark);
		if (message.Benchmark is not null) context.Validators.Benchmark(context, check, message.Benchmark);
	}

	static void ValidateListCancelRequest(Fix42Context context, FixCheck check, FixMessage.ListCancelRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListExecute(Fix42Context context, FixCheck check, FixMessage.ListExecute message)
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

	static void ValidateListStatus(Fix42Context context, FixCheck check, FixMessage.ListStatus message)
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
		if (message.NoOrders is null) Missing(check, FixTag.NoOrders);
		else context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.ListStatus_NoOrders);
	}

	static void ValidateListStatusRequest(Fix42Context context, FixCheck check, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateListStrikePrice(Fix42Context context, FixCheck check, FixMessage.ListStrikePrice message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.TotNoStrikes is null) Missing(check, FixTag.TotNoStrikes);
		else context.Validators.TotNoStrikes(context, check, message.TotNoStrikes);
		if (message.NoStrikes is null) Missing(check, FixTag.NoStrikes);
		else context.Validators.NoStrikes(context, check, message.NoStrikes);
		Counted(check, message.NoStrikes, message.NoStrikesGroups);
		Each(context, check, FixTag.NoStrikes, message.NoStrikesGroups, context.Validators.ListStrikePrice_NoStrikes);
	}

	static void ValidateLogon(Fix42Context context, FixCheck check, FixMessage.Logon message)
	{
		if (message.EncryptMethod is null) Missing(check, FixTag.EncryptMethod);
		else context.Validators.EncryptMethod(context, check, message.EncryptMethod);
		if (message.HeartBtInt is null) Missing(check, FixTag.HeartBtInt);
		else context.Validators.HeartBtInt(context, check, message.HeartBtInt);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
		if (message.ResetSeqNumFlag is not null) context.Validators.ResetSeqNumFlag(context, check, message.ResetSeqNumFlag);
		if (message.MaxMessageSize is not null) context.Validators.MaxMessageSize(context, check, message.MaxMessageSize);
		if (message.NoMsgTypes is not null) context.Validators.NoMsgTypes(context, check, message.NoMsgTypes);
		Counted(check, message.NoMsgTypes, message.NoMsgTypesGroups);
		Each(context, check, FixTag.NoMsgTypes, message.NoMsgTypesGroups, context.Validators.Logon_NoMsgTypes);
	}

	static void ValidateLogout(Fix42Context context, FixCheck check, FixMessage.Logout message)
	{
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataIncrementalRefresh(Fix42Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.NoMDEntries is null) Missing(check, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, check, message.NoMDEntries);
		Counted(check, message.NoMDEntries, message.NoMDEntriesGroups);
		Each(context, check, FixTag.NoMDEntries, message.NoMDEntriesGroups, context.Validators.MarketDataIncrementalRefresh_NoMDEntries);
	}

	static void ValidateMarketDataRequest(Fix42Context context, FixCheck check, FixMessage.MarketDataRequest message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.MarketDepth is null) Missing(check, FixTag.MarketDepth);
		else context.Validators.MarketDepth(context, check, message.MarketDepth);
		if (message.MDUpdateType is not null) context.Validators.MDUpdateType(context, check, message.MDUpdateType);
		if (message.AggregatedBook is not null) context.Validators.AggregatedBook(context, check, message.AggregatedBook);
		if (message.NoMDEntryTypes is null) Missing(check, FixTag.NoMDEntryTypes);
		else context.Validators.NoMDEntryTypes(context, check, message.NoMDEntryTypes);
		Counted(check, message.NoMDEntryTypes, message.NoMDEntryTypesGroups);
		Each(context, check, FixTag.NoMDEntryTypes, message.NoMDEntryTypesGroups, context.Validators.MarketDataRequest_NoMDEntryTypes);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.MarketDataRequest_NoRelatedSym);
	}

	static void ValidateMarketDataRequestReject(Fix42Context context, FixCheck check, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(check, FixTag.MDReqID);
		else context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, check, message.MDReqRejReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateMarketDataSnapshotFullRefresh(Fix42Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, check, message.MDReqID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, check, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, check, message.CorporateAction);
		if (message.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, check, message.TotalVolumeTraded);
		if (message.NoMDEntries is null) Missing(check, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, check, message.NoMDEntries);
		Counted(check, message.NoMDEntries, message.NoMDEntriesGroups);
		Each(context, check, FixTag.NoMDEntries, message.NoMDEntriesGroups, context.Validators.MarketDataSnapshotFullRefresh_NoMDEntries);
	}

	static void ValidateMassQuote(Fix42Context context, FixCheck check, FixMessage.MassQuote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.DefBidSize is not null) context.Validators.DefBidSize(context, check, message.DefBidSize);
		if (message.DefOfferSize is not null) context.Validators.DefOfferSize(context, check, message.DefOfferSize);
		if (message.NoQuoteSets is null) Missing(check, FixTag.NoQuoteSets);
		else context.Validators.NoQuoteSets(context, check, message.NoQuoteSets);
		Counted(check, message.NoQuoteSets, message.NoQuoteSetsGroups);
		Each(context, check, FixTag.NoQuoteSets, message.NoQuoteSetsGroups, context.Validators.MassQuote_NoQuoteSets);
	}

	static void ValidateNewOrderList(Fix42Context context, FixCheck check, FixMessage.NewOrderList message)
	{
		if (message.ListID is null) Missing(check, FixTag.ListID);
		else context.Validators.ListID(context, check, message.ListID);
		if (message.BidID is not null) context.Validators.BidID(context, check, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, check, message.ClientBidID);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, check, message.ProgRptReqs);
		if (message.BidType is null) Missing(check, FixTag.BidType);
		else context.Validators.BidType(context, check, message.BidType);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, check, message.ProgPeriodInterval);
		if (message.ListExecInstType is not null) context.Validators.ListExecInstType(context, check, message.ListExecInstType);
		if (message.ListExecInst is not null) context.Validators.ListExecInst(context, check, message.ListExecInst);
		if (message.EncodedListExecInstLen is not null) context.Validators.EncodedListExecInstLen(context, check, message.EncodedListExecInstLen);
		if (message.EncodedListExecInst is not null) context.Validators.EncodedListExecInst(context, check, message.EncodedListExecInst);
		if (message.TotNoOrders is null) Missing(check, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, check, message.TotNoOrders);
		if (message.NoOrders is null) Missing(check, FixTag.NoOrders);
		else context.Validators.NoOrders(context, check, message.NoOrders);
		Counted(check, message.NoOrders, message.NoOrdersGroups);
		Each(context, check, FixTag.NoOrders, message.NoOrdersGroups, context.Validators.NewOrderList_NoOrders);
	}

	static void ValidateNewOrderSingle(Fix42Context context, FixCheck check, FixMessage.NewOrderSingle message)
	{
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.NewOrderSingle_NoAllocs);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, check, message.FutSettDate);
		if (message.HandlInst is null) Missing(check, FixTag.HandlInst);
		else context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.NewOrderSingle_NoTradingSessions);
		if (message.ProcessCode is not null) context.Validators.ProcessCode(context, check, message.ProcessCode);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, message.PrevClosePx);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, check, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, message.CashOrderQty);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
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
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, check, message.Rule80A);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, check, message.OpenClose);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, check, message.CustomerOrFirm);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, check, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, check, message.DiscretionOffset);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, check, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, check, message.ClearingAccount);
	}

	static void ValidateNews(Fix42Context context, FixCheck check, FixMessage.News message)
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
		if (message.LinesOfText is null) Missing(check, FixTag.NoLinesOfText);
		else context.Validators.LinesOfText(context, check, message.LinesOfText);
		Counted(check, message.LinesOfText, message.LinesOfTextGroups);
		Each(context, check, FixTag.NoLinesOfText, message.LinesOfTextGroups, context.Validators.News_LinesOfText);
		if (message.URLLink is not null) context.Validators.URLLink(context, check, message.URLLink);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, check, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, check, message.RawData);
	}

	static void ValidateOrderCancelReject(Fix42Context context, FixCheck check, FixMessage.OrderCancelReject message)
	{
		if (message.OrderID is null) Missing(check, FixTag.OrderID);
		else context.Validators.OrderID(context, check, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, message.SecondaryOrderID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.OrdStatus is null) Missing(check, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, check, message.OrdStatus);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.CxlRejResponseTo is null) Missing(check, FixTag.CxlRejResponseTo);
		else context.Validators.CxlRejResponseTo(context, check, message.CxlRejResponseTo);
		if (message.CxlRejReason is not null) context.Validators.CxlRejReason(context, check, message.CxlRejReason);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderCancelReplaceRequest(Fix42Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, check, message.NoAllocs);
		Counted(check, message.NoAllocs, message.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, message.NoAllocsGroups, context.Validators.OrderCancelReplaceRequest_NoAllocs);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, check, message.FutSettDate);
		if (message.HandlInst is null) Missing(check, FixTag.HandlInst);
		else context.Validators.HandlInst(context, check, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, check, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, check, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, check, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, check, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, message.NoTradingSessions);
		Counted(check, message.NoTradingSessions, message.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, message.NoTradingSessionsGroups, context.Validators.OrderCancelReplaceRequest_NoTradingSessions);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, check, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, message.CashOrderQty);
		if (message.OrdType is null) Missing(check, FixTag.OrdType);
		else context.Validators.OrdType(context, check, message.OrdType);
		if (message.Price is not null) context.Validators.Price(context, check, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, check, message.StopPx);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, check, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, check, message.DiscretionOffset);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, check, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, check, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, check, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, check, message.GTBookingInst);
		if (message.Commission is not null) context.Validators.Commission(context, check, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, check, message.CommType);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, check, message.Rule80A);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, check, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, message.SettlCurrency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, check, message.OpenClose);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, message.CoveredOrUncovered);
		if (message.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, check, message.CustomerOrFirm);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, check, message.MaxShow);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, check, message.LocateReqd);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, check, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, check, message.ClearingAccount);
	}

	static void ValidateOrderCancelRequest(Fix42Context context, FixCheck check, FixMessage.OrderCancelRequest message)
	{
		if (message.OrigClOrdID is null) Missing(check, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, check, message.OrigClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ListID is not null) context.Validators.ListID(context, check, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, check, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, message.CashOrderQty);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, check, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, check, message.SolicitedFlag);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
	}

	static void ValidateOrderStatusRequest(Fix42Context context, FixCheck check, FixMessage.OrderStatusRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, check, message.OrderID);
		if (message.ClOrdID is null) Missing(check, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, check, message.ClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.Account is not null) context.Validators.Account(context, check, message.Account);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(check, FixTag.Side);
		else context.Validators.Side(context, check, message.Side);
	}

	static void ValidateQuote(Fix42Context context, FixCheck check, FixMessage.Quote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.BidPx is not null) context.Validators.BidPx(context, check, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, check, message.OfferPx);
		if (message.BidSize is not null) context.Validators.BidSize(context, check, message.BidSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, check, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, message.OfferForwardPoints);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, check, message.FutSettDate);
		if (message.OrdType is not null) context.Validators.OrdType(context, check, message.OrdType);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, check, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
	}

	static void ValidateQuoteAcknowledgement(Fix42Context context, FixCheck check, FixMessage.QuoteAcknowledgement message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteAckStatus is null) Missing(check, FixTag.QuoteStatus);
		else context.Validators.QuoteAckStatus(context, check, message.QuoteAckStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, check, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.NoQuoteSets is not null) context.Validators.NoQuoteSets(context, check, message.NoQuoteSets);
		Counted(check, message.NoQuoteSets, message.NoQuoteSetsGroups);
		Each(context, check, FixTag.NoQuoteSets, message.NoQuoteSetsGroups, context.Validators.QuoteAcknowledgement_NoQuoteSets);
	}

	static void ValidateQuoteCancel(Fix42Context context, FixCheck check, FixMessage.QuoteCancel message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.QuoteID is null) Missing(check, FixTag.QuoteID);
		else context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.QuoteCancelType is null) Missing(check, FixTag.QuoteCancelType);
		else context.Validators.QuoteCancelType(context, check, message.QuoteCancelType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, check, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.NoQuoteEntries is null) Missing(check, FixTag.NoQuoteEntries);
		else context.Validators.NoQuoteEntries(context, check, message.NoQuoteEntries);
		Counted(check, message.NoQuoteEntries, message.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, message.NoQuoteEntriesGroups, context.Validators.QuoteCancel_NoQuoteEntries);
	}

	static void ValidateQuoteRequest(Fix42Context context, FixCheck check, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID is null) Missing(check, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, check, message.QuoteReqID);
		if (message.NoRelatedSym is null) Missing(check, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.QuoteRequest_NoRelatedSym);
	}

	static void ValidateQuoteStatusRequest(Fix42Context context, FixCheck check, FixMessage.QuoteStatusRequest message)
	{
		if (message.QuoteID is not null) context.Validators.QuoteID(context, check, message.QuoteID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
	}

	static void ValidateReject(Fix42Context context, FixCheck check, FixMessage.Reject message)
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

	static void ValidateResendRequest(Fix42Context context, FixCheck check, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(check, FixTag.BeginSeqNo);
		else context.Validators.BeginSeqNo(context, check, message.BeginSeqNo);
		if (message.EndSeqNo is null) Missing(check, FixTag.EndSeqNo);
		else context.Validators.EndSeqNo(context, check, message.EndSeqNo);
	}

	static void ValidateSecurityDefinition(Fix42Context context, FixCheck check, FixMessage.SecurityDefinition message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(check, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, check, message.SecurityResponseID);
		if (message.SecurityResponseType is not null) context.Validators.SecurityResponseType(context, check, message.SecurityResponseType);
		if (message.TotalNumSecurities is null) Missing(check, FixTag.TotNoRelatedSym);
		else context.Validators.TotalNumSecurities(context, check, message.TotalNumSecurities);
		if (message.Symbol is not null) context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.SecurityDefinition_NoRelatedSym);
	}

	static void ValidateSecurityDefinitionRequest(Fix42Context context, FixCheck check, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID is null) Missing(check, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, check, message.SecurityReqID);
		if (message.SecurityRequestType is null) Missing(check, FixTag.SecurityRequestType);
		else context.Validators.SecurityRequestType(context, check, message.SecurityRequestType);
		if (message.Symbol is not null) context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, check, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, check, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, check, message.NoRelatedSym);
		Counted(check, message.NoRelatedSym, message.NoRelatedSymGroups);
		Each(context, check, FixTag.NoRelatedSym, message.NoRelatedSymGroups, context.Validators.SecurityDefinitionRequest_NoRelatedSym);
	}

	static void ValidateSecurityStatus(Fix42Context context, FixCheck check, FixMessage.SecurityStatus message)
	{
		if (message.SecurityStatusReqID is not null) context.Validators.SecurityStatusReqID(context, check, message.SecurityStatusReqID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
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
	}

	static void ValidateSecurityStatusRequest(Fix42Context context, FixCheck check, FixMessage.SecurityStatusRequest message)
	{
		if (message.SecurityStatusReqID is null) Missing(check, FixTag.SecurityStatusReqID);
		else context.Validators.SecurityStatusReqID(context, check, message.SecurityStatusReqID);
		if (message.Symbol is null) Missing(check, FixTag.Symbol);
		else context.Validators.Symbol(context, check, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, check, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, check, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, check, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, check, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, check, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, check, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, check, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, check, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, check, message.Currency);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
	}

	static void ValidateSequenceReset(Fix42Context context, FixCheck check, FixMessage.SequenceReset message)
	{
		if (message.GapFillFlag is not null) context.Validators.GapFillFlag(context, check, message.GapFillFlag);
		if (message.NewSeqNo is null) Missing(check, FixTag.NewSeqNo);
		else context.Validators.NewSeqNo(context, check, message.NewSeqNo);
	}

	static void ValidateSettlementInstructions(Fix42Context context, FixCheck check, FixMessage.SettlementInstructions message)
	{
		if (message.SettlInstID is null) Missing(check, FixTag.SettlInstID);
		else context.Validators.SettlInstID(context, check, message.SettlInstID);
		if (message.SettlInstTransType is null) Missing(check, FixTag.SettlInstTransType);
		else context.Validators.SettlInstTransType(context, check, message.SettlInstTransType);
		if (message.SettlInstRefID is null) Missing(check, FixTag.SettlInstRefID);
		else context.Validators.SettlInstRefID(context, check, message.SettlInstRefID);
		if (message.SettlInstMode is null) Missing(check, FixTag.SettlInstMode);
		else context.Validators.SettlInstMode(context, check, message.SettlInstMode);
		if (message.SettlInstSource is null) Missing(check, FixTag.SettlInstSource);
		else context.Validators.SettlInstSource(context, check, message.SettlInstSource);
		if (message.AllocAccount is null) Missing(check, FixTag.AllocAccount);
		else context.Validators.AllocAccount(context, check, message.AllocAccount);
		if (message.SettlLocation is not null) context.Validators.SettlLocation(context, check, message.SettlLocation);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, check, message.TradeDate);
		if (message.AllocID is not null) context.Validators.AllocID(context, check, message.AllocID);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, check, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.Side is not null) context.Validators.Side(context, check, message.Side);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, check, message.SecurityType);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, check, message.EffectiveTime);
		if (message.TransactTime is null) Missing(check, FixTag.TransactTime);
		else context.Validators.TransactTime(context, check, message.TransactTime);
		if (message.ClientID is not null) context.Validators.ClientID(context, check, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, check, message.ExecBroker);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, check, message.StandInstDbType);
		if (message.StandInstDbName is not null) context.Validators.StandInstDbName(context, check, message.StandInstDbName);
		if (message.StandInstDbID is not null) context.Validators.StandInstDbID(context, check, message.StandInstDbID);
		if (message.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, check, message.SettlDeliveryType);
		if (message.SettlDepositoryCode is not null) context.Validators.SettlDepositoryCode(context, check, message.SettlDepositoryCode);
		if (message.SettlBrkrCode is not null) context.Validators.SettlBrkrCode(context, check, message.SettlBrkrCode);
		if (message.SettlInstCode is not null) context.Validators.SettlInstCode(context, check, message.SettlInstCode);
		if (message.SecuritySettlAgentName is not null) context.Validators.SecuritySettlAgentName(context, check, message.SecuritySettlAgentName);
		if (message.SecuritySettlAgentCode is not null) context.Validators.SecuritySettlAgentCode(context, check, message.SecuritySettlAgentCode);
		if (message.SecuritySettlAgentAcctNum is not null) context.Validators.SecuritySettlAgentAcctNum(context, check, message.SecuritySettlAgentAcctNum);
		if (message.SecuritySettlAgentAcctName is not null) context.Validators.SecuritySettlAgentAcctName(context, check, message.SecuritySettlAgentAcctName);
		if (message.SecuritySettlAgentContactName is not null) context.Validators.SecuritySettlAgentContactName(context, check, message.SecuritySettlAgentContactName);
		if (message.SecuritySettlAgentContactPhone is not null) context.Validators.SecuritySettlAgentContactPhone(context, check, message.SecuritySettlAgentContactPhone);
		if (message.CashSettlAgentName is not null) context.Validators.CashSettlAgentName(context, check, message.CashSettlAgentName);
		if (message.CashSettlAgentCode is not null) context.Validators.CashSettlAgentCode(context, check, message.CashSettlAgentCode);
		if (message.CashSettlAgentAcctNum is not null) context.Validators.CashSettlAgentAcctNum(context, check, message.CashSettlAgentAcctNum);
		if (message.CashSettlAgentAcctName is not null) context.Validators.CashSettlAgentAcctName(context, check, message.CashSettlAgentAcctName);
		if (message.CashSettlAgentContactName is not null) context.Validators.CashSettlAgentContactName(context, check, message.CashSettlAgentContactName);
		if (message.CashSettlAgentContactPhone is not null) context.Validators.CashSettlAgentContactPhone(context, check, message.CashSettlAgentContactPhone);
	}

	static void ValidateTestRequest(Fix42Context context, FixCheck check, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(check, FixTag.TestReqID);
		else context.Validators.TestReqID(context, check, message.TestReqID);
	}

	static void ValidateTradingSessionStatus(Fix42Context context, FixCheck check, FixMessage.TradingSessionStatus message)
	{
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.TradingSessionID is null) Missing(check, FixTag.TradingSessionID);
		else context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, check, message.UnsolicitedIndicator);
		if (message.TradSesStatus is null) Missing(check, FixTag.TradSesStatus);
		else context.Validators.TradSesStatus(context, check, message.TradSesStatus);
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

	static void ValidateTradingSessionStatusRequest(Fix42Context context, FixCheck check, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.TradSesReqID is null) Missing(check, FixTag.TradSesReqID);
		else context.Validators.TradSesReqID(context, check, message.TradSesReqID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, message.TradingSessionID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, check, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, check, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(check, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, check, message.SubscriptionRequestType);
	}

	static void ValidateAllocation_NoOrders(Fix42Context context, FixCheck check, FixMessage.Allocation.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, check, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, check, entry.SecondaryOrderID);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (entry.WaveNo is not null) context.Validators.WaveNo(context, check, entry.WaveNo);
	}

	static void ValidateAllocation_NoExecs(Fix42Context context, FixCheck check, FixMessage.Allocation.NoExecsGroup entry)
	{
		context.Validators.LastShares(context, check, entry.LastShares);
		if (entry.ExecID is not null) context.Validators.ExecID(context, check, entry.ExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, check, entry.LastPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, check, entry.LastCapacity);
	}

	static void ValidateAllocation_NoAllocs(Fix42Context context, FixCheck check, FixMessage.Allocation.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, check, entry.AllocPrice);
		if (entry.AllocShares is null) Missing(check, FixTag.AllocQty, entry.AllocAccount.Position);
		else context.Validators.AllocShares(context, check, entry.AllocShares);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.BrokerOfCredit is not null) context.Validators.BrokerOfCredit(context, check, entry.BrokerOfCredit);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, check, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, check, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, check, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, check, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, check, entry.EncodedAllocText);
		if (entry.ExecBroker is not null) context.Validators.ExecBroker(context, check, entry.ExecBroker);
		if (entry.ClientID is not null) context.Validators.ClientID(context, check, entry.ClientID);
		if (entry.Commission is not null) context.Validators.Commission(context, check, entry.Commission);
		if (entry.CommType is not null) context.Validators.CommType(context, check, entry.CommType);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, check, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, check, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, check, entry.SettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, check, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, check, entry.SettlCurrFxRateCalc);
		if (entry.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, check, entry.AccruedInterestAmt);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, check, entry.SettlInstMode);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, check, entry.NoMiscFees);
		Counted(check, entry.NoMiscFees, entry.NoMiscFeesGroups);
		Each(context, check, FixTag.NoMiscFees, entry.NoMiscFeesGroups, context.Validators.Allocation_NoAllocs_NoMiscFees);
	}

	static void ValidateAllocation_NoAllocs_NoMiscFees(Fix42Context context, FixCheck check, FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup entry)
	{
		context.Validators.MiscFeeAmt(context, check, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, check, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, check, entry.MiscFeeType);
	}

	static void ValidateBidRequest_NoBidDescriptors(Fix42Context context, FixCheck check, FixMessage.BidRequest.NoBidDescriptorsGroup entry)
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

	static void ValidateBidRequest_NoBidComponents(Fix42Context context, FixCheck check, FixMessage.BidRequest.NoBidComponentsGroup entry)
	{
		context.Validators.ListID(context, check, entry.ListID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, check, entry.FutSettDate);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
	}

	static void ValidateBidResponse_NoBidComponents(Fix42Context context, FixCheck check, FixMessage.BidResponse.NoBidComponentsGroup entry)
	{
		context.Validators.Commission(context, check, entry.Commission);
		if (entry.CommType is null) Missing(check, FixTag.CommType, entry.Commission.Position);
		else context.Validators.CommType(context, check, entry.CommType);
		if (entry.ListID is not null) context.Validators.ListID(context, check, entry.ListID);
		if (entry.Country is not null) context.Validators.Country(context, check, entry.Country);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.PriceType is not null) context.Validators.PriceType(context, check, entry.PriceType);
		if (entry.FairValue is not null) context.Validators.FairValue(context, check, entry.FairValue);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, check, entry.NetGrossInd);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, check, entry.FutSettDate);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateEmail_NoRoutingIDs(Fix42Context context, FixCheck check, FixMessage.Email.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateEmail_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.Email.NoRelatedSymGroup entry)
	{
		context.Validators.RelatdSym(context, check, entry.RelatdSym);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
	}

	static void ValidateEmail_LinesOfText(Fix42Context context, FixCheck check, FixMessage.Email.LinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateExecutionReport_NoContraBrokers(Fix42Context context, FixCheck check, FixMessage.ExecutionReport.NoContraBrokersGroup entry)
	{
		context.Validators.ContraBroker(context, check, entry.ContraBroker);
		if (entry.ContraTrader is not null) context.Validators.ContraTrader(context, check, entry.ContraTrader);
		if (entry.ContraTradeQty is not null) context.Validators.ContraTradeQty(context, check, entry.ContraTradeQty);
		if (entry.ContraTradeTime is not null) context.Validators.ContraTradeTime(context, check, entry.ContraTradeTime);
	}

	static void ValidateIndicationofInterest_NoIOIQualifiers(Fix42Context context, FixCheck check, FixMessage.IndicationofInterest.NoIOIQualifiersGroup entry)
	{
		context.Validators.IOIQualifier(context, check, entry.IOIQualifier);
	}

	static void ValidateIndicationofInterest_NoRoutingIDs(Fix42Context context, FixCheck check, FixMessage.IndicationofInterest.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateListStatus_NoOrders(Fix42Context context, FixCheck check, FixMessage.ListStatus.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.CumQty is null) Missing(check, FixTag.CumQty, entry.ClOrdID.Position);
		else context.Validators.CumQty(context, check, entry.CumQty);
		if (entry.OrdStatus is null) Missing(check, FixTag.OrdStatus, entry.ClOrdID.Position);
		else context.Validators.OrdStatus(context, check, entry.OrdStatus);
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

	static void ValidateListStrikePrice_NoStrikes(Fix42Context context, FixCheck check, FixMessage.ListStrikePrice.NoStrikesGroup entry)
	{
		context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.Price is null) Missing(check, FixTag.Price, entry.Symbol.Position);
		else context.Validators.Price(context, check, entry.Price);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateLogon_NoMsgTypes(Fix42Context context, FixCheck check, FixMessage.Logon.NoMsgTypesGroup entry)
	{
		context.Validators.RefMsgType(context, check, entry.RefMsgType);
		if (entry.MsgDirection is not null) context.Validators.MsgDirection(context, check, entry.MsgDirection);
	}

	static void ValidateMarketDataIncrementalRefresh_NoMDEntries(Fix42Context context, FixCheck check, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDUpdateAction(context, check, entry.MDUpdateAction);
		if (entry.DeleteReason is not null) context.Validators.DeleteReason(context, check, entry.DeleteReason);
		if (entry.MDEntryType is not null) context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, check, entry.MDEntryID);
		if (entry.MDEntryRefID is not null) context.Validators.MDEntryRefID(context, check, entry.MDEntryRefID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
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
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, check, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, check, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, check, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.OpenCloseSettleFlag is not null) context.Validators.OpenCloseSettleFlag(context, check, entry.OpenCloseSettleFlag);
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
		if (entry.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, check, entry.TotalVolumeTraded);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateMarketDataRequest_NoMDEntryTypes(Fix42Context context, FixCheck check, FixMessage.MarketDataRequest.NoMDEntryTypesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
	}

	static void ValidateMarketDataRequest_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.MarketDataRequest.NoRelatedSymGroup entry)
	{
		context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
	}

	static void ValidateMarketDataSnapshotFullRefresh_NoMDEntries(Fix42Context context, FixCheck check, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup entry)
	{
		context.Validators.MDEntryType(context, check, entry.MDEntryType);
		if (entry.MDEntryPx is null) Missing(check, FixTag.MDEntryPx, entry.MDEntryType.Position);
		else context.Validators.MDEntryPx(context, check, entry.MDEntryPx);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, check, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, check, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, check, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, check, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, check, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, check, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, check, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, check, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, check, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, check, entry.DeskID);
		if (entry.OpenCloseSettleFlag is not null) context.Validators.OpenCloseSettleFlag(context, check, entry.OpenCloseSettleFlag);
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
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateMassQuote_NoQuoteSets(Fix42Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup entry)
	{
		context.Validators.QuoteSetID(context, check, entry.QuoteSetID);
		if (entry.UnderlyingSymbol is null) Missing(check, FixTag.UnderlyingSymbol, entry.QuoteSetID.Position);
		else context.Validators.UnderlyingSymbol(context, check, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, check, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, check, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, check, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, check, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, check, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, check, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, check, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, check, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, check, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, check, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, check, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, check, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, check, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, check, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, check, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, check, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, check, entry.EncodedUnderlyingSecurityDesc);
		if (entry.QuoteSetValidUntilTime is not null) context.Validators.QuoteSetValidUntilTime(context, check, entry.QuoteSetValidUntilTime);
		if (entry.TotQuoteEntries is null) Missing(check, FixTag.TotNoQuoteEntries, entry.QuoteSetID.Position);
		else context.Validators.TotQuoteEntries(context, check, entry.TotQuoteEntries);
		if (entry.NoQuoteEntries is null) Missing(check, FixTag.NoQuoteEntries, entry.QuoteSetID.Position);
		else context.Validators.NoQuoteEntries(context, check, entry.NoQuoteEntries);
		Counted(check, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, entry.NoQuoteEntriesGroups, context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries);
	}

	static void ValidateMassQuote_NoQuoteSets_NoQuoteEntries(Fix42Context context, FixCheck check, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
	{
		context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.BidPx is not null) context.Validators.BidPx(context, check, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, check, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, check, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, check, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, check, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, check, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, check, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, check, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, check, entry.OfferForwardPoints);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, check, entry.FutSettDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
	}

	static void ValidateNewOrderList_NoOrders(Fix42Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup entry)
	{
		context.Validators.ClOrdID(context, check, entry.ClOrdID);
		if (entry.ListSeqNo is null) Missing(check, FixTag.ListSeqNo, entry.ClOrdID.Position);
		else context.Validators.ListSeqNo(context, check, entry.ListSeqNo);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, check, entry.SettlInstMode);
		if (entry.ClientID is not null) context.Validators.ClientID(context, check, entry.ClientID);
		if (entry.ExecBroker is not null) context.Validators.ExecBroker(context, check, entry.ExecBroker);
		if (entry.Account is not null) context.Validators.Account(context, check, entry.Account);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, check, entry.NoAllocs);
		Counted(check, entry.NoAllocs, entry.NoAllocsGroups);
		Each(context, check, FixTag.NoAllocs, entry.NoAllocsGroups, context.Validators.NewOrderList_NoOrders_NoAllocs);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, check, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, check, entry.FutSettDate);
		if (entry.HandlInst is not null) context.Validators.HandlInst(context, check, entry.HandlInst);
		if (entry.ExecInst is not null) context.Validators.ExecInst(context, check, entry.ExecInst);
		if (entry.MinQty is not null) context.Validators.MinQty(context, check, entry.MinQty);
		if (entry.MaxFloor is not null) context.Validators.MaxFloor(context, check, entry.MaxFloor);
		if (entry.ExDestination is not null) context.Validators.ExDestination(context, check, entry.ExDestination);
		if (entry.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, check, entry.NoTradingSessions);
		Counted(check, entry.NoTradingSessions, entry.NoTradingSessionsGroups);
		Each(context, check, FixTag.NoTradingSessions, entry.NoTradingSessionsGroups, context.Validators.NewOrderList_NoOrders_NoTradingSessions);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, check, entry.ProcessCode);
		if (entry.Symbol is null) Missing(check, FixTag.Symbol, entry.ClOrdID.Position);
		else context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.Side is null) Missing(check, FixTag.Side, entry.ClOrdID.Position);
		else context.Validators.Side(context, check, entry.Side);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, check, entry.SideValueInd);
		if (entry.LocateReqd is not null) context.Validators.LocateReqd(context, check, entry.LocateReqd);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.CashOrderQty is not null) context.Validators.CashOrderQty(context, check, entry.CashOrderQty);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.Price is not null) context.Validators.Price(context, check, entry.Price);
		if (entry.StopPx is not null) context.Validators.StopPx(context, check, entry.StopPx);
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
		if (entry.Commission is not null) context.Validators.Commission(context, check, entry.Commission);
		if (entry.CommType is not null) context.Validators.CommType(context, check, entry.CommType);
		if (entry.Rule80A is not null) context.Validators.Rule80A(context, check, entry.Rule80A);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, check, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, check, entry.SettlCurrency);
		if (entry.Text is not null) context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.OpenClose is not null) context.Validators.OpenClose(context, check, entry.OpenClose);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, check, entry.CoveredOrUncovered);
		if (entry.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, check, entry.CustomerOrFirm);
		if (entry.MaxShow is not null) context.Validators.MaxShow(context, check, entry.MaxShow);
		if (entry.PegDifference is not null) context.Validators.PegDifference(context, check, entry.PegDifference);
		if (entry.DiscretionInst is not null) context.Validators.DiscretionInst(context, check, entry.DiscretionInst);
		if (entry.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, check, entry.DiscretionOffset);
		if (entry.ClearingFirm is not null) context.Validators.ClearingFirm(context, check, entry.ClearingFirm);
		if (entry.ClearingAccount is not null) context.Validators.ClearingAccount(context, check, entry.ClearingAccount);
	}

	static void ValidateNewOrderList_NoOrders_NoAllocs(Fix42Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, check, entry.AllocShares);
	}

	static void ValidateNewOrderList_NoOrders_NoTradingSessions(Fix42Context context, FixCheck check, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
	}

	static void ValidateNewOrderSingle_NoAllocs(Fix42Context context, FixCheck check, FixMessage.NewOrderSingle.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, check, entry.AllocShares);
	}

	static void ValidateNewOrderSingle_NoTradingSessions(Fix42Context context, FixCheck check, FixMessage.NewOrderSingle.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
	}

	static void ValidateNews_NoRoutingIDs(Fix42Context context, FixCheck check, FixMessage.News.NoRoutingIDsGroup entry)
	{
		context.Validators.RoutingType(context, check, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, check, entry.RoutingID);
	}

	static void ValidateNews_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.News.NoRelatedSymGroup entry)
	{
		context.Validators.RelatdSym(context, check, entry.RelatdSym);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
	}

	static void ValidateNews_LinesOfText(Fix42Context context, FixCheck check, FixMessage.News.LinesOfTextGroup entry)
	{
		context.Validators.Text(context, check, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, check, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, check, entry.EncodedText);
	}

	static void ValidateOrderCancelReplaceRequest_NoAllocs(Fix42Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup entry)
	{
		context.Validators.AllocAccount(context, check, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, check, entry.AllocShares);
	}

	static void ValidateOrderCancelReplaceRequest_NoTradingSessions(Fix42Context context, FixCheck check, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup entry)
	{
		context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
	}

	static void ValidateQuoteAcknowledgement_NoQuoteSets(Fix42Context context, FixCheck check, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup entry)
	{
		context.Validators.QuoteSetID(context, check, entry.QuoteSetID);
		if (entry.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, check, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, check, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, check, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, check, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, check, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, check, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, check, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, check, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, check, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, check, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, check, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, check, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, check, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, check, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, check, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, check, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, check, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, check, entry.EncodedUnderlyingSecurityDesc);
		if (entry.TotQuoteEntries is not null) context.Validators.TotQuoteEntries(context, check, entry.TotQuoteEntries);
		if (entry.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, check, entry.NoQuoteEntries);
		Counted(check, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		Each(context, check, FixTag.NoQuoteEntries, entry.NoQuoteEntriesGroups, context.Validators.QuoteAcknowledgement_NoQuoteSets_NoQuoteEntries);
	}

	static void ValidateQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(Fix42Context context, FixCheck check, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup entry)
	{
		context.Validators.QuoteEntryID(context, check, entry.QuoteEntryID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.QuoteEntryRejectReason is not null) context.Validators.QuoteEntryRejectReason(context, check, entry.QuoteEntryRejectReason);
	}

	static void ValidateQuoteCancel_NoQuoteEntries(Fix42Context context, FixCheck check, FixMessage.QuoteCancel.NoQuoteEntriesGroup entry)
	{
		context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, check, entry.UnderlyingSymbol);
	}

	static void ValidateQuoteRequest_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.QuoteRequest.NoRelatedSymGroup entry)
	{
		context.Validators.Symbol(context, check, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, check, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, check, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, check, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, check, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, check, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, check, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, check, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, check, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, check, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, check, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, check, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, check, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, check, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, check, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, check, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, check, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, check, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, check, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, check, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, check, entry.QuoteRequestType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, check, entry.TradingSessionID);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, check, entry.OrderQty);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, check, entry.FutSettDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, check, entry.OrdType);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, check, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, check, entry.OrderQty2);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, check, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, check, entry.TransactTime);
		if (entry.Currency is not null) context.Validators.Currency(context, check, entry.Currency);
	}

	static void ValidateSecurityDefinition_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.SecurityDefinition.NoRelatedSymGroup entry)
	{
		context.Validators.UnderlyingSymbol(context, check, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, check, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, check, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, check, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, check, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, check, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, check, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, check, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, check, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, check, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, check, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, check, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, check, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, check, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, check, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, check, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, check, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, check, entry.EncodedUnderlyingSecurityDesc);
		if (entry.RatioQty is not null) context.Validators.RatioQty(context, check, entry.RatioQty);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, check, entry.UnderlyingCurrency);
	}

	static void ValidateSecurityDefinitionRequest_NoRelatedSym(Fix42Context context, FixCheck check, FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup entry)
	{
		context.Validators.UnderlyingSymbol(context, check, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, check, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, check, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, check, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, check, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, check, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, check, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, check, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, check, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, check, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, check, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, check, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, check, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, check, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, check, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, check, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, check, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, check, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, check, entry.EncodedUnderlyingSecurityDesc);
		if (entry.RatioQty is not null) context.Validators.RatioQty(context, check, entry.RatioQty);
		if (entry.Side is not null) context.Validators.Side(context, check, entry.Side);
		if (entry.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, check, entry.UnderlyingCurrency);
	}

}
