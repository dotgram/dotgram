using System;

namespace DotGram.Finance.Fix;

/// <summary>The name FIX 4.4 gives each tag this package has a field class for, and the tag a name means.</summary>
/// <remarks>
/// A dictionary refers to a field by name, and a fragment of one may carry no field table of its
/// own, so the standard answers for it. One array, indexed by tag, and a map built from it the
/// first time a name is asked; both are the names the classes were written under.
/// </remarks>
static class FixNames
{
	/// <summary>The name of a tag, or null where this package has no field class for it.</summary>
	public static string? Name(int tag)
	{
		return (uint)tag < (uint)_names.Length ? _names[tag] : null;
	}

	/// <summary>One past the largest tag the standard defines.</summary>
	public static int Limit => _names.Length;

	/// <summary>The tag a name means, or zero where no field class of this package is called that.</summary>
	public static int Tag(string name)
	{
		return (tags ??= Index()).TryGetValue(name, out var tag) ? tag : 0;
	}

	/// <summary>The class this package gives a message type, or null where it has none.</summary>
	/// <remarks>
	/// A dictionary spells a type by whatever name its author chose — QuickFIX writes
	/// <c>IndicationOfInterest</c> where the repository and this package write <c>IndicationOfInterest</c> — and the
	/// MsgType is what the two agree on.
	/// </remarks>
	public static string? MessageName(string messageType)
	{
		return messageType switch
		{
			"0"   => "Heartbeat",
			"1"   => "TestRequest",
			"2"   => "ResendRequest",
			"3"   => "Reject",
			"4"   => "SequenceReset",
			"5"   => "Logout",
			"6"   => "IndicationOfInterest",
			"7"   => "Advertisement",
			"8"   => "ExecutionReport",
			"9"   => "OrderCancelReject",
			"A"   => "Logon",
			"AA"  => "DerivativeSecurityList",
			"AB"  => "NewOrderMultileg",
			"AC"  => "MultilegOrderCancelReplaceRequest",
			"AD"  => "TradeCaptureReportRequest",
			"AE"  => "TradeCaptureReport",
			"AF"  => "OrderMassStatusRequest",
			"AG"  => "QuoteRequestReject",
			"AH"  => "RFQRequest",
			"AI"  => "QuoteStatusReport",
			"AJ"  => "QuoteResponse",
			"AK"  => "Confirmation",
			"AL"  => "PositionMaintenanceRequest",
			"AM"  => "PositionMaintenanceReport",
			"AN"  => "RequestForPositions",
			"AO"  => "RequestForPositionsAck",
			"AP"  => "PositionReport",
			"AQ"  => "TradeCaptureReportRequestAck",
			"AR"  => "TradeCaptureReportAck",
			"AS"  => "AllocationReport",
			"AT"  => "AllocationReportAck",
			"AU"  => "ConfirmationAck",
			"AV"  => "SettlementInstructionRequest",
			"AW"  => "AssignmentReport",
			"AX"  => "CollateralRequest",
			"AY"  => "CollateralAssignment",
			"AZ"  => "CollateralResponse",
			"B"   => "News",
			"BA"  => "CollateralReport",
			"BB"  => "CollateralInquiry",
			"BC"  => "NetworkStatusRequest",
			"BD"  => "NetworkStatusResponse",
			"BE"  => "UserRequest",
			"BF"  => "UserResponse",
			"BG"  => "CollateralInquiryAck",
			"BH"  => "ConfirmationRequest",
			"C"   => "Email",
			"D"   => "NewOrderSingle",
			"E"   => "NewOrderList",
			"F"   => "OrderCancelRequest",
			"G"   => "OrderCancelReplaceRequest",
			"H"   => "OrderStatusRequest",
			"J"   => "AllocationInstruction",
			"K"   => "ListCancelRequest",
			"L"   => "ListExecute",
			"M"   => "ListStatusRequest",
			"N"   => "ListStatus",
			"P"   => "AllocationInstructionAck",
			"Q"   => "DontKnowTrade",
			"R"   => "QuoteRequest",
			"S"   => "Quote",
			"T"   => "SettlementInstructions",
			"V"   => "MarketDataRequest",
			"W"   => "MarketDataSnapshotFullRefresh",
			"X"   => "MarketDataIncrementalRefresh",
			"Y"   => "MarketDataRequestReject",
			"Z"   => "QuoteCancel",
			"a"   => "QuoteStatusRequest",
			"b"   => "MassQuoteAcknowledgement",
			"c"   => "SecurityDefinitionRequest",
			"d"   => "SecurityDefinition",
			"e"   => "SecurityStatusRequest",
			"f"   => "SecurityStatus",
			"g"   => "TradingSessionStatusRequest",
			"h"   => "TradingSessionStatus",
			"i"   => "MassQuote",
			"j"   => "BusinessMessageReject",
			"k"   => "BidRequest",
			"l"   => "BidResponse",
			"m"   => "ListStrikePrice",
			"n"   => "XMLnonFIX",
			"o"   => "RegistrationInstructions",
			"p"   => "RegistrationInstructionsResponse",
			"q"   => "OrderMassCancelRequest",
			"r"   => "OrderMassCancelReport",
			"s"   => "NewOrderCross",
			"t"   => "CrossOrderCancelReplaceRequest",
			"u"   => "CrossOrderCancelRequest",
			"v"   => "SecurityTypeRequest",
			"w"   => "SecurityTypes",
			"x"   => "SecurityListRequest",
			"y"   => "SecurityList",
			"z"   => "DerivativeSecurityListRequest",
			_    => null,
		};
	}

	static Dictionary<string, int>? tags;

	static Dictionary<string, int> Index()
	{
		var index = new Dictionary<string, int>(_names.Length, StringComparer.Ordinal);

		for (var tag = 1; tag < _names.Length; tag++)
			if (_names[tag] is { } name)
				index[name] = tag;

		return index;
	}

	static readonly string?[] _names =
	[
		null, "Account", "AdvId", "AdvRefID", "AdvSide", "AdvTransType", "AvgPx", "BeginSeqNo",
		"BeginString", "BodyLength", "CheckSum", "ClOrdID", "Commission", "CommType", "CumQty", "Currency",
		"EndSeqNo", "ExecID", "ExecInst", "ExecRefID", null, "HandlInst", "SecurityIDSource", "IOIid",
		null, "IOIQltyInd", "IOIRefID", "IOIQty", "IOITransType", "LastCapacity", "LastMkt", "LastPx",
		"LastQty", "LinesOfText", "MsgSeqNum", "MsgType", "NewSeqNo", "OrderID", "OrderQty", "OrdStatus",
		"OrdType", "OrigClOrdID", "OrigTime", "PossDupFlag", "Price", "RefSeqNum", null, null,
		"SecurityID", "SenderCompID", "SenderSubID", null, "SendingTime", "Quantity", "Side", "Symbol",
		"TargetCompID", "TargetSubID", "Text", "TimeInForce", "TransactTime", "Urgency", "ValidUntilTime", "SettlType",
		"SettlDate", "SymbolSfx", "ListID", "ListSeqNo", "TotNoOrders", "ListExecInst", "AllocID", "AllocTransType",
		"RefAllocID", "NoOrders", "AvgPxPrecision", "TradeDate", null, "PositionEffect", "NoAllocs", "AllocAccount",
		"AllocQty", "ProcessCode", "NoRpts", "RptSeq", "CxlQty", "NoDlvyInst", null, "AllocStatus",
		"AllocRejCode", "Signature", "SecureDataLen", "SecureData", null, "SignatureLength", "EmailType", "RawDataLength",
		"RawData", "PossResend", "EncryptMethod", "StopPx", "ExDestination", null, "CxlRejReason", "OrdRejReason",
		"IOIQualifier", null, "Issuer", "SecurityDesc", "HeartBtInt", null, "MinQty", "MaxFloor",
		"TestReqID", "ReportToExch", "LocateReqd", "OnBehalfOfCompID", "OnBehalfOfSubID", "QuoteID", "NetMoney", "SettlCurrAmt",
		"SettlCurrency", "ForexReq", "OrigSendingTime", "GapFillFlag", "NoExecs", null, "ExpireTime", "DKReason",
		"DeliverToCompID", "DeliverToSubID", "IOINaturalFlag", "QuoteReqID", "BidPx", "OfferPx", "BidSize", "OfferSize",
		"NoMiscFees", "MiscFeeAmt", "MiscFeeCurr", "MiscFeeType", "PrevClosePx", "ResetSeqNumFlag", "SenderLocationID", "TargetLocationID",
		"OnBehalfOfLocationID", "DeliverToLocationID", "NoRelatedSym", "Subject", "Headline", "URLLink", "ExecType", "LeavesQty",
		"CashOrderQty", "AllocAvgPx", "AllocNetMoney", "SettlCurrFxRate", "SettlCurrFxRateCalc", "NumDaysInterest", "AccruedInterestRate", "AccruedInterestAmt",
		"SettlInstMode", "AllocText", "SettlInstID", "SettlInstTransType", "EmailThreadID", "SettlInstSource", null, "SecurityType",
		"EffectiveTime", "StandInstDbType", "StandInstDbName", "StandInstDbID", "SettlDeliveryType", null, null, null,
		null, null, null, null, null, null, null, null,
		null, null, null, null, "BidSpotRate", "BidForwardPoints", "OfferSpotRate", "OfferForwardPoints",
		"OrderQty2", "SettlDate2", "LastSpotRate", "LastForwardPoints", "AllocLinkID", "AllocLinkType", "SecondaryOrderID", "NoIOIQualifiers",
		"MaturityMonthYear", "PutOrCall", "StrikePrice", "CoveredOrUncovered", null, null, "OptAttribute", "SecurityExchange",
		"NotifyBrokerOfCredit", "AllocHandlInst", "MaxShow", "PegOffsetValue", "XmlDataLen", "XmlData", "SettlInstRefID", "NoRoutingIDs",
		"RoutingType", "RoutingID", "Spread", null, "BenchmarkCurveCurrency", "BenchmarkCurveName", "BenchmarkCurvePoint", "CouponRate",
		"CouponPaymentDate", "IssueDate", "RepurchaseTerm", "RepurchaseRate", "Factor", "TradeOriginationDate", "ExDate", "ContractMultiplier",
		"NoStipulations", "StipulationType", "StipulationValue", "YieldType", "Yield", "TotalTakedown", "Concession", "RepoCollateralSecurityType",
		"RedemptionDate", "UnderlyingCouponPaymentDate", "UnderlyingIssueDate", "UnderlyingRepoCollateralSecurityType", "UnderlyingRepurchaseTerm", "UnderlyingRepurchaseRate", "UnderlyingFactor", "UnderlyingRedemptionDate",
		"LegCouponPaymentDate", "LegIssueDate", "LegRepoCollateralSecurityType", "LegRepurchaseTerm", "LegRepurchaseRate", "LegFactor", "LegRedemptionDate", "CreditRating",
		"UnderlyingCreditRating", "LegCreditRating", "TradedFlatSwitch", "BasisFeatureDate", "BasisFeaturePrice", null, "MDReqID", "SubscriptionRequestType",
		"MarketDepth", "MDUpdateType", "AggregatedBook", "NoMDEntryTypes", "NoMDEntries", "MDEntryType", "MDEntryPx", "MDEntrySize",
		"MDEntryDate", "MDEntryTime", "TickDirection", "MDMkt", "QuoteCondition", "TradeCondition", "MDEntryID", "MDUpdateAction",
		"MDEntryRefID", "MDReqRejReason", "MDEntryOriginator", "LocationID", "DeskID", "DeleteReason", "OpenCloseSettlFlag", "SellerDays",
		"MDEntryBuyer", "MDEntrySeller", "MDEntryPositionNo", "FinancialStatus", "CorporateAction", "DefBidSize", "DefOfferSize", "NoQuoteEntries",
		"NoQuoteSets", "QuoteStatus", "QuoteCancelType", "QuoteEntryID", "QuoteRejectReason", "QuoteResponseLevel", "QuoteSetID", "QuoteRequestType",
		"TotNoQuoteEntries", "UnderlyingSecurityIDSource", "UnderlyingIssuer", "UnderlyingSecurityDesc", "UnderlyingSecurityExchange", "UnderlyingSecurityID", "UnderlyingSecurityType", "UnderlyingSymbol",
		"UnderlyingSymbolSfx", "UnderlyingMaturityMonthYear", null, "UnderlyingPutOrCall", "UnderlyingStrikePrice", "UnderlyingOptAttribute", "UnderlyingCurrency", null,
		"SecurityReqID", "SecurityRequestType", "SecurityResponseID", "SecurityResponseType", "SecurityStatusReqID", "UnsolicitedIndicator", "SecurityTradingStatus", "HaltReason",
		"InViewOfCommon", "DueToRelated", "BuyVolume", "SellVolume", "HighPx", "LowPx", "Adjustment", "TradSesReqID",
		"TradingSessionID", "ContraTrader", "TradSesMethod", "TradSesMode", "TradSesStatus", "TradSesStartTime", "TradSesOpenTime", "TradSesPreCloseTime",
		"TradSesCloseTime", "TradSesEndTime", "NumberOfOrders", "MessageEncoding", "EncodedIssuerLen", "EncodedIssuer", "EncodedSecurityDescLen", "EncodedSecurityDesc",
		"EncodedListExecInstLen", "EncodedListExecInst", "EncodedTextLen", "EncodedText", "EncodedSubjectLen", "EncodedSubject", "EncodedHeadlineLen", "EncodedHeadline",
		"EncodedAllocTextLen", "EncodedAllocText", "EncodedUnderlyingIssuerLen", "EncodedUnderlyingIssuer", "EncodedUnderlyingSecurityDescLen", "EncodedUnderlyingSecurityDesc", "AllocPrice", "QuoteSetValidUntilTime",
		"QuoteEntryRejectReason", "LastMsgSeqNumProcessed", null, "RefTagID", "RefMsgType", "SessionRejectReason", "BidRequestTransType", "ContraBroker",
		"ComplianceID", "SolicitedFlag", "ExecRestatementReason", "BusinessRejectRefID", "BusinessRejectReason", "GrossTradeAmt", "NoContraBrokers", "MaxMessageSize",
		"NoMsgTypes", "MsgDirection", "NoTradingSessions", "TotalVolumeTraded", "DiscretionInst", "DiscretionOffsetValue", "BidID", "ClientBidID",
		"ListName", "TotNoRelatedSym", "BidType", "NumTickets", "SideValue1", "SideValue2", "NoBidDescriptors", "BidDescriptorType",
		"BidDescriptor", "SideValueInd", "LiquidityPctLow", "LiquidityPctHigh", "LiquidityValue", "EFPTrackingError", "FairValue", "OutsideIndexPct",
		"ValueOfFutures", "LiquidityIndType", "WtAverageLiquidity", "ExchangeForPhysical", "OutMainCntryUIndex", "CrossPercent", "ProgRptReqs", "ProgPeriodInterval",
		"IncTaxInd", "NumBidders", "BidTradeType", "BasisPxType", "NoBidComponents", "Country", "TotNoStrikes", "PriceType",
		"DayOrderQty", "DayCumQty", "DayAvgPx", "GTBookingInst", "NoStrikes", "ListStatusType", "NetGrossInd", "ListOrderStatus",
		"ExpireDate", "ListExecInstType", "CxlRejResponseTo", "UnderlyingCouponRate", "UnderlyingContractMultiplier", "ContraTradeQty", "ContraTradeTime", null,
		null, "LiquidityNumSecurities", "MultiLegReportingType", "StrikeTime", "ListStatusText", "EncodedListStatusTextLen", "EncodedListStatusText", "PartyIDSource",
		"PartyID", null, null, "NetChgPrevDay", "PartyRole", "NoPartyIDs", "NoSecurityAltID", "SecurityAltID",
		"SecurityAltIDSource", "NoUnderlyingSecurityAltID", "UnderlyingSecurityAltID", "UnderlyingSecurityAltIDSource", "Product", "CFICode", "UnderlyingProduct", "UnderlyingCFICode",
		"TestMessageIndicator", null, "BookingRefID", "IndividualAllocID", "RoundingDirection", "RoundingModulus", "CountryOfIssue", "StateOrProvinceOfIssue",
		"LocaleOfIssue", "NoRegistDtls", "MailingDtls", "InvestorCountryOfResidence", "PaymentRef", "DistribPaymentMethod", "CashDistribCurr", "CommCurrency",
		"CancellationRights", "MoneyLaunderingStatus", "MailingInst", "TransBkdTime", "ExecPriceType", "ExecPriceAdjustment", "DateOfBirth", "TradeReportTransType",
		"CardHolderName", "CardNumber", "CardExpDate", "CardIssNum", "PaymentMethod", "RegistAcctType", "Designation", "TaxAdvantageType",
		"RegistRejReasonText", "FundRenewWaiv", "CashDistribAgentName", "CashDistribAgentCode", "CashDistribAgentAcctNumber", "CashDistribPayRef", "CashDistribAgentAcctName", "CardStartDate",
		"PaymentDate", "PaymentRemitterID", "RegistStatus", "RegistRejReasonCode", "RegistRefID", "RegistDtls", "NoDistribInsts", "RegistEmail",
		"DistribPercentage", "RegistID", "RegistTransType", "ExecValuationPoint", "OrderPercent", "OwnershipType", "NoContAmts", "ContAmtType",
		"ContAmtValue", "ContAmtCurr", "OwnerType", "PartySubID", "NestedPartyID", "NestedPartyIDSource", "SecondaryClOrdID", "SecondaryExecID",
		"OrderCapacity", "OrderRestrictions", "MassCancelRequestType", "MassCancelResponse", "MassCancelRejectReason", "TotalAffectedOrders", "NoAffectedOrders", "AffectedOrderID",
		"AffectedSecondaryOrderID", "QuoteType", "NestedPartyRole", "NoNestedPartyIDs", "TotalAccruedInterestAmt", "MaturityDate", "UnderlyingMaturityDate", "InstrRegistry",
		"CashMargin", "NestedPartySubID", "Scope", "MDImplicitDelete", "CrossID", "CrossType", "CrossPrioritization", "OrigCrossID",
		"NoSides", "Username", "Password", "NoLegs", "LegCurrency", "TotNoSecurityTypes", "NoSecurityTypes", "SecurityListRequestType",
		"SecurityRequestResult", "RoundLot", "MinTradeVol", "MultiLegRptTypeReq", "LegPositionEffect", "LegCoveredOrUncovered", "LegPrice", "TradSesStatusRejReason",
		"TradeRequestID", "TradeRequestType", "PreviouslyReported", "TradeReportID", "TradeReportRefID", "MatchStatus", "MatchType", "OddLot",
		"NoClearingInstructions", "ClearingInstruction", "TradeInputSource", "TradeInputDevice", "NoDates", "AccountType", "CustOrderCapacity", "ClOrdLinkID",
		"MassStatusReqID", "MassStatusReqType", "OrigOrdModTime", "LegSettlType", "LegSettlDate", "DayBookingInst", "BookingUnit", "PreallocMethod",
		"UnderlyingCountryOfIssue", "UnderlyingStateOrProvinceOfIssue", "UnderlyingLocaleOfIssue", "UnderlyingInstrRegistry", "LegCountryOfIssue", "LegStateOrProvinceOfIssue", "LegLocaleOfIssue", "LegInstrRegistry",
		"LegSymbol", "LegSymbolSfx", "LegSecurityID", "LegSecurityIDSource", "NoLegSecurityAltID", "LegSecurityAltID", "LegSecurityAltIDSource", "LegProduct",
		"LegCFICode", "LegSecurityType", "LegMaturityMonthYear", "LegMaturityDate", "LegStrikePrice", "LegOptAttribute", "LegContractMultiplier", "LegCouponRate",
		"LegSecurityExchange", "LegIssuer", "EncodedLegIssuerLen", "EncodedLegIssuer", "LegSecurityDesc", "EncodedLegSecurityDescLen", "EncodedLegSecurityDesc", "LegRatioQty",
		"LegSide", "TradingSessionSubID", "AllocType", "NoHops", "HopCompID", "HopSendingTime", "HopRefID", "MidPx",
		"BidYield", "MidYield", "OfferYield", "ClearingFeeIndicator", "WorkingIndicator", "LegLastPx", "PriorityIndicator", "PriceImprovement",
		"Price2", "LastForwardPoints2", "BidForwardPoints2", "OfferForwardPoints2", "RFQReqID", "MktBidPx", "MktOfferPx", "MinBidSize",
		"MinOfferSize", "QuoteStatusReqID", "LegalConfirm", "UnderlyingLastPx", "UnderlyingLastQty", null, "LegRefID", "ContraLegRefID",
		"SettlCurrBidFxRate", "SettlCurrOfferFxRate", "QuoteRequestRejectReason", "SideComplianceID", "AcctIDSource", "AllocAcctIDSource", "BenchmarkPrice", "BenchmarkPriceType",
		"ConfirmID", "ConfirmStatus", "ConfirmTransType", "ContractSettlMonth", "DeliveryForm", "LastParPx", "NoLegAllocs", "LegAllocAccount",
		"LegIndividualAllocID", "LegAllocQty", "LegAllocAcctIDSource", "LegSettlCurrency", "LegBenchmarkCurveCurrency", "LegBenchmarkCurveName", "LegBenchmarkCurvePoint", "LegBenchmarkPrice",
		"LegBenchmarkPriceType", "LegBidPx", "LegIOIQty", "NoLegStipulations", "LegOfferPx", null, "LegPriceType", "LegQty",
		"LegStipulationType", "LegStipulationValue", "LegSwapType", "Pool", "QuotePriceType", "QuoteRespID", "QuoteRespType", "QuoteQualifier",
		"YieldRedemptionDate", "YieldRedemptionPrice", "YieldRedemptionPriceType", "BenchmarkSecurityID", "ReversalIndicator", "YieldCalcDate", "NoPositions", "PosType",
		"LongQty", "ShortQty", "PosQtyStatus", "PosAmtType", "PosAmt", "PosTransType", "PosReqID", "NoUnderlyings",
		"PosMaintAction", "OrigPosReqRefID", "PosMaintRptRefID", "ClearingBusinessDate", "SettlSessID", "SettlSessSubID", "AdjustmentType", "ContraryInstructionIndicator",
		"PriorSpreadIndicator", "PosMaintRptID", "PosMaintStatus", "PosMaintResult", "PosReqType", "ResponseTransportType", "ResponseDestination", "TotalNumPosReports",
		"PosReqResult", "PosReqStatus", "SettlPrice", "SettlPriceType", "UnderlyingSettlPrice", "UnderlyingSettlPriceType", "PriorSettlPrice", "NoQuoteQualifiers",
		"AllocSettlCurrency", "AllocSettlCurrAmt", "InterestAtMaturity", "LegDatedDate", "LegPool", "AllocInterestAtMaturity", "AllocAccruedInterestAmt", "DeliveryDate",
		"AssignmentMethod", "AssignmentUnit", "OpenInterest", "ExerciseMethod", "TotNumTradeReports", "TradeRequestResult", "TradeRequestStatus", "TradeReportRejectReason",
		"SideMultiLegReportingType", "NoPosAmt", "AutoAcceptIndicator", "AllocReportID", "NoNested2PartyIDs", "Nested2PartyID", "Nested2PartyIDSource", "Nested2PartyRole",
		"Nested2PartySubID", "BenchmarkSecurityIDSource", "SecuritySubType", "UnderlyingSecuritySubType", "LegSecuritySubType", "AllowableOneSidednessPct", "AllowableOneSidednessValue", "AllowableOneSidednessCurr",
		"NoTrdRegTimestamps", "TrdRegTimestamp", "TrdRegTimestampType", "TrdRegTimestampOrigin", "ConfirmRefID", "ConfirmType", "ConfirmRejReason", "BookingType",
		"IndividualAllocRejCode", "SettlInstMsgID", "NoSettlInst", "LastUpdateTime", "AllocSettlInstType", "NoSettlPartyIDs", "SettlPartyID", "SettlPartyIDSource",
		"SettlPartyRole", "SettlPartySubID", "SettlPartySubIDType", "DlvyInstType", "TerminationType", "NextExpectedMsgSeqNum", "OrdStatusReqID", "SettlInstReqID",
		"SettlInstReqRejCode", "SecondaryAllocID", "AllocReportType", "AllocReportRefID", "AllocCancReplaceReason", "CopyMsgIndicator", "AllocAccountType", "OrderAvgPx",
		"OrderBookingQty", "NoSettlPartySubIDs", "NoPartySubIDs", "PartySubIDType", "NoNestedPartySubIDs", "NestedPartySubIDType", "NoNested2PartySubIDs", "Nested2PartySubIDType",
		"AllocIntermedReqType", null, "UnderlyingPx", "PriceDelta", "ApplQueueMax", "ApplQueueDepth", "ApplQueueResolution", "ApplQueueAction",
		"NoAltMDSource", "AltMDSourceID", "SecondaryTradeReportID", "AvgPxIndicator", "TradeLinkID", "OrderInputDevice", "UnderlyingTradingSessionID", "UnderlyingTradingSessionSubID",
		"TradeLegRefID", "ExchangeRule", "TradeAllocIndicator", "ExpirationCycle", "TrdType", "TrdSubType", "TransferReason", null,
		"TotNumAssignmentReports", "AsgnRptID", "ThresholdAmount", "PegMoveType", "PegOffsetType", "PegLimitType", "PegRoundDirection", "PeggedPrice",
		"PegScope", "DiscretionMoveType", "DiscretionOffsetType", "DiscretionLimitType", "DiscretionRoundDirection", "DiscretionPrice", "DiscretionScope", "TargetStrategy",
		"TargetStrategyParameters", "ParticipationRate", "TargetStrategyPerformance", "LastLiquidityInd", "PublishTrdIndicator", "ShortSaleReason", "QtyType", "SecondaryTrdType",
		"TradeReportType", "AllocNoOrdersType", "SharedCommission", "ConfirmReqID", "AvgParPx", "ReportedPx", "NoCapacities", "OrderCapacityQty",
		"NoEvents", "EventType", "EventDate", "EventPx", "EventText", "PctAtRisk", "NoInstrAttrib", "InstrAttribType",
		"InstrAttribValue", "DatedDate", "InterestAccrualDate", "CPProgram", "CPRegType", "UnderlyingCPProgram", "UnderlyingCPRegType", "UnderlyingQty",
		"TrdMatchID", "SecondaryTradeReportRefID", "UnderlyingDirtyPrice", "UnderlyingEndPrice", "UnderlyingStartValue", "UnderlyingCurrentValue", "UnderlyingEndValue", "NoUnderlyingStips",
		"UnderlyingStipType", "UnderlyingStipValue", "MaturityNetMoney", "MiscFeeBasis", "TotNoAllocs", "LastFragment", "CollReqID", "CollAsgnReason",
		"CollInquiryQualifier", "NoTrades", "MarginRatio", "MarginExcess", "TotalNetValue", "CashOutstanding", "CollAsgnID", "CollAsgnTransType",
		"CollRespID", "CollAsgnRespType", "CollAsgnRejectReason", "CollAsgnRefID", "CollRptID", "CollInquiryID", "CollStatus", "TotNumReports",
		"LastRptRequested", "AgreementDesc", "AgreementID", "AgreementDate", "StartDate", "EndDate", "AgreementCurrency", "DeliveryType",
		"EndAccruedInterestAmt", "StartCash", "EndCash", "UserRequestID", "UserRequestType", "NewPassword", "UserStatus", "UserStatusText",
		"StatusValue", "StatusText", "RefCompID", "RefSubID", "NetworkResponseID", "NetworkRequestID", "LastNetworkResponseID", "NetworkRequestType",
		"NoCompIDs", "NetworkStatusResponseType", "NoCollInquiryQualifier", "TrdRptStatus", "AffirmStatus", "UnderlyingStrikeCurrency", "LegStrikeCurrency", "TimeBracket",
		"CollAction", "CollInquiryStatus", "CollInquiryResult", "StrikeCurrency", "NoNested3PartyIDs", "Nested3PartyID", "Nested3PartyIDSource", "Nested3PartyRole",
		"NoNested3PartySubIDs", "Nested3PartySubID", "Nested3PartySubIDType", "LegContractSettlMonth", "LegInterestAccrualDate",
	];
}
