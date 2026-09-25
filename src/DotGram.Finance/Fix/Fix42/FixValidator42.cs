using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand.
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
	public Func<Fix42Context, FixMessage.Advertisement, bool> Advertisement { get; set; } = ValidateAdvertisement;

	/// <summary>Holds a FIX 4.2 Allocation to the schema.</summary>
	public Func<Fix42Context, FixMessage.Allocation, bool> Allocation { get; set; } = ValidateAllocation;

	/// <summary>Holds a FIX 4.2 AllocationACK to the schema.</summary>
	public Func<Fix42Context, FixMessage.AllocationACK, bool> AllocationACK { get; set; } = ValidateAllocationACK;

	/// <summary>Holds a FIX 4.2 BidRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.BidRequest, bool> BidRequest { get; set; } = ValidateBidRequest;

	/// <summary>Holds a FIX 4.2 BidResponse to the schema.</summary>
	public Func<Fix42Context, FixMessage.BidResponse, bool> BidResponse { get; set; } = ValidateBidResponse;

	/// <summary>Holds a FIX 4.2 BusinessMessageReject to the schema.</summary>
	public Func<Fix42Context, FixMessage.BusinessMessageReject, bool> BusinessMessageReject { get; set; } = ValidateBusinessMessageReject;

	/// <summary>Holds a FIX 4.2 DontKnowTrade to the schema.</summary>
	public Func<Fix42Context, FixMessage.DontKnowTrade, bool> DontKnowTrade { get; set; } = ValidateDontKnowTrade;

	/// <summary>Holds a FIX 4.2 Email to the schema.</summary>
	public Func<Fix42Context, FixMessage.Email, bool> Email { get; set; } = ValidateEmail;

	/// <summary>Holds a FIX 4.2 ExecutionReport to the schema.</summary>
	public Func<Fix42Context, FixMessage.ExecutionReport, bool> ExecutionReport { get; set; } = ValidateExecutionReport;

	/// <summary>Holds a FIX 4.2 Heartbeat to the schema.</summary>
	public Func<Fix42Context, FixMessage.Heartbeat, bool> Heartbeat { get; set; } = ValidateHeartbeat;

	/// <summary>Holds a FIX 4.2 IndicationofInterest to the schema.</summary>
	public Func<Fix42Context, FixMessage.IndicationofInterest, bool> IndicationofInterest { get; set; } = ValidateIndicationofInterest;

	/// <summary>Holds a FIX 4.2 ListCancelRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.ListCancelRequest, bool> ListCancelRequest { get; set; } = ValidateListCancelRequest;

	/// <summary>Holds a FIX 4.2 ListExecute to the schema.</summary>
	public Func<Fix42Context, FixMessage.ListExecute, bool> ListExecute { get; set; } = ValidateListExecute;

	/// <summary>Holds a FIX 4.2 ListStatus to the schema.</summary>
	public Func<Fix42Context, FixMessage.ListStatus, bool> ListStatus { get; set; } = ValidateListStatus;

	/// <summary>Holds a FIX 4.2 ListStatusRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.ListStatusRequest, bool> ListStatusRequest { get; set; } = ValidateListStatusRequest;

	/// <summary>Holds a FIX 4.2 ListStrikePrice to the schema.</summary>
	public Func<Fix42Context, FixMessage.ListStrikePrice, bool> ListStrikePrice { get; set; } = ValidateListStrikePrice;

	/// <summary>Holds a FIX 4.2 Logon to the schema.</summary>
	public Func<Fix42Context, FixMessage.Logon, bool> Logon { get; set; } = ValidateLogon;

	/// <summary>Holds a FIX 4.2 Logout to the schema.</summary>
	public Func<Fix42Context, FixMessage.Logout, bool> Logout { get; set; } = ValidateLogout;

	/// <summary>Holds a FIX 4.2 MarketDataIncrementalRefresh to the schema.</summary>
	public Func<Fix42Context, FixMessage.MarketDataIncrementalRefresh, bool> MarketDataIncrementalRefresh { get; set; } = ValidateMarketDataIncrementalRefresh;

	/// <summary>Holds a FIX 4.2 MarketDataRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.MarketDataRequest, bool> MarketDataRequest { get; set; } = ValidateMarketDataRequest;

	/// <summary>Holds a FIX 4.2 MarketDataRequestReject to the schema.</summary>
	public Func<Fix42Context, FixMessage.MarketDataRequestReject, bool> MarketDataRequestReject { get; set; } = ValidateMarketDataRequestReject;

	/// <summary>Holds a FIX 4.2 MarketDataSnapshotFullRefresh to the schema.</summary>
	public Func<Fix42Context, FixMessage.MarketDataSnapshotFullRefresh, bool> MarketDataSnapshotFullRefresh { get; set; } = ValidateMarketDataSnapshotFullRefresh;

	/// <summary>Holds a FIX 4.2 MassQuote to the schema.</summary>
	public Func<Fix42Context, FixMessage.MassQuote, bool> MassQuote { get; set; } = ValidateMassQuote;

	/// <summary>Holds a FIX 4.2 NewOrderList to the schema.</summary>
	public Func<Fix42Context, FixMessage.NewOrderList, bool> NewOrderList { get; set; } = ValidateNewOrderList;

	/// <summary>Holds a FIX 4.2 NewOrderSingle to the schema.</summary>
	public Func<Fix42Context, FixMessage.NewOrderSingle, bool> NewOrderSingle { get; set; } = ValidateNewOrderSingle;

	/// <summary>Holds a FIX 4.2 News to the schema.</summary>
	public Func<Fix42Context, FixMessage.News, bool> News { get; set; } = ValidateNews;

	/// <summary>Holds a FIX 4.2 OrderCancelReject to the schema.</summary>
	public Func<Fix42Context, FixMessage.OrderCancelReject, bool> OrderCancelReject { get; set; } = ValidateOrderCancelReject;

	/// <summary>Holds a FIX 4.2 OrderCancelReplaceRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.OrderCancelReplaceRequest, bool> OrderCancelReplaceRequest { get; set; } = ValidateOrderCancelReplaceRequest;

	/// <summary>Holds a FIX 4.2 OrderCancelRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.OrderCancelRequest, bool> OrderCancelRequest { get; set; } = ValidateOrderCancelRequest;

	/// <summary>Holds a FIX 4.2 OrderStatusRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.OrderStatusRequest, bool> OrderStatusRequest { get; set; } = ValidateOrderStatusRequest;

	/// <summary>Holds a FIX 4.2 Quote to the schema.</summary>
	public Func<Fix42Context, FixMessage.Quote, bool> Quote { get; set; } = ValidateQuote;

	/// <summary>Holds a FIX 4.2 QuoteAcknowledgement to the schema.</summary>
	public Func<Fix42Context, FixMessage.QuoteAcknowledgement, bool> QuoteAcknowledgement { get; set; } = ValidateQuoteAcknowledgement;

	/// <summary>Holds a FIX 4.2 QuoteCancel to the schema.</summary>
	public Func<Fix42Context, FixMessage.QuoteCancel, bool> QuoteCancel { get; set; } = ValidateQuoteCancel;

	/// <summary>Holds a FIX 4.2 QuoteRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.QuoteRequest, bool> QuoteRequest { get; set; } = ValidateQuoteRequest;

	/// <summary>Holds a FIX 4.2 QuoteStatusRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.QuoteStatusRequest, bool> QuoteStatusRequest { get; set; } = ValidateQuoteStatusRequest;

	/// <summary>Holds a FIX 4.2 Reject to the schema.</summary>
	public Func<Fix42Context, FixMessage.Reject, bool> Reject { get; set; } = ValidateReject;

	/// <summary>Holds a FIX 4.2 ResendRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.ResendRequest, bool> ResendRequest { get; set; } = ValidateResendRequest;

	/// <summary>Holds a FIX 4.2 SecurityDefinition to the schema.</summary>
	public Func<Fix42Context, FixMessage.SecurityDefinition, bool> SecurityDefinition { get; set; } = ValidateSecurityDefinition;

	/// <summary>Holds a FIX 4.2 SecurityDefinitionRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.SecurityDefinitionRequest, bool> SecurityDefinitionRequest { get; set; } = ValidateSecurityDefinitionRequest;

	/// <summary>Holds a FIX 4.2 SecurityStatus to the schema.</summary>
	public Func<Fix42Context, FixMessage.SecurityStatus, bool> SecurityStatus { get; set; } = ValidateSecurityStatus;

	/// <summary>Holds a FIX 4.2 SecurityStatusRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.SecurityStatusRequest, bool> SecurityStatusRequest { get; set; } = ValidateSecurityStatusRequest;

	/// <summary>Holds a FIX 4.2 SequenceReset to the schema.</summary>
	public Func<Fix42Context, FixMessage.SequenceReset, bool> SequenceReset { get; set; } = ValidateSequenceReset;

	/// <summary>Holds a FIX 4.2 SettlementInstructions to the schema.</summary>
	public Func<Fix42Context, FixMessage.SettlementInstructions, bool> SettlementInstructions { get; set; } = ValidateSettlementInstructions;

	/// <summary>Holds a FIX 4.2 TestRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.TestRequest, bool> TestRequest { get; set; } = ValidateTestRequest;

	/// <summary>Holds a FIX 4.2 TradingSessionStatus to the schema.</summary>
	public Func<Fix42Context, FixMessage.TradingSessionStatus, bool> TradingSessionStatus { get; set; } = ValidateTradingSessionStatus;

	/// <summary>Holds a FIX 4.2 TradingSessionStatusRequest to the schema.</summary>
	public Func<Fix42Context, FixMessage.TradingSessionStatusRequest, bool> TradingSessionStatusRequest { get; set; } = ValidateTradingSessionStatusRequest;

	/// <summary>Holds one entry of FixMessage.Allocation.NoOrdersGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Allocation.NoOrdersGroup, int, bool> Allocation_NoOrders { get; set; } = ValidateAllocation_NoOrders;

	/// <summary>Holds one entry of FixMessage.Allocation.NoExecsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Allocation.NoExecsGroup, int, bool> Allocation_NoExecs { get; set; } = ValidateAllocation_NoExecs;

	/// <summary>Holds one entry of FixMessage.Allocation.NoAllocsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Allocation.NoAllocsGroup, int, bool> Allocation_NoAllocs { get; set; } = ValidateAllocation_NoAllocs;

	/// <summary>Holds one entry of FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup, int, bool> Allocation_NoAllocs_NoMiscFees { get; set; } = ValidateAllocation_NoAllocs_NoMiscFees;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidDescriptorsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.BidRequest.NoBidDescriptorsGroup, int, bool> BidRequest_NoBidDescriptors { get; set; } = ValidateBidRequest_NoBidDescriptors;

	/// <summary>Holds one entry of FixMessage.BidRequest.NoBidComponentsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.BidRequest.NoBidComponentsGroup, int, bool> BidRequest_NoBidComponents { get; set; } = ValidateBidRequest_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.BidResponse.NoBidComponentsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.BidResponse.NoBidComponentsGroup, int, bool> BidResponse_NoBidComponents { get; set; } = ValidateBidResponse_NoBidComponents;

	/// <summary>Holds one entry of FixMessage.Email.NoRoutingIDsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Email.NoRoutingIDsGroup, int, bool> Email_NoRoutingIDs { get; set; } = ValidateEmail_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.Email.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Email.NoRelatedSymGroup, int, bool> Email_NoRelatedSym { get; set; } = ValidateEmail_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.Email.LinesOfTextGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Email.LinesOfTextGroup, int, bool> Email_LinesOfText { get; set; } = ValidateEmail_LinesOfText;

	/// <summary>Holds one entry of FixMessage.ExecutionReport.NoContraBrokersGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.ExecutionReport.NoContraBrokersGroup, int, bool> ExecutionReport_NoContraBrokers { get; set; } = ValidateExecutionReport_NoContraBrokers;

	/// <summary>Holds one entry of FixMessage.IndicationofInterest.NoIOIQualifiersGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.IndicationofInterest.NoIOIQualifiersGroup, int, bool> IndicationofInterest_NoIOIQualifiers { get; set; } = ValidateIndicationofInterest_NoIOIQualifiers;

	/// <summary>Holds one entry of FixMessage.IndicationofInterest.NoRoutingIDsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.IndicationofInterest.NoRoutingIDsGroup, int, bool> IndicationofInterest_NoRoutingIDs { get; set; } = ValidateIndicationofInterest_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.ListStatus.NoOrdersGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.ListStatus.NoOrdersGroup, int, bool> ListStatus_NoOrders { get; set; } = ValidateListStatus_NoOrders;

	/// <summary>Holds one entry of FixMessage.ListStrikePrice.NoStrikesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.ListStrikePrice.NoStrikesGroup, int, bool> ListStrikePrice_NoStrikes { get; set; } = ValidateListStrikePrice_NoStrikes;

	/// <summary>Holds one entry of FixMessage.Logon.NoMsgTypesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.Logon.NoMsgTypesGroup, int, bool> Logon_NoMsgTypes { get; set; } = ValidateLogon_NoMsgTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup, int, bool> MarketDataIncrementalRefresh_NoMDEntries { get; set; } = ValidateMarketDataIncrementalRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoMDEntryTypesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MarketDataRequest.NoMDEntryTypesGroup, int, bool> MarketDataRequest_NoMDEntryTypes { get; set; } = ValidateMarketDataRequest_NoMDEntryTypes;

	/// <summary>Holds one entry of FixMessage.MarketDataRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MarketDataRequest.NoRelatedSymGroup, int, bool> MarketDataRequest_NoRelatedSym { get; set; } = ValidateMarketDataRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup, int, bool> MarketDataSnapshotFullRefresh_NoMDEntries { get; set; } = ValidateMarketDataSnapshotFullRefresh_NoMDEntries;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MassQuote.NoQuoteSetsGroup, int, bool> MassQuote_NoQuoteSets { get; set; } = ValidateMassQuote_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup, int, bool> MassQuote_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateMassQuote_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.NewOrderList.NoOrdersGroup, int, bool> NewOrderList_NoOrders { get; set; } = ValidateNewOrderList_NoOrders;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup, int, bool> NewOrderList_NoOrders_NoAllocs { get; set; } = ValidateNewOrderList_NoOrders_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup, int, bool> NewOrderList_NoOrders_NoTradingSessions { get; set; } = ValidateNewOrderList_NoOrders_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoAllocsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.NewOrderSingle.NoAllocsGroup, int, bool> NewOrderSingle_NoAllocs { get; set; } = ValidateNewOrderSingle_NoAllocs;

	/// <summary>Holds one entry of FixMessage.NewOrderSingle.NoTradingSessionsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.NewOrderSingle.NoTradingSessionsGroup, int, bool> NewOrderSingle_NoTradingSessions { get; set; } = ValidateNewOrderSingle_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.News.NoRoutingIDsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.News.NoRoutingIDsGroup, int, bool> News_NoRoutingIDs { get; set; } = ValidateNews_NoRoutingIDs;

	/// <summary>Holds one entry of FixMessage.News.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.News.NoRelatedSymGroup, int, bool> News_NoRelatedSym { get; set; } = ValidateNews_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.News.LinesOfTextGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.News.LinesOfTextGroup, int, bool> News_LinesOfText { get; set; } = ValidateNews_LinesOfText;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoAllocsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup, int, bool> OrderCancelReplaceRequest_NoAllocs { get; set; } = ValidateOrderCancelReplaceRequest_NoAllocs;

	/// <summary>Holds one entry of FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup, int, bool> OrderCancelReplaceRequest_NoTradingSessions { get; set; } = ValidateOrderCancelReplaceRequest_NoTradingSessions;

	/// <summary>Holds one entry of FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup, int, bool> QuoteAcknowledgement_NoQuoteSets { get; set; } = ValidateQuoteAcknowledgement_NoQuoteSets;

	/// <summary>Holds one entry of FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup, int, bool> QuoteAcknowledgement_NoQuoteSets_NoQuoteEntries { get; set; } = ValidateQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteCancel.NoQuoteEntriesGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.QuoteCancel.NoQuoteEntriesGroup, int, bool> QuoteCancel_NoQuoteEntries { get; set; } = ValidateQuoteCancel_NoQuoteEntries;

	/// <summary>Holds one entry of FixMessage.QuoteRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.QuoteRequest.NoRelatedSymGroup, int, bool> QuoteRequest_NoRelatedSym { get; set; } = ValidateQuoteRequest_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityDefinition.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.SecurityDefinition.NoRelatedSymGroup, int, bool> SecurityDefinition_NoRelatedSym { get; set; } = ValidateSecurityDefinition_NoRelatedSym;

	/// <summary>Holds one entry of FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup to the schema.</summary>
	public Func<Fix42Context, FixMessage, FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup, int, bool> SecurityDefinitionRequest_NoRelatedSym { get; set; } = ValidateSecurityDefinitionRequest_NoRelatedSym;

	static bool ValidateAdvertisement(Fix42Context context, FixMessage.Advertisement message)
	{
		if (message.AdvId is null) Missing(message, FixTag.AdvId);
		else context.Validators.AdvId(context, message, message.AdvId);
		if (message.AdvTransType is null) Missing(message, FixTag.AdvTransType);
		else context.Validators.AdvTransType(context, message, message.AdvTransType);
		if (message.AdvRefID is not null) context.Validators.AdvRefID(context, message, message.AdvRefID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.AdvSide is null) Missing(message, FixTag.AdvSide);
		else context.Validators.AdvSide(context, message, message.AdvSide);
		if (message.Shares is null) Missing(message, FixTag.Quantity);
		else context.Validators.Shares(context, message, message.Shares);
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

		return message.IsValid;
	}

	static bool ValidateAllocation(Fix42Context context, FixMessage.Allocation message)
	{
		if (message.AllocID is null) Missing(message, FixTag.AllocID);
		else context.Validators.AllocID(context, message, message.AllocID);
		if (message.AllocTransType is null) Missing(message, FixTag.AllocTransType);
		else context.Validators.AllocTransType(context, message, message.AllocTransType);
		if (message.RefAllocID is not null) context.Validators.RefAllocID(context, message, message.RefAllocID);
		if (message.AllocLinkID is not null) context.Validators.AllocLinkID(context, message, message.AllocLinkID);
		if (message.AllocLinkType is not null) context.Validators.AllocLinkType(context, message, message.AllocLinkType);
		if (message.NoOrders is not null) context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.Allocation_NoOrders(context, message, message.NoOrdersGroups[i], i);
		if (message.NoExecs is not null) context.Validators.NoExecs(context, message, message.NoExecs);
		Counted(message, message.NoExecs, message.NoExecsGroups);
		if (message.NoExecsGroups is not null)
			for (var i = 0; i < message.NoExecsGroups.Count; i++)
				context.Validators.Allocation_NoExecs(context, message, message.NoExecsGroups[i], i);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Shares is null) Missing(message, FixTag.Quantity);
		else context.Validators.Shares(context, message, message.Shares);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.AvgPx is null) Missing(message, FixTag.AvgPx);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.AvgPrxPrecision is not null) context.Validators.AvgPrxPrecision(context, message, message.AvgPrxPrecision);
		if (message.TradeDate is null) Missing(message, FixTag.TradeDate);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, message, message.FutSettDate);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.NetMoney is not null) context.Validators.NetMoney(context, message, message.NetMoney);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, message, message.OpenClose);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NumDaysInterest is not null) context.Validators.NumDaysInterest(context, message, message.NumDaysInterest);
		if (message.AccruedInterestRate is not null) context.Validators.AccruedInterestRate(context, message, message.AccruedInterestRate);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.Allocation_NoAllocs(context, message, message.NoAllocsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAllocationACK(Fix42Context context, FixMessage.AllocationACK message)
	{
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.AllocID is null) Missing(message, FixTag.AllocID);
		else context.Validators.AllocID(context, message, message.AllocID);
		if (message.TradeDate is null) Missing(message, FixTag.TradeDate);
		else context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.AllocStatus is null) Missing(message, FixTag.AllocStatus);
		else context.Validators.AllocStatus(context, message, message.AllocStatus);
		if (message.AllocRejCode is not null) context.Validators.AllocRejCode(context, message, message.AllocRejCode);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateBidRequest(Fix42Context context, FixMessage.BidRequest message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is null) Missing(message, FixTag.ClientBidID);
		else context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.BidRequestTransType is null) Missing(message, FixTag.BidRequestTransType);
		else context.Validators.BidRequestTransType(context, message, message.BidRequestTransType);
		if (message.ListName is not null) context.Validators.ListName(context, message, message.ListName);
		if (message.TotalNumSecurities is null) Missing(message, FixTag.TotNoRelatedSym);
		else context.Validators.TotalNumSecurities(context, message, message.TotalNumSecurities);
		if (message.BidType is null) Missing(message, FixTag.BidType);
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
		if (message.TradeType is null) Missing(message, FixTag.BidTradeType);
		else context.Validators.TradeType(context, message, message.TradeType);
		if (message.BasisPxType is null) Missing(message, FixTag.BasisPxType);
		else context.Validators.BasisPxType(context, message, message.BasisPxType);
		if (message.StrikeTime is not null) context.Validators.StrikeTime(context, message, message.StrikeTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateBidResponse(Fix42Context context, FixMessage.BidResponse message)
	{
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.NoBidComponents is null) Missing(message, FixTag.NoBidComponents);
		else context.Validators.NoBidComponents(context, message, message.NoBidComponents);
		Counted(message, message.NoBidComponents, message.NoBidComponentsGroups);
		if (message.NoBidComponentsGroups is not null)
			for (var i = 0; i < message.NoBidComponentsGroups.Count; i++)
				context.Validators.BidResponse_NoBidComponents(context, message, message.NoBidComponentsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateBusinessMessageReject(Fix42Context context, FixMessage.BusinessMessageReject message)
	{
		if (message.RefSeqNum is not null) context.Validators.RefSeqNum(context, message, message.RefSeqNum);
		if (message.RefMsgType is null) Missing(message, FixTag.RefMsgType);
		else context.Validators.RefMsgType(context, message, message.RefMsgType);
		if (message.BusinessRejectRefID is not null) context.Validators.BusinessRejectRefID(context, message, message.BusinessRejectRefID);
		if (message.BusinessRejectReason is null) Missing(message, FixTag.BusinessRejectReason);
		else context.Validators.BusinessRejectReason(context, message, message.BusinessRejectReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateDontKnowTrade(Fix42Context context, FixMessage.DontKnowTrade message)
	{
		if (message.OrderID is null) Missing(message, FixTag.OrderID);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.ExecID is null) Missing(message, FixTag.ExecID);
		else context.Validators.ExecID(context, message, message.ExecID);
		if (message.DKReason is null) Missing(message, FixTag.DKReason);
		else context.Validators.DKReason(context, message, message.DKReason);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, message, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, message.CashOrderQty);
		if (message.LastShares is not null) context.Validators.LastShares(context, message, message.LastShares);
		if (message.LastPx is not null) context.Validators.LastPx(context, message, message.LastPx);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateEmail(Fix42Context context, FixMessage.Email message)
	{
		if (message.EmailThreadID is null) Missing(message, FixTag.EmailThreadID);
		else context.Validators.EmailThreadID(context, message, message.EmailThreadID);
		if (message.EmailType is null) Missing(message, FixTag.EmailType);
		else context.Validators.EmailType(context, message, message.EmailType);
		if (message.OrigTime is not null) context.Validators.OrigTime(context, message, message.OrigTime);
		if (message.Subject is null) Missing(message, FixTag.Subject);
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
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.LinesOfText is null) Missing(message, FixTag.NoLinesOfText);
		else context.Validators.LinesOfText(context, message, message.LinesOfText);
		Counted(message, message.LinesOfText, message.LinesOfTextGroups);
		if (message.LinesOfTextGroups is not null)
			for (var i = 0; i < message.LinesOfTextGroups.Count; i++)
				context.Validators.Email_LinesOfText(context, message, message.LinesOfTextGroups[i], i);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);

		return message.IsValid;
	}

	static bool ValidateExecutionReport(Fix42Context context, FixMessage.ExecutionReport message)
	{
		if (message.OrderID is null) Missing(message, FixTag.OrderID);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.ClOrdID is not null) context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrigClOrdID is not null) context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.NoContraBrokers is not null) context.Validators.NoContraBrokers(context, message, message.NoContraBrokers);
		Counted(message, message.NoContraBrokers, message.NoContraBrokersGroups);
		if (message.NoContraBrokersGroups is not null)
			for (var i = 0; i < message.NoContraBrokersGroups.Count; i++)
				context.Validators.ExecutionReport_NoContraBrokers(context, message, message.NoContraBrokersGroups[i], i);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.ExecID is null) Missing(message, FixTag.ExecID);
		else context.Validators.ExecID(context, message, message.ExecID);
		if (message.ExecTransType is null) Missing(message, FixTag.ExecTransType);
		else context.Validators.ExecTransType(context, message, message.ExecTransType);
		if (message.ExecRefID is not null) context.Validators.ExecRefID(context, message, message.ExecRefID);
		if (message.ExecType is null) Missing(message, FixTag.ExecType);
		else context.Validators.ExecType(context, message, message.ExecType);
		if (message.OrdStatus is null) Missing(message, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.OrdRejReason is not null) context.Validators.OrdRejReason(context, message, message.OrdRejReason);
		if (message.ExecRestatementReason is not null) context.Validators.ExecRestatementReason(context, message, message.ExecRestatementReason);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, message, message.FutSettDate);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, message, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, message.CashOrderQty);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, message, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, message, message.DiscretionOffset);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, message, message.Rule80A);
		if (message.LastShares is not null) context.Validators.LastShares(context, message, message.LastShares);
		if (message.LastPx is not null) context.Validators.LastPx(context, message, message.LastPx);
		if (message.LastSpotRate is not null) context.Validators.LastSpotRate(context, message, message.LastSpotRate);
		if (message.LastForwardPoints is not null) context.Validators.LastForwardPoints(context, message, message.LastForwardPoints);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.LastCapacity is not null) context.Validators.LastCapacity(context, message, message.LastCapacity);
		if (message.LeavesQty is null) Missing(message, FixTag.LeavesQty);
		else context.Validators.LeavesQty(context, message, message.LeavesQty);
		if (message.CumQty is null) Missing(message, FixTag.CumQty);
		else context.Validators.CumQty(context, message, message.CumQty);
		if (message.AvgPx is null) Missing(message, FixTag.AvgPx);
		else context.Validators.AvgPx(context, message, message.AvgPx);
		if (message.DayOrderQty is not null) context.Validators.DayOrderQty(context, message, message.DayOrderQty);
		if (message.DayCumQty is not null) context.Validators.DayCumQty(context, message, message.DayCumQty);
		if (message.DayAvgPx is not null) context.Validators.DayAvgPx(context, message, message.DayAvgPx);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ReportToExch is not null) context.Validators.ReportToExch(context, message, message.ReportToExch);
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.GrossTradeAmt is not null) context.Validators.GrossTradeAmt(context, message, message.GrossTradeAmt);
		if (message.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, message.SettlCurrAmt);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, message.SettlCurrFxRate);
		if (message.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, message.SettlCurrFxRateCalc);
		if (message.HandlInst is not null) context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, message, message.OpenClose);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, message, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, message, message.ClearingAccount);
		if (message.MultiLegReportingType is not null) context.Validators.MultiLegReportingType(context, message, message.MultiLegReportingType);

		return message.IsValid;
	}

	static bool ValidateHeartbeat(Fix42Context context, FixMessage.Heartbeat message)
	{
		if (message.TestReqID is not null) context.Validators.TestReqID(context, message, message.TestReqID);

		return message.IsValid;
	}

	static bool ValidateIndicationofInterest(Fix42Context context, FixMessage.IndicationofInterest message)
	{
		if (message.IOIid is null) Missing(message, FixTag.IOIID);
		else context.Validators.IOIid(context, message, message.IOIid);
		if (message.IOITransType is null) Missing(message, FixTag.IOITransType);
		else context.Validators.IOITransType(context, message, message.IOITransType);
		if (message.IOIRefID is not null) context.Validators.IOIRefID(context, message, message.IOIRefID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.IOIShares is null) Missing(message, FixTag.IOIQty);
		else context.Validators.IOIShares(context, message, message.IOIShares);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.IOIQltyInd is not null) context.Validators.IOIQltyInd(context, message, message.IOIQltyInd);
		if (message.IOINaturalFlag is not null) context.Validators.IOINaturalFlag(context, message, message.IOINaturalFlag);
		if (message.NoIOIQualifiers is not null) context.Validators.NoIOIQualifiers(context, message, message.NoIOIQualifiers);
		Counted(message, message.NoIOIQualifiers, message.NoIOIQualifiersGroups);
		if (message.NoIOIQualifiersGroups is not null)
			for (var i = 0; i < message.NoIOIQualifiersGroups.Count; i++)
				context.Validators.IndicationofInterest_NoIOIQualifiers(context, message, message.NoIOIQualifiersGroups[i], i);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.URLLink is not null) context.Validators.URLLink(context, message, message.URLLink);
		if (message.NoRoutingIDs is not null) context.Validators.NoRoutingIDs(context, message, message.NoRoutingIDs);
		Counted(message, message.NoRoutingIDs, message.NoRoutingIDsGroups);
		if (message.NoRoutingIDsGroups is not null)
			for (var i = 0; i < message.NoRoutingIDsGroups.Count; i++)
				context.Validators.IndicationofInterest_NoRoutingIDs(context, message, message.NoRoutingIDsGroups[i], i);
		if (message.SpreadToBenchmark is not null) context.Validators.SpreadToBenchmark(context, message, message.SpreadToBenchmark);
		if (message.Benchmark is not null) context.Validators.Benchmark(context, message, message.Benchmark);

		return message.IsValid;
	}

	static bool ValidateListCancelRequest(Fix42Context context, FixMessage.ListCancelRequest message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListExecute(Fix42Context context, FixMessage.ListExecute message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStatus(Fix42Context context, FixMessage.ListStatus message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.ListStatusType is null) Missing(message, FixTag.ListStatusType);
		else context.Validators.ListStatusType(context, message, message.ListStatusType);
		if (message.NoRpts is null) Missing(message, FixTag.NoRpts);
		else context.Validators.NoRpts(context, message, message.NoRpts);
		if (message.ListOrderStatus is null) Missing(message, FixTag.ListOrderStatus);
		else context.Validators.ListOrderStatus(context, message, message.ListOrderStatus);
		if (message.RptSeq is null) Missing(message, FixTag.RptSeq);
		else context.Validators.RptSeq(context, message, message.RptSeq);
		if (message.ListStatusText is not null) context.Validators.ListStatusText(context, message, message.ListStatusText);
		if (message.EncodedListStatusTextLen is not null) context.Validators.EncodedListStatusTextLen(context, message, message.EncodedListStatusTextLen);
		if (message.EncodedListStatusText is not null) context.Validators.EncodedListStatusText(context, message, message.EncodedListStatusText);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.TotNoOrders is null) Missing(message, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, message, message.TotNoOrders);
		if (message.NoOrders is null) Missing(message, FixTag.NoOrders);
		else context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.ListStatus_NoOrders(context, message, message.NoOrdersGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateListStatusRequest(Fix42Context context, FixMessage.ListStatusRequest message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice(Fix42Context context, FixMessage.ListStrikePrice message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.TotNoStrikes is null) Missing(message, FixTag.TotNoStrikes);
		else context.Validators.TotNoStrikes(context, message, message.TotNoStrikes);
		if (message.NoStrikes is null) Missing(message, FixTag.NoStrikes);
		else context.Validators.NoStrikes(context, message, message.NoStrikes);
		Counted(message, message.NoStrikes, message.NoStrikesGroups);
		if (message.NoStrikesGroups is not null)
			for (var i = 0; i < message.NoStrikesGroups.Count; i++)
				context.Validators.ListStrikePrice_NoStrikes(context, message, message.NoStrikesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateLogon(Fix42Context context, FixMessage.Logon message)
	{
		if (message.EncryptMethod is null) Missing(message, FixTag.EncryptMethod);
		else context.Validators.EncryptMethod(context, message, message.EncryptMethod);
		if (message.HeartBtInt is null) Missing(message, FixTag.HeartBtInt);
		else context.Validators.HeartBtInt(context, message, message.HeartBtInt);
		if (message.RawDataLength is not null) context.Validators.RawDataLength(context, message, message.RawDataLength);
		if (message.RawData is not null) context.Validators.RawData(context, message, message.RawData);
		if (message.ResetSeqNumFlag is not null) context.Validators.ResetSeqNumFlag(context, message, message.ResetSeqNumFlag);
		if (message.MaxMessageSize is not null) context.Validators.MaxMessageSize(context, message, message.MaxMessageSize);
		if (message.NoMsgTypes is not null) context.Validators.NoMsgTypes(context, message, message.NoMsgTypes);
		Counted(message, message.NoMsgTypes, message.NoMsgTypesGroups);
		if (message.NoMsgTypesGroups is not null)
			for (var i = 0; i < message.NoMsgTypesGroups.Count; i++)
				context.Validators.Logon_NoMsgTypes(context, message, message.NoMsgTypesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateLogout(Fix42Context context, FixMessage.Logout message)
	{
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh(Fix42Context context, FixMessage.MarketDataIncrementalRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.NoMDEntries is null) Missing(message, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, message, message.NoMDEntries);
		Counted(message, message.NoMDEntries, message.NoMDEntriesGroups);
		if (message.NoMDEntriesGroups is not null)
			for (var i = 0; i < message.NoMDEntriesGroups.Count; i++)
				context.Validators.MarketDataIncrementalRefresh_NoMDEntries(context, message, message.NoMDEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest(Fix42Context context, FixMessage.MarketDataRequest message)
	{
		if (message.MDReqID is null) Missing(message, FixTag.MDReqID);
		else context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.SubscriptionRequestType is null) Missing(message, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.MarketDepth is null) Missing(message, FixTag.MarketDepth);
		else context.Validators.MarketDepth(context, message, message.MarketDepth);
		if (message.MDUpdateType is not null) context.Validators.MDUpdateType(context, message, message.MDUpdateType);
		if (message.AggregatedBook is not null) context.Validators.AggregatedBook(context, message, message.AggregatedBook);
		if (message.NoMDEntryTypes is null) Missing(message, FixTag.NoMDEntryTypes);
		else context.Validators.NoMDEntryTypes(context, message, message.NoMDEntryTypes);
		Counted(message, message.NoMDEntryTypes, message.NoMDEntryTypesGroups);
		if (message.NoMDEntryTypesGroups is not null)
			for (var i = 0; i < message.NoMDEntryTypesGroups.Count; i++)
				context.Validators.MarketDataRequest_NoMDEntryTypes(context, message, message.NoMDEntryTypesGroups[i], i);
		if (message.NoRelatedSym is null) Missing(message, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.MarketDataRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequestReject(Fix42Context context, FixMessage.MarketDataRequestReject message)
	{
		if (message.MDReqID is null) Missing(message, FixTag.MDReqID);
		else context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.MDReqRejReason is not null) context.Validators.MDReqRejReason(context, message, message.MDReqRejReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh(Fix42Context context, FixMessage.MarketDataSnapshotFullRefresh message)
	{
		if (message.MDReqID is not null) context.Validators.MDReqID(context, message, message.MDReqID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.FinancialStatus is not null) context.Validators.FinancialStatus(context, message, message.FinancialStatus);
		if (message.CorporateAction is not null) context.Validators.CorporateAction(context, message, message.CorporateAction);
		if (message.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, message, message.TotalVolumeTraded);
		if (message.NoMDEntries is null) Missing(message, FixTag.NoMDEntries);
		else context.Validators.NoMDEntries(context, message, message.NoMDEntries);
		Counted(message, message.NoMDEntries, message.NoMDEntriesGroups);
		if (message.NoMDEntriesGroups is not null)
			for (var i = 0; i < message.NoMDEntriesGroups.Count; i++)
				context.Validators.MarketDataSnapshotFullRefresh_NoMDEntries(context, message, message.NoMDEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMassQuote(Fix42Context context, FixMessage.MassQuote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, FixTag.QuoteID);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.DefBidSize is not null) context.Validators.DefBidSize(context, message, message.DefBidSize);
		if (message.DefOfferSize is not null) context.Validators.DefOfferSize(context, message, message.DefOfferSize);
		if (message.NoQuoteSets is null) Missing(message, FixTag.NoQuoteSets);
		else context.Validators.NoQuoteSets(context, message, message.NoQuoteSets);
		Counted(message, message.NoQuoteSets, message.NoQuoteSetsGroups);
		if (message.NoQuoteSetsGroups is not null)
			for (var i = 0; i < message.NoQuoteSetsGroups.Count; i++)
				context.Validators.MassQuote_NoQuoteSets(context, message, message.NoQuoteSetsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNewOrderList(Fix42Context context, FixMessage.NewOrderList message)
	{
		if (message.ListID is null) Missing(message, FixTag.ListID);
		else context.Validators.ListID(context, message, message.ListID);
		if (message.BidID is not null) context.Validators.BidID(context, message, message.BidID);
		if (message.ClientBidID is not null) context.Validators.ClientBidID(context, message, message.ClientBidID);
		if (message.ProgRptReqs is not null) context.Validators.ProgRptReqs(context, message, message.ProgRptReqs);
		if (message.BidType is null) Missing(message, FixTag.BidType);
		else context.Validators.BidType(context, message, message.BidType);
		if (message.ProgPeriodInterval is not null) context.Validators.ProgPeriodInterval(context, message, message.ProgPeriodInterval);
		if (message.ListExecInstType is not null) context.Validators.ListExecInstType(context, message, message.ListExecInstType);
		if (message.ListExecInst is not null) context.Validators.ListExecInst(context, message, message.ListExecInst);
		if (message.EncodedListExecInstLen is not null) context.Validators.EncodedListExecInstLen(context, message, message.EncodedListExecInstLen);
		if (message.EncodedListExecInst is not null) context.Validators.EncodedListExecInst(context, message, message.EncodedListExecInst);
		if (message.TotNoOrders is null) Missing(message, FixTag.TotNoOrders);
		else context.Validators.TotNoOrders(context, message, message.TotNoOrders);
		if (message.NoOrders is null) Missing(message, FixTag.NoOrders);
		else context.Validators.NoOrders(context, message, message.NoOrders);
		Counted(message, message.NoOrders, message.NoOrdersGroups);
		if (message.NoOrdersGroups is not null)
			for (var i = 0; i < message.NoOrdersGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders(context, message, message.NoOrdersGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle(Fix42Context context, FixMessage.NewOrderSingle message)
	{
		if (message.ClOrdID is null) Missing(message, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderSingle_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, message, message.FutSettDate);
		if (message.HandlInst is null) Missing(message, FixTag.HandlInst);
		else context.Validators.HandlInst(context, message, message.HandlInst);
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
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, message.PrevClosePx);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, message, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, message.CashOrderQty);
		if (message.OrdType is null) Missing(message, FixTag.OrdType);
		else context.Validators.OrdType(context, message, message.OrdType);
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
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, message, message.Rule80A);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, message, message.OpenClose);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, message, message.CustomerOrFirm);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, message, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, message, message.DiscretionOffset);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, message, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, message, message.ClearingAccount);

		return message.IsValid;
	}

	static bool ValidateNews(Fix42Context context, FixMessage.News message)
	{
		if (message.OrigTime is not null) context.Validators.OrigTime(context, message, message.OrigTime);
		if (message.Urgency is not null) context.Validators.Urgency(context, message, message.Urgency);
		if (message.Headline is null) Missing(message, FixTag.Headline);
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
		if (message.LinesOfText is null) Missing(message, FixTag.NoLinesOfText);
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

	static bool ValidateOrderCancelReject(Fix42Context context, FixMessage.OrderCancelReject message)
	{
		if (message.OrderID is null) Missing(message, FixTag.OrderID);
		else context.Validators.OrderID(context, message, message.OrderID);
		if (message.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, message.SecondaryOrderID);
		if (message.ClOrdID is null) Missing(message, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.OrigClOrdID is null) Missing(message, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.OrdStatus is null) Missing(message, FixTag.OrdStatus);
		else context.Validators.OrdStatus(context, message, message.OrdStatus);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.CxlRejResponseTo is null) Missing(message, FixTag.CxlRejResponseTo);
		else context.Validators.CxlRejResponseTo(context, message, message.CxlRejResponseTo);
		if (message.CxlRejReason is not null) context.Validators.CxlRejReason(context, message, message.CxlRejReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest(Fix42Context context, FixMessage.OrderCancelReplaceRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.OrigClOrdID is null) Missing(message, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.ClOrdID is null) Missing(message, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.NoAllocs is not null) context.Validators.NoAllocs(context, message, message.NoAllocs);
		Counted(message, message.NoAllocs, message.NoAllocsGroups);
		if (message.NoAllocsGroups is not null)
			for (var i = 0; i < message.NoAllocsGroups.Count; i++)
				context.Validators.OrderCancelReplaceRequest_NoAllocs(context, message, message.NoAllocsGroups[i], i);
		if (message.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, message.SettlmntTyp);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, message, message.FutSettDate);
		if (message.HandlInst is null) Missing(message, FixTag.HandlInst);
		else context.Validators.HandlInst(context, message, message.HandlInst);
		if (message.ExecInst is not null) context.Validators.ExecInst(context, message, message.ExecInst);
		if (message.MinQty is not null) context.Validators.MinQty(context, message, message.MinQty);
		if (message.MaxFloor is not null) context.Validators.MaxFloor(context, message, message.MaxFloor);
		if (message.ExDestination is not null) context.Validators.ExDestination(context, message, message.ExDestination);
		if (message.NoTradingSessions is not null) context.Validators.NoTradingSessions(context, message, message.NoTradingSessions);
		Counted(message, message.NoTradingSessions, message.NoTradingSessionsGroups);
		if (message.NoTradingSessionsGroups is not null)
			for (var i = 0; i < message.NoTradingSessionsGroups.Count; i++)
				context.Validators.OrderCancelReplaceRequest_NoTradingSessions(context, message, message.NoTradingSessionsGroups[i], i);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, message, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, message.CashOrderQty);
		if (message.OrdType is null) Missing(message, FixTag.OrdType);
		else context.Validators.OrdType(context, message, message.OrdType);
		if (message.Price is not null) context.Validators.Price(context, message, message.Price);
		if (message.StopPx is not null) context.Validators.StopPx(context, message, message.StopPx);
		if (message.PegDifference is not null) context.Validators.PegDifference(context, message, message.PegDifference);
		if (message.DiscretionInst is not null) context.Validators.DiscretionInst(context, message, message.DiscretionInst);
		if (message.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, message, message.DiscretionOffset);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TimeInForce is not null) context.Validators.TimeInForce(context, message, message.TimeInForce);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.ExpireDate is not null) context.Validators.ExpireDate(context, message, message.ExpireDate);
		if (message.ExpireTime is not null) context.Validators.ExpireTime(context, message, message.ExpireTime);
		if (message.GTBookingInst is not null) context.Validators.GTBookingInst(context, message, message.GTBookingInst);
		if (message.Commission is not null) context.Validators.Commission(context, message, message.Commission);
		if (message.CommType is not null) context.Validators.CommType(context, message, message.CommType);
		if (message.Rule80A is not null) context.Validators.Rule80A(context, message, message.Rule80A);
		if (message.ForexReq is not null) context.Validators.ForexReq(context, message, message.ForexReq);
		if (message.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, message.SettlCurrency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.OpenClose is not null) context.Validators.OpenClose(context, message, message.OpenClose);
		if (message.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, message.CoveredOrUncovered);
		if (message.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, message, message.CustomerOrFirm);
		if (message.MaxShow is not null) context.Validators.MaxShow(context, message, message.MaxShow);
		if (message.LocateReqd is not null) context.Validators.LocateReqd(context, message, message.LocateReqd);
		if (message.ClearingFirm is not null) context.Validators.ClearingFirm(context, message, message.ClearingFirm);
		if (message.ClearingAccount is not null) context.Validators.ClearingAccount(context, message, message.ClearingAccount);

		return message.IsValid;
	}

	static bool ValidateOrderCancelRequest(Fix42Context context, FixMessage.OrderCancelRequest message)
	{
		if (message.OrigClOrdID is null) Missing(message, FixTag.OrigClOrdID);
		else context.Validators.OrigClOrdID(context, message, message.OrigClOrdID);
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is null) Missing(message, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.ListID is not null) context.Validators.ListID(context, message, message.ListID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.OrderQty is not null) context.Validators.OrderQty(context, message, message.OrderQty);
		if (message.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, message.CashOrderQty);
		if (message.ComplianceID is not null) context.Validators.ComplianceID(context, message, message.ComplianceID);
		if (message.SolicitedFlag is not null) context.Validators.SolicitedFlag(context, message, message.SolicitedFlag);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderStatusRequest(Fix42Context context, FixMessage.OrderStatusRequest message)
	{
		if (message.OrderID is not null) context.Validators.OrderID(context, message, message.OrderID);
		if (message.ClOrdID is null) Missing(message, FixTag.ClOrdID);
		else context.Validators.ClOrdID(context, message, message.ClOrdID);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.Account is not null) context.Validators.Account(context, message, message.Account);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is null) Missing(message, FixTag.Side);
		else context.Validators.Side(context, message, message.Side);

		return message.IsValid;
	}

	static bool ValidateQuote(Fix42Context context, FixMessage.Quote message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, FixTag.QuoteID);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.BidPx is not null) context.Validators.BidPx(context, message, message.BidPx);
		if (message.OfferPx is not null) context.Validators.OfferPx(context, message, message.OfferPx);
		if (message.BidSize is not null) context.Validators.BidSize(context, message, message.BidSize);
		if (message.OfferSize is not null) context.Validators.OfferSize(context, message, message.OfferSize);
		if (message.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, message.ValidUntilTime);
		if (message.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, message.BidSpotRate);
		if (message.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, message.OfferSpotRate);
		if (message.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, message.BidForwardPoints);
		if (message.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, message.OfferForwardPoints);
		if (message.TransactTime is not null) context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.FutSettDate is not null) context.Validators.FutSettDate(context, message, message.FutSettDate);
		if (message.OrdType is not null) context.Validators.OrdType(context, message, message.OrdType);
		if (message.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, message.FutSettDate2);
		if (message.OrderQty2 is not null) context.Validators.OrderQty2(context, message, message.OrderQty2);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);

		return message.IsValid;
	}

	static bool ValidateQuoteAcknowledgement(Fix42Context context, FixMessage.QuoteAcknowledgement message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteAckStatus is null) Missing(message, FixTag.QuoteStatus);
		else context.Validators.QuoteAckStatus(context, message, message.QuoteAckStatus);
		if (message.QuoteRejectReason is not null) context.Validators.QuoteRejectReason(context, message, message.QuoteRejectReason);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.NoQuoteSets is not null) context.Validators.NoQuoteSets(context, message, message.NoQuoteSets);
		Counted(message, message.NoQuoteSets, message.NoQuoteSetsGroups);
		if (message.NoQuoteSetsGroups is not null)
			for (var i = 0; i < message.NoQuoteSetsGroups.Count; i++)
				context.Validators.QuoteAcknowledgement_NoQuoteSets(context, message, message.NoQuoteSetsGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel(Fix42Context context, FixMessage.QuoteCancel message)
	{
		if (message.QuoteReqID is not null) context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.QuoteID is null) Missing(message, FixTag.QuoteID);
		else context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.QuoteCancelType is null) Missing(message, FixTag.QuoteCancelType);
		else context.Validators.QuoteCancelType(context, message, message.QuoteCancelType);
		if (message.QuoteResponseLevel is not null) context.Validators.QuoteResponseLevel(context, message, message.QuoteResponseLevel);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.NoQuoteEntries is null) Missing(message, FixTag.NoQuoteEntries);
		else context.Validators.NoQuoteEntries(context, message, message.NoQuoteEntries);
		Counted(message, message.NoQuoteEntries, message.NoQuoteEntriesGroups);
		if (message.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < message.NoQuoteEntriesGroups.Count; i++)
				context.Validators.QuoteCancel_NoQuoteEntries(context, message, message.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest(Fix42Context context, FixMessage.QuoteRequest message)
	{
		if (message.QuoteReqID is null) Missing(message, FixTag.QuoteReqID);
		else context.Validators.QuoteReqID(context, message, message.QuoteReqID);
		if (message.NoRelatedSym is null) Missing(message, FixTag.NoRelatedSym);
		else context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.QuoteRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteStatusRequest(Fix42Context context, FixMessage.QuoteStatusRequest message)
	{
		if (message.QuoteID is not null) context.Validators.QuoteID(context, message, message.QuoteID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateReject(Fix42Context context, FixMessage.Reject message)
	{
		if (message.RefSeqNum is null) Missing(message, FixTag.RefSeqNum);
		else context.Validators.RefSeqNum(context, message, message.RefSeqNum);
		if (message.RefTagID is not null) context.Validators.RefTagID(context, message, message.RefTagID);
		if (message.RefMsgType is not null) context.Validators.RefMsgType(context, message, message.RefMsgType);
		if (message.SessionRejectReason is not null) context.Validators.SessionRejectReason(context, message, message.SessionRejectReason);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);

		return message.IsValid;
	}

	static bool ValidateResendRequest(Fix42Context context, FixMessage.ResendRequest message)
	{
		if (message.BeginSeqNo is null) Missing(message, FixTag.BeginSeqNo);
		else context.Validators.BeginSeqNo(context, message, message.BeginSeqNo);
		if (message.EndSeqNo is null) Missing(message, FixTag.EndSeqNo);
		else context.Validators.EndSeqNo(context, message, message.EndSeqNo);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition(Fix42Context context, FixMessage.SecurityDefinition message)
	{
		if (message.SecurityReqID is null) Missing(message, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityResponseID is null) Missing(message, FixTag.SecurityResponseID);
		else context.Validators.SecurityResponseID(context, message, message.SecurityResponseID);
		if (message.SecurityResponseType is not null) context.Validators.SecurityResponseType(context, message, message.SecurityResponseType);
		if (message.TotalNumSecurities is null) Missing(message, FixTag.TotNoRelatedSym);
		else context.Validators.TotalNumSecurities(context, message, message.TotalNumSecurities);
		if (message.Symbol is not null) context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.SecurityDefinition_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest(Fix42Context context, FixMessage.SecurityDefinitionRequest message)
	{
		if (message.SecurityReqID is null) Missing(message, FixTag.SecurityReqID);
		else context.Validators.SecurityReqID(context, message, message.SecurityReqID);
		if (message.SecurityRequestType is null) Missing(message, FixTag.SecurityRequestType);
		else context.Validators.SecurityRequestType(context, message, message.SecurityRequestType);
		if (message.Symbol is not null) context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.Text is not null) context.Validators.Text(context, message, message.Text);
		if (message.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, message.EncodedTextLen);
		if (message.EncodedText is not null) context.Validators.EncodedText(context, message, message.EncodedText);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.NoRelatedSym is not null) context.Validators.NoRelatedSym(context, message, message.NoRelatedSym);
		Counted(message, message.NoRelatedSym, message.NoRelatedSymGroups);
		if (message.NoRelatedSymGroups is not null)
			for (var i = 0; i < message.NoRelatedSymGroups.Count; i++)
				context.Validators.SecurityDefinitionRequest_NoRelatedSym(context, message, message.NoRelatedSymGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateSecurityStatus(Fix42Context context, FixMessage.SecurityStatus message)
	{
		if (message.SecurityStatusReqID is not null) context.Validators.SecurityStatusReqID(context, message, message.SecurityStatusReqID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
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

		return message.IsValid;
	}

	static bool ValidateSecurityStatusRequest(Fix42Context context, FixMessage.SecurityStatusRequest message)
	{
		if (message.SecurityStatusReqID is null) Missing(message, FixTag.SecurityStatusReqID);
		else context.Validators.SecurityStatusReqID(context, message, message.SecurityStatusReqID);
		if (message.Symbol is null) Missing(message, FixTag.Symbol);
		else context.Validators.Symbol(context, message, message.Symbol);
		if (message.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, message.SymbolSfx);
		if (message.SecurityID is not null) context.Validators.SecurityID(context, message, message.SecurityID);
		if (message.IDSource is not null) context.Validators.IDSource(context, message, message.IDSource);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, message.MaturityMonthYear);
		if (message.MaturityDay is not null) context.Validators.MaturityDay(context, message, message.MaturityDay);
		if (message.PutOrCall is not null) context.Validators.PutOrCall(context, message, message.PutOrCall);
		if (message.StrikePrice is not null) context.Validators.StrikePrice(context, message, message.StrikePrice);
		if (message.OptAttribute is not null) context.Validators.OptAttribute(context, message, message.OptAttribute);
		if (message.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, message.ContractMultiplier);
		if (message.CouponRate is not null) context.Validators.CouponRate(context, message, message.CouponRate);
		if (message.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, message.SecurityExchange);
		if (message.Issuer is not null) context.Validators.Issuer(context, message, message.Issuer);
		if (message.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, message.EncodedIssuerLen);
		if (message.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, message.EncodedIssuer);
		if (message.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, message.SecurityDesc);
		if (message.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, message.EncodedSecurityDescLen);
		if (message.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, message.EncodedSecurityDesc);
		if (message.Currency is not null) context.Validators.Currency(context, message, message.Currency);
		if (message.SubscriptionRequestType is null) Missing(message, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateSequenceReset(Fix42Context context, FixMessage.SequenceReset message)
	{
		if (message.GapFillFlag is not null) context.Validators.GapFillFlag(context, message, message.GapFillFlag);
		if (message.NewSeqNo is null) Missing(message, FixTag.NewSeqNo);
		else context.Validators.NewSeqNo(context, message, message.NewSeqNo);

		return message.IsValid;
	}

	static bool ValidateSettlementInstructions(Fix42Context context, FixMessage.SettlementInstructions message)
	{
		if (message.SettlInstID is null) Missing(message, FixTag.SettlInstID);
		else context.Validators.SettlInstID(context, message, message.SettlInstID);
		if (message.SettlInstTransType is null) Missing(message, FixTag.SettlInstTransType);
		else context.Validators.SettlInstTransType(context, message, message.SettlInstTransType);
		if (message.SettlInstRefID is null) Missing(message, FixTag.SettlInstRefID);
		else context.Validators.SettlInstRefID(context, message, message.SettlInstRefID);
		if (message.SettlInstMode is null) Missing(message, FixTag.SettlInstMode);
		else context.Validators.SettlInstMode(context, message, message.SettlInstMode);
		if (message.SettlInstSource is null) Missing(message, FixTag.SettlInstSource);
		else context.Validators.SettlInstSource(context, message, message.SettlInstSource);
		if (message.AllocAccount is null) Missing(message, FixTag.AllocAccount);
		else context.Validators.AllocAccount(context, message, message.AllocAccount);
		if (message.SettlLocation is not null) context.Validators.SettlLocation(context, message, message.SettlLocation);
		if (message.TradeDate is not null) context.Validators.TradeDate(context, message, message.TradeDate);
		if (message.AllocID is not null) context.Validators.AllocID(context, message, message.AllocID);
		if (message.LastMkt is not null) context.Validators.LastMkt(context, message, message.LastMkt);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.Side is not null) context.Validators.Side(context, message, message.Side);
		if (message.SecurityType is not null) context.Validators.SecurityType(context, message, message.SecurityType);
		if (message.EffectiveTime is not null) context.Validators.EffectiveTime(context, message, message.EffectiveTime);
		if (message.TransactTime is null) Missing(message, FixTag.TransactTime);
		else context.Validators.TransactTime(context, message, message.TransactTime);
		if (message.ClientID is not null) context.Validators.ClientID(context, message, message.ClientID);
		if (message.ExecBroker is not null) context.Validators.ExecBroker(context, message, message.ExecBroker);
		if (message.StandInstDbType is not null) context.Validators.StandInstDbType(context, message, message.StandInstDbType);
		if (message.StandInstDbName is not null) context.Validators.StandInstDbName(context, message, message.StandInstDbName);
		if (message.StandInstDbID is not null) context.Validators.StandInstDbID(context, message, message.StandInstDbID);
		if (message.SettlDeliveryType is not null) context.Validators.SettlDeliveryType(context, message, message.SettlDeliveryType);
		if (message.SettlDepositoryCode is not null) context.Validators.SettlDepositoryCode(context, message, message.SettlDepositoryCode);
		if (message.SettlBrkrCode is not null) context.Validators.SettlBrkrCode(context, message, message.SettlBrkrCode);
		if (message.SettlInstCode is not null) context.Validators.SettlInstCode(context, message, message.SettlInstCode);
		if (message.SecuritySettlAgentName is not null) context.Validators.SecuritySettlAgentName(context, message, message.SecuritySettlAgentName);
		if (message.SecuritySettlAgentCode is not null) context.Validators.SecuritySettlAgentCode(context, message, message.SecuritySettlAgentCode);
		if (message.SecuritySettlAgentAcctNum is not null) context.Validators.SecuritySettlAgentAcctNum(context, message, message.SecuritySettlAgentAcctNum);
		if (message.SecuritySettlAgentAcctName is not null) context.Validators.SecuritySettlAgentAcctName(context, message, message.SecuritySettlAgentAcctName);
		if (message.SecuritySettlAgentContactName is not null) context.Validators.SecuritySettlAgentContactName(context, message, message.SecuritySettlAgentContactName);
		if (message.SecuritySettlAgentContactPhone is not null) context.Validators.SecuritySettlAgentContactPhone(context, message, message.SecuritySettlAgentContactPhone);
		if (message.CashSettlAgentName is not null) context.Validators.CashSettlAgentName(context, message, message.CashSettlAgentName);
		if (message.CashSettlAgentCode is not null) context.Validators.CashSettlAgentCode(context, message, message.CashSettlAgentCode);
		if (message.CashSettlAgentAcctNum is not null) context.Validators.CashSettlAgentAcctNum(context, message, message.CashSettlAgentAcctNum);
		if (message.CashSettlAgentAcctName is not null) context.Validators.CashSettlAgentAcctName(context, message, message.CashSettlAgentAcctName);
		if (message.CashSettlAgentContactName is not null) context.Validators.CashSettlAgentContactName(context, message, message.CashSettlAgentContactName);
		if (message.CashSettlAgentContactPhone is not null) context.Validators.CashSettlAgentContactPhone(context, message, message.CashSettlAgentContactPhone);

		return message.IsValid;
	}

	static bool ValidateTestRequest(Fix42Context context, FixMessage.TestRequest message)
	{
		if (message.TestReqID is null) Missing(message, FixTag.TestReqID);
		else context.Validators.TestReqID(context, message, message.TestReqID);

		return message.IsValid;
	}

	static bool ValidateTradingSessionStatus(Fix42Context context, FixMessage.TradingSessionStatus message)
	{
		if (message.TradSesReqID is not null) context.Validators.TradSesReqID(context, message, message.TradSesReqID);
		if (message.TradingSessionID is null) Missing(message, FixTag.TradingSessionID);
		else context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);
		if (message.UnsolicitedIndicator is not null) context.Validators.UnsolicitedIndicator(context, message, message.UnsolicitedIndicator);
		if (message.TradSesStatus is null) Missing(message, FixTag.TradSesStatus);
		else context.Validators.TradSesStatus(context, message, message.TradSesStatus);
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

	static bool ValidateTradingSessionStatusRequest(Fix42Context context, FixMessage.TradingSessionStatusRequest message)
	{
		if (message.TradSesReqID is null) Missing(message, FixTag.TradSesReqID);
		else context.Validators.TradSesReqID(context, message, message.TradSesReqID);
		if (message.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, message.TradingSessionID);
		if (message.TradSesMethod is not null) context.Validators.TradSesMethod(context, message, message.TradSesMethod);
		if (message.TradSesMode is not null) context.Validators.TradSesMode(context, message, message.TradSesMode);
		if (message.SubscriptionRequestType is null) Missing(message, FixTag.SubscriptionRequestType);
		else context.Validators.SubscriptionRequestType(context, message, message.SubscriptionRequestType);

		return message.IsValid;
	}

	static bool ValidateAllocation_NoOrders(Fix42Context context, FixMessage message, FixMessage.Allocation.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.OrderID is not null) context.Validators.OrderID(context, message, entry.OrderID);
		if (entry.SecondaryOrderID is not null) context.Validators.SecondaryOrderID(context, message, entry.SecondaryOrderID);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (entry.WaveNo is not null) context.Validators.WaveNo(context, message, entry.WaveNo);

		return message.IsValid;
	}

	static bool ValidateAllocation_NoExecs(Fix42Context context, FixMessage message, FixMessage.Allocation.NoExecsGroup entry, int index)
	{
		context.Validators.LastShares(context, message, entry.LastShares);
		if (entry.ExecID is not null) context.Validators.ExecID(context, message, entry.ExecID);
		if (entry.LastPx is not null) context.Validators.LastPx(context, message, entry.LastPx);
		if (entry.LastCapacity is not null) context.Validators.LastCapacity(context, message, entry.LastCapacity);

		return message.IsValid;
	}

	static bool ValidateAllocation_NoAllocs(Fix42Context context, FixMessage message, FixMessage.Allocation.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocPrice is not null) context.Validators.AllocPrice(context, message, entry.AllocPrice);
		if (entry.AllocShares is null) Missing(message, FixTag.AllocQty, entry.AllocAccount.Position, index);
		else context.Validators.AllocShares(context, message, entry.AllocShares);
		if (entry.ProcessCode is not null) context.Validators.ProcessCode(context, message, entry.ProcessCode);
		if (entry.BrokerOfCredit is not null) context.Validators.BrokerOfCredit(context, message, entry.BrokerOfCredit);
		if (entry.NotifyBrokerOfCredit is not null) context.Validators.NotifyBrokerOfCredit(context, message, entry.NotifyBrokerOfCredit);
		if (entry.AllocHandlInst is not null) context.Validators.AllocHandlInst(context, message, entry.AllocHandlInst);
		if (entry.AllocText is not null) context.Validators.AllocText(context, message, entry.AllocText);
		if (entry.EncodedAllocTextLen is not null) context.Validators.EncodedAllocTextLen(context, message, entry.EncodedAllocTextLen);
		if (entry.EncodedAllocText is not null) context.Validators.EncodedAllocText(context, message, entry.EncodedAllocText);
		if (entry.ExecBroker is not null) context.Validators.ExecBroker(context, message, entry.ExecBroker);
		if (entry.ClientID is not null) context.Validators.ClientID(context, message, entry.ClientID);
		if (entry.Commission is not null) context.Validators.Commission(context, message, entry.Commission);
		if (entry.CommType is not null) context.Validators.CommType(context, message, entry.CommType);
		if (entry.AllocAvgPx is not null) context.Validators.AllocAvgPx(context, message, entry.AllocAvgPx);
		if (entry.AllocNetMoney is not null) context.Validators.AllocNetMoney(context, message, entry.AllocNetMoney);
		if (entry.SettlCurrAmt is not null) context.Validators.SettlCurrAmt(context, message, entry.SettlCurrAmt);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.SettlCurrFxRate is not null) context.Validators.SettlCurrFxRate(context, message, entry.SettlCurrFxRate);
		if (entry.SettlCurrFxRateCalc is not null) context.Validators.SettlCurrFxRateCalc(context, message, entry.SettlCurrFxRateCalc);
		if (entry.AccruedInterestAmt is not null) context.Validators.AccruedInterestAmt(context, message, entry.AccruedInterestAmt);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, message, entry.SettlInstMode);
		if (entry.NoMiscFees is not null) context.Validators.NoMiscFees(context, message, entry.NoMiscFees);
		Counted(message, entry.NoMiscFees, entry.NoMiscFeesGroups);
		if (entry.NoMiscFeesGroups is not null)
			for (var i = 0; i < entry.NoMiscFeesGroups.Count; i++)
				context.Validators.Allocation_NoAllocs_NoMiscFees(context, message, entry.NoMiscFeesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateAllocation_NoAllocs_NoMiscFees(Fix42Context context, FixMessage message, FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup entry, int index)
	{
		context.Validators.MiscFeeAmt(context, message, entry.MiscFeeAmt);
		if (entry.MiscFeeCurr is not null) context.Validators.MiscFeeCurr(context, message, entry.MiscFeeCurr);
		if (entry.MiscFeeType is not null) context.Validators.MiscFeeType(context, message, entry.MiscFeeType);

		return message.IsValid;
	}

	static bool ValidateBidRequest_NoBidDescriptors(Fix42Context context, FixMessage message, FixMessage.BidRequest.NoBidDescriptorsGroup entry, int index)
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

	static bool ValidateBidRequest_NoBidComponents(Fix42Context context, FixMessage message, FixMessage.BidRequest.NoBidComponentsGroup entry, int index)
	{
		context.Validators.ListID(context, message, entry.ListID);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, message, entry.NetGrossInd);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, message, entry.FutSettDate);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);

		return message.IsValid;
	}

	static bool ValidateBidResponse_NoBidComponents(Fix42Context context, FixMessage message, FixMessage.BidResponse.NoBidComponentsGroup entry, int index)
	{
		context.Validators.Commission(context, message, entry.Commission);
		if (entry.CommType is null) Missing(message, FixTag.CommType, entry.Commission.Position, index);
		else context.Validators.CommType(context, message, entry.CommType);
		if (entry.ListID is not null) context.Validators.ListID(context, message, entry.ListID);
		if (entry.Country is not null) context.Validators.Country(context, message, entry.Country);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.PriceType is not null) context.Validators.PriceType(context, message, entry.PriceType);
		if (entry.FairValue is not null) context.Validators.FairValue(context, message, entry.FairValue);
		if (entry.NetGrossInd is not null) context.Validators.NetGrossInd(context, message, entry.NetGrossInd);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, message, entry.FutSettDate);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateEmail_NoRoutingIDs(Fix42Context context, FixMessage message, FixMessage.Email.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateEmail_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.Email.NoRelatedSymGroup entry, int index)
	{
		context.Validators.RelatdSym(context, message, entry.RelatdSym);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);

		return message.IsValid;
	}

	static bool ValidateEmail_LinesOfText(Fix42Context context, FixMessage message, FixMessage.Email.LinesOfTextGroup entry, int index)
	{
		context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateExecutionReport_NoContraBrokers(Fix42Context context, FixMessage message, FixMessage.ExecutionReport.NoContraBrokersGroup entry, int index)
	{
		context.Validators.ContraBroker(context, message, entry.ContraBroker);
		if (entry.ContraTrader is not null) context.Validators.ContraTrader(context, message, entry.ContraTrader);
		if (entry.ContraTradeQty is not null) context.Validators.ContraTradeQty(context, message, entry.ContraTradeQty);
		if (entry.ContraTradeTime is not null) context.Validators.ContraTradeTime(context, message, entry.ContraTradeTime);

		return message.IsValid;
	}

	static bool ValidateIndicationofInterest_NoIOIQualifiers(Fix42Context context, FixMessage message, FixMessage.IndicationofInterest.NoIOIQualifiersGroup entry, int index)
	{
		context.Validators.IOIQualifier(context, message, entry.IOIQualifier);

		return message.IsValid;
	}

	static bool ValidateIndicationofInterest_NoRoutingIDs(Fix42Context context, FixMessage message, FixMessage.IndicationofInterest.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateListStatus_NoOrders(Fix42Context context, FixMessage message, FixMessage.ListStatus.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.CumQty is null) Missing(message, FixTag.CumQty, entry.ClOrdID.Position, index);
		else context.Validators.CumQty(context, message, entry.CumQty);
		if (entry.OrdStatus is null) Missing(message, FixTag.OrdStatus, entry.ClOrdID.Position, index);
		else context.Validators.OrdStatus(context, message, entry.OrdStatus);
		if (entry.LeavesQty is null) Missing(message, FixTag.LeavesQty, entry.ClOrdID.Position, index);
		else context.Validators.LeavesQty(context, message, entry.LeavesQty);
		if (entry.CxlQty is null) Missing(message, FixTag.CxlQty, entry.ClOrdID.Position, index);
		else context.Validators.CxlQty(context, message, entry.CxlQty);
		if (entry.AvgPx is null) Missing(message, FixTag.AvgPx, entry.ClOrdID.Position, index);
		else context.Validators.AvgPx(context, message, entry.AvgPx);
		if (entry.OrdRejReason is not null) context.Validators.OrdRejReason(context, message, entry.OrdRejReason);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateListStrikePrice_NoStrikes(Fix42Context context, FixMessage message, FixMessage.ListStrikePrice.NoStrikesGroup entry, int index)
	{
		context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.ClOrdID is not null) context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.Price is null) Missing(message, FixTag.Price, entry.Symbol.Position, index);
		else context.Validators.Price(context, message, entry.Price);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateLogon_NoMsgTypes(Fix42Context context, FixMessage message, FixMessage.Logon.NoMsgTypesGroup entry, int index)
	{
		context.Validators.RefMsgType(context, message, entry.RefMsgType);
		if (entry.MsgDirection is not null) context.Validators.MsgDirection(context, message, entry.MsgDirection);

		return message.IsValid;
	}

	static bool ValidateMarketDataIncrementalRefresh_NoMDEntries(Fix42Context context, FixMessage message, FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup entry, int index)
	{
		context.Validators.MDUpdateAction(context, message, entry.MDUpdateAction);
		if (entry.DeleteReason is not null) context.Validators.DeleteReason(context, message, entry.DeleteReason);
		if (entry.MDEntryType is not null) context.Validators.MDEntryType(context, message, entry.MDEntryType);
		if (entry.MDEntryID is not null) context.Validators.MDEntryID(context, message, entry.MDEntryID);
		if (entry.MDEntryRefID is not null) context.Validators.MDEntryRefID(context, message, entry.MDEntryRefID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
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
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, message, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, message, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, message, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);
		if (entry.OpenCloseSettleFlag is not null) context.Validators.OpenCloseSettleFlag(context, message, entry.OpenCloseSettleFlag);
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
		if (entry.TotalVolumeTraded is not null) context.Validators.TotalVolumeTraded(context, message, entry.TotalVolumeTraded);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoMDEntryTypes(Fix42Context context, FixMessage message, FixMessage.MarketDataRequest.NoMDEntryTypesGroup entry, int index)
	{
		context.Validators.MDEntryType(context, message, entry.MDEntryType);

		return message.IsValid;
	}

	static bool ValidateMarketDataRequest_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.MarketDataRequest.NoRelatedSymGroup entry, int index)
	{
		context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateMarketDataSnapshotFullRefresh_NoMDEntries(Fix42Context context, FixMessage message, FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup entry, int index)
	{
		context.Validators.MDEntryType(context, message, entry.MDEntryType);
		if (entry.MDEntryPx is null) Missing(message, FixTag.MDEntryPx, entry.MDEntryType.Position, index);
		else context.Validators.MDEntryPx(context, message, entry.MDEntryPx);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);
		if (entry.MDEntrySize is not null) context.Validators.MDEntrySize(context, message, entry.MDEntrySize);
		if (entry.MDEntryDate is not null) context.Validators.MDEntryDate(context, message, entry.MDEntryDate);
		if (entry.MDEntryTime is not null) context.Validators.MDEntryTime(context, message, entry.MDEntryTime);
		if (entry.TickDirection is not null) context.Validators.TickDirection(context, message, entry.TickDirection);
		if (entry.MDMkt is not null) context.Validators.MDMkt(context, message, entry.MDMkt);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.QuoteCondition is not null) context.Validators.QuoteCondition(context, message, entry.QuoteCondition);
		if (entry.TradeCondition is not null) context.Validators.TradeCondition(context, message, entry.TradeCondition);
		if (entry.MDEntryOriginator is not null) context.Validators.MDEntryOriginator(context, message, entry.MDEntryOriginator);
		if (entry.LocationID is not null) context.Validators.LocationID(context, message, entry.LocationID);
		if (entry.DeskID is not null) context.Validators.DeskID(context, message, entry.DeskID);
		if (entry.OpenCloseSettleFlag is not null) context.Validators.OpenCloseSettleFlag(context, message, entry.OpenCloseSettleFlag);
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
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateMassQuote_NoQuoteSets(Fix42Context context, FixMessage message, FixMessage.MassQuote.NoQuoteSetsGroup entry, int index)
	{
		context.Validators.QuoteSetID(context, message, entry.QuoteSetID);
		if (entry.UnderlyingSymbol is null) Missing(message, FixTag.UnderlyingSymbol, entry.QuoteSetID.Position, index);
		else context.Validators.UnderlyingSymbol(context, message, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, message, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, message, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, message, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, message, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, message, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, message, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, message, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, message, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, message, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, message, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, message, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, message, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, message, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, message, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, message, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, message, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, message, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, message, entry.EncodedUnderlyingSecurityDesc);
		if (entry.QuoteSetValidUntilTime is not null) context.Validators.QuoteSetValidUntilTime(context, message, entry.QuoteSetValidUntilTime);
		if (entry.TotQuoteEntries is null) Missing(message, FixTag.TotNoQuoteEntries, entry.QuoteSetID.Position, index);
		else context.Validators.TotQuoteEntries(context, message, entry.TotQuoteEntries);
		if (entry.NoQuoteEntries is null) Missing(message, FixTag.NoQuoteEntries, entry.QuoteSetID.Position, index);
		else context.Validators.NoQuoteEntries(context, message, entry.NoQuoteEntries);
		Counted(message, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		if (entry.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < entry.NoQuoteEntriesGroups.Count; i++)
				context.Validators.MassQuote_NoQuoteSets_NoQuoteEntries(context, message, entry.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateMassQuote_NoQuoteSets_NoQuoteEntries(Fix42Context context, FixMessage message, FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup entry, int index)
	{
		context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.BidPx is not null) context.Validators.BidPx(context, message, entry.BidPx);
		if (entry.OfferPx is not null) context.Validators.OfferPx(context, message, entry.OfferPx);
		if (entry.BidSize is not null) context.Validators.BidSize(context, message, entry.BidSize);
		if (entry.OfferSize is not null) context.Validators.OfferSize(context, message, entry.OfferSize);
		if (entry.ValidUntilTime is not null) context.Validators.ValidUntilTime(context, message, entry.ValidUntilTime);
		if (entry.BidSpotRate is not null) context.Validators.BidSpotRate(context, message, entry.BidSpotRate);
		if (entry.OfferSpotRate is not null) context.Validators.OfferSpotRate(context, message, entry.OfferSpotRate);
		if (entry.BidForwardPoints is not null) context.Validators.BidForwardPoints(context, message, entry.BidForwardPoints);
		if (entry.OfferForwardPoints is not null) context.Validators.OfferForwardPoints(context, message, entry.OfferForwardPoints);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, message, entry.FutSettDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders(Fix42Context context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup entry, int index)
	{
		context.Validators.ClOrdID(context, message, entry.ClOrdID);
		if (entry.ListSeqNo is null) Missing(message, FixTag.ListSeqNo, entry.ClOrdID.Position, index);
		else context.Validators.ListSeqNo(context, message, entry.ListSeqNo);
		if (entry.SettlInstMode is not null) context.Validators.SettlInstMode(context, message, entry.SettlInstMode);
		if (entry.ClientID is not null) context.Validators.ClientID(context, message, entry.ClientID);
		if (entry.ExecBroker is not null) context.Validators.ExecBroker(context, message, entry.ExecBroker);
		if (entry.Account is not null) context.Validators.Account(context, message, entry.Account);
		if (entry.NoAllocs is not null) context.Validators.NoAllocs(context, message, entry.NoAllocs);
		Counted(message, entry.NoAllocs, entry.NoAllocsGroups);
		if (entry.NoAllocsGroups is not null)
			for (var i = 0; i < entry.NoAllocsGroups.Count; i++)
				context.Validators.NewOrderList_NoOrders_NoAllocs(context, message, entry.NoAllocsGroups[i], i);
		if (entry.SettlmntTyp is not null) context.Validators.SettlmntTyp(context, message, entry.SettlmntTyp);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, message, entry.FutSettDate);
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
		if (entry.Symbol is null) Missing(message, FixTag.Symbol, entry.ClOrdID.Position, index);
		else context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.Side is null) Missing(message, FixTag.Side, entry.ClOrdID.Position, index);
		else context.Validators.Side(context, message, entry.Side);
		if (entry.SideValueInd is not null) context.Validators.SideValueInd(context, message, entry.SideValueInd);
		if (entry.LocateReqd is not null) context.Validators.LocateReqd(context, message, entry.LocateReqd);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.CashOrderQty is not null) context.Validators.CashOrderQty(context, message, entry.CashOrderQty);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.Price is not null) context.Validators.Price(context, message, entry.Price);
		if (entry.StopPx is not null) context.Validators.StopPx(context, message, entry.StopPx);
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
		if (entry.Commission is not null) context.Validators.Commission(context, message, entry.Commission);
		if (entry.CommType is not null) context.Validators.CommType(context, message, entry.CommType);
		if (entry.Rule80A is not null) context.Validators.Rule80A(context, message, entry.Rule80A);
		if (entry.ForexReq is not null) context.Validators.ForexReq(context, message, entry.ForexReq);
		if (entry.SettlCurrency is not null) context.Validators.SettlCurrency(context, message, entry.SettlCurrency);
		if (entry.Text is not null) context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.OpenClose is not null) context.Validators.OpenClose(context, message, entry.OpenClose);
		if (entry.CoveredOrUncovered is not null) context.Validators.CoveredOrUncovered(context, message, entry.CoveredOrUncovered);
		if (entry.CustomerOrFirm is not null) context.Validators.CustomerOrFirm(context, message, entry.CustomerOrFirm);
		if (entry.MaxShow is not null) context.Validators.MaxShow(context, message, entry.MaxShow);
		if (entry.PegDifference is not null) context.Validators.PegDifference(context, message, entry.PegDifference);
		if (entry.DiscretionInst is not null) context.Validators.DiscretionInst(context, message, entry.DiscretionInst);
		if (entry.DiscretionOffset is not null) context.Validators.DiscretionOffset(context, message, entry.DiscretionOffset);
		if (entry.ClearingFirm is not null) context.Validators.ClearingFirm(context, message, entry.ClearingFirm);
		if (entry.ClearingAccount is not null) context.Validators.ClearingAccount(context, message, entry.ClearingAccount);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders_NoAllocs(Fix42Context context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, message, entry.AllocShares);

		return message.IsValid;
	}

	static bool ValidateNewOrderList_NoOrders_NoTradingSessions(Fix42Context context, FixMessage message, FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle_NoAllocs(Fix42Context context, FixMessage message, FixMessage.NewOrderSingle.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, message, entry.AllocShares);

		return message.IsValid;
	}

	static bool ValidateNewOrderSingle_NoTradingSessions(Fix42Context context, FixMessage message, FixMessage.NewOrderSingle.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateNews_NoRoutingIDs(Fix42Context context, FixMessage message, FixMessage.News.NoRoutingIDsGroup entry, int index)
	{
		context.Validators.RoutingType(context, message, entry.RoutingType);
		if (entry.RoutingID is not null) context.Validators.RoutingID(context, message, entry.RoutingID);

		return message.IsValid;
	}

	static bool ValidateNews_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.News.NoRelatedSymGroup entry, int index)
	{
		context.Validators.RelatdSym(context, message, entry.RelatdSym);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);

		return message.IsValid;
	}

	static bool ValidateNews_LinesOfText(Fix42Context context, FixMessage message, FixMessage.News.LinesOfTextGroup entry, int index)
	{
		context.Validators.Text(context, message, entry.Text);
		if (entry.EncodedTextLen is not null) context.Validators.EncodedTextLen(context, message, entry.EncodedTextLen);
		if (entry.EncodedText is not null) context.Validators.EncodedText(context, message, entry.EncodedText);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest_NoAllocs(Fix42Context context, FixMessage message, FixMessage.OrderCancelReplaceRequest.NoAllocsGroup entry, int index)
	{
		context.Validators.AllocAccount(context, message, entry.AllocAccount);
		if (entry.AllocShares is not null) context.Validators.AllocShares(context, message, entry.AllocShares);

		return message.IsValid;
	}

	static bool ValidateOrderCancelReplaceRequest_NoTradingSessions(Fix42Context context, FixMessage message, FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup entry, int index)
	{
		context.Validators.TradingSessionID(context, message, entry.TradingSessionID);

		return message.IsValid;
	}

	static bool ValidateQuoteAcknowledgement_NoQuoteSets(Fix42Context context, FixMessage message, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup entry, int index)
	{
		context.Validators.QuoteSetID(context, message, entry.QuoteSetID);
		if (entry.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, message, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, message, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, message, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, message, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, message, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, message, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, message, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, message, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, message, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, message, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, message, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, message, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, message, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, message, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, message, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, message, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, message, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, message, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, message, entry.EncodedUnderlyingSecurityDesc);
		if (entry.TotQuoteEntries is not null) context.Validators.TotQuoteEntries(context, message, entry.TotQuoteEntries);
		if (entry.NoQuoteEntries is not null) context.Validators.NoQuoteEntries(context, message, entry.NoQuoteEntries);
		Counted(message, entry.NoQuoteEntries, entry.NoQuoteEntriesGroups);
		if (entry.NoQuoteEntriesGroups is not null)
			for (var i = 0; i < entry.NoQuoteEntriesGroups.Count; i++)
				context.Validators.QuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(context, message, entry.NoQuoteEntriesGroups[i], i);

		return message.IsValid;
	}

	static bool ValidateQuoteAcknowledgement_NoQuoteSets_NoQuoteEntries(Fix42Context context, FixMessage message, FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup entry, int index)
	{
		context.Validators.QuoteEntryID(context, message, entry.QuoteEntryID);
		if (entry.Symbol is not null) context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.QuoteEntryRejectReason is not null) context.Validators.QuoteEntryRejectReason(context, message, entry.QuoteEntryRejectReason);

		return message.IsValid;
	}

	static bool ValidateQuoteCancel_NoQuoteEntries(Fix42Context context, FixMessage message, FixMessage.QuoteCancel.NoQuoteEntriesGroup entry, int index)
	{
		context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.UnderlyingSymbol is not null) context.Validators.UnderlyingSymbol(context, message, entry.UnderlyingSymbol);

		return message.IsValid;
	}

	static bool ValidateQuoteRequest_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.QuoteRequest.NoRelatedSymGroup entry, int index)
	{
		context.Validators.Symbol(context, message, entry.Symbol);
		if (entry.SymbolSfx is not null) context.Validators.SymbolSfx(context, message, entry.SymbolSfx);
		if (entry.SecurityID is not null) context.Validators.SecurityID(context, message, entry.SecurityID);
		if (entry.IDSource is not null) context.Validators.IDSource(context, message, entry.IDSource);
		if (entry.SecurityType is not null) context.Validators.SecurityType(context, message, entry.SecurityType);
		if (entry.MaturityMonthYear is not null) context.Validators.MaturityMonthYear(context, message, entry.MaturityMonthYear);
		if (entry.MaturityDay is not null) context.Validators.MaturityDay(context, message, entry.MaturityDay);
		if (entry.PutOrCall is not null) context.Validators.PutOrCall(context, message, entry.PutOrCall);
		if (entry.StrikePrice is not null) context.Validators.StrikePrice(context, message, entry.StrikePrice);
		if (entry.OptAttribute is not null) context.Validators.OptAttribute(context, message, entry.OptAttribute);
		if (entry.ContractMultiplier is not null) context.Validators.ContractMultiplier(context, message, entry.ContractMultiplier);
		if (entry.CouponRate is not null) context.Validators.CouponRate(context, message, entry.CouponRate);
		if (entry.SecurityExchange is not null) context.Validators.SecurityExchange(context, message, entry.SecurityExchange);
		if (entry.Issuer is not null) context.Validators.Issuer(context, message, entry.Issuer);
		if (entry.EncodedIssuerLen is not null) context.Validators.EncodedIssuerLen(context, message, entry.EncodedIssuerLen);
		if (entry.EncodedIssuer is not null) context.Validators.EncodedIssuer(context, message, entry.EncodedIssuer);
		if (entry.SecurityDesc is not null) context.Validators.SecurityDesc(context, message, entry.SecurityDesc);
		if (entry.EncodedSecurityDescLen is not null) context.Validators.EncodedSecurityDescLen(context, message, entry.EncodedSecurityDescLen);
		if (entry.EncodedSecurityDesc is not null) context.Validators.EncodedSecurityDesc(context, message, entry.EncodedSecurityDesc);
		if (entry.PrevClosePx is not null) context.Validators.PrevClosePx(context, message, entry.PrevClosePx);
		if (entry.QuoteRequestType is not null) context.Validators.QuoteRequestType(context, message, entry.QuoteRequestType);
		if (entry.TradingSessionID is not null) context.Validators.TradingSessionID(context, message, entry.TradingSessionID);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.OrderQty is not null) context.Validators.OrderQty(context, message, entry.OrderQty);
		if (entry.FutSettDate is not null) context.Validators.FutSettDate(context, message, entry.FutSettDate);
		if (entry.OrdType is not null) context.Validators.OrdType(context, message, entry.OrdType);
		if (entry.FutSettDate2 is not null) context.Validators.FutSettDate2(context, message, entry.FutSettDate2);
		if (entry.OrderQty2 is not null) context.Validators.OrderQty2(context, message, entry.OrderQty2);
		if (entry.ExpireTime is not null) context.Validators.ExpireTime(context, message, entry.ExpireTime);
		if (entry.TransactTime is not null) context.Validators.TransactTime(context, message, entry.TransactTime);
		if (entry.Currency is not null) context.Validators.Currency(context, message, entry.Currency);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinition_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.SecurityDefinition.NoRelatedSymGroup entry, int index)
	{
		context.Validators.UnderlyingSymbol(context, message, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, message, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, message, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, message, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, message, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, message, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, message, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, message, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, message, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, message, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, message, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, message, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, message, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, message, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, message, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, message, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, message, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, message, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, message, entry.EncodedUnderlyingSecurityDesc);
		if (entry.RatioQty is not null) context.Validators.RatioQty(context, message, entry.RatioQty);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, message, entry.UnderlyingCurrency);

		return message.IsValid;
	}

	static bool ValidateSecurityDefinitionRequest_NoRelatedSym(Fix42Context context, FixMessage message, FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup entry, int index)
	{
		context.Validators.UnderlyingSymbol(context, message, entry.UnderlyingSymbol);
		if (entry.UnderlyingSymbolSfx is not null) context.Validators.UnderlyingSymbolSfx(context, message, entry.UnderlyingSymbolSfx);
		if (entry.UnderlyingSecurityID is not null) context.Validators.UnderlyingSecurityID(context, message, entry.UnderlyingSecurityID);
		if (entry.UnderlyingIDSource is not null) context.Validators.UnderlyingIDSource(context, message, entry.UnderlyingIDSource);
		if (entry.UnderlyingSecurityType is not null) context.Validators.UnderlyingSecurityType(context, message, entry.UnderlyingSecurityType);
		if (entry.UnderlyingMaturityMonthYear is not null) context.Validators.UnderlyingMaturityMonthYear(context, message, entry.UnderlyingMaturityMonthYear);
		if (entry.UnderlyingMaturityDay is not null) context.Validators.UnderlyingMaturityDay(context, message, entry.UnderlyingMaturityDay);
		if (entry.UnderlyingPutOrCall is not null) context.Validators.UnderlyingPutOrCall(context, message, entry.UnderlyingPutOrCall);
		if (entry.UnderlyingStrikePrice is not null) context.Validators.UnderlyingStrikePrice(context, message, entry.UnderlyingStrikePrice);
		if (entry.UnderlyingOptAttribute is not null) context.Validators.UnderlyingOptAttribute(context, message, entry.UnderlyingOptAttribute);
		if (entry.UnderlyingContractMultiplier is not null) context.Validators.UnderlyingContractMultiplier(context, message, entry.UnderlyingContractMultiplier);
		if (entry.UnderlyingCouponRate is not null) context.Validators.UnderlyingCouponRate(context, message, entry.UnderlyingCouponRate);
		if (entry.UnderlyingSecurityExchange is not null) context.Validators.UnderlyingSecurityExchange(context, message, entry.UnderlyingSecurityExchange);
		if (entry.UnderlyingIssuer is not null) context.Validators.UnderlyingIssuer(context, message, entry.UnderlyingIssuer);
		if (entry.EncodedUnderlyingIssuerLen is not null) context.Validators.EncodedUnderlyingIssuerLen(context, message, entry.EncodedUnderlyingIssuerLen);
		if (entry.EncodedUnderlyingIssuer is not null) context.Validators.EncodedUnderlyingIssuer(context, message, entry.EncodedUnderlyingIssuer);
		if (entry.UnderlyingSecurityDesc is not null) context.Validators.UnderlyingSecurityDesc(context, message, entry.UnderlyingSecurityDesc);
		if (entry.EncodedUnderlyingSecurityDescLen is not null) context.Validators.EncodedUnderlyingSecurityDescLen(context, message, entry.EncodedUnderlyingSecurityDescLen);
		if (entry.EncodedUnderlyingSecurityDesc is not null) context.Validators.EncodedUnderlyingSecurityDesc(context, message, entry.EncodedUnderlyingSecurityDesc);
		if (entry.RatioQty is not null) context.Validators.RatioQty(context, message, entry.RatioQty);
		if (entry.Side is not null) context.Validators.Side(context, message, entry.Side);
		if (entry.UnderlyingCurrency is not null) context.Validators.UnderlyingCurrency(context, message, entry.UnderlyingCurrency);

		return message.IsValid;
	}

}
