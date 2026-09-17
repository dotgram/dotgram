using System;

namespace DotGram.Finance.Fix;

static class FixSchema
{
	// CommissionData
	static readonly SchemaRef[] C1000 =
	[
		new SchemaRef(12,  false, 0), // Commission
		new SchemaRef(13,  false, 0), // CommType
		new SchemaRef(479, false, 0), // CommCurrency
		new SchemaRef(497, false, 0)  // FundRenewWaiv
	];

	// DiscretionInstructions
	static readonly SchemaRef[] C1001 =
	[
		new SchemaRef(388, false, 0), // DiscretionInst
		new SchemaRef(389, false, 0), // DiscretionOffsetValue
		new SchemaRef(841, false, 0), // DiscretionMoveType
		new SchemaRef(842, false, 0), // DiscretionOffsetType
		new SchemaRef(843, false, 0), // DiscretionLimitType
		new SchemaRef(844, false, 0), // DiscretionRoundDirection
		new SchemaRef(846, false, 0), // DiscretionScope
	];

	// FinancingDetails
	static readonly SchemaRef[] C1002 =
	[
		new SchemaRef(913, false, 0), // AgreementDesc
		new SchemaRef(914, false, 0), // AgreementID
		new SchemaRef(915, false, 0), // AgreementDate
		new SchemaRef(918, false, 0), // AgreementCurrency
		new SchemaRef(788, false, 0), // TerminationType
		new SchemaRef(916, false, 0), // StartDate
		new SchemaRef(917, false, 0), // EndDate
		new SchemaRef(919, false, 0), // DeliveryType
		new SchemaRef(898, false, 0), // MarginRatio
	];

	// Instrument
	static readonly SchemaRef[] C1003 =
	[
		new SchemaRef(55, false, 0), // Symbol
		new SchemaRef(65, false, 0), // SymbolSfx
		new SchemaRef(48, false, 0), // SecurityID
		new SchemaRef(22, false, 0), // SecurityIDSource
		new SchemaRef(2071, false, 2), // SecAltIDGrp
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(461, false, 0), // CFICode
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(762, false, 0), // SecuritySubType
		new SchemaRef(200, false, 0), // MaturityMonthYear
		new SchemaRef(541, false, 0), // MaturityDate
		new SchemaRef(201, false, 0), // PutOrCall
		new SchemaRef(224, false, 0), // CouponPaymentDate
		new SchemaRef(225, false, 0), // IssueDate
		new SchemaRef(239, false, 0), // RepoCollateralSecurityType
		new SchemaRef(226, false, 0), // RepurchaseTerm
		new SchemaRef(227, false, 0), // RepurchaseRate
		new SchemaRef(228, false, 0), // Factor
		new SchemaRef(255, false, 0), // CreditRating
		new SchemaRef(543, false, 0), // InstrRegistry
		new SchemaRef(470, false, 0), // CountryOfIssue
		new SchemaRef(471, false, 0), // StateOrProvinceOfIssue
		new SchemaRef(472, false, 0), // LocaleOfIssue
		new SchemaRef(240, false, 0), // RedemptionDate
		new SchemaRef(202, false, 0), // StrikePrice
		new SchemaRef(947, false, 0), // StrikeCurrency
		new SchemaRef(206, false, 0), // OptAttribute
		new SchemaRef(231, false, 0), // ContractMultiplier
		new SchemaRef(223, false, 0), // CouponRate
		new SchemaRef(207, false, 0), // SecurityExchange
		new SchemaRef(106, false, 0), // Issuer
		new SchemaRef(348, false, 0), // EncodedIssuerLen
		new SchemaRef(349, false, 0), // EncodedIssuer
		new SchemaRef(107, false, 0), // SecurityDesc
		new SchemaRef(350, false, 0), // EncodedSecurityDescLen
		new SchemaRef(351, false, 0), // EncodedSecurityDesc
		new SchemaRef(691, false, 0), // Pool
		new SchemaRef(667, false, 0), // ContractSettlMonth
		new SchemaRef(875, false, 0), // CPProgram
		new SchemaRef(876, false, 0), // CPRegType
		new SchemaRef(2070, false, 2), // EvntGrp
		new SchemaRef(873, false, 0), // DatedDate
		new SchemaRef(874, false, 0), // InterestAccrualDate
	];

	// InstrumentExtension
	static readonly SchemaRef[] C1004 =
	[
		new SchemaRef(668, false, 0), // DeliveryForm
		new SchemaRef(869, false, 0), // PctAtRisk
		new SchemaRef(2074, false, 2), // AttrbGrp
	];

	// InstrumentLeg
	static readonly SchemaRef[] C1005 =
	[
		new SchemaRef(600, false, 0), // LegSymbol
		new SchemaRef(601, false, 0), // LegSymbolSfx
		new SchemaRef(602, false, 0), // LegSecurityID
		new SchemaRef(603, false, 0), // LegSecurityIDSource
		new SchemaRef(2072, false, 2), // LegSecAltIDGrp
		new SchemaRef(607, false, 0), // LegProduct
		new SchemaRef(608, false, 0), // LegCFICode
		new SchemaRef(609, false, 0), // LegSecurityType
		new SchemaRef(764, false, 0), // LegSecuritySubType
		new SchemaRef(610, false, 0), // LegMaturityMonthYear
		new SchemaRef(611, false, 0), // LegMaturityDate
		new SchemaRef(248, false, 0), // LegCouponPaymentDate
		new SchemaRef(249, false, 0), // LegIssueDate
		new SchemaRef(250, false, 0), // LegRepoCollateralSecurityType
		new SchemaRef(251, false, 0), // LegRepurchaseTerm
		new SchemaRef(252, false, 0), // LegRepurchaseRate
		new SchemaRef(253, false, 0), // LegFactor
		new SchemaRef(257, false, 0), // LegCreditRating
		new SchemaRef(599, false, 0), // LegInstrRegistry
		new SchemaRef(596, false, 0), // LegCountryOfIssue
		new SchemaRef(597, false, 0), // LegStateOrProvinceOfIssue
		new SchemaRef(598, false, 0), // LegLocaleOfIssue
		new SchemaRef(254, false, 0), // LegRedemptionDate
		new SchemaRef(612, false, 0), // LegStrikePrice
		new SchemaRef(942, false, 0), // LegStrikeCurrency
		new SchemaRef(613, false, 0), // LegOptAttribute
		new SchemaRef(614, false, 0), // LegContractMultiplier
		new SchemaRef(615, false, 0), // LegCouponRate
		new SchemaRef(616, false, 0), // LegSecurityExchange
		new SchemaRef(617, false, 0), // LegIssuer
		new SchemaRef(618, false, 0), // EncodedLegIssuerLen
		new SchemaRef(619, false, 0), // EncodedLegIssuer
		new SchemaRef(620, false, 0), // LegSecurityDesc
		new SchemaRef(621, false, 0), // EncodedLegSecurityDescLen
		new SchemaRef(622, false, 0), // EncodedLegSecurityDesc
		new SchemaRef(623, false, 0), // LegRatioQty
		new SchemaRef(624, false, 0), // LegSide
		new SchemaRef(556, false, 0), // LegCurrency
		new SchemaRef(740, false, 0), // LegPool
		new SchemaRef(739, false, 0), // LegDatedDate
		new SchemaRef(955, false, 0), // LegContractSettlMonth
		new SchemaRef(956, false, 0), // LegInterestAccrualDate
	];

	// LegBenchmarkCurveData
	static readonly SchemaRef[] C1006 =
	[
		new SchemaRef(676, false, 0), // LegBenchmarkCurveCurrency
		new SchemaRef(677, false, 0), // LegBenchmarkCurveName
		new SchemaRef(678, false, 0), // LegBenchmarkCurvePoint
		new SchemaRef(679, false, 0), // LegBenchmarkPrice
		new SchemaRef(680, false, 0), // LegBenchmarkPriceType
	];

	// OrderQtyData
	static readonly SchemaRef[] C1011 =
	[
		new SchemaRef(38, false, 0), // OrderQty
		new SchemaRef(152, false, 0), // CashOrderQty
		new SchemaRef(516, false, 0), // OrderPercent
		new SchemaRef(468, false, 0), // RoundingDirection
		new SchemaRef(469, false, 0), // RoundingModulus
	];

	// PegInstructions
	static readonly SchemaRef[] C1013 =
	[
		new SchemaRef(211, false, 0), // PegOffsetValue
		new SchemaRef(835, false, 0), // PegMoveType
		new SchemaRef(836, false, 0), // PegOffsetType
		new SchemaRef(837, false, 0), // PegLimitType
		new SchemaRef(838, false, 0), // PegRoundDirection
		new SchemaRef(840, false, 0), // PegScope
	];

	// SettlInstructionsData
	static readonly SchemaRef[] C1016 =
	[
		new SchemaRef(172, false, 0), // SettlDeliveryType
		new SchemaRef(169, false, 0), // StandInstDbType
		new SchemaRef(170, false, 0), // StandInstDbName
		new SchemaRef(171, false, 0), // StandInstDbID
		new SchemaRef(2075, false, 2), // DlvyInstGrp
	];

	// SpreadOrBenchmarkCurveData
	static readonly SchemaRef[] C1018 =
	[
		new SchemaRef(218, false, 0), // Spread
		new SchemaRef(220, false, 0), // BenchmarkCurveCurrency
		new SchemaRef(221, false, 0), // BenchmarkCurveName
		new SchemaRef(222, false, 0), // BenchmarkCurvePoint
		new SchemaRef(662, false, 0), // BenchmarkPrice
		new SchemaRef(663, false, 0), // BenchmarkPriceType
		new SchemaRef(699, false, 0), // BenchmarkSecurityID
		new SchemaRef(761, false, 0), // BenchmarkSecurityIDSource
	];

	// UnderlyingInstrument
	static readonly SchemaRef[] C1021 =
	[
		new SchemaRef(311, false, 0), // UnderlyingSymbol
		new SchemaRef(312, false, 0), // UnderlyingSymbolSfx
		new SchemaRef(309, false, 0), // UnderlyingSecurityID
		new SchemaRef(305, false, 0), // UnderlyingSecurityIDSource
		new SchemaRef(2073, false, 2), // UndSecAltIDGrp
		new SchemaRef(462, false, 0), // UnderlyingProduct
		new SchemaRef(463, false, 0), // UnderlyingCFICode
		new SchemaRef(310, false, 0), // UnderlyingSecurityType
		new SchemaRef(763, false, 0), // UnderlyingSecuritySubType
		new SchemaRef(313, false, 0), // UnderlyingMaturityMonthYear
		new SchemaRef(542, false, 0), // UnderlyingMaturityDate
		new SchemaRef(315, false, 0), // UnderlyingPutOrCall
		new SchemaRef(241, false, 0), // UnderlyingCouponPaymentDate
		new SchemaRef(242, false, 0), // UnderlyingIssueDate
		new SchemaRef(243, false, 0), // UnderlyingRepoCollateralSecurityType
		new SchemaRef(244, false, 0), // UnderlyingRepurchaseTerm
		new SchemaRef(245, false, 0), // UnderlyingRepurchaseRate
		new SchemaRef(246, false, 0), // UnderlyingFactor
		new SchemaRef(256, false, 0), // UnderlyingCreditRating
		new SchemaRef(595, false, 0), // UnderlyingInstrRegistry
		new SchemaRef(592, false, 0), // UnderlyingCountryOfIssue
		new SchemaRef(593, false, 0), // UnderlyingStateOrProvinceOfIssue
		new SchemaRef(594, false, 0), // UnderlyingLocaleOfIssue
		new SchemaRef(247, false, 0), // UnderlyingRedemptionDate
		new SchemaRef(316, false, 0), // UnderlyingStrikePrice
		new SchemaRef(941, false, 0), // UnderlyingStrikeCurrency
		new SchemaRef(317, false, 0), // UnderlyingOptAttribute
		new SchemaRef(436, false, 0), // UnderlyingContractMultiplier
		new SchemaRef(435, false, 0), // UnderlyingCouponRate
		new SchemaRef(308, false, 0), // UnderlyingSecurityExchange
		new SchemaRef(306, false, 0), // UnderlyingIssuer
		new SchemaRef(362, false, 0), // EncodedUnderlyingIssuerLen
		new SchemaRef(363, false, 0), // EncodedUnderlyingIssuer
		new SchemaRef(307, false, 0), // UnderlyingSecurityDesc
		new SchemaRef(364, false, 0), // EncodedUnderlyingSecurityDescLen
		new SchemaRef(365, false, 0), // EncodedUnderlyingSecurityDesc
		new SchemaRef(877, false, 0), // UnderlyingCPProgram
		new SchemaRef(878, false, 0), // UnderlyingCPRegType
		new SchemaRef(318, false, 0), // UnderlyingCurrency
		new SchemaRef(879, false, 0), // UnderlyingQty
		new SchemaRef(810, false, 0), // UnderlyingPx
		new SchemaRef(882, false, 0), // UnderlyingDirtyPrice
		new SchemaRef(883, false, 0), // UnderlyingEndPrice
		new SchemaRef(884, false, 0), // UnderlyingStartValue
		new SchemaRef(885, false, 0), // UnderlyingCurrentValue
		new SchemaRef(886, false, 0), // UnderlyingEndValue
		new SchemaRef(1023, false, 2), // UnderlyingStipulations
	];

	// YieldData
	static readonly SchemaRef[] C1022 =
	[
		new SchemaRef(235, false, 0), // YieldType
		new SchemaRef(236, false, 0), // Yield
		new SchemaRef(701, false, 0), // YieldCalcDate
		new SchemaRef(696, false, 0), // YieldRedemptionDate
		new SchemaRef(697, false, 0), // YieldRedemptionPrice
		new SchemaRef(698, false, 0), // YieldRedemptionPriceType
	];

	// StandardHeader
	static readonly SchemaRef[] C1024 =
	[
		new SchemaRef(   8, true,  0), // BeginString
		new SchemaRef(   9, true,  0), // BodyLength
		new SchemaRef(  35, true,  0), // MsgType
		new SchemaRef(  49, true,  0), // SenderCompID
		new SchemaRef(  56, true,  0), // TargetCompID
		new SchemaRef( 115, false, 0), // OnBehalfOfCompID
		new SchemaRef( 128, false, 0), // DeliverToCompID
		new SchemaRef(  90, false, 0), // SecureDataLen
		new SchemaRef(  91, false, 0), // SecureData
		new SchemaRef(  34, true,  0), // MsgSeqNum
		new SchemaRef(  50, false, 0), // SenderSubID
		new SchemaRef( 142, false, 0), // SenderLocationID
		new SchemaRef(  57, false, 0), // TargetSubID
		new SchemaRef( 143, false, 0), // TargetLocationID
		new SchemaRef( 116, false, 0), // OnBehalfOfSubID
		new SchemaRef( 144, false, 0), // OnBehalfOfLocationID
		new SchemaRef( 129, false, 0), // DeliverToSubID
		new SchemaRef( 145, false, 0), // DeliverToLocationID
		new SchemaRef(  43, false, 0), // PossDupFlag
		new SchemaRef(  97, false, 0), // PossResend
		new SchemaRef(  52, true,  0), // SendingTime
		new SchemaRef( 122, false, 0), // OrigSendingTime
		new SchemaRef( 212, false, 0), // XmlDataLen
		new SchemaRef( 213, false, 0), // XmlData
		new SchemaRef( 347, false, 0), // MessageEncoding
		new SchemaRef( 369, false, 0), // LastMsgSeqNumProcessed
		new SchemaRef(2085, false, 2), // HopGrp
	];

	// StandardTrailer
	static readonly SchemaRef[] C1025 =
	[
		new SchemaRef(93, false, 0), // SignatureLength
		new SchemaRef(89, false, 0), // Signature
		new SchemaRef(10, true, 0), // CheckSum
	];

	// LegStipulations
	static readonly SchemaRef[] G1007 =
	[
		new SchemaRef(688, false, 0), // LegStipulationType
		new SchemaRef(689, false, 0), // LegStipulationValue
	];

	// NestedParties
	static readonly SchemaRef[] G1008 =
	[
		new SchemaRef(524, false, 0), // NestedPartyID
		new SchemaRef(525, false, 0), // NestedPartyIDSource
		new SchemaRef(538, false, 0), // NestedPartyRole
		new SchemaRef(2078, false, 2), // NstdPtysSubGrp
	];

	// NestedParties2
	static readonly SchemaRef[] G1009 =
	[
		new SchemaRef(757, false, 0), // Nested2PartyID
		new SchemaRef(758, false, 0), // Nested2PartyIDSource
		new SchemaRef(759, false, 0), // Nested2PartyRole
		new SchemaRef(2079, false, 2), // NstdPtys2SubGrp
	];

	// NestedParties3
	static readonly SchemaRef[] G1010 =
	[
		new SchemaRef(949, false, 0), // Nested3PartyID
		new SchemaRef(950, false, 0), // Nested3PartyIDSource
		new SchemaRef(951, false, 0), // Nested3PartyRole
		new SchemaRef(2080, false, 2), // NstdPtys3SubGrp
	];

	// Parties
	static readonly SchemaRef[] G1012 =
	[
		new SchemaRef( 448, false, 0), // PartyID
		new SchemaRef( 447, false, 0), // PartyIDSource
		new SchemaRef( 452, false, 0), // PartyRole
		new SchemaRef(2077, false, 2), // PtysSubGrp
	];

	// PositionAmountData
	static readonly SchemaRef[] G1014 =
	[
		new SchemaRef(707, false, 0), // PosAmtType
		new SchemaRef(708, false, 0), // PosAmt
	];

	// PositionQty
	static readonly SchemaRef[] G1015 =
	[
		new SchemaRef( 703, false, 0), // PosType
		new SchemaRef( 704, false, 0), // LongQty
		new SchemaRef( 705, false, 0), // ShortQty
		new SchemaRef( 706, false, 0), // PosQtyStatus
		new SchemaRef(1008, false, 2), // NestedParties
	];

	// SettlParties
	static readonly SchemaRef[] G1017 =
	[
		new SchemaRef( 782, false, 0), // SettlPartyID
		new SchemaRef( 783, false, 0), // SettlPartyIDSource
		new SchemaRef( 784, false, 0), // SettlPartyRole
		new SchemaRef(2076, false, 2), // SettlPtysSubGrp
	];

	// Stipulations
	static readonly SchemaRef[] G1019 =
	[
		new SchemaRef(233, false, 0), // StipulationType
		new SchemaRef(234, false, 0), // StipulationValue
	];

	// TrdRegTimestamps
	static readonly SchemaRef[] G1020 =
	[
		new SchemaRef(769, false, 0), // TrdRegTimestamp
		new SchemaRef(770, false, 0), // TrdRegTimestampType
		new SchemaRef(771, false, 0), // TrdRegTimestampOrigin
	];

	// UnderlyingStipulations
	static readonly SchemaRef[] G1023 =
	[
		new SchemaRef(888, false, 0), // UnderlyingStipType
		new SchemaRef(889, false, 0), // UnderlyingStipValue
	];

	// AffectedOrdGrp
	static readonly SchemaRef[] G2001 =
	[
		new SchemaRef(41, false, 0), // OrigClOrdID
		new SchemaRef(535, false, 0), // AffectedOrderID
		new SchemaRef(536, false, 0), // AffectedSecondaryOrderID
	];

	// AllocAckGrp
	static readonly SchemaRef[] G2002 =
	[
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(366, false, 0), // AllocPrice
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(776, false, 0), // IndividualAllocRejCode
		new SchemaRef(161, false, 0), // AllocText
		new SchemaRef(360, false, 0), // EncodedAllocTextLen
		new SchemaRef(361, false, 0), // EncodedAllocText
	];

	// AllocGrp
	static readonly SchemaRef[] G2003 =
	[
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(366, false, 0), // AllocPrice
		new SchemaRef(80, false, 0), // AllocQty
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(208, false, 0), // NotifyBrokerOfCredit
		new SchemaRef(209, false, 0), // AllocHandlInst
		new SchemaRef(161, false, 0), // AllocText
		new SchemaRef(360, false, 0), // EncodedAllocTextLen
		new SchemaRef(361, false, 0), // EncodedAllocText
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(153, false, 0), // AllocAvgPx
		new SchemaRef(154, false, 0), // AllocNetMoney
		new SchemaRef(119, false, 0), // SettlCurrAmt
		new SchemaRef(737, false, 0), // AllocSettlCurrAmt
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(736, false, 0), // AllocSettlCurrency
		new SchemaRef(155, false, 0), // SettlCurrFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(742, false, 0), // AllocAccruedInterestAmt
		new SchemaRef(741, false, 0), // AllocInterestAtMaturity
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(2007, false, 2), // ClrInstGrp
		new SchemaRef(780, false, 0), // AllocSettlInstType
		new SchemaRef(1016, false, 1), // SettlInstructionsData
	];

	// BidCompReqGrp
	static readonly SchemaRef[] G2004 =
	[
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(430, false, 0), // NetGrossInd
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
	];

	// BidCompRspGrp
	static readonly SchemaRef[] G2005 =
	[
		new SchemaRef(1000, true, 1), // CommissionData
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(421, false, 0), // Country
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(406, false, 0), // FairValue
		new SchemaRef(430, false, 0), // NetGrossInd
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// BidDescReqGrp
	static readonly SchemaRef[] G2006 =
	[
		new SchemaRef(399, false, 0), // BidDescriptorType
		new SchemaRef(400, false, 0), // BidDescriptor
		new SchemaRef(401, false, 0), // SideValueInd
		new SchemaRef(404, false, 0), // LiquidityValue
		new SchemaRef(441, false, 0), // LiquidityNumSecurities
		new SchemaRef(402, false, 0), // LiquidityPctLow
		new SchemaRef(403, false, 0), // LiquidityPctHigh
		new SchemaRef(405, false, 0), // EFPTrackingError
		new SchemaRef(406, false, 0), // FairValue
		new SchemaRef(407, false, 0), // OutsideIndexPct
		new SchemaRef(408, false, 0), // ValueOfFutures
	];

	// ClrInstGrp
	static readonly SchemaRef[] G2007 =
	[
		new SchemaRef(577, false, 0), // ClearingInstruction
	];

	// CollInqQualGrp
	static readonly SchemaRef[] G2008 =
	[
		new SchemaRef(896, false, 0), // CollInquiryQualifier
	];

	// CompIDReqGrp
	static readonly SchemaRef[] G2009 =
	[
		new SchemaRef(930, false, 0), // RefCompID
		new SchemaRef(931, false, 0), // RefSubID
		new SchemaRef(283, false, 0), // LocationID
		new SchemaRef(284, false, 0), // DeskID
	];

	// CompIDStatGrp
	static readonly SchemaRef[] G2010 =
	[
		new SchemaRef(930, false, 0), // RefCompID
		new SchemaRef(931, false, 0), // RefSubID
		new SchemaRef(283, false, 0), // LocationID
		new SchemaRef(284, false, 0), // DeskID
		new SchemaRef(928, false, 0), // StatusValue
		new SchemaRef(929, false, 0), // StatusText
	];

	// ContAmtGrp
	static readonly SchemaRef[] G2011 =
	[
		new SchemaRef(519, false, 0), // ContAmtType
		new SchemaRef(520, false, 0), // ContAmtValue
		new SchemaRef(521, false, 0), // ContAmtCurr
	];

	// ContraGrp
	static readonly SchemaRef[] G2012 =
	[
		new SchemaRef(375, false, 0), // ContraBroker
		new SchemaRef(337, false, 0), // ContraTrader
		new SchemaRef(437, false, 0), // ContraTradeQty
		new SchemaRef(438, false, 0), // ContraTradeTime
		new SchemaRef(655, false, 0), // ContraLegRefID
	];

	// CpctyConfGrp
	static readonly SchemaRef[] G2013 =
	[
		new SchemaRef(528, true, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(863, true, 0), // OrderCapacityQty
	];

	// ExecAllocGrp
	static readonly SchemaRef[] G2014 =
	[
		new SchemaRef(32, false, 0), // LastQty
		new SchemaRef(17, false, 0), // ExecID
		new SchemaRef(527, false, 0), // SecondaryExecID
		new SchemaRef(31, false, 0), // LastPx
		new SchemaRef(669, false, 0), // LastParPx
		new SchemaRef(29, false, 0), // LastCapacity
	];

	// ExecCollGrp
	static readonly SchemaRef[] G2015 =
	[
		new SchemaRef(17, false, 0), // ExecID
	];

	// ExecsGrp
	static readonly SchemaRef[] G2016 =
	[
		new SchemaRef(17, false, 0), // ExecID
	];

	// InstrmtGrp
	static readonly SchemaRef[] G2017 =
	[
		new SchemaRef(1003, false, 1), // Instrument
	];

	// InstrmtLegExecGrp
	static readonly SchemaRef[] G2018 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(564, false, 0), // LegPositionEffect
		new SchemaRef(565, false, 0), // LegCoveredOrUncovered
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(654, false, 0), // LegRefID
		new SchemaRef(566, false, 0), // LegPrice
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
		new SchemaRef(637, false, 0), // LegLastPx
	];

	// InstrmtLegGrp
	static readonly SchemaRef[] G2019 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
	];

	// InstrmtLegIOIGrp
	static readonly SchemaRef[] G2020 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(682, false, 0), // LegIOIQty
		new SchemaRef(1007, false, 2), // LegStipulations
	];

	// InstrmtLegSecListGrp
	static readonly SchemaRef[] G2021 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(1006, false, 1), // LegBenchmarkCurveData
	];

	// InstrmtMDReqGrp
	static readonly SchemaRef[] G2022 =
	[
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
	];

	// InstrmtStrkPxGrp
	static readonly SchemaRef[] G2023 =
	[
		new SchemaRef(1003, true, 1), // Instrument
	];

	// IOIQualGrp
	static readonly SchemaRef[] G2024 =
	[
		new SchemaRef(104, false, 0), // IOIQualifier
	];

	// LegOrdGrp
	static readonly SchemaRef[] G2025 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(2026, false, 2), // LegPreAllocGrp
		new SchemaRef(564, false, 0), // LegPositionEffect
		new SchemaRef(565, false, 0), // LegCoveredOrUncovered
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(654, false, 0), // LegRefID
		new SchemaRef(566, false, 0), // LegPrice
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
	];

	// LegPreAllocGrp
	static readonly SchemaRef[] G2026 =
	[
		new SchemaRef(671, false, 0), // LegAllocAccount
		new SchemaRef(672, false, 0), // LegIndividualAllocID
		new SchemaRef(1009, false, 2), // NestedParties2
		new SchemaRef(673, false, 0), // LegAllocQty
		new SchemaRef(674, false, 0), // LegAllocAcctIDSource
		new SchemaRef(675, false, 0), // LegSettlCurrency
	];

	// LegQuotGrp
	static readonly SchemaRef[] G2027 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(686, false, 0), // LegPriceType
		new SchemaRef(681, false, 0), // LegBidPx
		new SchemaRef(684, false, 0), // LegOfferPx
		new SchemaRef(1006, false, 1), // LegBenchmarkCurveData
	];

	// LegQuotStatGrp
	static readonly SchemaRef[] G2028 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(1008, false, 2), // NestedParties
	];

	// LinesOfTextGrp
	static readonly SchemaRef[] G2029 =
	[
		new SchemaRef(58, true, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// ListOrdGrp
	static readonly SchemaRef[] G2030 =
	[
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(67, true, 0), // ListSeqNo
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(160, false, 0), // SettlInstMode
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(2039, false, 2), // PreAllocGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(401, false, 0), // SideValueInd
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(640, false, 0), // Price2
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(494, false, 0), // Designation
	];

	// MDFullGrp
	static readonly SchemaRef[] G2031 =
	[
		new SchemaRef(269, true, 0), // MDEntryType
		new SchemaRef(270, false, 0), // MDEntryPx
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(271, false, 0), // MDEntrySize
		new SchemaRef(272, false, 0), // MDEntryDate
		new SchemaRef(273, false, 0), // MDEntryTime
		new SchemaRef(274, false, 0), // TickDirection
		new SchemaRef(275, false, 0), // MDMkt
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(276, false, 0), // QuoteCondition
		new SchemaRef(277, false, 0), // TradeCondition
		new SchemaRef(282, false, 0), // MDEntryOriginator
		new SchemaRef(283, false, 0), // LocationID
		new SchemaRef(284, false, 0), // DeskID
		new SchemaRef(286, false, 0), // OpenCloseSettlFlag
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(287, false, 0), // SellerDays
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(299, false, 0), // QuoteEntryID
		new SchemaRef(288, false, 0), // MDEntryBuyer
		new SchemaRef(289, false, 0), // MDEntrySeller
		new SchemaRef(346, false, 0), // NumberOfOrders
		new SchemaRef(290, false, 0), // MDEntryPositionNo
		new SchemaRef(546, false, 0), // Scope
		new SchemaRef(811, false, 0), // PriceDelta
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// MDIncGrp
	static readonly SchemaRef[] G2032 =
	[
		new SchemaRef(279, true, 0), // MDUpdateAction
		new SchemaRef(285, false, 0), // DeleteReason
		new SchemaRef(269, false, 0), // MDEntryType
		new SchemaRef(278, false, 0), // MDEntryID
		new SchemaRef(280, false, 0), // MDEntryRefID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(291, false, 0), // FinancialStatus
		new SchemaRef(292, false, 0), // CorporateAction
		new SchemaRef(270, false, 0), // MDEntryPx
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(271, false, 0), // MDEntrySize
		new SchemaRef(272, false, 0), // MDEntryDate
		new SchemaRef(273, false, 0), // MDEntryTime
		new SchemaRef(274, false, 0), // TickDirection
		new SchemaRef(275, false, 0), // MDMkt
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(276, false, 0), // QuoteCondition
		new SchemaRef(277, false, 0), // TradeCondition
		new SchemaRef(282, false, 0), // MDEntryOriginator
		new SchemaRef(283, false, 0), // LocationID
		new SchemaRef(284, false, 0), // DeskID
		new SchemaRef(286, false, 0), // OpenCloseSettlFlag
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(287, false, 0), // SellerDays
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(299, false, 0), // QuoteEntryID
		new SchemaRef(288, false, 0), // MDEntryBuyer
		new SchemaRef(289, false, 0), // MDEntrySeller
		new SchemaRef(346, false, 0), // NumberOfOrders
		new SchemaRef(290, false, 0), // MDEntryPositionNo
		new SchemaRef(546, false, 0), // Scope
		new SchemaRef(811, false, 0), // PriceDelta
		new SchemaRef(451, false, 0), // NetChgPrevDay
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// MDReqGrp
	static readonly SchemaRef[] G2033 =
	[
		new SchemaRef(269, true, 0), // MDEntryType
	];

	// MDRjctGrp
	static readonly SchemaRef[] G2034 =
	[
		new SchemaRef(817, false, 0), // AltMDSourceID
	];

	// MiscFeesGrp
	static readonly SchemaRef[] G2035 =
	[
		new SchemaRef(137, false, 0), // MiscFeeAmt
		new SchemaRef(138, false, 0), // MiscFeeCurr
		new SchemaRef(139, false, 0), // MiscFeeType
		new SchemaRef(891, false, 0), // MiscFeeBasis
	];

	// OrdAllocGrp
	static readonly SchemaRef[] G2036 =
	[
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(1009, false, 2), // NestedParties2
		new SchemaRef(38, false, 0), // OrderQty
		new SchemaRef(799, false, 0), // OrderAvgPx
		new SchemaRef(800, false, 0), // OrderBookingQty
	];

	// OrdListStatGrp
	static readonly SchemaRef[] G2037 =
	[
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(14, true, 0), // CumQty
		new SchemaRef(39, true, 0), // OrdStatus
		new SchemaRef(636, false, 0), // WorkingIndicator
		new SchemaRef(151, true, 0), // LeavesQty
		new SchemaRef(84, true, 0), // CxlQty
		new SchemaRef(6, true, 0), // AvgPx
		new SchemaRef(103, false, 0), // OrdRejReason
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// PosUndInstrmtGrp
	static readonly SchemaRef[] G2038 =
	[
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(732, true, 0), // UnderlyingSettlPrice
		new SchemaRef(733, true, 0), // UnderlyingSettlPriceType
	];

	// PreAllocGrp
	static readonly SchemaRef[] G2039 =
	[
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(736, false, 0), // AllocSettlCurrency
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(80, false, 0), // AllocQty
	];

	// PreAllocMlegGrp
	static readonly SchemaRef[] G2040 =
	[
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(736, false, 0), // AllocSettlCurrency
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(1010, false, 2), // NestedParties3
		new SchemaRef(80, false, 0), // AllocQty
	];

	// QuotCxlEntriesGrp
	static readonly SchemaRef[] G2041 =
	[
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
	];

	// QuotEntryAckGrp
	static readonly SchemaRef[] G2042 =
	[
		new SchemaRef(299, false, 0), // QuoteEntryID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(132, false, 0), // BidPx
		new SchemaRef(133, false, 0), // OfferPx
		new SchemaRef(134, false, 0), // BidSize
		new SchemaRef(135, false, 0), // OfferSize
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(188, false, 0), // BidSpotRate
		new SchemaRef(190, false, 0), // OfferSpotRate
		new SchemaRef(189, false, 0), // BidForwardPoints
		new SchemaRef(191, false, 0), // OfferForwardPoints
		new SchemaRef(631, false, 0), // MidPx
		new SchemaRef(632, false, 0), // BidYield
		new SchemaRef(633, false, 0), // MidYield
		new SchemaRef(634, false, 0), // OfferYield
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(642, false, 0), // BidForwardPoints2
		new SchemaRef(643, false, 0), // OfferForwardPoints2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(368, false, 0), // QuoteEntryRejectReason
	];

	// QuotEntryGrp
	static readonly SchemaRef[] G2043 =
	[
		new SchemaRef(299, true, 0), // QuoteEntryID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(132, false, 0), // BidPx
		new SchemaRef(133, false, 0), // OfferPx
		new SchemaRef(134, false, 0), // BidSize
		new SchemaRef(135, false, 0), // OfferSize
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(188, false, 0), // BidSpotRate
		new SchemaRef(190, false, 0), // OfferSpotRate
		new SchemaRef(189, false, 0), // BidForwardPoints
		new SchemaRef(191, false, 0), // OfferForwardPoints
		new SchemaRef(631, false, 0), // MidPx
		new SchemaRef(632, false, 0), // BidYield
		new SchemaRef(633, false, 0), // MidYield
		new SchemaRef(634, false, 0), // OfferYield
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(642, false, 0), // BidForwardPoints2
		new SchemaRef(643, false, 0), // OfferForwardPoints2
		new SchemaRef(15, false, 0), // Currency
	];

	// QuotQualGrp
	static readonly SchemaRef[] G2044 =
	[
		new SchemaRef(695, false, 0), // QuoteQualifier
	];

	// QuotReqGrp
	static readonly SchemaRef[] G2045 =
	[
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(303, false, 0), // QuoteRequestType
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(2046, false, 2), // QuotReqLegsGrp
		new SchemaRef(2044, false, 2), // QuotQualGrp
		new SchemaRef(692, false, 0), // QuotePriceType
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(640, false, 0), // Price2
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(1012, false, 2), // Parties
	];

	// QuotReqLegsGrp
	static readonly SchemaRef[] G2046 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(1006, false, 1), // LegBenchmarkCurveData
	];

	// QuotReqRjctGrp
	static readonly SchemaRef[] G2047 =
	[
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(303, false, 0), // QuoteRequestType
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(2046, false, 2), // QuotReqLegsGrp
		new SchemaRef(2044, false, 2), // QuotQualGrp
		new SchemaRef(692, false, 0), // QuotePriceType
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(640, false, 0), // Price2
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(1012, false, 2), // Parties
	];

	// QuotSetAckGrp
	static readonly SchemaRef[] G2048 =
	[
		new SchemaRef(302, false, 0), // QuoteSetID
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(304, false, 0), // TotNoQuoteEntries
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2042, false, 2), // QuotEntryAckGrp
	];

	// QuotSetGrp
	static readonly SchemaRef[] G2049 =
	[
		new SchemaRef(302, true, 0), // QuoteSetID
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(367, false, 0), // QuoteSetValidUntilTime
		new SchemaRef(304, true, 0), // TotNoQuoteEntries
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2043, true, 2), // QuotEntryGrp
	];

	// RelSymDerivSecGrp
	static readonly SchemaRef[] G2050 =
	[
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(827, false, 0), // ExpirationCycle
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// RFQReqGrp
	static readonly SchemaRef[] G2051 =
	[
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(303, false, 0), // QuoteRequestType
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
	];

	// RgstDistInstGrp
	static readonly SchemaRef[] G2052 =
	[
		new SchemaRef(477, false, 0), // DistribPaymentMethod
		new SchemaRef(512, false, 0), // DistribPercentage
		new SchemaRef(478, false, 0), // CashDistribCurr
		new SchemaRef(498, false, 0), // CashDistribAgentName
		new SchemaRef(499, false, 0), // CashDistribAgentCode
		new SchemaRef(500, false, 0), // CashDistribAgentAcctNumber
		new SchemaRef(501, false, 0), // CashDistribPayRef
		new SchemaRef(502, false, 0), // CashDistribAgentAcctName
	];

	// RgstDtlsGrp
	static readonly SchemaRef[] G2053 =
	[
		new SchemaRef(509, false, 0), // RegistDtls
		new SchemaRef(511, false, 0), // RegistEmail
		new SchemaRef(474, false, 0), // MailingDtls
		new SchemaRef(482, false, 0), // MailingInst
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(522, false, 0), // OwnerType
		new SchemaRef(486, false, 0), // DateOfBirth
		new SchemaRef(475, false, 0), // InvestorCountryOfResidence
	];

	// RoutingGrp
	static readonly SchemaRef[] G2054 =
	[
		new SchemaRef(216, false, 0), // RoutingType
		new SchemaRef(217, false, 0), // RoutingID
	];

	// SecListGrp
	static readonly SchemaRef[] G2055 =
	[
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(2021, false, 2), // InstrmtLegSecListGrp
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(561, false, 0), // RoundLot
		new SchemaRef(562, false, 0), // MinTradeVol
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(827, false, 0), // ExpirationCycle
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// SecTypesGrp
	static readonly SchemaRef[] G2056 =
	[
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(762, false, 0), // SecuritySubType
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(461, false, 0), // CFICode
	];

	// SettlInstGrp
	static readonly SchemaRef[] G2057 =
	[
		new SchemaRef(162, false, 0), // SettlInstID
		new SchemaRef(163, false, 0), // SettlInstTransType
		new SchemaRef(214, false, 0), // SettlInstRefID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(461, false, 0), // CFICode
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(779, false, 0), // LastUpdateTime
		new SchemaRef(1016, false, 1), // SettlInstructionsData
		new SchemaRef(492, false, 0), // PaymentMethod
		new SchemaRef(476, false, 0), // PaymentRef
		new SchemaRef(488, false, 0), // CardHolderName
		new SchemaRef(489, false, 0), // CardNumber
		new SchemaRef(503, false, 0), // CardStartDate
		new SchemaRef(490, false, 0), // CardExpDate
		new SchemaRef(491, false, 0), // CardIssNum
		new SchemaRef(504, false, 0), // PaymentDate
		new SchemaRef(505, false, 0), // PaymentRemitterID
	];

	// SideCrossOrdCxlGrp
	static readonly SchemaRef[] G2058 =
	[
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(41, true, 0), // OrigClOrdID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// SideCrossOrdModGrp
	static readonly SchemaRef[] G2059 =
	[
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2039, false, 2), // PreAllocGrp
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(659, false, 0), // SideComplianceID
	];

	// TrdAllocGrp
	static readonly SchemaRef[] G2060 =
	[
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(736, false, 0), // AllocSettlCurrency
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(1009, false, 2), // NestedParties2
		new SchemaRef(80, false, 0), // AllocQty
	];

	// TrdCapRptSideGrp
	static readonly SchemaRef[] G2061 =
	[
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(37, true, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(575, false, 0), // OddLot
		new SchemaRef(2007, false, 2), // ClrInstGrp
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(578, false, 0), // TradeInputSource
		new SchemaRef(579, false, 0), // TradeInputDevice
		new SchemaRef(821, false, 0), // OrderInputDevice
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(483, false, 0), // TransBkdTime
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(943, false, 0), // TimeBracket
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(381, false, 0), // GrossTradeAmt
		new SchemaRef(157, false, 0), // NumDaysInterest
		new SchemaRef(230, false, 0), // ExDate
		new SchemaRef(158, false, 0), // AccruedInterestRate
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(738, false, 0), // InterestAtMaturity
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(238, false, 0), // Concession
		new SchemaRef(237, false, 0), // TotalTakedown
		new SchemaRef(118, false, 0), // NetMoney
		new SchemaRef(119, false, 0), // SettlCurrAmt
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(155, false, 0), // SettlCurrFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(752, false, 0), // SideMultiLegReportingType
		new SchemaRef(2011, false, 2), // ContAmtGrp
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(825, false, 0), // ExchangeRule
		new SchemaRef(826, false, 0), // TradeAllocIndicator
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2060, false, 2), // TrdAllocGrp
	];

	// TrdCollGrp
	static readonly SchemaRef[] G2062 =
	[
		new SchemaRef(571, false, 0), // TradeReportID
		new SchemaRef(818, false, 0), // SecondaryTradeReportID
	];

	// TrdInstrmtLegGrp
	static readonly SchemaRef[] G2063 =
	[
		new SchemaRef(1005, false, 1), // InstrumentLeg
		new SchemaRef(687, false, 0), // LegQty
		new SchemaRef(690, false, 0), // LegSwapType
		new SchemaRef(1007, false, 2), // LegStipulations
		new SchemaRef(564, false, 0), // LegPositionEffect
		new SchemaRef(565, false, 0), // LegCoveredOrUncovered
		new SchemaRef(1008, false, 2), // NestedParties
		new SchemaRef(654, false, 0), // LegRefID
		new SchemaRef(566, false, 0), // LegPrice
		new SchemaRef(587, false, 0), // LegSettlType
		new SchemaRef(588, false, 0), // LegSettlDate
		new SchemaRef(637, false, 0), // LegLastPx
	];

	// TrdgSesGrp
	static readonly SchemaRef[] G2064 =
	[
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
	];

	// UndInstrmtCollGrp
	static readonly SchemaRef[] G2065 =
	[
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(944, false, 0), // CollAction
	];

	// UndInstrmtGrp
	static readonly SchemaRef[] G2066 =
	[
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
	];

	// UndInstrmtStrkPxGrp
	static readonly SchemaRef[] G2067 =
	[
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(44, true, 0), // Price
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
	];

	// TrdCapDtGrp
	static readonly SchemaRef[] G2069 =
	[
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
	];

	// EvntGrp
	static readonly SchemaRef[] G2070 =
	[
		new SchemaRef(865, false, 0), // EventType
		new SchemaRef(866, false, 0), // EventDate
		new SchemaRef(867, false, 0), // EventPx
		new SchemaRef(868, false, 0), // EventText
	];

	// SecAltIDGrp
	static readonly SchemaRef[] G2071 =
	[
		new SchemaRef(455, false, 0), // SecurityAltID
		new SchemaRef(456, false, 0), // SecurityAltIDSource
	];

	// LegSecAltIDGrp
	static readonly SchemaRef[] G2072 =
	[
		new SchemaRef(605, false, 0), // LegSecurityAltID
		new SchemaRef(606, false, 0), // LegSecurityAltIDSource
	];

	// UndSecAltIDGrp
	static readonly SchemaRef[] G2073 =
	[
		new SchemaRef(458, false, 0), // UnderlyingSecurityAltID
		new SchemaRef(459, false, 0), // UnderlyingSecurityAltIDSource
	];

	// AttrbGrp
	static readonly SchemaRef[] G2074 =
	[
		new SchemaRef(871, false, 0), // InstrAttribType
		new SchemaRef(872, false, 0), // InstrAttribValue
	];

	// DlvyInstGrp
	static readonly SchemaRef[] G2075 =
	[
		new SchemaRef(165, false, 0), // SettlInstSource
		new SchemaRef(787, false, 0), // DlvyInstType
		new SchemaRef(1017, false, 2), // SettlParties
	];

	// SettlPtysSubGrp
	static readonly SchemaRef[] G2076 =
	[
		new SchemaRef(785, false, 0), // SettlPartySubID
		new SchemaRef(786, false, 0), // SettlPartySubIDType
	];

	// PtysSubGrp
	static readonly SchemaRef[] G2077 =
	[
		new SchemaRef(523, false, 0), // PartySubID
		new SchemaRef(803, false, 0), // PartySubIDType
	];

	// NstdPtysSubGrp
	static readonly SchemaRef[] G2078 =
	[
		new SchemaRef(545, false, 0), // NestedPartySubID
		new SchemaRef(805, false, 0), // NestedPartySubIDType
	];

	// NstdPtys2SubGrp
	static readonly SchemaRef[] G2079 =
	[
		new SchemaRef(760, false, 0), // Nested2PartySubID
		new SchemaRef(807, false, 0), // Nested2PartySubIDType
	];

	// NstdPtys3SubGrp
	static readonly SchemaRef[] G2080 =
	[
		new SchemaRef(953, false, 0), // Nested3PartySubID
		new SchemaRef(954, false, 0), // Nested3PartySubIDType
	];

	// HopGrp
	static readonly SchemaRef[] G2085 =
	[
		new SchemaRef(628, false, 0), // HopCompID
		new SchemaRef(629, false, 0), // HopSendingTime
		new SchemaRef(630, false, 0), // HopRefID
	];

	// MsgTypeGrp
	static readonly SchemaRef[] G2098 =
	[
		new SchemaRef(372, false, 0), // RefMsgType
		new SchemaRef(385, false, 0), // MsgDirection
	];

	// Heartbeat
	static readonly SchemaRef[] M1 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(112, false, 0), // TestReqID
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TestRequest
	static readonly SchemaRef[] M2 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(112, true, 0), // TestReqID
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ResendRequest
	static readonly SchemaRef[] M3 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(7, true, 0), // BeginSeqNo
		new SchemaRef(16, true, 0), // EndSeqNo
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Reject
	static readonly SchemaRef[] M4 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(45, true, 0), // RefSeqNum
		new SchemaRef(371, false, 0), // RefTagID
		new SchemaRef(372, false, 0), // RefMsgType
		new SchemaRef(373, false, 0), // SessionRejectReason
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SequenceReset
	static readonly SchemaRef[] M5 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(123, false, 0), // GapFillFlag
		new SchemaRef(36, true, 0), // NewSeqNo
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Logout
	static readonly SchemaRef[] M6 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// IOI
	static readonly SchemaRef[] M7 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(23, true, 0), // IOIID
		new SchemaRef(28, true, 0), // IOITransType
		new SchemaRef(26, false, 0), // IOIRefID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(27, true, 0), // IOIQty
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(2020, false, 2), // InstrmtLegIOIGrp
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(25, false, 0), // IOIQltyInd
		new SchemaRef(130, false, 0), // IOINaturalFlag
		new SchemaRef(2024, false, 2), // IOIQualGrp
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(149, false, 0), // URLLink
		new SchemaRef(2054, false, 2), // RoutingGrp
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Advertisement
	static readonly SchemaRef[] M8 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(2, true, 0), // AdvId
		new SchemaRef(5, true, 0), // AdvTransType
		new SchemaRef(3, false, 0), // AdvRefID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(4, true, 0), // AdvSide
		new SchemaRef(53, true, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(149, false, 0), // URLLink
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ExecutionReport
	static readonly SchemaRef[] M9 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, true, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(527, false, 0), // SecondaryExecID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(41, false, 0), // OrigClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(693, false, 0), // QuoteRespID
		new SchemaRef(790, false, 0), // OrdStatusReqID
		new SchemaRef(584, false, 0), // MassStatusReqID
		new SchemaRef(911, false, 0), // TotNumReports
		new SchemaRef(912, false, 0), // LastRptRequested
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(2012, false, 2), // ContraGrp
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(548, false, 0), // CrossID
		new SchemaRef(551, false, 0), // OrigCrossID
		new SchemaRef(549, false, 0), // CrossType
		new SchemaRef(17, true, 0), // ExecID
		new SchemaRef(19, false, 0), // ExecRefID
		new SchemaRef(150, true, 0), // ExecType
		new SchemaRef(39, true, 0), // OrdStatus
		new SchemaRef(636, false, 0), // WorkingIndicator
		new SchemaRef(103, false, 0), // OrdRejReason
		new SchemaRef(378, false, 0), // ExecRestatementReason
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(839, false, 0), // PeggedPrice
		new SchemaRef(845, false, 0), // DiscretionPrice
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(850, false, 0), // TargetStrategyPerformance
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(32, false, 0), // LastQty
		new SchemaRef(652, false, 0), // UnderlyingLastQty
		new SchemaRef(31, false, 0), // LastPx
		new SchemaRef(651, false, 0), // UnderlyingLastPx
		new SchemaRef(669, false, 0), // LastParPx
		new SchemaRef(194, false, 0), // LastSpotRate
		new SchemaRef(195, false, 0), // LastForwardPoints
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(943, false, 0), // TimeBracket
		new SchemaRef(29, false, 0), // LastCapacity
		new SchemaRef(151, true, 0), // LeavesQty
		new SchemaRef(14, true, 0), // CumQty
		new SchemaRef(6, true, 0), // AvgPx
		new SchemaRef(424, false, 0), // DayOrderQty
		new SchemaRef(425, false, 0), // DayCumQty
		new SchemaRef(426, false, 0), // DayAvgPx
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(113, false, 0), // ReportToExch
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(381, false, 0), // GrossTradeAmt
		new SchemaRef(157, false, 0), // NumDaysInterest
		new SchemaRef(230, false, 0), // ExDate
		new SchemaRef(158, false, 0), // AccruedInterestRate
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(738, false, 0), // InterestAtMaturity
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(258, false, 0), // TradedFlatSwitch
		new SchemaRef(259, false, 0), // BasisFeatureDate
		new SchemaRef(260, false, 0), // BasisFeaturePrice
		new SchemaRef(238, false, 0), // Concession
		new SchemaRef(237, false, 0), // TotalTakedown
		new SchemaRef(118, false, 0), // NetMoney
		new SchemaRef(119, false, 0), // SettlCurrAmt
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(155, false, 0), // SettlCurrFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(641, false, 0), // LastForwardPoints2
		new SchemaRef(442, false, 0), // MultiLegReportingType
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(483, false, 0), // TransBkdTime
		new SchemaRef(515, false, 0), // ExecValuationPoint
		new SchemaRef(484, false, 0), // ExecPriceType
		new SchemaRef(485, false, 0), // ExecPriceAdjustment
		new SchemaRef(638, false, 0), // PriorityIndicator
		new SchemaRef(639, false, 0), // PriceImprovement
		new SchemaRef(851, false, 0), // LastLiquidityInd
		new SchemaRef(2011, false, 2), // ContAmtGrp
		new SchemaRef(2018, false, 2), // InstrmtLegExecGrp
		new SchemaRef(797, false, 0), // CopyMsgIndicator
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderCancelReject
	static readonly SchemaRef[] M10 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, true, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(41, true, 0), // OrigClOrdID
		new SchemaRef(39, true, 0), // OrdStatus
		new SchemaRef(636, false, 0), // WorkingIndicator
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(434, true, 0), // CxlRejResponseTo
		new SchemaRef(102, false, 0), // CxlRejReason
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Logon
	static readonly SchemaRef[] M11 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(98, true, 0), // EncryptMethod
		new SchemaRef(108, true, 0), // HeartBtInt
		new SchemaRef(95, false, 0), // RawDataLength
		new SchemaRef(96, false, 0), // RawData
		new SchemaRef(141, false, 0), // ResetSeqNumFlag
		new SchemaRef(789, false, 0), // NextExpectedMsgSeqNum
		new SchemaRef(383, false, 0), // MaxMessageSize
		new SchemaRef(2098, false, 2), // MsgTypeGrp
		new SchemaRef(464, false, 0), // TestMessageIndicator
		new SchemaRef(553, false, 0), // Username
		new SchemaRef(554, false, 0), // Password
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// News
	static readonly SchemaRef[] M12 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(42, false, 0), // OrigTime
		new SchemaRef(61, false, 0), // Urgency
		new SchemaRef(148, true, 0), // Headline
		new SchemaRef(358, false, 0), // EncodedHeadlineLen
		new SchemaRef(359, false, 0), // EncodedHeadline
		new SchemaRef(2054, false, 2), // RoutingGrp
		new SchemaRef(2017, false, 2), // InstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2029, true, 2), // LinesOfTextGrp
		new SchemaRef(149, false, 0), // URLLink
		new SchemaRef(95, false, 0), // RawDataLength
		new SchemaRef(96, false, 0), // RawData
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Email
	static readonly SchemaRef[] M13 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(164, true, 0), // EmailThreadID
		new SchemaRef(94, true, 0), // EmailType
		new SchemaRef(42, false, 0), // OrigTime
		new SchemaRef(147, true, 0), // Subject
		new SchemaRef(356, false, 0), // EncodedSubjectLen
		new SchemaRef(357, false, 0), // EncodedSubject
		new SchemaRef(2054, false, 2), // RoutingGrp
		new SchemaRef(2017, false, 2), // InstrmtGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(2029, true, 2), // LinesOfTextGrp
		new SchemaRef(95, false, 0), // RawDataLength
		new SchemaRef(96, false, 0), // RawData
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NewOrderSingle
	static readonly SchemaRef[] M14 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2039, false, 2), // PreAllocGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(640, false, 0), // Price2
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NewOrderList
	static readonly SchemaRef[] M15 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(390, false, 0), // BidID
		new SchemaRef(391, false, 0), // ClientBidID
		new SchemaRef(414, false, 0), // ProgRptReqs
		new SchemaRef(394, true, 0), // BidType
		new SchemaRef(415, false, 0), // ProgPeriodInterval
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(433, false, 0), // ListExecInstType
		new SchemaRef(69, false, 0), // ListExecInst
		new SchemaRef(352, false, 0), // EncodedListExecInstLen
		new SchemaRef(353, false, 0), // EncodedListExecInst
		new SchemaRef(765, false, 0), // AllowableOneSidednessPct
		new SchemaRef(766, false, 0), // AllowableOneSidednessValue
		new SchemaRef(767, false, 0), // AllowableOneSidednessCurr
		new SchemaRef(68, true, 0), // TotNoOrders
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2030, true, 2), // ListOrdGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderCancelRequest
	static readonly SchemaRef[] M16 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(41, true, 0), // OrigClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderCancelReplaceRequest
	static readonly SchemaRef[] M17 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(41, true, 0), // OrigClOrdID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(66, false, 0), // ListID
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2039, false, 2), // PreAllocGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(640, false, 0), // Price2
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderStatusRequest
	static readonly SchemaRef[] M18 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(790, false, 0), // OrdStatusReqID
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// AllocationInstruction
	static readonly SchemaRef[] M19 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(70, true, 0), // AllocID
		new SchemaRef(71, true, 0), // AllocTransType
		new SchemaRef(626, true, 0), // AllocType
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(72, false, 0), // RefAllocID
		new SchemaRef(796, false, 0), // AllocCancReplaceReason
		new SchemaRef(808, false, 0), // AllocIntermedReqType
		new SchemaRef(196, false, 0), // AllocLinkID
		new SchemaRef(197, false, 0), // AllocLinkType
		new SchemaRef(466, false, 0), // BookingRefID
		new SchemaRef(857, true, 0), // AllocNoOrdersType
		new SchemaRef(2036, false, 2), // OrdAllocGrp
		new SchemaRef(2014, false, 2), // ExecAllocGrp
		new SchemaRef(570, false, 0), // PreviouslyReported
		new SchemaRef(700, false, 0), // ReversalIndicator
		new SchemaRef(574, false, 0), // MatchType
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(53, true, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(6, true, 0), // AvgPx
		new SchemaRef(860, false, 0), // AvgParPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(74, false, 0), // AvgPxPrecision
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(75, true, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(381, false, 0), // GrossTradeAmt
		new SchemaRef(238, false, 0), // Concession
		new SchemaRef(237, false, 0), // TotalTakedown
		new SchemaRef(118, false, 0), // NetMoney
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(754, false, 0), // AutoAcceptIndicator
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(157, false, 0), // NumDaysInterest
		new SchemaRef(158, false, 0), // AccruedInterestRate
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(540, false, 0), // TotalAccruedInterestAmt
		new SchemaRef(738, false, 0), // InterestAtMaturity
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(650, false, 0), // LegalConfirm
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(892, false, 0), // TotNoAllocs
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2003, false, 2), // AllocGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ListCancelRequest
	static readonly SchemaRef[] M20 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ListExecute
	static readonly SchemaRef[] M21 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(391, false, 0), // ClientBidID
		new SchemaRef(390, false, 0), // BidID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ListStatusRequest
	static readonly SchemaRef[] M22 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ListStatus
	static readonly SchemaRef[] M23 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(429, true, 0), // ListStatusType
		new SchemaRef(82, true, 0), // NoRpts
		new SchemaRef(431, true, 0), // ListOrderStatus
		new SchemaRef(83, true, 0), // RptSeq
		new SchemaRef(444, false, 0), // ListStatusText
		new SchemaRef(445, false, 0), // EncodedListStatusTextLen
		new SchemaRef(446, false, 0), // EncodedListStatusText
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(68, true, 0), // TotNoOrders
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2037, true, 2), // OrdListStatGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// AllocationInstructionAck
	static readonly SchemaRef[] M24 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(70, true, 0), // AllocID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(87, true, 0), // AllocStatus
		new SchemaRef(88, false, 0), // AllocRejCode
		new SchemaRef(626, false, 0), // AllocType
		new SchemaRef(808, false, 0), // AllocIntermedReqType
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(2002, false, 2), // AllocAckGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// DontKnowTrade
	static readonly SchemaRef[] M25 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, true, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(17, true, 0), // ExecID
		new SchemaRef(127, true, 0), // DKReason
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(32, false, 0), // LastQty
		new SchemaRef(31, false, 0), // LastPx
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteRequest
	static readonly SchemaRef[] M26 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, true, 0), // QuoteReqID
		new SchemaRef(644, false, 0), // RFQReqID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(2045, true, 2), // QuotReqGrp
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Quote
	static readonly SchemaRef[] M27 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, false, 0), // QuoteReqID
		new SchemaRef(117, true, 0), // QuoteID
		new SchemaRef(693, false, 0), // QuoteRespID
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(2044, false, 2), // QuotQualGrp
		new SchemaRef(301, false, 0), // QuoteResponseLevel
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(2027, false, 2), // LegQuotGrp
		new SchemaRef(132, false, 0), // BidPx
		new SchemaRef(133, false, 0), // OfferPx
		new SchemaRef(645, false, 0), // MktBidPx
		new SchemaRef(646, false, 0), // MktOfferPx
		new SchemaRef(647, false, 0), // MinBidSize
		new SchemaRef(134, false, 0), // BidSize
		new SchemaRef(648, false, 0), // MinOfferSize
		new SchemaRef(135, false, 0), // OfferSize
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(188, false, 0), // BidSpotRate
		new SchemaRef(190, false, 0), // OfferSpotRate
		new SchemaRef(189, false, 0), // BidForwardPoints
		new SchemaRef(191, false, 0), // OfferForwardPoints
		new SchemaRef(631, false, 0), // MidPx
		new SchemaRef(632, false, 0), // BidYield
		new SchemaRef(633, false, 0), // MidYield
		new SchemaRef(634, false, 0), // OfferYield
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(642, false, 0), // BidForwardPoints2
		new SchemaRef(643, false, 0), // OfferForwardPoints2
		new SchemaRef(656, false, 0), // SettlCurrBidFxRate
		new SchemaRef(657, false, 0), // SettlCurrOfferFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(13, false, 0), // CommType
		new SchemaRef(12, false, 0), // Commission
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SettlementInstructions
	static readonly SchemaRef[] M28 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(777, true, 0), // SettlInstMsgID
		new SchemaRef(791, false, 0), // SettlInstReqID
		new SchemaRef(160, true, 0), // SettlInstMode
		new SchemaRef(792, false, 0), // SettlInstReqRejCode
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(2057, false, 2), // SettlInstGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MarketDataRequest
	static readonly SchemaRef[] M29 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(262, true, 0), // MDReqID
		new SchemaRef(263, true, 0), // SubscriptionRequestType
		new SchemaRef(264, true, 0), // MarketDepth
		new SchemaRef(265, false, 0), // MDUpdateType
		new SchemaRef(266, false, 0), // AggregatedBook
		new SchemaRef(286, false, 0), // OpenCloseSettlFlag
		new SchemaRef(546, false, 0), // Scope
		new SchemaRef(547, false, 0), // MDImplicitDelete
		new SchemaRef(2033, true, 2), // MDReqGrp
		new SchemaRef(2022, true, 2), // InstrmtMDReqGrp
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(815, false, 0), // ApplQueueAction
		new SchemaRef(812, false, 0), // ApplQueueMax
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MarketDataSnapshotFullRefresh
	static readonly SchemaRef[] M30 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(262, false, 0), // MDReqID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(291, false, 0), // FinancialStatus
		new SchemaRef(292, false, 0), // CorporateAction
		new SchemaRef(451, false, 0), // NetChgPrevDay
		new SchemaRef(2031, true, 2), // MDFullGrp
		new SchemaRef(813, false, 0), // ApplQueueDepth
		new SchemaRef(814, false, 0), // ApplQueueResolution
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MarketDataIncrementalRefresh
	static readonly SchemaRef[] M31 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(262, false, 0), // MDReqID
		new SchemaRef(2032, true, 2), // MDIncGrp
		new SchemaRef(813, false, 0), // ApplQueueDepth
		new SchemaRef(814, false, 0), // ApplQueueResolution
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MarketDataRequestReject
	static readonly SchemaRef[] M32 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(262, true, 0), // MDReqID
		new SchemaRef(281, false, 0), // MDReqRejReason
		new SchemaRef(2034, false, 2), // MDRjctGrp
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteCancel
	static readonly SchemaRef[] M33 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, false, 0), // QuoteReqID
		new SchemaRef(117, true, 0), // QuoteID
		new SchemaRef(298, true, 0), // QuoteCancelType
		new SchemaRef(301, false, 0), // QuoteResponseLevel
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(2041, false, 2), // QuotCxlEntriesGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteStatusRequest
	static readonly SchemaRef[] M34 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(649, false, 0), // QuoteStatusReqID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MassQuoteAcknowledgement
	static readonly SchemaRef[] M35 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, false, 0), // QuoteReqID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(297, true, 0), // QuoteStatus
		new SchemaRef(300, false, 0), // QuoteRejectReason
		new SchemaRef(301, false, 0), // QuoteResponseLevel
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(2048, false, 2), // QuotSetAckGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityDefinitionRequest
	static readonly SchemaRef[] M36 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(321, true, 0), // SecurityRequestType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(827, false, 0), // ExpirationCycle
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityDefinition
	static readonly SchemaRef[] M37 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(322, true, 0), // SecurityResponseID
		new SchemaRef(323, true, 0), // SecurityResponseType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(827, false, 0), // ExpirationCycle
		new SchemaRef(561, false, 0), // RoundLot
		new SchemaRef(562, false, 0), // MinTradeVol
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityStatusRequest
	static readonly SchemaRef[] M38 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(324, true, 0), // SecurityStatusReqID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(263, true, 0), // SubscriptionRequestType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityStatus
	static readonly SchemaRef[] M39 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(324, false, 0), // SecurityStatusReqID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(325, false, 0), // UnsolicitedIndicator
		new SchemaRef(326, false, 0), // SecurityTradingStatus
		new SchemaRef(291, false, 0), // FinancialStatus
		new SchemaRef(292, false, 0), // CorporateAction
		new SchemaRef(327, false, 0), // HaltReason
		new SchemaRef(328, false, 0), // InViewOfCommon
		new SchemaRef(329, false, 0), // DueToRelated
		new SchemaRef(330, false, 0), // BuyVolume
		new SchemaRef(331, false, 0), // SellVolume
		new SchemaRef(332, false, 0), // HighPx
		new SchemaRef(333, false, 0), // LowPx
		new SchemaRef(31, false, 0), // LastPx
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(334, false, 0), // Adjustment
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradingSessionStatusRequest
	static readonly SchemaRef[] M40 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(335, true, 0), // TradSesReqID
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(338, false, 0), // TradSesMethod
		new SchemaRef(339, false, 0), // TradSesMode
		new SchemaRef(263, true, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradingSessionStatus
	static readonly SchemaRef[] M41 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(335, false, 0), // TradSesReqID
		new SchemaRef(336, true, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(338, false, 0), // TradSesMethod
		new SchemaRef(339, false, 0), // TradSesMode
		new SchemaRef(325, false, 0), // UnsolicitedIndicator
		new SchemaRef(340, true, 0), // TradSesStatus
		new SchemaRef(567, false, 0), // TradSesStatusRejReason
		new SchemaRef(341, false, 0), // TradSesStartTime
		new SchemaRef(342, false, 0), // TradSesOpenTime
		new SchemaRef(343, false, 0), // TradSesPreCloseTime
		new SchemaRef(344, false, 0), // TradSesCloseTime
		new SchemaRef(345, false, 0), // TradSesEndTime
		new SchemaRef(387, false, 0), // TotalVolumeTraded
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MassQuote
	static readonly SchemaRef[] M42 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, false, 0), // QuoteReqID
		new SchemaRef(117, true, 0), // QuoteID
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(301, false, 0), // QuoteResponseLevel
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(293, false, 0), // DefBidSize
		new SchemaRef(294, false, 0), // DefOfferSize
		new SchemaRef(2049, true, 2), // QuotSetGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// BusinessMessageReject
	static readonly SchemaRef[] M43 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(45, false, 0), // RefSeqNum
		new SchemaRef(372, true, 0), // RefMsgType
		new SchemaRef(379, false, 0), // BusinessRejectRefID
		new SchemaRef(380, true, 0), // BusinessRejectReason
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// BidRequest
	static readonly SchemaRef[] M44 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(390, false, 0), // BidID
		new SchemaRef(391, true, 0), // ClientBidID
		new SchemaRef(374, true, 0), // BidRequestTransType
		new SchemaRef(392, false, 0), // ListName
		new SchemaRef(393, true, 0), // TotNoRelatedSym
		new SchemaRef(394, true, 0), // BidType
		new SchemaRef(395, false, 0), // NumTickets
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(396, false, 0), // SideValue1
		new SchemaRef(397, false, 0), // SideValue2
		new SchemaRef(2006, false, 2), // BidDescReqGrp
		new SchemaRef(2004, false, 2), // BidCompReqGrp
		new SchemaRef(409, false, 0), // LiquidityIndType
		new SchemaRef(410, false, 0), // WtAverageLiquidity
		new SchemaRef(411, false, 0), // ExchangeForPhysical
		new SchemaRef(412, false, 0), // OutMainCntryUIndex
		new SchemaRef(413, false, 0), // CrossPercent
		new SchemaRef(414, false, 0), // ProgRptReqs
		new SchemaRef(415, false, 0), // ProgPeriodInterval
		new SchemaRef(416, false, 0), // IncTaxInd
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(417, false, 0), // NumBidders
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(418, true, 0), // BidTradeType
		new SchemaRef(419, true, 0), // BasisPxType
		new SchemaRef(443, false, 0), // StrikeTime
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// BidResponse
	static readonly SchemaRef[] M45 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(390, false, 0), // BidID
		new SchemaRef(391, false, 0), // ClientBidID
		new SchemaRef(2005, true, 2), // BidCompRspGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ListStrikePrice
	static readonly SchemaRef[] M46 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(66, true, 0), // ListID
		new SchemaRef(422, true, 0), // TotNoStrikes
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2023, true, 2), // InstrmtStrkPxGrp
		new SchemaRef(2067, false, 2), // UndInstrmtStrkPxGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// XMLnonFIX
	static readonly SchemaRef[] M47 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// RegistrationInstructions
	static readonly SchemaRef[] M48 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(513, true, 0), // RegistID
		new SchemaRef(514, true, 0), // RegistTransType
		new SchemaRef(508, true, 0), // RegistRefID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(493, false, 0), // RegistAcctType
		new SchemaRef(495, false, 0), // TaxAdvantageType
		new SchemaRef(517, false, 0), // OwnershipType
		new SchemaRef(2053, false, 2), // RgstDtlsGrp
		new SchemaRef(2052, false, 2), // RgstDistInstGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// RegistrationInstructionsResponse
	static readonly SchemaRef[] M49 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(513, true, 0), // RegistID
		new SchemaRef(514, true, 0), // RegistTransType
		new SchemaRef(508, true, 0), // RegistRefID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(506, true, 0), // RegistStatus
		new SchemaRef(507, false, 0), // RegistRejReasonCode
		new SchemaRef(496, false, 0), // RegistRejReasonText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderMassCancelRequest
	static readonly SchemaRef[] M50 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(530, true, 0), // MassCancelRequestType
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderMassCancelReport
	static readonly SchemaRef[] M51 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(37, true, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(530, true, 0), // MassCancelRequestType
		new SchemaRef(531, true, 0), // MassCancelResponse
		new SchemaRef(532, false, 0), // MassCancelRejectReason
		new SchemaRef(533, false, 0), // TotalAffectedOrders
		new SchemaRef(2001, false, 2), // AffectedOrdGrp
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NewOrderCross
	static readonly SchemaRef[] M52 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(548, true, 0), // CrossID
		new SchemaRef(549, true, 0), // CrossType
		new SchemaRef(550, true, 0), // CrossPrioritization
		new SchemaRef(2059, true, 2), // SideCrossOrdModGrp
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CrossOrderCancelReplaceRequest
	static readonly SchemaRef[] M53 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(548, true, 0), // CrossID
		new SchemaRef(551, true, 0), // OrigCrossID
		new SchemaRef(549, true, 0), // CrossType
		new SchemaRef(550, true, 0), // CrossPrioritization
		new SchemaRef(2059, true, 2), // SideCrossOrdModGrp
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CrossOrderCancelRequest
	static readonly SchemaRef[] M54 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(548, true, 0), // CrossID
		new SchemaRef(551, true, 0), // OrigCrossID
		new SchemaRef(549, true, 0), // CrossType
		new SchemaRef(550, true, 0), // CrossPrioritization
		new SchemaRef(2058, true, 2), // SideCrossOrdCxlGrp
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityTypeRequest
	static readonly SchemaRef[] M55 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(762, false, 0), // SecuritySubType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityTypes
	static readonly SchemaRef[] M56 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(322, true, 0), // SecurityResponseID
		new SchemaRef(323, true, 0), // SecurityResponseType
		new SchemaRef(557, false, 0), // TotNoSecurityTypes
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2056, false, 2), // SecTypesGrp
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityListRequest
	static readonly SchemaRef[] M57 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(559, true, 0), // SecurityListRequestType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SecurityList
	static readonly SchemaRef[] M58 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(322, true, 0), // SecurityResponseID
		new SchemaRef(560, true, 0), // SecurityRequestResult
		new SchemaRef(393, false, 0), // TotNoRelatedSym
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2055, false, 2), // SecListGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// DerivativeSecurityListRequest
	static readonly SchemaRef[] M59 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(559, true, 0), // SecurityListRequestType
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(762, false, 0), // SecuritySubType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// DerivativeSecurityList
	static readonly SchemaRef[] M60 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(320, true, 0), // SecurityReqID
		new SchemaRef(322, true, 0), // SecurityResponseID
		new SchemaRef(560, true, 0), // SecurityRequestResult
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(393, false, 0), // TotNoRelatedSym
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2050, false, 2), // RelSymDerivSecGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NewOrderMultileg
	static readonly SchemaRef[] M61 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2040, false, 2), // PreAllocMlegGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(2025, true, 2), // LegOrdGrp
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(563, false, 0), // MultiLegRptTypeReq
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// MultilegOrderCancelReplace
	static readonly SchemaRef[] M62 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(41, true, 0), // OrigClOrdID
		new SchemaRef(11, true, 0), // ClOrdID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(583, false, 0), // ClOrdLinkID
		new SchemaRef(586, false, 0), // OrigOrdModTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(589, false, 0), // DayBookingInst
		new SchemaRef(590, false, 0), // BookingUnit
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(2040, false, 2), // PreAllocMlegGrp
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(544, false, 0), // CashMargin
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(21, false, 0), // HandlInst
		new SchemaRef(18, false, 0), // ExecInst
		new SchemaRef(110, false, 0), // MinQty
		new SchemaRef(111, false, 0), // MaxFloor
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(140, false, 0), // PrevClosePx
		new SchemaRef(2025, true, 2), // LegOrdGrp
		new SchemaRef(114, false, 0), // LocateReqd
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1011, true, 1), // OrderQtyData
		new SchemaRef(40, true, 0), // OrdType
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(99, false, 0), // StopPx
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(376, false, 0), // ComplianceID
		new SchemaRef(377, false, 0), // SolicitedFlag
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(59, false, 0), // TimeInForce
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(427, false, 0), // GTBookingInst
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(121, false, 0), // ForexReq
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(203, false, 0), // CoveredOrUncovered
		new SchemaRef(210, false, 0), // MaxShow
		new SchemaRef(1013, false, 1), // PegInstructions
		new SchemaRef(1001, false, 1), // DiscretionInstructions
		new SchemaRef(847, false, 0), // TargetStrategy
		new SchemaRef(848, false, 0), // TargetStrategyParameters
		new SchemaRef(849, false, 0), // ParticipationRate
		new SchemaRef(480, false, 0), // CancellationRights
		new SchemaRef(481, false, 0), // MoneyLaunderingStatus
		new SchemaRef(513, false, 0), // RegistID
		new SchemaRef(494, false, 0), // Designation
		new SchemaRef(563, false, 0), // MultiLegRptTypeReq
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradeCaptureReportRequest
	static readonly SchemaRef[] M63 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(568, true, 0), // TradeRequestID
		new SchemaRef(569, true, 0), // TradeRequestType
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(571, false, 0), // TradeReportID
		new SchemaRef(818, false, 0), // SecondaryTradeReportID
		new SchemaRef(17, false, 0), // ExecID
		new SchemaRef(150, false, 0), // ExecType
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(828, false, 0), // TrdType
		new SchemaRef(829, false, 0), // TrdSubType
		new SchemaRef(830, false, 0), // TransferReason
		new SchemaRef(855, false, 0), // SecondaryTrdType
		new SchemaRef(820, false, 0), // TradeLinkID
		new SchemaRef(880, false, 0), // TrdMatchID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2069, false, 2), // TrdCapDtGrp
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(943, false, 0), // TimeBracket
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(442, false, 0), // MultiLegReportingType
		new SchemaRef(578, false, 0), // TradeInputSource
		new SchemaRef(579, false, 0), // TradeInputDevice
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradeCaptureReport
	static readonly SchemaRef[] M64 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(571, true, 0), // TradeReportID
		new SchemaRef(487, false, 0), // TradeReportTransType
		new SchemaRef(856, false, 0), // TradeReportType
		new SchemaRef(568, false, 0), // TradeRequestID
		new SchemaRef(828, false, 0), // TrdType
		new SchemaRef(829, false, 0), // TrdSubType
		new SchemaRef(855, false, 0), // SecondaryTrdType
		new SchemaRef(830, false, 0), // TransferReason
		new SchemaRef(150, false, 0), // ExecType
		new SchemaRef(748, false, 0), // TotNumTradeReports
		new SchemaRef(912, false, 0), // LastRptRequested
		new SchemaRef(325, false, 0), // UnsolicitedIndicator
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(572, false, 0), // TradeReportRefID
		new SchemaRef(881, false, 0), // SecondaryTradeReportRefID
		new SchemaRef(818, false, 0), // SecondaryTradeReportID
		new SchemaRef(820, false, 0), // TradeLinkID
		new SchemaRef(880, false, 0), // TrdMatchID
		new SchemaRef(17, false, 0), // ExecID
		new SchemaRef(39, false, 0), // OrdStatus
		new SchemaRef(527, false, 0), // SecondaryExecID
		new SchemaRef(378, false, 0), // ExecRestatementReason
		new SchemaRef(570, true, 0), // PreviouslyReported
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(822, false, 0), // UnderlyingTradingSessionID
		new SchemaRef(823, false, 0), // UnderlyingTradingSessionSubID
		new SchemaRef(32, true, 0), // LastQty
		new SchemaRef(31, true, 0), // LastPx
		new SchemaRef(669, false, 0), // LastParPx
		new SchemaRef(194, false, 0), // LastSpotRate
		new SchemaRef(195, false, 0), // LastForwardPoints
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(75, true, 0), // TradeDate
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(6, false, 0), // AvgPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(819, false, 0), // AvgPxIndicator
		new SchemaRef(1014, false, 2), // PositionAmountData
		new SchemaRef(442, false, 0), // MultiLegReportingType
		new SchemaRef(824, false, 0), // TradeLegRefID
		new SchemaRef(2063, false, 2), // TrdInstrmtLegGrp
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(574, false, 0), // MatchType
		new SchemaRef(2061, true, 2), // TrdCapRptSideGrp
		new SchemaRef(797, false, 0), // CopyMsgIndicator
		new SchemaRef(852, false, 0), // PublishTrdIndicator
		new SchemaRef(853, false, 0), // ShortSaleReason
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// OrderMassStatusRequest
	static readonly SchemaRef[] M65 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(584, true, 0), // MassStatusReqID
		new SchemaRef(585, true, 0), // MassStatusReqType
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1021, false, 1), // UnderlyingInstrument
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteRequestReject
	static readonly SchemaRef[] M66 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(131, true, 0), // QuoteReqID
		new SchemaRef(644, false, 0), // RFQReqID
		new SchemaRef(658, true, 0), // QuoteRequestRejectReason
		new SchemaRef(2047, true, 2), // QuotReqRjctGrp
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// RFQRequest
	static readonly SchemaRef[] M67 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(644, true, 0), // RFQReqID
		new SchemaRef(2051, true, 2), // RFQReqGrp
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteStatusReport
	static readonly SchemaRef[] M68 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(649, false, 0), // QuoteStatusReqID
		new SchemaRef(131, false, 0), // QuoteReqID
		new SchemaRef(117, true, 0), // QuoteID
		new SchemaRef(693, false, 0), // QuoteRespID
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(2028, false, 2), // LegQuotStatGrp
		new SchemaRef(2044, false, 2), // QuotQualGrp
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(132, false, 0), // BidPx
		new SchemaRef(133, false, 0), // OfferPx
		new SchemaRef(645, false, 0), // MktBidPx
		new SchemaRef(646, false, 0), // MktOfferPx
		new SchemaRef(647, false, 0), // MinBidSize
		new SchemaRef(134, false, 0), // BidSize
		new SchemaRef(648, false, 0), // MinOfferSize
		new SchemaRef(135, false, 0), // OfferSize
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(188, false, 0), // BidSpotRate
		new SchemaRef(190, false, 0), // OfferSpotRate
		new SchemaRef(189, false, 0), // BidForwardPoints
		new SchemaRef(191, false, 0), // OfferForwardPoints
		new SchemaRef(631, false, 0), // MidPx
		new SchemaRef(632, false, 0), // BidYield
		new SchemaRef(633, false, 0), // MidYield
		new SchemaRef(634, false, 0), // OfferYield
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(642, false, 0), // BidForwardPoints2
		new SchemaRef(643, false, 0), // OfferForwardPoints2
		new SchemaRef(656, false, 0), // SettlCurrBidFxRate
		new SchemaRef(657, false, 0), // SettlCurrOfferFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(13, false, 0), // CommType
		new SchemaRef(12, false, 0), // Commission
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(297, false, 0), // QuoteStatus
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// QuoteResponse
	static readonly SchemaRef[] M69 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(693, true, 0), // QuoteRespID
		new SchemaRef(117, false, 0), // QuoteID
		new SchemaRef(694, true, 0), // QuoteRespType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(23, false, 0), // IOIID
		new SchemaRef(537, false, 0), // QuoteType
		new SchemaRef(2044, false, 2), // QuotQualGrp
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(1011, false, 1), // OrderQtyData
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(193, false, 0), // SettlDate2
		new SchemaRef(192, false, 0), // OrderQty2
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(2027, false, 2), // LegQuotGrp
		new SchemaRef(132, false, 0), // BidPx
		new SchemaRef(133, false, 0), // OfferPx
		new SchemaRef(645, false, 0), // MktBidPx
		new SchemaRef(646, false, 0), // MktOfferPx
		new SchemaRef(647, false, 0), // MinBidSize
		new SchemaRef(134, false, 0), // BidSize
		new SchemaRef(648, false, 0), // MinOfferSize
		new SchemaRef(135, false, 0), // OfferSize
		new SchemaRef(62, false, 0), // ValidUntilTime
		new SchemaRef(188, false, 0), // BidSpotRate
		new SchemaRef(190, false, 0), // OfferSpotRate
		new SchemaRef(189, false, 0), // BidForwardPoints
		new SchemaRef(191, false, 0), // OfferForwardPoints
		new SchemaRef(631, false, 0), // MidPx
		new SchemaRef(632, false, 0), // BidYield
		new SchemaRef(633, false, 0), // MidYield
		new SchemaRef(634, false, 0), // OfferYield
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(40, false, 0), // OrdType
		new SchemaRef(642, false, 0), // BidForwardPoints2
		new SchemaRef(643, false, 0), // OfferForwardPoints2
		new SchemaRef(656, false, 0), // SettlCurrBidFxRate
		new SchemaRef(657, false, 0), // SettlCurrOfferFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(12, false, 0), // Commission
		new SchemaRef(13, false, 0), // CommType
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(100, false, 0), // ExDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// Confirmation
	static readonly SchemaRef[] M70 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(664, true, 0), // ConfirmID
		new SchemaRef(772, false, 0), // ConfirmRefID
		new SchemaRef(859, false, 0), // ConfirmReqID
		new SchemaRef(666, true, 0), // ConfirmTransType
		new SchemaRef(773, true, 0), // ConfirmType
		new SchemaRef(797, false, 0), // CopyMsgIndicator
		new SchemaRef(650, false, 0), // LegalConfirm
		new SchemaRef(665, true, 0), // ConfirmStatus
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(2036, false, 2), // OrdAllocGrp
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(75, true, 0), // TradeDate
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, true, 2), // UndInstrmtGrp
		new SchemaRef(2019, true, 2), // InstrmtLegGrp
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(80, true, 0), // AllocQty
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(2013, true, 2), // CpctyConfGrp
		new SchemaRef(79, true, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(798, false, 0), // AllocAccountType
		new SchemaRef(6, true, 0), // AvgPx
		new SchemaRef(74, false, 0), // AvgPxPrecision
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(860, false, 0), // AvgParPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(861, false, 0), // ReportedPx
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(81, false, 0), // ProcessCode
		new SchemaRef(381, true, 0), // GrossTradeAmt
		new SchemaRef(157, false, 0), // NumDaysInterest
		new SchemaRef(230, false, 0), // ExDate
		new SchemaRef(158, false, 0), // AccruedInterestRate
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(738, false, 0), // InterestAtMaturity
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(238, false, 0), // Concession
		new SchemaRef(237, false, 0), // TotalTakedown
		new SchemaRef(118, true, 0), // NetMoney
		new SchemaRef(890, false, 0), // MaturityNetMoney
		new SchemaRef(119, false, 0), // SettlCurrAmt
		new SchemaRef(120, false, 0), // SettlCurrency
		new SchemaRef(155, false, 0), // SettlCurrFxRate
		new SchemaRef(156, false, 0), // SettlCurrFxRateCalc
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(1016, false, 1), // SettlInstructionsData
		new SchemaRef(1000, false, 1), // CommissionData
		new SchemaRef(858, false, 0), // SharedCommission
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// PositionMaintenanceRequest
	static readonly SchemaRef[] M71 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(710, true, 0), // PosReqID
		new SchemaRef(709, true, 0), // PosTransType
		new SchemaRef(712, true, 0), // PosMaintAction
		new SchemaRef(713, false, 0), // OrigPosReqRefID
		new SchemaRef(714, false, 0), // PosMaintRptRefID
		new SchemaRef(715, true, 0), // ClearingBusinessDate
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(1012, true, 2), // Parties
		new SchemaRef(1, true, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1015, true, 2), // PositionQty
		new SchemaRef(718, false, 0), // AdjustmentType
		new SchemaRef(719, false, 0), // ContraryInstructionIndicator
		new SchemaRef(720, false, 0), // PriorSpreadIndicator
		new SchemaRef(834, false, 0), // ThresholdAmount
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// PositionMaintenanceReport
	static readonly SchemaRef[] M72 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(721, true, 0), // PosMaintRptID
		new SchemaRef(709, true, 0), // PosTransType
		new SchemaRef(710, false, 0), // PosReqID
		new SchemaRef(712, true, 0), // PosMaintAction
		new SchemaRef(713, true, 0), // OrigPosReqRefID
		new SchemaRef(722, true, 0), // PosMaintStatus
		new SchemaRef(723, false, 0), // PosMaintResult
		new SchemaRef(715, true, 0), // ClearingBusinessDate
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, true, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1015, true, 2), // PositionQty
		new SchemaRef(1014, true, 2), // PositionAmountData
		new SchemaRef(718, false, 0), // AdjustmentType
		new SchemaRef(834, false, 0), // ThresholdAmount
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// RequestForPositions
	static readonly SchemaRef[] M73 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(710, true, 0), // PosReqID
		new SchemaRef(724, true, 0), // PosReqType
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(1012, true, 2), // Parties
		new SchemaRef(1, true, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(715, true, 0), // ClearingBusinessDate
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(2064, false, 2), // TrdgSesGrp
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// RequestForPositionsAck
	static readonly SchemaRef[] M74 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(721, true, 0), // PosMaintRptID
		new SchemaRef(710, false, 0), // PosReqID
		new SchemaRef(727, false, 0), // TotalNumPosReports
		new SchemaRef(325, false, 0), // UnsolicitedIndicator
		new SchemaRef(728, true, 0), // PosReqResult
		new SchemaRef(729, true, 0), // PosReqStatus
		new SchemaRef(1012, true, 2), // Parties
		new SchemaRef(1, true, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// PositionReport
	static readonly SchemaRef[] M75 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(721, true, 0), // PosMaintRptID
		new SchemaRef(710, false, 0), // PosReqID
		new SchemaRef(724, false, 0), // PosReqType
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(727, false, 0), // TotalNumPosReports
		new SchemaRef(325, false, 0), // UnsolicitedIndicator
		new SchemaRef(728, true, 0), // PosReqResult
		new SchemaRef(715, true, 0), // ClearingBusinessDate
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(1012, true, 2), // Parties
		new SchemaRef(1, true, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(730, true, 0), // SettlPrice
		new SchemaRef(731, true, 0), // SettlPriceType
		new SchemaRef(734, true, 0), // PriorSettlPrice
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2038, false, 2), // PosUndInstrmtGrp
		new SchemaRef(1015, true, 2), // PositionQty
		new SchemaRef(1014, true, 2), // PositionAmountData
		new SchemaRef(506, false, 0), // RegistStatus
		new SchemaRef(743, false, 0), // DeliveryDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradeCaptureReportRequestAck
	static readonly SchemaRef[] M76 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(568, true, 0), // TradeRequestID
		new SchemaRef(569, true, 0), // TradeRequestType
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(748, false, 0), // TotNumTradeReports
		new SchemaRef(749, true, 0), // TradeRequestResult
		new SchemaRef(750, true, 0), // TradeRequestStatus
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(442, false, 0), // MultiLegReportingType
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// TradeCaptureReportAck
	static readonly SchemaRef[] M77 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(571, true, 0), // TradeReportID
		new SchemaRef(487, false, 0), // TradeReportTransType
		new SchemaRef(856, false, 0), // TradeReportType
		new SchemaRef(828, false, 0), // TrdType
		new SchemaRef(829, false, 0), // TrdSubType
		new SchemaRef(855, false, 0), // SecondaryTrdType
		new SchemaRef(830, false, 0), // TransferReason
		new SchemaRef(150, true, 0), // ExecType
		new SchemaRef(572, false, 0), // TradeReportRefID
		new SchemaRef(881, false, 0), // SecondaryTradeReportRefID
		new SchemaRef(939, false, 0), // TrdRptStatus
		new SchemaRef(751, false, 0), // TradeReportRejectReason
		new SchemaRef(818, false, 0), // SecondaryTradeReportID
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(820, false, 0), // TradeLinkID
		new SchemaRef(880, false, 0), // TrdMatchID
		new SchemaRef(17, false, 0), // ExecID
		new SchemaRef(527, false, 0), // SecondaryExecID
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(2063, false, 2), // TrdInstrmtLegGrp
		new SchemaRef(635, false, 0), // ClearingFeeIndicator
		new SchemaRef(528, false, 0), // OrderCapacity
		new SchemaRef(529, false, 0), // OrderRestrictions
		new SchemaRef(582, false, 0), // CustOrderCapacity
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(660, false, 0), // AcctIDSource
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(591, false, 0), // PreallocMethod
		new SchemaRef(2060, false, 2), // TrdAllocGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// AllocationReport
	static readonly SchemaRef[] M78 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(755, true, 0), // AllocReportID
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(71, true, 0), // AllocTransType
		new SchemaRef(795, false, 0), // AllocReportRefID
		new SchemaRef(796, false, 0), // AllocCancReplaceReason
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(794, true, 0), // AllocReportType
		new SchemaRef(87, true, 0), // AllocStatus
		new SchemaRef(88, false, 0), // AllocRejCode
		new SchemaRef(72, false, 0), // RefAllocID
		new SchemaRef(808, false, 0), // AllocIntermedReqType
		new SchemaRef(196, false, 0), // AllocLinkID
		new SchemaRef(197, false, 0), // AllocLinkType
		new SchemaRef(466, false, 0), // BookingRefID
		new SchemaRef(857, true, 0), // AllocNoOrdersType
		new SchemaRef(2036, false, 2), // OrdAllocGrp
		new SchemaRef(2014, false, 2), // ExecAllocGrp
		new SchemaRef(570, false, 0), // PreviouslyReported
		new SchemaRef(700, false, 0), // ReversalIndicator
		new SchemaRef(574, false, 0), // MatchType
		new SchemaRef(54, true, 0), // Side
		new SchemaRef(1003, true, 1), // Instrument
		new SchemaRef(1004, false, 1), // InstrumentExtension
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(53, true, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(30, false, 0), // LastMkt
		new SchemaRef(229, false, 0), // TradeOriginationDate
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(6, true, 0), // AvgPx
		new SchemaRef(860, false, 0), // AvgParPx
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(74, false, 0), // AvgPxPrecision
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(75, true, 0), // TradeDate
		new SchemaRef(60, false, 0), // TransactTime
		new SchemaRef(63, false, 0), // SettlType
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(775, false, 0), // BookingType
		new SchemaRef(381, false, 0), // GrossTradeAmt
		new SchemaRef(238, false, 0), // Concession
		new SchemaRef(237, false, 0), // TotalTakedown
		new SchemaRef(118, false, 0), // NetMoney
		new SchemaRef(77, false, 0), // PositionEffect
		new SchemaRef(754, false, 0), // AutoAcceptIndicator
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(157, false, 0), // NumDaysInterest
		new SchemaRef(158, false, 0), // AccruedInterestRate
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(540, false, 0), // TotalAccruedInterestAmt
		new SchemaRef(738, false, 0), // InterestAtMaturity
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(650, false, 0), // LegalConfirm
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1022, false, 1), // YieldData
		new SchemaRef(892, false, 0), // TotNoAllocs
		new SchemaRef(893, false, 0), // LastFragment
		new SchemaRef(2003, false, 2), // AllocGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// AllocationReportAck
	static readonly SchemaRef[] M79 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(755, true, 0), // AllocReportID
		new SchemaRef(70, true, 0), // AllocID
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(75, false, 0), // TradeDate
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(87, true, 0), // AllocStatus
		new SchemaRef(88, false, 0), // AllocRejCode
		new SchemaRef(794, false, 0), // AllocReportType
		new SchemaRef(808, false, 0), // AllocIntermedReqType
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(2002, false, 2), // AllocAckGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ConfirmationAck
	static readonly SchemaRef[] M80 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(664, true, 0), // ConfirmID
		new SchemaRef(75, true, 0), // TradeDate
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(940, true, 0), // AffirmStatus
		new SchemaRef(774, false, 0), // ConfirmRejReason
		new SchemaRef(573, false, 0), // MatchStatus
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// SettlementInstructionRequest
	static readonly SchemaRef[] M81 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(791, true, 0), // SettlInstReqID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(460, false, 0), // Product
		new SchemaRef(167, false, 0), // SecurityType
		new SchemaRef(461, false, 0), // CFICode
		new SchemaRef(168, false, 0), // EffectiveTime
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(779, false, 0), // LastUpdateTime
		new SchemaRef(169, false, 0), // StandInstDbType
		new SchemaRef(170, false, 0), // StandInstDbName
		new SchemaRef(171, false, 0), // StandInstDbID
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// AssignmentReport
	static readonly SchemaRef[] M82 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(833, true, 0), // AsgnRptID
		new SchemaRef(832, false, 0), // TotNumAssignmentReports
		new SchemaRef(912, false, 0), // LastRptRequested
		new SchemaRef(1012, true, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, true, 0), // AccountType
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(1015, true, 2), // PositionQty
		new SchemaRef(1014, true, 2), // PositionAmountData
		new SchemaRef(834, false, 0), // ThresholdAmount
		new SchemaRef(730, true, 0), // SettlPrice
		new SchemaRef(731, true, 0), // SettlPriceType
		new SchemaRef(732, true, 0), // UnderlyingSettlPrice
		new SchemaRef(432, false, 0), // ExpireDate
		new SchemaRef(744, true, 0), // AssignmentMethod
		new SchemaRef(745, false, 0), // AssignmentUnit
		new SchemaRef(746, true, 0), // OpenInterest
		new SchemaRef(747, true, 0), // ExerciseMethod
		new SchemaRef(716, true, 0), // SettlSessID
		new SchemaRef(717, true, 0), // SettlSessSubID
		new SchemaRef(715, true, 0), // ClearingBusinessDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralRequest
	static readonly SchemaRef[] M83 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(894, true, 0), // CollReqID
		new SchemaRef(895, true, 0), // CollAsgnReason
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2065, false, 2), // UndInstrmtCollGrp
		new SchemaRef(899, false, 0), // MarginExcess
		new SchemaRef(900, false, 0), // TotalNetValue
		new SchemaRef(901, false, 0), // CashOutstanding
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralAssignment
	static readonly SchemaRef[] M84 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(902, true, 0), // CollAsgnID
		new SchemaRef(894, false, 0), // CollReqID
		new SchemaRef(895, true, 0), // CollAsgnReason
		new SchemaRef(903, true, 0), // CollAsgnTransType
		new SchemaRef(907, false, 0), // CollAsgnRefID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(126, false, 0), // ExpireTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2065, false, 2), // UndInstrmtCollGrp
		new SchemaRef(899, false, 0), // MarginExcess
		new SchemaRef(900, false, 0), // TotalNetValue
		new SchemaRef(901, false, 0), // CashOutstanding
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1016, false, 1), // SettlInstructionsData
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralResponse
	static readonly SchemaRef[] M85 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(904, true, 0), // CollRespID
		new SchemaRef(902, true, 0), // CollAsgnID
		new SchemaRef(894, false, 0), // CollReqID
		new SchemaRef(895, true, 0), // CollAsgnReason
		new SchemaRef(903, false, 0), // CollAsgnTransType
		new SchemaRef(905, true, 0), // CollAsgnRespType
		new SchemaRef(906, false, 0), // CollAsgnRejectReason
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2065, false, 2), // UndInstrmtCollGrp
		new SchemaRef(899, false, 0), // MarginExcess
		new SchemaRef(900, false, 0), // TotalNetValue
		new SchemaRef(901, false, 0), // CashOutstanding
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralReport
	static readonly SchemaRef[] M86 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(908, true, 0), // CollRptID
		new SchemaRef(909, false, 0), // CollInquiryID
		new SchemaRef(910, true, 0), // CollStatus
		new SchemaRef(911, false, 0), // TotNumReports
		new SchemaRef(912, false, 0), // LastRptRequested
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(899, false, 0), // MarginExcess
		new SchemaRef(900, false, 0), // TotalNetValue
		new SchemaRef(901, false, 0), // CashOutstanding
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(2035, false, 2), // MiscFeesGrp
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1016, false, 1), // SettlInstructionsData
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralInquiry
	static readonly SchemaRef[] M87 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(909, true, 0), // CollInquiryID
		new SchemaRef(2008, false, 2), // CollInqQualGrp
		new SchemaRef(263, false, 0), // SubscriptionRequestType
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(899, false, 0), // MarginExcess
		new SchemaRef(900, false, 0), // TotalNetValue
		new SchemaRef(901, false, 0), // CashOutstanding
		new SchemaRef(1020, false, 2), // TrdRegTimestamps
		new SchemaRef(54, false, 0), // Side
		new SchemaRef(44, false, 0), // Price
		new SchemaRef(423, false, 0), // PriceType
		new SchemaRef(159, false, 0), // AccruedInterestAmt
		new SchemaRef(920, false, 0), // EndAccruedInterestAmt
		new SchemaRef(921, false, 0), // StartCash
		new SchemaRef(922, false, 0), // EndCash
		new SchemaRef(1018, false, 1), // SpreadOrBenchmarkCurveData
		new SchemaRef(1019, false, 2), // Stipulations
		new SchemaRef(1016, false, 1), // SettlInstructionsData
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NetworkCounterpartySystemStatusRequest
	static readonly SchemaRef[] M88 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(935, true, 0), // NetworkRequestType
		new SchemaRef(933, true, 0), // NetworkRequestID
		new SchemaRef(2009, false, 2), // CompIDReqGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// NetworkCounterpartySystemStatusResponse
	static readonly SchemaRef[] M89 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(937, true, 0), // NetworkStatusResponseType
		new SchemaRef(933, false, 0), // NetworkRequestID
		new SchemaRef(932, true, 0), // NetworkResponseID
		new SchemaRef(934, false, 0), // LastNetworkResponseID
		new SchemaRef(2010, true, 2), // CompIDStatGrp
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// UserRequest
	static readonly SchemaRef[] M90 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(923, true, 0), // UserRequestID
		new SchemaRef(924, true, 0), // UserRequestType
		new SchemaRef(553, true, 0), // Username
		new SchemaRef(554, false, 0), // Password
		new SchemaRef(925, false, 0), // NewPassword
		new SchemaRef(95, false, 0), // RawDataLength
		new SchemaRef(96, false, 0), // RawData
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// UserResponse
	static readonly SchemaRef[] M91 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(923, true, 0), // UserRequestID
		new SchemaRef(553, true, 0), // Username
		new SchemaRef(926, false, 0), // UserStatus
		new SchemaRef(927, false, 0), // UserStatusText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// CollateralInquiryAck
	static readonly SchemaRef[] M92 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(909, true, 0), // CollInquiryID
		new SchemaRef(945, true, 0), // CollInquiryStatus
		new SchemaRef(946, false, 0), // CollInquiryResult
		new SchemaRef(2008, false, 2), // CollInqQualGrp
		new SchemaRef(911, false, 0), // TotNumReports
		new SchemaRef(1012, false, 2), // Parties
		new SchemaRef(1, false, 0), // Account
		new SchemaRef(581, false, 0), // AccountType
		new SchemaRef(11, false, 0), // ClOrdID
		new SchemaRef(37, false, 0), // OrderID
		new SchemaRef(198, false, 0), // SecondaryOrderID
		new SchemaRef(526, false, 0), // SecondaryClOrdID
		new SchemaRef(2015, false, 2), // ExecCollGrp
		new SchemaRef(2062, false, 2), // TrdCollGrp
		new SchemaRef(1003, false, 1), // Instrument
		new SchemaRef(1002, false, 1), // FinancingDetails
		new SchemaRef(64, false, 0), // SettlDate
		new SchemaRef(53, false, 0), // Quantity
		new SchemaRef(854, false, 0), // QtyType
		new SchemaRef(15, false, 0), // Currency
		new SchemaRef(2019, false, 2), // InstrmtLegGrp
		new SchemaRef(2066, false, 2), // UndInstrmtGrp
		new SchemaRef(336, false, 0), // TradingSessionID
		new SchemaRef(625, false, 0), // TradingSessionSubID
		new SchemaRef(716, false, 0), // SettlSessID
		new SchemaRef(717, false, 0), // SettlSessSubID
		new SchemaRef(715, false, 0), // ClearingBusinessDate
		new SchemaRef(725, false, 0), // ResponseTransportType
		new SchemaRef(726, false, 0), // ResponseDestination
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	// ConfirmationRequest
	static readonly SchemaRef[] M93 =
	[
		new SchemaRef(1024, true, 1), // StandardHeader
		new SchemaRef(859, true, 0), // ConfirmReqID
		new SchemaRef(773, true, 0), // ConfirmType
		new SchemaRef(2036, false, 2), // OrdAllocGrp
		new SchemaRef(70, false, 0), // AllocID
		new SchemaRef(793, false, 0), // SecondaryAllocID
		new SchemaRef(467, false, 0), // IndividualAllocID
		new SchemaRef(60, true, 0), // TransactTime
		new SchemaRef(79, false, 0), // AllocAccount
		new SchemaRef(661, false, 0), // AllocAcctIDSource
		new SchemaRef(798, false, 0), // AllocAccountType
		new SchemaRef(58, false, 0), // Text
		new SchemaRef(354, false, 0), // EncodedTextLen
		new SchemaRef(355, false, 0), // EncodedText
		new SchemaRef(1025, true, 1), // StandardTrailer
	];

	public static SchemaRef[] Component(int id) => id switch
	{
		1000 => C1000,
		1001 => C1001,
		1002 => C1002,
		1003 => C1003,
		1004 => C1004,
		1005 => C1005,
		1006 => C1006,
		1011 => C1011,
		1013 => C1013,
		1016 => C1016,
		1018 => C1018,
		1021 => C1021,
		1022 => C1022,
		1024 => C1024,
		1025 => C1025,
		_    => throw new InvalidOperationException("Unknown component."),
	};

	public static SchemaRef[] Group(int id) => id switch
	{
		1007 => G1007,
		1008 => G1008,
		1009 => G1009,
		1010 => G1010,
		1012 => G1012,
		1014 => G1014,
		1015 => G1015,
		1017 => G1017,
		1019 => G1019,
		1020 => G1020,
		1023 => G1023,
		2001 => G2001,
		2002 => G2002,
		2003 => G2003,
		2004 => G2004,
		2005 => G2005,
		2006 => G2006,
		2007 => G2007,
		2008 => G2008,
		2009 => G2009,
		2010 => G2010,
		2011 => G2011,
		2012 => G2012,
		2013 => G2013,
		2014 => G2014,
		2015 => G2015,
		2016 => G2016,
		2017 => G2017,
		2018 => G2018,
		2019 => G2019,
		2020 => G2020,
		2021 => G2021,
		2022 => G2022,
		2023 => G2023,
		2024 => G2024,
		2025 => G2025,
		2026 => G2026,
		2027 => G2027,
		2028 => G2028,
		2029 => G2029,
		2030 => G2030,
		2031 => G2031,
		2032 => G2032,
		2033 => G2033,
		2034 => G2034,
		2035 => G2035,
		2036 => G2036,
		2037 => G2037,
		2038 => G2038,
		2039 => G2039,
		2040 => G2040,
		2041 => G2041,
		2042 => G2042,
		2043 => G2043,
		2044 => G2044,
		2045 => G2045,
		2046 => G2046,
		2047 => G2047,
		2048 => G2048,
		2049 => G2049,
		2050 => G2050,
		2051 => G2051,
		2052 => G2052,
		2053 => G2053,
		2054 => G2054,
		2055 => G2055,
		2056 => G2056,
		2057 => G2057,
		2058 => G2058,
		2059 => G2059,
		2060 => G2060,
		2061 => G2061,
		2062 => G2062,
		2063 => G2063,
		2064 => G2064,
		2065 => G2065,
		2066 => G2066,
		2067 => G2067,
		2069 => G2069,
		2070 => G2070,
		2071 => G2071,
		2072 => G2072,
		2073 => G2073,
		2074 => G2074,
		2075 => G2075,
		2076 => G2076,
		2077 => G2077,
		2078 => G2078,
		2079 => G2079,
		2080 => G2080,
		2085 => G2085,
		2098 => G2098,
		_ => throw new InvalidOperationException("Unknown group."),
	};

	public static SchemaRef[] Message(string type) => type switch
	{
		"0" => M1,
		"1" => M2,
		"2" => M3,
		"3" => M4,
		"4" => M5,
		"5" => M6,
		"6" => M7,
		"7" => M8,
		"8" => M9,
		"9" => M10,
		"A" => M11,
		"B" => M12,
		"C" => M13,
		"D" => M14,
		"E" => M15,
		"F" => M16,
		"G" => M17,
		"H" => M18,
		"J" => M19,
		"K" => M20,
		"L" => M21,
		"M" => M22,
		"N" => M23,
		"P" => M24,
		"Q" => M25,
		"R" => M26,
		"S" => M27,
		"T" => M28,
		"V" => M29,
		"W" => M30,
		"X" => M31,
		"Y" => M32,
		"Z" => M33,
		"a" => M34,
		"b" => M35,
		"c" => M36,
		"d" => M37,
		"e" => M38,
		"f" => M39,
		"g" => M40,
		"h" => M41,
		"i" => M42,
		"j" => M43,
		"k" => M44,
		"l" => M45,
		"m" => M46,
		"n" => M47,
		"o" => M48,
		"p" => M49,
		"q" => M50,
		"r" => M51,
		"s" => M52,
		"t" => M53,
		"u" => M54,
		"v" => M55,
		"w" => M56,
		"x" => M57,
		"y" => M58,
		"z" => M59,
		"AA" => M60,
		"AB" => M61,
		"AC" => M62,
		"AD" => M63,
		"AE" => M64,
		"AF" => M65,
		"AG" => M66,
		"AH" => M67,
		"AI" => M68,
		"AJ" => M69,
		"AK" => M70,
		"AL" => M71,
		"AM" => M72,
		"AN" => M73,
		"AO" => M74,
		"AP" => M75,
		"AQ" => M76,
		"AR" => M77,
		"AS" => M78,
		"AT" => M79,
		"AU" => M80,
		"AV" => M81,
		"AW" => M82,
		"AX" => M83,
		"AY" => M84,
		"AZ" => M85,
		"BA" => M86,
		"BB" => M87,
		"BC" => M88,
		"BD" => M89,
		"BE" => M90,
		"BF" => M91,
		"BG" => M92,
		"BH" => M93,
		_    => Array.Empty<SchemaRef>(),
	};

	public static int Counter(int group) => group switch
	{
		1007 => 683,
		1008 => 539,
		1009 => 756,
		1010 => 948,
		1012 => 453,
		1014 => 753,
		1015 => 702,
		1017 => 781,
		1019 => 232,
		1020 => 768,
		1023 => 887,
		2001 => 534,
		2002 => 78,
		2003 => 78,
		2004 => 420,
		2005 => 420,
		2006 => 398,
		2007 => 576,
		2008 => 938,
		2009 => 936,
		2010 => 936,
		2011 => 518,
		2012 => 382,
		2013 => 862,
		2014 => 124,
		2015 => 124,
		2016 => 124,
		2017 => 146,
		2018 => 555,
		2019 => 555,
		2020 => 555,
		2021 => 555,
		2022 => 146,
		2023 => 428,
		2024 => 199,
		2025 => 555,
		2026 => 670,
		2027 => 555,
		2028 => 555,
		2029 => 33,
		2030 => 73,
		2031 => 268,
		2032 => 268,
		2033 => 267,
		2034 => 816,
		2035 => 136,
		2036 => 73,
		2037 => 73,
		2038 => 711,
		2039 => 78,
		2040 => 78,
		2041 => 295,
		2042 => 295,
		2043 => 295,
		2044 => 735,
		2045 => 146,
		2046 => 555,
		2047 => 146,
		2048 => 296,
		2049 => 296,
		2050 => 146,
		2051 => 146,
		2052 => 510,
		2053 => 473,
		2054 => 215,
		2055 => 146,
		2056 => 558,
		2057 => 778,
		2058 => 552,
		2059 => 552,
		2060 => 78,
		2061 => 552,
		2062 => 897,
		2063 => 555,
		2064 => 386,
		2065 => 711,
		2066 => 711,
		2067 => 711,
		2069 => 580,
		2070 => 864,
		2071 => 454,
		2072 => 604,
		2073 => 457,
		2074 => 870,
		2075 => 85,
		2076 => 801,
		2077 => 802,
		2078 => 804,
		2079 => 806,
		2080 => 952,
		2085 => 627,
		2098 => 384,
		_    => 0,
	};

	public static int LengthTag(int dataTag) => dataTag switch
	{
		 89 =>  93,
		 91 =>  90,
		 96 =>  95,
		213 => 212,
		349 => 348,
		351 => 350,
		353 => 352,
		355 => 354,
		357 => 356,
		359 => 358,
		361 => 360,
		363 => 362,
		365 => 364,
		446 => 445,
		619 => 618,
		622 => 621,
		_   => 0,
	};

	public static int DataTag(int lengthTag) => lengthTag switch
	{
		 93 =>  89,
		 90 =>  91,
		 95 =>  96,
		212 => 213,
		348 => 349,
		350 => 351,
		352 => 353,
		354 => 355,
		356 => 357,
		358 => 359,
		360 => 361,
		362 => 363,
		364 => 365,
		445 => 446,
		618 => 619,
		621 => 622,
		_ => 0,
	};

	public static string? Type(int tag) => tag switch
	{
		 1 => "String",
		 2 => "String",
		 3 => "String",
		 4 => "char",
		 5 => "String",
		 6 => "Price",
		 7 => "SeqNum",
		 8 => "String",
		 9 => "Length",
		10 => "String",
		11 => "String",
		12 => "Amt",
		13 => "char",
		14 => "Qty",
		15 => "Currency",
		16 => "SeqNum",
		17 => "String",
		18 => "MultipleValueString",
		19 => "String",
		21 => "char",
		22 => "String",
		23 => "String",
		25 => "char",
		26 => "String",
		27 => "String",
		28 => "char",
		29 => "char",
		30 => "Exchange",
		31 => "Price",
		32 => "Qty",
		33 => "NumInGroup",
		34 => "SeqNum",
		35 => "String",
		36 => "SeqNum",
		37 => "String",
		38 => "Qty",
		39 => "char",
		40 => "char",
		41 => "String",
		42 => "UTCTimestamp",
		43 => "Boolean",
		44 => "Price",
		45 => "SeqNum",
		48 => "String",
		49 => "String",
		50 => "String",
		52 => "UTCTimestamp",
		53 => "Qty",
		54 => "char",
		55 => "String",
		56 => "String",
		57 => "String",
		58 => "String",
		59 => "char",
		60 => "UTCTimestamp",
		61 => "char",
		62 => "UTCTimestamp",
		63 => "char",
		64 => "LocalMktDate",
		65 => "String",
		66 => "String",
		67 => "int",
		68 => "int",
		69 => "String",
		70 => "String",
		71 => "char",
		72 => "String",
		73 => "NumInGroup",
		74 => "int",
		75 => "LocalMktDate",
		77 => "char",
		78 => "NumInGroup",
		79 => "String",
		80 => "Qty",
		81 => "char",
		82 => "int",
		83 => "int",
		84 => "Qty",
		85 => "NumInGroup",
		87 => "int",
		88 => "int",
		89 => "data",
		90 => "Length",
		91 => "data",
		93 => "Length",
		94 => "char",
		95 => "Length",
		96 => "data",
		97 => "Boolean",
		98 => "int",
		99 => "Price",
		100 => "Exchange",
		102 => "int",
		103 => "int",
		104 => "char",
		106 => "String",
		107 => "String",
		108 => "int",
		110 => "Qty",
		111 => "Qty",
		112 => "String",
		113 => "Boolean",
		114 => "Boolean",
		115 => "String",
		116 => "String",
		117 => "String",
		118 => "Amt",
		119 => "Amt",
		120 => "Currency",
		121 => "Boolean",
		122 => "UTCTimestamp",
		123 => "Boolean",
		124 => "NumInGroup",
		126 => "UTCTimestamp",
		127 => "char",
		128 => "String",
		129 => "String",
		130 => "Boolean",
		131 => "String",
		132 => "Price",
		133 => "Price",
		134 => "Qty",
		135 => "Qty",
		136 => "NumInGroup",
		137 => "Amt",
		138 => "Currency",
		139 => "String",
		140 => "Price",
		141 => "Boolean",
		142 => "String",
		143 => "String",
		144 => "String",
		145 => "String",
		146 => "NumInGroup",
		147 => "String",
		148 => "String",
		149 => "String",
		150 => "char",
		151 => "Qty",
		152 => "Qty",
		153 => "Price",
		154 => "Amt",
		155 => "float",
		156 => "char",
		157 => "int",
		158 => "Percentage",
		159 => "Amt",
		160 => "char",
		161 => "String",
		162 => "String",
		163 => "char",
		164 => "String",
		165 => "char",
		167 => "String",
		168 => "UTCTimestamp",
		169 => "int",
		170 => "String",
		171 => "String",
		172 => "int",
		188 => "Price",
		189 => "PriceOffset",
		190 => "Price",
		191 => "PriceOffset",
		192 => "Qty",
		193 => "LocalMktDate",
		194 => "Price",
		195 => "PriceOffset",
		196 => "String",
		197 => "int",
		198 => "String",
		199 => "NumInGroup",
		200 => "MonthYear",
		201 => "int",
		202 => "Price",
		203 => "int",
		206 => "char",
		207 => "Exchange",
		208 => "Boolean",
		209 => "int",
		210 => "Qty",
		211 => "float",
		212 => "Length",
		213 => "data",
		214 => "String",
		215 => "NumInGroup",
		216 => "int",
		217 => "String",
		218 => "PriceOffset",
		220 => "Currency",
		221 => "String",
		222 => "String",
		223 => "Percentage",
		224 => "LocalMktDate",
		225 => "LocalMktDate",
		226 => "int",
		227 => "Percentage",
		228 => "float",
		229 => "LocalMktDate",
		230 => "LocalMktDate",
		231 => "float",
		232 => "NumInGroup",
		233 => "String",
		234 => "String",
		235 => "String",
		236 => "Percentage",
		237 => "Amt",
		238 => "Amt",
		239 => "String",
		240 => "LocalMktDate",
		241 => "LocalMktDate",
		242 => "LocalMktDate",
		243 => "String",
		244 => "int",
		245 => "Percentage",
		246 => "float",
		247 => "LocalMktDate",
		248 => "LocalMktDate",
		249 => "LocalMktDate",
		250 => "String",
		251 => "int",
		252 => "Percentage",
		253 => "float",
		254 => "LocalMktDate",
		255 => "String",
		256 => "String",
		257 => "String",
		258 => "Boolean",
		259 => "LocalMktDate",
		260 => "Price",
		262 => "String",
		263 => "char",
		264 => "int",
		265 => "int",
		266 => "Boolean",
		267 => "NumInGroup",
		268 => "NumInGroup",
		269 => "char",
		270 => "Price",
		271 => "Qty",
		272 => "UTCDateOnly",
		273 => "UTCTimeOnly",
		274 => "char",
		275 => "Exchange",
		276 => "MultipleValueString",
		277 => "MultipleValueString",
		278 => "String",
		279 => "char",
		280 => "String",
		281 => "char",
		282 => "String",
		283 => "String",
		284 => "String",
		285 => "char",
		286 => "MultipleValueString",
		287 => "int",
		288 => "String",
		289 => "String",
		290 => "int",
		291 => "MultipleValueString",
		292 => "MultipleValueString",
		293 => "Qty",
		294 => "Qty",
		295 => "NumInGroup",
		296 => "NumInGroup",
		297 => "int",
		298 => "int",
		299 => "String",
		300 => "int",
		301 => "int",
		302 => "String",
		303 => "int",
		304 => "int",
		305 => "String",
		306 => "String",
		307 => "String",
		308 => "Exchange",
		309 => "String",
		310 => "String",
		311 => "String",
		312 => "String",
		313 => "MonthYear",
		315 => "int",
		316 => "Price",
		317 => "char",
		318 => "Currency",
		320 => "String",
		321 => "int",
		322 => "String",
		323 => "int",
		324 => "String",
		325 => "Boolean",
		326 => "int",
		327 => "char",
		328 => "Boolean",
		329 => "Boolean",
		330 => "Qty",
		331 => "Qty",
		332 => "Price",
		333 => "Price",
		334 => "int",
		335 => "String",
		336 => "String",
		337 => "String",
		338 => "int",
		339 => "int",
		340 => "int",
		341 => "UTCTimestamp",
		342 => "UTCTimestamp",
		343 => "UTCTimestamp",
		344 => "UTCTimestamp",
		345 => "UTCTimestamp",
		346 => "int",
		347 => "String",
		348 => "Length",
		349 => "data",
		350 => "Length",
		351 => "data",
		352 => "Length",
		353 => "data",
		354 => "Length",
		355 => "data",
		356 => "Length",
		357 => "data",
		358 => "Length",
		359 => "data",
		360 => "Length",
		361 => "data",
		362 => "Length",
		363 => "data",
		364 => "Length",
		365 => "data",
		366 => "Price",
		367 => "UTCTimestamp",
		368 => "int",
		369 => "SeqNum",
		371 => "int",
		372 => "String",
		373 => "int",
		374 => "char",
		375 => "String",
		376 => "String",
		377 => "Boolean",
		378 => "int",
		379 => "String",
		380 => "int",
		381 => "Amt",
		382 => "NumInGroup",
		383 => "Length",
		384 => "NumInGroup",
		385 => "char",
		386 => "NumInGroup",
		387 => "Qty",
		388 => "char",
		389 => "float",
		390 => "String",
		391 => "String",
		392 => "String",
		393 => "int",
		394 => "int",
		395 => "int",
		396 => "Amt",
		397 => "Amt",
		398 => "NumInGroup",
		399 => "int",
		400 => "String",
		401 => "int",
		402 => "Percentage",
		403 => "Percentage",
		404 => "Amt",
		405 => "Percentage",
		406 => "Amt",
		407 => "Percentage",
		408 => "Amt",
		409 => "int",
		410 => "Percentage",
		411 => "Boolean",
		412 => "Amt",
		413 => "Percentage",
		414 => "int",
		415 => "int",
		416 => "int",
		417 => "int",
		418 => "char",
		419 => "char",
		420 => "NumInGroup",
		421 => "Country",
		422 => "int",
		423 => "int",
		424 => "Qty",
		425 => "Qty",
		426 => "Price",
		427 => "int",
		428 => "NumInGroup",
		429 => "int",
		430 => "int",
		431 => "int",
		432 => "LocalMktDate",
		433 => "char",
		434 => "char",
		435 => "Percentage",
		436 => "float",
		437 => "Qty",
		438 => "UTCTimestamp",
		441 => "int",
		442 => "char",
		443 => "UTCTimestamp",
		444 => "String",
		445 => "Length",
		446 => "data",
		447 => "char",
		448 => "String",
		451 => "PriceOffset",
		452 => "int",
		453 => "NumInGroup",
		454 => "NumInGroup",
		455 => "String",
		456 => "String",
		457 => "NumInGroup",
		458 => "String",
		459 => "String",
		460 => "int",
		461 => "String",
		462 => "int",
		463 => "String",
		464 => "Boolean",
		466 => "String",
		467 => "String",
		468 => "char",
		469 => "float",
		470 => "Country",
		471 => "String",
		472 => "String",
		473 => "NumInGroup",
		474 => "String",
		475 => "Country",
		476 => "String",
		477 => "int",
		478 => "Currency",
		479 => "Currency",
		480 => "char",
		481 => "char",
		482 => "String",
		483 => "UTCTimestamp",
		484 => "char",
		485 => "float",
		486 => "LocalMktDate",
		487 => "int",
		488 => "String",
		489 => "String",
		490 => "LocalMktDate",
		491 => "String",
		492 => "int",
		493 => "String",
		494 => "String",
		495 => "int",
		496 => "String",
		497 => "char",
		498 => "String",
		499 => "String",
		500 => "String",
		501 => "String",
		502 => "String",
		503 => "LocalMktDate",
		504 => "LocalMktDate",
		505 => "String",
		506 => "char",
		507 => "int",
		508 => "String",
		509 => "String",
		510 => "NumInGroup",
		511 => "String",
		512 => "Percentage",
		513 => "String",
		514 => "char",
		515 => "UTCTimestamp",
		516 => "Percentage",
		517 => "char",
		518 => "NumInGroup",
		519 => "int",
		520 => "float",
		521 => "Currency",
		522 => "int",
		523 => "String",
		524 => "String",
		525 => "char",
		526 => "String",
		527 => "String",
		528 => "char",
		529 => "MultipleValueString",
		530 => "char",
		531 => "char",
		532 => "int",
		533 => "int",
		534 => "NumInGroup",
		535 => "String",
		536 => "String",
		537 => "int",
		538 => "int",
		539 => "NumInGroup",
		540 => "Amt",
		541 => "LocalMktDate",
		542 => "LocalMktDate",
		543 => "String",
		544 => "char",
		545 => "String",
		546 => "MultipleValueString",
		547 => "Boolean",
		548 => "String",
		549 => "int",
		550 => "int",
		551 => "String",
		552 => "NumInGroup",
		553 => "String",
		554 => "String",
		555 => "NumInGroup",
		556 => "Currency",
		557 => "int",
		558 => "NumInGroup",
		559 => "int",
		560 => "int",
		561 => "Qty",
		562 => "Qty",
		563 => "int",
		564 => "char",
		565 => "int",
		566 => "Price",
		567 => "int",
		568 => "String",
		569 => "int",
		570 => "Boolean",
		571 => "String",
		572 => "String",
		573 => "char",
		574 => "String",
		575 => "Boolean",
		576 => "NumInGroup",
		577 => "int",
		578 => "String",
		579 => "String",
		580 => "NumInGroup",
		581 => "int",
		582 => "int",
		583 => "String",
		584 => "String",
		585 => "int",
		586 => "UTCTimestamp",
		587 => "char",
		588 => "LocalMktDate",
		589 => "char",
		590 => "char",
		591 => "char",
		592 => "Country",
		593 => "String",
		594 => "String",
		595 => "String",
		596 => "Country",
		597 => "String",
		598 => "String",
		599 => "String",
		600 => "String",
		601 => "String",
		602 => "String",
		603 => "String",
		604 => "NumInGroup",
		605 => "String",
		606 => "String",
		607 => "int",
		608 => "String",
		609 => "String",
		610 => "MonthYear",
		611 => "LocalMktDate",
		612 => "Price",
		613 => "char",
		614 => "float",
		615 => "Percentage",
		616 => "Exchange",
		617 => "String",
		618 => "Length",
		619 => "data",
		620 => "String",
		621 => "Length",
		622 => "data",
		623 => "float",
		624 => "char",
		625 => "String",
		626 => "int",
		627 => "NumInGroup",
		628 => "String",
		629 => "UTCTimestamp",
		630 => "SeqNum",
		631 => "Price",
		632 => "Percentage",
		633 => "Percentage",
		634 => "Percentage",
		635 => "String",
		636 => "Boolean",
		637 => "Price",
		638 => "int",
		639 => "PriceOffset",
		640 => "Price",
		641 => "PriceOffset",
		642 => "PriceOffset",
		643 => "PriceOffset",
		644 => "String",
		645 => "Price",
		646 => "Price",
		647 => "Qty",
		648 => "Qty",
		649 => "String",
		650 => "Boolean",
		651 => "Price",
		652 => "Qty",
		654 => "String",
		655 => "String",
		656 => "float",
		657 => "float",
		658 => "int",
		659 => "String",
		660 => "int",
		661 => "int",
		662 => "Price",
		663 => "int",
		664 => "String",
		665 => "int",
		666 => "int",
		667 => "MonthYear",
		668 => "int",
		669 => "Price",
		670 => "NumInGroup",
		671 => "String",
		672 => "String",
		673 => "Qty",
		674 => "int",
		675 => "Currency",
		676 => "Currency",
		677 => "String",
		678 => "String",
		679 => "Price",
		680 => "int",
		681 => "Price",
		682 => "String",
		683 => "NumInGroup",
		684 => "Price",
		686 => "int",
		687 => "Qty",
		688 => "String",
		689 => "String",
		690 => "int",
		691 => "String",
		692 => "int",
		693 => "String",
		694 => "int",
		695 => "char",
		696 => "LocalMktDate",
		697 => "Price",
		698 => "int",
		699 => "String",
		700 => "Boolean",
		701 => "LocalMktDate",
		702 => "NumInGroup",
		703 => "String",
		704 => "Qty",
		705 => "Qty",
		706 => "int",
		707 => "String",
		708 => "Amt",
		709 => "int",
		710 => "String",
		711 => "NumInGroup",
		712 => "int",
		713 => "String",
		714 => "String",
		715 => "LocalMktDate",
		716 => "String",
		717 => "String",
		718 => "int",
		719 => "Boolean",
		720 => "Boolean",
		721 => "String",
		722 => "int",
		723 => "int",
		724 => "int",
		725 => "int",
		726 => "String",
		727 => "int",
		728 => "int",
		729 => "int",
		730 => "Price",
		731 => "int",
		732 => "Price",
		733 => "int",
		734 => "Price",
		735 => "NumInGroup",
		736 => "Currency",
		737 => "Amt",
		738 => "Amt",
		739 => "LocalMktDate",
		740 => "String",
		741 => "Amt",
		742 => "Amt",
		743 => "LocalMktDate",
		744 => "char",
		745 => "Qty",
		746 => "Amt",
		747 => "char",
		748 => "int",
		749 => "int",
		750 => "int",
		751 => "int",
		752 => "int",
		753 => "NumInGroup",
		754 => "Boolean",
		755 => "String",
		756 => "NumInGroup",
		757 => "String",
		758 => "char",
		759 => "int",
		760 => "String",
		761 => "String",
		762 => "String",
		763 => "String",
		764 => "String",
		765 => "Percentage",
		766 => "Amt",
		767 => "Currency",
		768 => "NumInGroup",
		769 => "UTCTimestamp",
		770 => "int",
		771 => "String",
		772 => "String",
		773 => "int",
		774 => "int",
		775 => "int",
		776 => "int",
		777 => "String",
		778 => "NumInGroup",
		779 => "UTCTimestamp",
		780 => "int",
		781 => "NumInGroup",
		782 => "String",
		783 => "char",
		784 => "int",
		785 => "String",
		786 => "int",
		787 => "char",
		788 => "int",
		789 => "SeqNum",
		790 => "String",
		791 => "String",
		792 => "int",
		793 => "String",
		794 => "int",
		795 => "String",
		796 => "int",
		797 => "Boolean",
		798 => "int",
		799 => "Price",
		800 => "Qty",
		801 => "NumInGroup",
		802 => "NumInGroup",
		803 => "int",
		804 => "NumInGroup",
		805 => "int",
		806 => "NumInGroup",
		807 => "int",
		808 => "int",
		810 => "Price",
		811 => "float",
		812 => "int",
		813 => "int",
		814 => "int",
		815 => "int",
		816 => "NumInGroup",
		817 => "String",
		818 => "String",
		819 => "int",
		820 => "String",
		821 => "String",
		822 => "String",
		823 => "String",
		824 => "String",
		825 => "String",
		826 => "int",
		827 => "int",
		828 => "int",
		829 => "int",
		830 => "String",
		832 => "int",
		833 => "String",
		834 => "PriceOffset",
		835 => "int",
		836 => "int",
		837 => "int",
		838 => "int",
		839 => "Price",
		840 => "int",
		841 => "int",
		842 => "int",
		843 => "int",
		844 => "int",
		845 => "Price",
		846 => "int",
		847 => "int",
		848 => "String",
		849 => "Percentage",
		850 => "float",
		851 => "int",
		852 => "Boolean",
		853 => "int",
		854 => "int",
		855 => "int",
		856 => "int",
		857 => "int",
		858 => "Amt",
		859 => "String",
		860 => "Price",
		861 => "Price",
		862 => "NumInGroup",
		863 => "Qty",
		864 => "NumInGroup",
		865 => "int",
		866 => "LocalMktDate",
		867 => "Price",
		868 => "String",
		869 => "Percentage",
		870 => "NumInGroup",
		871 => "int",
		872 => "String",
		873 => "LocalMktDate",
		874 => "LocalMktDate",
		875 => "int",
		876 => "String",
		877 => "String",
		878 => "String",
		879 => "Qty",
		880 => "String",
		881 => "String",
		882 => "Price",
		883 => "Price",
		884 => "Amt",
		885 => "Amt",
		886 => "Amt",
		887 => "NumInGroup",
		888 => "String",
		889 => "String",
		890 => "Amt",
		891 => "int",
		892 => "int",
		893 => "Boolean",
		894 => "String",
		895 => "int",
		896 => "int",
		897 => "NumInGroup",
		898 => "Percentage",
		899 => "Amt",
		900 => "Amt",
		901 => "Amt",
		902 => "String",
		903 => "int",
		904 => "String",
		905 => "int",
		906 => "int",
		907 => "String",
		908 => "String",
		909 => "String",
		910 => "int",
		911 => "int",
		912 => "Boolean",
		913 => "String",
		914 => "String",
		915 => "LocalMktDate",
		916 => "LocalMktDate",
		917 => "LocalMktDate",
		918 => "Currency",
		919 => "int",
		920 => "Amt",
		921 => "Amt",
		922 => "Amt",
		923 => "String",
		924 => "int",
		925 => "String",
		926 => "int",
		927 => "String",
		928 => "int",
		929 => "String",
		930 => "String",
		931 => "String",
		932 => "String",
		933 => "String",
		934 => "String",
		935 => "int",
		936 => "NumInGroup",
		937 => "int",
		938 => "NumInGroup",
		939 => "int",
		940 => "int",
		941 => "Currency",
		942 => "Currency",
		943 => "String",
		944 => "int",
		945 => "int",
		946 => "int",
		947 => "Currency",
		948 => "NumInGroup",
		949 => "String",
		950 => "char",
		951 => "int",
		952 => "NumInGroup",
		953 => "String",
		954 => "int",
		955 => "MonthYear",
		956 => "LocalMktDate",
		_   => null,
	};

	static readonly string[] AdvSideCodeSet =
	[
		"B", // Buy
		"S", // Sell
		"X", // Cross
		"T", // Trade
	];

	static readonly string[] AdvTransTypeCodeSet =
	[
		"N", // New
		"C", // Cancel
		"R", // Replace
	];

	static readonly string[] CommTypeCodeSet =
	[
		"1", // PerUnit
		"2", // Percent
		"3", // Absolute
		"4", // PercentageWaivedCashDiscount
		"5", // PercentageWaivedEnhancedUnits
		"6", // PointsPerBondOrContract
	];

	static readonly string[] ExecInstCodeSet =
	[
		"1", // NotHeld
		"2", // Work
		"3", // GoAlong
		"4", // OverTheDay
		"5", // Held
		"6", // ParticipateDoNotInitiate
		"7", // StrictScale
		"8", // TryToScale
		"9", // StayOnBidSide
		"0", // StayOnOfferSide
		"A", // NoCross
		"B", // OKToCross
		"C", // CallFirst
		"D", // PercentOfVolume
		"E", // DoNotIncrease
		"F", // DoNotReduce
		"G", // AllOrNone
		"H", // ReinstateOnSystemFailure
		"I", // InstitutionsOnly
		"J", // ReinstateOnTradingHalt
		"K", // CancelOnTradingHalt
		"L", // LastPeg
		"M", // MidPricePeg
		"N", // NonNegotiable
		"O", // OpeningPeg
		"P", // MarketPeg
		"Q", // CancelOnSystemFailure
		"R", // PrimaryPeg
		"S", // Suspend
		"T", // FixedPegToLocalBestBidOrOfferAtTimeOfOrder
		"U", // CustomerDisplayInstruction
		"V", // Netting
		"W", // PegToVWAP
		"X", // TradeAlong
		"Y", // TryToStop
		"Z", // CancelIfNotBest
		"a", // TrailingStopPeg
		"b", // StrictLimit
		"c", // IgnorePriceValidityChecks
		"d", // PegToLimitPrice
		"e", // WorkToTargetStrategy
	];

	static readonly string[] HandlInstCodeSet =
	[
		"1", // AutomatedExecutionNoIntervention
		"2", // AutomatedExecutionInterventionOK
		"3", // ManualOrder
	];

	static readonly string[] SecurityIDSourceCodeSet =
	[
		"1", // CUSIP
		"2", // SEDOL
		"3", // QUIK
		"4", // ISINNumber
		"5", // RICCode
		"6", // ISOCurrencyCode
		"7", // ISOCountryCode
		"8", // ExchangeSymbol
		"9", // ConsolidatedTapeAssociation
		"A", // BloombergSymbol
		"B", // Wertpapier
		"C", // Dutch
		"D", // Valoren
		"E", // Sicovam
		"F", // Belgian
		"G", // Common
		"H", // ClearingHouse
		"I", // ISDAFpMLSpecification
		"J", // OptionPriceReportingAuthority
	];

	static readonly string[] IOIQltyIndCodeSet =
	[
		"L", // Low
		"M", // Medium
		"H", // High
	];

	static readonly string[] IOIQtyCodeSet =
	[
		"S", // Small
		"M", // Medium
		"L", // Large
	];

	static readonly string[] IOITransTypeCodeSet =
	[
		"N", // New
		"C", // Cancel
		"R", // Replace
	];

	static readonly string[] LastCapacityCodeSet =
	[
		"1", // Agent
		"2", // CrossAsAgent
		"3", // CrossAsPrincipal
		"4", // Principal
	];

	static readonly string[] MsgTypeCodeSet =
	[
		"0",  // Heartbeat
		"1",  // TestRequest
		"2",  // ResendRequest
		"3",  // Reject
		"4",  // SequenceReset
		"5",  // Logout
		"6",  // IOI
		"7",  // Advertisement
		"8",  // ExecutionReport
		"9",  // OrderCancelReject
		"A",  // Logon
		"B",  // News
		"C",  // Email
		"D",  // NewOrderSingle
		"E",  // NewOrderList
		"F",  // OrderCancelRequest
		"G",  // OrderCancelReplaceRequest
		"H",  // OrderStatusRequest
		"J",  // AllocationInstruction
		"K",  // ListCancelRequest
		"L",  // ListExecute
		"M",  // ListStatusRequest
		"N",  // ListStatus
		"P",  // AllocationInstructionAck
		"Q",  // DontKnowTrade
		"R",  // QuoteRequest
		"S",  // Quote
		"T",  // SettlementInstructions
		"V",  // MarketDataRequest
		"W",  // MarketDataSnapshotFullRefresh
		"X",  // MarketDataIncrementalRefresh
		"Y",  // MarketDataRequestReject
		"Z",  // QuoteCancel
		"a",  // QuoteStatusRequest
		"b",  // MassQuoteAcknowledgement
		"c",  // SecurityDefinitionRequest
		"d",  // SecurityDefinition
		"e",  // SecurityStatusRequest
		"f",  // SecurityStatus
		"g",  // TradingSessionStatusRequest
		"h",  // TradingSessionStatus
		"i",  // MassQuote
		"j",  // BusinessMessageReject
		"k",  // BidRequest
		"l",  // BidResponse
		"m",  // ListStrikePrice
		"n",  // XMLNonFIX
		"o",  // RegistrationInstructions
		"p",  // RegistrationInstructionsResponse
		"q",  // OrderMassCancelRequest
		"r",  // OrderMassCancelReport
		"s",  // NewOrderCross
		"t",  // CrossOrderCancelReplaceRequest
		"u",  // CrossOrderCancelRequest
		"v",  // SecurityTypeRequest
		"w",  // SecurityTypes
		"x",  // SecurityListRequest
		"y",  // SecurityList
		"z",  // DerivativeSecurityListRequest
		"AA", // DerivativeSecurityList
		"AB", // NewOrderMultileg
		"AC", // MultilegOrderCancelReplace
		"AD", // TradeCaptureReportRequest
		"AE", // TradeCaptureReport
		"AF", // OrderMassStatusRequest
		"AG", // QuoteRequestReject
		"AH", // RFQRequest
		"AI", // QuoteStatusReport
		"AJ", // QuoteResponse
		"AK", // Confirmation
		"AL", // PositionMaintenanceRequest
		"AM", // PositionMaintenanceReport
		"AN", // RequestForPositions
		"AO", // RequestForPositionsAck
		"AP", // PositionReport
		"AQ", // TradeCaptureReportRequestAck
		"AR", // TradeCaptureReportAck
		"AS", // AllocationReport
		"AT", // AllocationReportAck
		"AU", // ConfirmationAck
		"AV", // SettlementInstructionRequest
		"AW", // AssignmentReport
		"AX", // CollateralRequest
		"AY", // CollateralAssignment
		"AZ", // CollateralResponse
		"BA", // CollateralReport
		"BB", // CollateralInquiry
		"BC", // NetworkCounterpartySystemStatusRequest
		"BD", // NetworkCounterpartySystemStatusResponse
		"BE", // UserRequest
		"BF", // UserResponse
		"BG", // CollateralInquiryAck
		"BH", // ConfirmationRequest
	];

	static readonly string[] OrdStatusCodeSet =
	[
		"0", // New
		"1", // PartiallyFilled
		"2", // Filled
		"3", // DoneForDay
		"4", // Canceled
		"5", // Replaced
		"6", // PendingCancel
		"7", // Stopped
		"8", // Rejected
		"9", // Suspended
		"A", // PendingNew
		"B", // Calculated
		"C", // Expired
		"D", // AcceptedForBidding
		"E", // PendingReplace
	];

	static readonly string[] OrdTypeCodeSet =
	[
		"1", // Market
		"2", // Limit
		"3", // Stop
		"4", // StopLimit
		"6", // WithOrWithout
		"7", // LimitOrBetter
		"8", // LimitWithOrWithout
		"9", // OnBasis
		"D", // PreviouslyQuoted
		"E", // PreviouslyIndicated
		"G", // ForexSwap
		"I", // Funari
		"J", // MarketIfTouched
		"K", // MarketWithLeftOverAsLimit
		"L", // PreviousFundValuationPoint
		"M", // NextFundValuationPoint
		"P", // Pegged
	];

	static readonly string[] PossDupFlagCodeSet =
	[
		"Y", // PossibleDuplicate
		"N", // OriginalTransmission
	];

	static readonly string[] SideCodeSet =
	[
		"1", // Buy
		"2", // Sell
		"3", // BuyMinus
		"4", // SellPlus
		"5", // SellShort
		"6", // SellShortExempt
		"7", // Undisclosed
		"8", // Cross
		"9", // CrossShort
		"A", // CrossShortExempt
		"B", // AsDefined
		"C", // Opposite
		"D", // Subscribe
		"E", // Redeem
		"F", // Lend
		"G", // Borrow
	];

	static readonly string[] TimeInForceCodeSet =
	[
		"0", // Day
		"1", // GoodTillCancel
		"2", // AtTheOpening
		"3", // ImmediateOrCancel
		"4", // FillOrKill
		"5", // GoodTillCrossing
		"6", // GoodTillDate
		"7", // AtTheClose
	];

	static readonly string[] UrgencyCodeSet =
	[
		"0", // Normal
		"1", // Flash
		"2", // Background
	];

	static readonly string[] SettlTypeCodeSet =
	[
		"0", // Regular
		"1", // Cash
		"2", // NextDay
		"3", // TPlus2
		"4", // TPlus3
		"5", // TPlus4
		"6", // Future
		"7", // WhenAndIfIssued
		"8", // SellersOption
		"9", // TPlus5
	];

	static readonly string[] AllocTransTypeCodeSet =
	[
		"0", // New
		"1", // Replace
		"2", // Cancel
	];

	static readonly string[] PositionEffectCodeSet =
	[
		"O", // Open
		"C", // Close
		"R", // Rolled
		"F", // FIFO
	];

	static readonly string[] ProcessCodeCodeSet =
	[
		"0", // Regular
		"1", // SoftDollar
		"2", // StepIn
		"3", // StepOut
		"4", // SoftDollarStepIn
		"5", // SoftDollarStepOut
		"6", // PlanSponsor
	];

	static readonly string[] AllocStatusCodeSet =
	[
		"0", // Accepted
		"1", // BlockLevelReject
		"2", // AccountLevelReject
		"3", // Received
		"4", // Incomplete
		"5", // RejectedByIntermediary
	];

	static readonly string[] AllocRejCodeCodeSet =
	[
		"0", // UnknownAccount
		"1", // IncorrectQuantity
		"2", // IncorrectAveragePrice
		"3", // UnknownExecutingBrokerMnemonic
		"4", // CommissionDifference
		"5", // UnknownOrderID
		"6", // UnknownListID
		"7", // OtherSeeText
		"8", // IncorrectAllocatedQuantity
		"9", // CalculationDifference
		"10", // UnknownOrStaleExecID
		"11", // MismatchedData
		"12", // UnknownClOrdID
		"13", // WarehouseRequestRejected
	];

	static readonly string[] EmailTypeCodeSet =
	[
		"0", // New
		"1", // Reply
		"2", // AdminReply
	];

	static readonly string[] PossResendCodeSet =
	[
		"Y", // PossibleResend
		"N", // OriginalTransmission
	];

	static readonly string[] EncryptMethodCodeSet =
	[
		"0", // None
		"1", // PKCS
		"2", // DES
		"3", // PKCSDES
		"4", // PGPDES
		"5", // PGPDESMD5
		"6", // PEM
	];

	static readonly string[] CxlRejReasonCodeSet =
	[
		"0", // TooLateToCancel
		"1", // UnknownOrder
		"2", // BrokerCredit
		"3", // OrderAlreadyInPendingStatus
		"4", // UnableToProcessOrderMassCancelRequest
		"5", // OrigOrdModTime
		"6", // DuplicateClOrdID
		"99", // Other
	];

	static readonly string[] OrdRejReasonCodeSet =
	[
		"0", // BrokerCredit
		"1", // UnknownSymbol
		"2", // ExchangeClosed
		"3", // OrderExceedsLimit
		"4", // TooLateToEnter
		"5", // UnknownOrder
		"6", // DuplicateOrder
		"7", // DuplicateOfAVerballyCommunicatedOrder
		"8", // StaleOrder
		"9", // TradeAlongRequired
		"10", // InvalidInvestorID
		"11", // UnsupportedOrderCharacteristic
		"12", // SurveillanceOption
		"13", // IncorrectQuantity
		"14", // IncorrectAllocatedQuantity
		"15", // UnknownAccount
		"99", // Other
	];

	static readonly string[] IOIQualifierCodeSet =
	[
		"A", // AllOrNone
		"B", // MarketOnClose
		"C", // AtTheClose
		"D", // VWAP
		"I", // InTouchWith
		"L", // Limit
		"M", // MoreBehind
		"O", // AtTheOpen
		"P", // TakingAPosition
		"Q", // AtTheMarket
		"R", // ReadyToTrade
		"S", // PortfolioShown
		"T", // ThroughTheDay
		"V", // Versus
		"W", // Indication
		"X", // CrossingOpportunity
		"Y", // AtTheMidpoint
		"Z", // PreOpen
	];

	static readonly string[] ReportToExchCodeSet =
	[
		"Y", // ReceiverReports
		"N", // SenderReports
	];

	static readonly string[] LocateReqdCodeSet =
	[
		"Y", // Yes
		"N", // No
	];

	static readonly string[] ForexReqCodeSet =
	[
		"Y", // ExecuteForexAfterSecurityTrade
		"N", // DoNotExecuteForexAfterSecurityTrade
	];

	static readonly string[] GapFillFlagCodeSet =
	[
		"Y", // GapFillMessage
		"N", // SequenceReset
	];

	static readonly string[] DKReasonCodeSet =
	[
		"A", // UnknownSymbol
		"B", // WrongSide
		"C", // QuantityExceedsOrder
		"D", // NoMatchingOrder
		"E", // PriceExceedsLimit
		"F", // CalculationDifference
		"Z", // Other
	];

	static readonly string[] IOINaturalFlagCodeSet =
	[
		"Y", // Natural
		"N", // NotNatural
	];

	static readonly string[] MiscFeeTypeCodeSet =
	[
		"1", // Regulatory
		"2", // Tax
		"3", // LocalCommission
		"4", // ExchangeFees
		"5", // Stamp
		"6", // Levy
		"7", // Other
		"8", // Markup
		"9", // ConsumptionTax
		"10", // PerTransaction
		"11", // Conversion
		"12", // Agent
	];

	static readonly string[] ResetSeqNumFlagCodeSet =
	[
		"Y", // Yes
		"N", // No
	];

	static readonly string[] ExecTypeCodeSet =
	[
		"0", // New
		"3", // DoneForDay
		"4", // Canceled
		"5", // Replaced
		"6", // PendingCancel
		"7", // Stopped
		"8", // Rejected
		"9", // Suspended
		"A", // PendingNew
		"B", // Calculated
		"C", // Expired
		"D", // Restated
		"E", // PendingReplace
		"F", // Trade
		"G", // TradeCorrect
		"H", // TradeCancel
		"I", // OrderStatus
	];

	static readonly string[] SettlCurrFxRateCalcCodeSet =
	[
		"M", // Multiply
		"D", // Divide
	];

	static readonly string[] SettlInstModeCodeSet =
	[
		"1", // StandingInstructionsProvided
		"4", // SpecificOrderForASingleAccount
		"5", // RequestReject
	];

	static readonly string[] SettlInstTransTypeCodeSet =
	[
		"N", // New
		"C", // Cancel
		"R", // Replace
		"T", // Restate
	];

	static readonly string[] SettlInstSourceCodeSet =
	[
		"1", // BrokerCredit
		"2", // Institution
		"3", // Investor
	];

	static readonly string[] SecurityTypeCodeSet =
	[
		"EUSUPRA", // EuroSupranationalCoupons
		"FAC", // FederalAgencyCoupon
		"FADN", // FederalAgencyDiscountNote
		"PEF", // PrivateExportFunding
		"SUPRA", // USDSupranationalCoupons
		"CORP", // CorporateBond
		"CPP", // CorporatePrivatePlacement
		"CB", // ConvertibleBond
		"DUAL", // DualCurrency
		"EUCORP", // EuroCorporateBond
		"XLINKD", // IndexedLinked
		"STRUCT", // StructuredNotes
		"YANK", // YankeeCorporateBond
		"FOR", // ForeignExchangeContract
		"CS", // CommonStock
		"PS", // PreferredStock
		"BRADY", // BradyBond
		"EUSOV", // EuroSovereigns
		"TBOND", // USTreasuryBond
		"TINT", // InterestStripFromAnyBondOrNote
		"TIPS", // TreasuryInflationProtectedSecurities
		"TCAL", // PrincipalStripOfACallableBondOrNote
		"TPRN", // PrincipalStripFromANonCallableBondOrNote
		"UST", // USTreasuryNoteOld
		"USTB", // USTreasuryBillOld
		"TNOTE", // USTreasuryNote
		"TBILL", // USTreasuryBill
		"REPO", // Repurchase
		"FORWARD", // Forward
		"BUYSELL", // BuySellback
		"SECLOAN", // SecuritiesLoan
		"SECPLEDGE", // SecuritiesPledge
		"TERM", // TermLoan
		"RVLV", // RevolverLoan
		"RVLVTRM", // Revolver
		"BRIDGE", // BridgeLoan
		"LOFC", // LetterOfCredit
		"SWING", // SwingLineFacility
		"DINP", // DebtorInPossession
		"DEFLTED", // Defaulted
		"WITHDRN", // Withdrawn
		"REPLACD", // Replaced
		"MATURED", // Matured
		"AMENDED", // Amended
		"RETIRED", // Retired
		"BA", // BankersAcceptance
		"BN", // BankNotes
		"BOX", // BillOfExchanges
		"CD", // CertificateOfDeposit
		"CL", // CallLoans
		"CP", // CommercialPaper
		"DN", // DepositNotes
		"EUCD", // EuroCertificateOfDeposit
		"EUCP", // EuroCommercialPaper
		"LQN", // LiquidityNote
		"MTN", // MediumTermNotes
		"ONITE", // Overnight
		"PN", // PromissoryNote
		"PZFJ", // PlazosFijos
		"STN", // ShortTermLoanNote
		"TD", // TimeDeposit
		"XCN", // ExtendedCommNote
		"YCD", // YankeeCertificateOfDeposit
		"ABS", // AssetBackedSecurities
		"CMBS", // Corp
		"CMO", // CollateralizedMortgageObligation
		"IET", // IOETTEMortgage
		"MBS", // MortgageBackedSecurities
		"MIO", // MortgageInterestOnly
		"MPO", // MortgagePrincipalOnly
		"MPP", // MortgagePrivatePlacement
		"MPT", // MiscellaneousPassThrough
		"PFAND", // Pfandbriefe
		"TBA", // ToBeAnnounced
		"AN", // OtherAnticipationNotes
		"COFO", // CertificateOfObligation
		"COFP", // CertificateOfParticipation
		"GO", // GeneralObligationBonds
		"MT", // MandatoryTender
		"RAN", // RevenueAnticipationNote
		"REV", // RevenueBonds
		"SPCLA", // SpecialAssessment
		"SPCLO", // SpecialObligation
		"SPCLT", // SpecialTax
		"TAN", // TaxAnticipationNote
		"TAXA", // TaxAllocation
		"TECP", // TaxExemptCommercialPaper
		"TRAN", // TaxRevenueAnticipationNote
		"VRDN", // VariableRateDemandNote
		"WAR", // Warrant
		"MF", // MutualFund
		"MLEG", // MultilegInstrument
		"NONE", // NoSecurityType
		"FUT", // Future
		"OPT", // Option
	];

	static readonly string[] StandInstDbTypeCodeSet =
	[
		"0", // Other
		"1", // DTCSID
		"2", // ThomsonALERT
		"3", // AGlobalCustodian
		"4", // AccountNet
	];

	static readonly string[] SettlDeliveryTypeCodeSet =
	[
		"0", // Versus
		"1", // Free
		"2", // TriParty
		"3", // HoldInCustody
	];

	static readonly string[] AllocLinkTypeCodeSet =
	[
		"0", // FXNetting
		"1", // FXSwap
	];

	static readonly string[] PutOrCallCodeSet =
	[
		"0", // Put
		"1", // Call
	];

	static readonly string[] CoveredOrUncoveredCodeSet =
	[
		"0", // Covered
		"1", // Uncovered
	];

	static readonly string[] NotifyBrokerOfCreditCodeSet =
	[
		"Y", // DetailsShouldBeCommunicated
		"N", // DetailsShouldNotBeCommunicated
	];

	static readonly string[] AllocHandlInstCodeSet =
	[
		"1", // Match
		"2", // Forward
		"3", // ForwardAndMatch
	];

	static readonly string[] RoutingTypeCodeSet =
	[
		"1", // TargetFirm
		"2", // TargetList
		"3", // BlockFirm
		"4", // BlockList
	];

	static readonly string[] BenchmarkCurveNameCodeSet =
	[
		"EONIA", // EONIA
		"EUREPO", // EUREPO
		"Euribor", // Euribor
		"FutureSWAP", // FutureSWAP
		"LIBID", // LIBID
		"LIBOR", // LIBOR
		"MuniAAA", // MuniAAA
		"OTHER", // OTHER
		"Pfandbriefe", // Pfandbriefe
		"SONIA", // SONIA
		"SWAP", // SWAP
		"Treasury", // Treasury
	];

	static readonly string[] StipulationTypeCodeSet =
	[
		"AMT", // AlternativeMinimumTax
		"AUTOREINV", // AutoReinvestment
		"BANKQUAL", // BankQualified
		"BGNCON", // BargainConditions
		"COUPON", // CouponRange
		"CURRENCY", // ISOCurrencyCode
		"CUSTOMDATE", // CustomStart
		"GEOG", // Geographics
		"HAIRCUT", // ValuationDiscount
		"INSURED", // Insured
		"ISSUE", // IssueDate
		"ISSUER", // Issuer
		"ISSUESIZE", // IssueSizeRange
		"LOOKBACK", // LookbackDays
		"LOT", // ExplicitLotIdentifier
		"LOTVAR", // LotVariance
		"MAT", // MaturityYearAndMonth
		"MATURITY", // MaturityRange
		"MAXSUBS", // MaximumSubstitutions
		"MINQTY", // MinimumQuantity
		"MININCR", // MinimumIncrement
		"MINDNOM", // MinimumDenomination
		"PAYFREQ", // PaymentFrequency
		"PIECES", // NumberOfPieces
		"PMAX", // PoolsMaximum
		"PPM", // PoolsPerMillion
		"PPL", // PoolsPerLot
		"PPT", // PoolsPerTrade
		"PRICE", // PriceRange
		"PRICEFREQ", // PricingFrequency
		"PROD", // ProductionYear
		"PROTECT", // CallProtection
		"PURPOSE", // Purpose
		"PXSOURCE", // BenchmarkPriceSource
		"RATING", // RatingSourceAndRange
		"REDEMPTION", // TypeOfRedemption
		"RESTRICTED", // Restricted
		"SECTOR", // MarketSector
		"SECTYPE", // SecurityTypeIncludedOrExcluded
		"STRUCT", // Structure
		"SUBSFREQ", // SubstitutionsFrequency
		"SUBSLEFT", // SubstitutionsLeft
		"TEXT", // FreeformText
		"TRDVAR", // TradeVariance
		"WAC", // WeightedAverageCoupon
		"WAL", // WeightedAverageLifeCoupon
		"WALA", // WeightedAverageLoanAge
		"WAM", // WeightedAverageMaturity
		"WHOLE", // WholePool
		"YIELD", // YieldRange
	];

	static readonly string[] YieldTypeCodeSet =
	[
		"AFTERTAX", // AfterTaxYield
		"ANNUAL", // AnnualYield
		"ATISSUE", // YieldAtIssue
		"AVGMATURITY", // YieldToAverageMaturity
		"BOOK", // BookYield
		"CALL", // YieldToNextCall
		"CHANGE", // YieldChangeSinceClose
		"CLOSE", // ClosingYield
		"COMPOUND", // CompoundYield
		"CURRENT", // CurrentYield
		"GROSS", // TrueGrossYield
		"GOVTEQUIV", // GvntEquivalentYield
		"INFLATION", // YieldWithInflationAssumption
		"INVERSEFLOATER", // InverseFloaterBondYield
		"LASTCLOSE", // MostRecentClosingYield
		"LASTMONTH", // ClosingYieldMostRecentMonth
		"LASTQUARTER", // ClosingYieldMostRecentQuarter
		"LASTYEAR", // ClosingYieldMostRecentYear
		"LONGAVGLIFE", // YieldToLongestAverageLife
		"MARK", // MarkToMarketYield
		"MATURITY", // YieldToMaturity
		"NEXTREFUND", // YieldToNextRefund
		"OPENAVG", // OpenAverageYield
		"PUT", // YieldToNextPut
		"PREVCLOSE", // PreviousCloseYield
		"PROCEEDS", // ProceedsYield
		"SEMIANNUAL", // SemiAnnualYield
		"SHORTAVGLIFE", // YieldToShortestAverageLife
		"SIMPLE", // SimpleYield
		"TAXEQUIV", // TaxEquivalentYield
		"TENDER", // YieldToTenderDate
		"TRUE", // TrueYield
		"VALUE1/32", // YieldValueOf132
		"WORST", // YieldToWorst
	];

	static readonly string[] TradedFlatSwitchCodeSet =
	[
		"Y", // TradedFlat
		"N", // NotTradedFlat
	];

	static readonly string[] SubscriptionRequestTypeCodeSet =
	[
		"0", // Snapshot
		"1", // SnapshotAndUpdates
		"2", // DisablePreviousSnapshot
	];

	static readonly string[] MDUpdateTypeCodeSet =
	[
		"0", // FullRefresh
		"1", // IncrementalRefresh
	];

	static readonly string[] AggregatedBookCodeSet =
	[
		"Y", // BookEntriesToBeAggregated
		"N", // BookEntriesShouldNotBeAggregated
	];

	static readonly string[] MDEntryTypeCodeSet =
	[
		"0", // Bid
		"1", // Offer
		"2", // Trade
		"3", // IndexValue
		"4", // OpeningPrice
		"5", // ClosingPrice
		"6", // SettlementPrice
		"7", // TradingSessionHighPrice
		"8", // TradingSessionLowPrice
		"9", // TradingSessionVWAPPrice
		"A", // Imbalance
		"B", // TradeVolume
		"C", // OpenInterest
	];

	static readonly string[] TickDirectionCodeSet =
	[
		"0", // PlusTick
		"1", // ZeroPlusTick
		"2", // MinusTick
		"3", // ZeroMinusTick
	];

	static readonly string[] QuoteConditionCodeSet =
	[
		"A", // Open
		"B", // Closed
		"C", // ExchangeBest
		"D", // ConsolidatedBest
		"E", // Locked
		"F", // Crossed
		"G", // Depth
		"H", // FastTrading
		"I", // NonFirm
	];

	static readonly string[] TradeConditionCodeSet =
	[
		"A", // Cash
		"B", // AveragePriceTrade
		"C", // CashTrade
		"D", // NextDay
		"E", // Opening
		"F", // IntradayTradeDetail
		"G", // Rule127Trade
		"H", // Rule155Trade
		"I", // SoldLast
		"J", // NextDayTrade
		"K", // Opened
		"L", // Seller
		"M", // Sold
		"N", // StoppedStock
		"P", // ImbalanceMoreBuyers
		"Q", // ImbalanceMoreSellers
		"R", // OpeningPrice
	];

	static readonly string[] MDUpdateActionCodeSet =
	[
		"0", // New
		"1", // Change
		"2", // Delete
	];

	static readonly string[] MDReqRejReasonCodeSet =
	[
		"0", // UnknownSymbol
		"1", // DuplicateMDReqID
		"2", // InsufficientBandwidth
		"3", // InsufficientPermissions
		"4", // UnsupportedSubscriptionRequestType
		"5", // UnsupportedMarketDepth
		"6", // UnsupportedMDUpdateType
		"7", // UnsupportedAggregatedBook
		"8", // UnsupportedMDEntryType
		"9", // UnsupportedTradingSessionID
		"A", // UnsupportedScope
		"B", // UnsupportedOpenCloseSettleFlag
		"C", // UnsupportedMDImplicitDelete
	];

	static readonly string[] DeleteReasonCodeSet =
	[
		"0", // Cancellation
		"1", // Error
	];

	static readonly string[] OpenCloseSettlFlagCodeSet =
	[
		"0", // DailyOpen
		"1", // SessionOpen
		"2", // DeliverySettlementEntry
		"3", // ExpectedEntry
		"4", // EntryFromPreviousBusinessDay
		"5", // TheoreticalPriceValue
	];

	static readonly string[] FinancialStatusCodeSet =
	[
		"1", // Bankrupt
		"2", // PendingDelisting
	];

	static readonly string[] CorporateActionCodeSet =
	[
		"A", // ExDividend
		"B", // ExDistribution
		"C", // ExRights
		"D", // New
		"E", // ExInterest
	];

	static readonly string[] QuoteStatusCodeSet =
	[
		"0", // Accepted
		"1", // CancelForSymbol
		"2", // CanceledForSecurityType
		"3", // CanceledForUnderlying
		"4", // CanceledAll
		"5", // Rejected
		"6", // RemovedFromMarket
		"7", // Expired
		"8", // Query
		"9", // QuoteNotFound
		"10", // Pending
		"11", // Pass
		"12", // LockedMarketWarning
		"13", // CrossMarketWarning
		"14", // CanceledDueToLockMarket
		"15", // CanceledDueToCrossMarket
	];

	static readonly string[] QuoteCancelTypeCodeSet =
	[
		"1", // CancelForOneOrMoreSecurities
		"2", // CancelForSecurityType
		"3", // CancelForUnderlyingSecurity
		"4", // CancelAllQuotes
	];

	static readonly string[] QuoteRejectReasonCodeSet =
	[
		"1", // UnknownSymbol
		"2", // Exchange
		"3", // QuoteRequestExceedsLimit
		"4", // TooLateToEnter
		"5", // UnknownQuote
		"6", // DuplicateQuote
		"7", // InvalidBid
		"8", // InvalidPrice
		"9", // NotAuthorizedToQuoteSecurity
		"99", // Other
	];

	static readonly string[] QuoteResponseLevelCodeSet =
	[
		"0", // NoAcknowledgement
		"1", // AcknowledgeOnlyNegativeOrErroneousQuotes
		"2", // AcknowledgeEachQuoteMessage
	];

	static readonly string[] QuoteRequestTypeCodeSet =
	[
		"1", // Manual
		"2", // Automatic
	];

	static readonly string[] SecurityRequestTypeCodeSet =
	[
		"0", // RequestSecurityIdentityAndSpecifications
		"1", // RequestSecurityIdentityForSpecifications
		"2", // RequestListSecurityTypes
		"3", // RequestListSecurities
	];

	static readonly string[] SecurityResponseTypeCodeSet =
	[
		"1", // AcceptAsIs
		"2", // AcceptWithRevisions
		"5", // RejectSecurityProposal
		"6", // CannotMatchSelectionCriteria
	];

	static readonly string[] UnsolicitedIndicatorCodeSet =
	[
		"Y", // MessageIsBeingSentUnsolicited
		"N", // MessageIsBeingSentAsAResultOfAPriorRequest
	];

	static readonly string[] SecurityTradingStatusCodeSet =
	[
		"1", // OpeningDelay
		"2", // TradingHalt
		"3", // Resume
		"4", // NoOpen
		"5", // PriceIndication
		"6", // TradingRangeIndication
		"7", // MarketImbalanceBuy
		"8", // MarketImbalanceSell
		"9", // MarketOnCloseImbalanceBuy
		"10", // MarketOnCloseImbalanceSell
		"12", // NoMarketImbalance
		"13", // NoMarketOnCloseImbalance
		"14", // ITSPreOpening
		"15", // NewPriceIndication
		"16", // TradeDisseminationTime
		"17", // ReadyToTrade
		"18", // NotAvailableForTrading
		"19", // NotTradedOnThisMarket
		"20", // UnknownOrInvalid
		"21", // PreOpen
		"22", // OpeningRotation
		"23", // FastMarket
	];

	static readonly string[] HaltReasonCodeSet =
	[
		"I", // OrderImbalance
		"X", // EquipmentChangeover
		"P", // NewsPending
		"D", // NewsDissemination
		"E", // OrderInflux
		"M", // AdditionalInformation
	];

	static readonly string[] InViewOfCommonCodeSet =
	[
		"Y", // HaltWasDueToCommonStockBeingHalted
		"N", // HaltWasNotRelatedToAHaltOfTheCommonStock
	];

	static readonly string[] DueToRelatedCodeSet =
	[
		"Y", // RelatedToSecurityHalt
		"N", // NotRelatedToSecurityHalt
	];

	static readonly string[] AdjustmentCodeSet =
	[
		"1", // Cancel
		"2", // Error
		"3", // Correction
	];

	static readonly string[] TradSesMethodCodeSet =
	[
		"1", // Electronic
		"2", // OpenOutcry
		"3", // TwoParty
	];

	static readonly string[] TradSesModeCodeSet =
	[
		"1", // Testing
		"2", // Simulated
		"3", // Production
	];

	static readonly string[] TradSesStatusCodeSet =
	[
		"0", // Unknown
		"1", // Halted
		"2", // Open
		"3", // Closed
		"4", // PreOpen
		"5", // PreClose
		"6", // RequestRejected
	];

	static readonly string[] MessageEncodingCodeSet =
	[
		"ISO-2022-JP", // ISO2022JP
		"EUC-JP", // EUCJP
		"Shift_JIS", // ShiftJIS
		"UTF-8", // UTF8
	];

	static readonly string[] SessionRejectReasonCodeSet =
	[
		"0", // InvalidTagNumber
		"1", // RequiredTagMissing
		"2", // TagNotDefinedForThisMessageType
		"3", // UndefinedTag
		"4", // TagSpecifiedWithoutAValue
		"5", // ValueIsIncorrect
		"6", // IncorrectDataFormatForValue
		"7", // DecryptionProblem
		"8", // SignatureProblem
		"9", // CompIDProblem
		"10", // SendingTimeAccuracyProblem
		"11", // InvalidMsgType
		"12", // XMLValidationError
		"13", // TagAppearsMoreThanOnce
		"14", // TagSpecifiedOutOfRequiredOrder
		"15", // RepeatingGroupFieldsOutOfOrder
		"16", // IncorrectNumInGroupCountForRepeatingGroup
		"17", // Non
		"99", // Other
	];

	static readonly string[] BidRequestTransTypeCodeSet =
	[
		"N", // New
		"C", // Cancel
	];

	static readonly string[] SolicitedFlagCodeSet =
	[
		"Y", // WasSolicited
		"N", // WasNotSolicited
	];

	static readonly string[] ExecRestatementReasonCodeSet =
	[
		"0", // GTCorporateAction
		"1", // GTRenewal
		"2", // VerbalChange
		"3", // RepricingOfOrder
		"4", // BrokerOption
		"5", // PartialDeclineOfOrderQty
		"6", // CancelOnTradingHalt
		"7", // CancelOnSystemFailure
		"8", // Market
		"9", // Canceled
		"10", // WarehouseRecap
		"99", // Other
	];

	static readonly string[] BusinessRejectReasonCodeSet =
	[
		"0", // Other
		"1", // UnknownID
		"2", // UnknownSecurity
		"3", // UnsupportedMessageType
		"4", // ApplicationNotAvailable
		"5", // ConditionallyRequiredFieldMissing
		"6", // NotAuthorized
		"7", // DeliverToFirmNotAvailableAtThisTime
	];

	static readonly string[] MsgDirectionCodeSet =
	[
		"S", // Send
		"R", // Receive
	];

	static readonly string[] DiscretionInstCodeSet =
	[
		"0", // RelatedToDisplayedPrice
		"1", // RelatedToMarketPrice
		"2", // RelatedToPrimaryPrice
		"3", // RelatedToLocalPrimaryPrice
		"4", // RelatedToMidpointPrice
		"5", // RelatedToLastTradePrice
		"6", // RelatedToVWAP
	];

	static readonly string[] BidTypeCodeSet =
	[
		"1", // NonDisclosed
		"2", // Disclosed
		"3", // NoBiddingProcess
	];

	static readonly string[] BidDescriptorTypeCodeSet =
	[
		"1", // Sector
		"2", // Country
		"3", // Index
	];

	static readonly string[] SideValueIndCodeSet =
	[
		"1", // SideValue1
		"2", // SideValue2
	];

	static readonly string[] LiquidityIndTypeCodeSet =
	[
		"1", // FiveDayMovingAverage
		"2", // TwentyDayMovingAverage
		"3", // NormalMarketSize
		"4", // Other
	];

	static readonly string[] ExchangeForPhysicalCodeSet =
	[
		"Y", // True
		"N", // False
	];

	static readonly string[] ProgRptReqsCodeSet =
	[
		"1", // BuySideRequests
		"2", // SellSideSends
		"3", // RealTimeExecutionReports
	];

	static readonly string[] IncTaxIndCodeSet =
	[
		"1", // Net
		"2", // Gross
	];

	static readonly string[] BidTradeTypeCodeSet =
	[
		"R", // RiskTrade
		"G", // VWAPGuarantee
		"A", // Agency
		"J", // GuaranteedClose
	];

	static readonly string[] BasisPxTypeCodeSet =
	[
		"2", // ClosingPriceAtMorningSession
		"3", // ClosingPrice
		"4", // CurrentPrice
		"5", // SQ
		"6", // VWAPThroughADay
		"7", // VWAPThroughAMorningSession
		"8", // VWAPThroughAnAfternoonSession
		"9", // VWAPThroughADayExcept
		"A", // VWAPThroughAMorningSessionExcept
		"B", // VWAPThroughAnAfternoonSessionExcept
		"C", // Strike
		"D", // Open
		"Z", // Others
	];

	static readonly string[] PriceTypeCodeSet =
	[
		"1", // Percentage
		"2", // PerUnit
		"3", // FixedAmount
		"4", // Discount
		"5", // Premium
		"6", // Spread
		"7", // TEDPrice
		"8", // TEDYield
		"9", // Yield
		"10", // FixedCabinetTradePrice
		"11", // VariableCabinetTradePrice
	];

	static readonly string[] GTBookingInstCodeSet =
	[
		"0", // BookOutAllTradesOnDayOfExecution
		"1", // AccumulateUntilFilledOrExpired
		"2", // AccumulateUntilVerballlyNotifiedOtherwise
	];

	static readonly string[] ListStatusTypeCodeSet =
	[
		"1", // Ack
		"2", // Response
		"3", // Timed
		"4", // ExecStarted
		"5", // AllDone
		"6", // Alert
	];

	static readonly string[] NetGrossIndCodeSet =
	[
		"1", // Net
		"2", // Gross
	];

	static readonly string[] ListOrderStatusCodeSet =
	[
		"1", // InBiddingProcess
		"2", // ReceivedForExecution
		"3", // Executing
		"4", // Cancelling
		"5", // Alert
		"6", // AllDone
		"7", // Reject
	];

	static readonly string[] ListExecInstTypeCodeSet =
	[
		"1", // Immediate
		"2", // WaitForInstruction
		"3", // SellDriven
		"4", // BuyDrivenCashTopUp
		"5", // BuyDrivenCashWithdraw
	];

	static readonly string[] CxlRejResponseToCodeSet =
	[
		"1", // OrderCancelRequest
		"2", // OrderCancel
	];

	static readonly string[] MultiLegReportingTypeCodeSet =
	[
		"1", // SingleSecurity
		"2", // IndividualLegOfAMultiLegSecurity
		"3", // MultiLegSecurity
	];

	static readonly string[] PartyIDSourceCodeSet =
	[
		"B", // BIC
		"C", // GeneralIdentifier
		"D", // Proprietary
		"E", // ISOCountryCode
		"F", // SettlementEntityLocation
		"G", // MIC
		"H", // CSDParticipant
		"1", // KoreanInvestorID
		"2", // TaiwaneseForeignInvestorID
		"3", // TaiwaneseTradingAcct
		"4", // MalaysianCentralDepository
		"5", // ChineseInvestorID
		"6", // UKNationalInsuranceOrPensionNumber
		"7", // USSocialSecurityNumber
		"8", // USEmployerOrTaxIDNumber
		"9", // AustralianBusinessNumber
		"A", // AustralianTaxFileNumber
		"I", // ISITCAcronym
	];

	static readonly string[] PartyRoleCodeSet =
	[
		"1", // ExecutingFirm
		"2", // BrokerOfCredit
		"3", // ClientID
		"4", // ClearingFirm
		"5", // InvestorID
		"6", // IntroducingFirm
		"7", // EnteringFirm
		"8", // Locate
		"9", // FundManagerClientID
		"10", // SettlementLocation
		"11", // OrderOriginationTrader
		"12", // ExecutingTrader
		"13", // OrderOriginationFirm
		"14", // GiveupClearingFirm
		"15", // CorrespondantClearingFirm
		"16", // ExecutingSystem
		"17", // ContraFirm
		"18", // ContraClearingFirm
		"19", // SponsoringFirm
		"20", // UnderlyingContraFirm
		"21", // ClearingOrganization
		"22", // Exchange
		"24", // CustomerAccount
		"25", // CorrespondentClearingOrganization
		"26", // CorrespondentBroker
		"27", // Buyer
		"28", // Custodian
		"29", // Intermediary
		"30", // Agent
		"31", // SubCustodian
		"32", // Beneficiary
		"33", // InterestedParty
		"34", // RegulatoryBody
		"35", // LiquidityProvider
		"36", // EnteringTrader
		"37", // ContraTrader
		"38", // PositionAccount
	];

	static readonly string[] ProductCodeSet =
	[
		"1", // AGENCY
		"2", // COMMODITY
		"3", // CORPORATE
		"4", // CURRENCY
		"5", // EQUITY
		"6", // GOVERNMENT
		"7", // INDEX
		"8", // LOAN
		"9", // MONEYMARKET
		"10", // MORTGAGE
		"11", // MUNICIPAL
		"12", // OTHER
		"13", // FINANCING
	];

	static readonly string[] TestMessageIndicatorCodeSet =
	[
		"Y", // True
		"N", // False
	];

	static readonly string[] RoundingDirectionCodeSet =
	[
		"0", // RoundToNearest
		"1", // RoundDown
		"2", // RoundUp
	];

	static readonly string[] DistribPaymentMethodCodeSet =
	[
		"1", // CREST
		"2", // NSCC
		"3", // Euroclear
		"4", // Clearstream
		"5", // Cheque
		"6", // TelegraphicTransfer
		"7", // FedWire
		"8", // DirectCredit
		"9", // ACHCredit
		"10", // BPAY
		"11", // HighValueClearingSystemHVACS
		"12", // ReinvestInFund
	];

	static readonly string[] CancellationRightsCodeSet =
	[
		"Y", // Yes
		"N", // NoExecutionOnly
		"M", // NoWaiverAgreement
		"O", // NoInstitutional
	];

	static readonly string[] MoneyLaunderingStatusCodeSet =
	[
		"Y", // Passed
		"N", // NotChecked
		"1", // ExemptBelowLimit
		"2", // ExemptMoneyType
		"3", // ExemptAuthorised
	];

	static readonly string[] ExecPriceTypeCodeSet =
	[
		"B", // BidPrice
		"C", // CreationPrice
		"D", // CreationPricePlusAdjustmentPercent
		"E", // CreationPricePlusAdjustmentAmount
		"O", // OfferPrice
		"P", // OfferPriceMinusAdjustmentPercent
		"Q", // OfferPriceMinusAdjustmentAmount
		"S", // SinglePrice
	];

	static readonly string[] TradeReportTransTypeCodeSet =
	[
		"0", // New
		"1", // Cancel
		"2", // Replace
		"3", // Release
		"4", // Reverse
	];

	static readonly string[] PaymentMethodCodeSet =
	[
		"1", // CREST
		"2", // NSCC
		"3", // Euroclear
		"4", // Clearstream
		"5", // Cheque
		"6", // TelegraphicTransfer
		"7", // FedWire
		"8", // DebitCard
		"9", // DirectDebit
		"10", // DirectCredit
		"11", // CreditCard
		"12", // ACHDebit
		"13", // ACHCredit
		"14", // BPAY
		"15", // HighValueClearingSystem
	];

	static readonly string[] TaxAdvantageTypeCodeSet =
	[
		"0", // None
		"1", // MaxiISA
		"2", // TESSA
		"3", // MiniCashISA
		"4", // MiniStocksAndSharesISA
		"5", // MiniInsuranceISA
		"6", // CurrentYearPayment
		"7", // PriorYearPayment
		"8", // AssetTransfer
		"9", // EmployeePriorYear
		"10", // EmployeeCurrentYear
		"11", // EmployerPriorYear
		"12", // EmployerCurrentYear
		"13", // NonFundPrototypeIRA
		"14", // NonFundQualifiedPlan
		"15", // DefinedContributionPlan
		"16", // IRA
		"17", // IRARollover
		"18", // KEOGH
		"19", // ProfitSharingPlan
		"20", // US401K
		"21", // SelfDirectedIRA
		"22", // US403b
		"23", // US457
		"24", // RothIRAPrototype
		"25", // RothIRANonPrototype
		"26", // RothConversionIRAPrototype
		"27", // RothConversionIRANonPrototype
		"28", // EducationIRAPrototype
		"29", // EducationIRANonPrototype
	];

	static readonly string[] FundRenewWaivCodeSet =
	[
		"Y", // Yes
		"N", // No
	];

	static readonly string[] RegistStatusCodeSet =
	[
		"A", // Accepted
		"R", // Rejected
		"H", // Held
		"N", // Reminder
	];

	static readonly string[] RegistRejReasonCodeCodeSet =
	[
		"1", // InvalidAccountType
		"2", // InvalidTaxExemptType
		"3", // InvalidOwnershipType
		"4", // NoRegDetails
		"5", // InvalidRegSeqNo
		"6", // InvalidRegDetails
		"7", // InvalidMailingDetails
		"8", // InvalidMailingInstructions
		"9", // InvalidInvestorID
		"10", // InvalidInvestorIDSource
		"11", // InvalidDateOfBirth
		"12", // InvalidCountry
		"13", // InvalidDistribInstns
		"14", // InvalidPercentage
		"15", // InvalidPaymentMethod
		"16", // InvalidAccountName
		"17", // InvalidAgentCode
		"18", // InvalidAccountNum
		"99", // Other
	];

	static readonly string[] RegistTransTypeCodeSet =
	[
		"0", // New
		"1", // Replace
		"2", // Cancel
	];

	static readonly string[] OwnershipTypeCodeSet =
	[
		"J", // JointInvestors
		"T", // TenantsInCommon
		"2", // JointTrustees
	];

	static readonly string[] ContAmtTypeCodeSet =
	[
		"1", // CommissionAmount
		"2", // CommissionPercent
		"3", // InitialChargeAmount
		"4", // InitialChargePercent
		"5", // DiscountAmount
		"6", // DiscountPercent
		"7", // DilutionLevyAmount
		"8", // DilutionLevyPercent
		"9", // ExitChargeAmount
		"10", // ExitChargePercent
		"11", // FundBasedRenewalCommissionPercent
		"12", // ProjectedFundValue
		"13", // FundBasedRenewalCommissionOnOrder
		"14", // FundBasedRenewalCommissionOnFund
		"15", // NetSettlementAmount
	];

	static readonly string[] OwnerTypeCodeSet =
	[
		"1", // IndividualInvestor
		"2", // PublicCompany
		"3", // PrivateCompany
		"4", // IndividualTrustee
		"5", // CompanyTrustee
		"6", // PensionPlan
		"7", // CustodianUnderGiftsToMinorsAct
		"8", // Trusts
		"9", // Fiduciaries
		"10", // NetworkingSubAccount
		"11", // NonProfitOrganization
		"12", // CorporateBody
		"13", // Nominee
	];

	static readonly string[] OrderCapacityCodeSet =
	[
		"A", // Agency
		"G", // Proprietary
		"I", // Individual
		"P", // Principal
		"R", // RisklessPrincipal
		"W", // AgentForOtherMember
	];

	static readonly string[] OrderRestrictionsCodeSet =
	[
		"1", // ProgramTrade
		"2", // IndexArbitrage
		"3", // NonIndexArbitrage
		"4", // CompetingMarketMaker
		"5", // ActingAsMarketMakerOrSpecialistInSecurity
		"6", // ActingAsMarketMakerOrSpecialistInUnderlying
		"7", // ForeignEntity
		"8", // ExternalMarketParticipant
		"9", // ExternalInterConnectedMarketLinkage
		"A", // RisklessArbitrage
	];

	static readonly string[] MassCancelRequestTypeCodeSet =
	[
		"1", // CancelOrdersForASecurity
		"2", // CancelOrdersForAnUnderlyingSecurity
		"3", // CancelOrdersForAProduct
		"4", // CancelOrdersForACFICode
		"5", // CancelOrdersForASecurityType
		"6", // CancelOrdersForATradingSession
		"7", // CancelAllOrders
	];

	static readonly string[] MassCancelResponseCodeSet =
	[
		"0", // CancelRequestRejected
		"1", // CancelOrdersForASecurity
		"2", // CancelOrdersForAnUnderlyingSecurity
		"3", // CancelOrdersForAProduct
		"4", // CancelOrdersForACFICode
		"5", // CancelOrdersForASecurityType
		"6", // CancelOrdersForATradingSession
		"7", // CancelAllOrders
	];

	static readonly string[] MassCancelRejectReasonCodeSet =
	[
		"0", // MassCancelNotSupported
		"1", // InvalidOrUnknownSecurity
		"2", // InvalidOrUnkownUnderlyingSecurity
		"3", // InvalidOrUnknownProduct
		"4", // InvalidOrUnknownCFICode
		"5", // InvalidOrUnknownSecurityType
		"6", // InvalidOrUnknownTradingSession
		"99", // Other
	];

	static readonly string[] QuoteTypeCodeSet =
	[
		"0", // Indicative
		"1", // Tradeable
		"2", // RestrictedTradeable
		"3", // Counter
	];

	static readonly string[] CashMarginCodeSet =
	[
		"1", // Cash
		"2", // MarginOpen
		"3", // MarginClose
	];

	static readonly string[] ScopeCodeSet =
	[
		"1", // LocalMarket
		"2", // National
		"3", // Global
	];

	static readonly string[] MDImplicitDeleteCodeSet =
	[
		"Y", // Yes
		"N", // No
	];

	static readonly string[] CrossTypeCodeSet =
	[
		"1", // CrossAON
		"2", // CrossIOC
		"3", // CrossOneSide
		"4", // CrossSamePrice
	];

	static readonly string[] CrossPrioritizationCodeSet =
	[
		"0", // None
		"1", // BuySideIsPrioritized
		"2", // SellSideIsPrioritized
	];

	static readonly string[] NoSidesCodeSet =
	[
		"1", // OneSide
		"2", // BothSides
	];

	static readonly string[] SecurityListRequestTypeCodeSet =
	[
		"0", // Symbol
		"1", // SecurityTypeAnd
		"2", // Product
		"3", // TradingSessionID
		"4", // AllSecurities
	];

	static readonly string[] SecurityRequestResultCodeSet =
	[
		"0", // ValidRequest
		"1", // InvalidOrUnsupportedRequest
		"2", // NoInstrumentsFound
		"3", // NotAuthorizedToRetrieveInstrumentData
		"4", // InstrumentDataTemporarilyUnavailable
		"5", // RequestForInstrumentDataNotSupported
	];

	static readonly string[] MultiLegRptTypeReqCodeSet =
	[
		"0", // ReportByMulitlegSecurityOnly
		"1", // ReportByMultilegSecurityAndInstrumentLegs
		"2", // ReportByInstrumentLegsOnly
	];

	static readonly string[] TradSesStatusRejReasonCodeSet =
	[
		"1", // UnknownOrInvalidTradingSessionID
		"99", // Other
	];

	static readonly string[] TradeRequestTypeCodeSet =
	[
		"0", // AllTrades
		"1", // MatchedTradesMatchingCriteria
		"2", // UnmatchedTradesThatMatchCriteria
		"3", // UnreportedTradesThatMatchCriteria
		"4", // AdvisoriesThatMatchCriteria
	];

	static readonly string[] PreviouslyReportedCodeSet =
	[
		"Y", // PerviouslyReportedToCounterparty
		"N", // NotReportedToCounterparty
	];

	static readonly string[] MatchStatusCodeSet =
	[
		"0", // Compared
		"1", // Uncompared
		"2", // AdvisoryOrAlert
	];

	static readonly string[] MatchTypeCodeSet =
	[
		"A1", // ExactMatchPlus4BadgesExecTime
		"A2", // ExactMatchPlus4Badges
		"A3", // ExactMatchPlus2BadgesExecTime
		"A4", // ExactMatchPlus2Badges
		"A5", // ExactMatchPlusExecTime
		"AQ", // StampedAdvisoriesOrSpecialistAccepts
		"S1", // A1ExactMatchSummarizedQuantity
		"S2", // A2ExactMatchSummarizedQuantity
		"S3", // A3ExactMatchSummarizedQuantity
		"S4", // A4ExactMatchSummarizedQuantity
		"S5", // A5ExactMatchSummarizedQuantity
		"M1", // ExactMatchMinusBadgesTimes
		"M2", // SummarizedMatchMinusBadgesTimes
		"MT", // OCSLockedIn
		"M3", // ACTAcceptedTrade
		"M4", // ACTDefaultTrade
		"M5", // ACTDefaultAfterM2
		"M6", // ACTM6Match
	];

	static readonly string[] OddLotCodeSet =
	[
		"Y", // TreatAsOddLot
		"N", // TreatAsRoundLot
	];

	static readonly string[] ClearingInstructionCodeSet =
	[
		"0", // ProcessNormally
		"1", // ExcludeFromAllNetting
		"2", // BilateralNettingOnly
		"3", // ExClearing
		"4", // SpecialTrade
		"5", // MultilateralNetting
		"6", // ClearAgainstCentralCounterparty
		"7", // ExcludeFromCentralCounterparty
		"8", // ManualMode
		"9", // AutomaticPostingMode
		"10", // AutomaticGiveUpMode
		"11", // QualifiedServiceRepresentativeQSR
		"12", // CustomerTrade
		"13", // SelfClearing
	];

	static readonly string[] AccountTypeCodeSet =
	[
		"1", // CarriedCustomerSide
		"2", // CarriedNonCustomerSide
		"3", // HouseTrader
		"4", // FloorTrader
		"6", // CarriedNonCustomerSideCrossMargined
		"7", // HouseTraderCrossMargined
		"8", // JointBackOfficeAccount
	];

	static readonly string[] CustOrderCapacityCodeSet =
	[
		"1", // MemberTradingForTheirOwnAccount
		"2", // ClearingFirmTradingForItsProprietaryAccount
		"3", // MemberTradingForAnotherMember
		"4", // AllOther
	];

	static readonly string[] MassStatusReqTypeCodeSet =
	[
		"1", // StatusForOrdersForASecurity
		"2", // StatusForOrdersForAnUnderlyingSecurity
		"3", // StatusForOrdersForAProduct
		"4", // StatusForOrdersForACFICode
		"5", // StatusForOrdersForASecurityType
		"6", // StatusForOrdersForATradingSession
		"7", // StatusForAllOrders
		"8", // StatusForOrdersForAPartyID
	];

	static readonly string[] DayBookingInstCodeSet =
	[
		"0", // Auto
		"1", // SpeakWithOrderInitiatorBeforeBooking
		"2", // Accumulate
	];

	static readonly string[] BookingUnitCodeSet =
	[
		"0", // EachPartialExecutionIsABookableUnit
		"1", // AggregatePartialExecutionsOnThisOrder
		"2", // AggregateExecutionsForThisSymbol
	];

	static readonly string[] PreallocMethodCodeSet =
	[
		"0", // ProRata
		"1", // DoNotProRata
	];

	static readonly string[] AllocTypeCodeSet =
	[
		"1", // Calculated
		"2", // Preliminary
		"5", // ReadyToBook
		"7", // WarehouseInstruction
		"8", // RequestToIntermediary
	];

	static readonly string[] ClearingFeeIndicatorCodeSet =
	[
		"B", // CBOEMember
		"C", // NonMemberAndCustomer
		"E", // EquityMemberAndClearingMember
		"F", // FullAndAssociateMember
		"H", // Firms106HAnd106J
		"I", // GIM
		"L", // Lessee106FEmployees
		"M", // AllOtherOwnershipTypes
		"1", // FirstYearDelegate
		"2", // SecondYearDelegate
		"3", // ThirdYearDelegate
		"4", // FourthYearDelegate
		"5", // FifthYearDelegate
		"9", // SixthYearDelegate
	];

	static readonly string[] WorkingIndicatorCodeSet =
	[
		"Y", // Working
		"N", // NotWorking
	];

	static readonly string[] PriorityIndicatorCodeSet =
	[
		"0", // PriorityUnchanged
		"1", // LostPriorityAsResultOfOrderChange
	];

	static readonly string[] LegalConfirmCodeSet =
	[
		"Y", // LegalConfirm
		"N", // DoesNotConsituteALegalConfirm
	];

	static readonly string[] QuoteRequestRejectReasonCodeSet =
	[
		"1", // UnknownSymbol
		"2", // Exchange
		"3", // QuoteRequestExceedsLimit
		"4", // TooLateToEnter
		"5", // InvalidPrice
		"6", // NotAuthorizedToRequestQuote
		"7", // NoMatchForInquiry
		"8", // NoMarketForInstrument
		"9", // NoInventory
		"10", // Pass
		"99", // Other
	];

	static readonly string[] AcctIDSourceCodeSet =
	[
		"1", // BIC
		"2", // SIDCode
		"3", // TFM
		"4", // OMGEO
		"5", // DTCCCode
		"99", // Other
	];

	static readonly string[] ConfirmStatusCodeSet =
	[
		"1", // Received
		"2", // MismatchedAccount
		"3", // MissingSettlementInstructions
		"4", // Confirmed
		"5", // RequestRejected
	];

	static readonly string[] ConfirmTransTypeCodeSet =
	[
		"0", // New
		"1", // Replace
		"2", // Cancel
	];

	static readonly string[] DeliveryFormCodeSet =
	[
		"1", // BookEntry
		"2", // Bearer
	];

	static readonly string[] LegSwapTypeCodeSet =
	[
		"1", // ParForPar
		"2", // ModifiedDuration
		"4", // Risk
		"5", // Proceeds
	];

	static readonly string[] QuotePriceTypeCodeSet =
	[
		"1", // Percent
		"2", // PerShare
		"3", // FixedAmount
		"4", // Discount
		"5", // Premium
		"6", // Spread
		"7", // TEDPrice
		"8", // TEDYield
		"9", // YieldSpread
		"10", // Yield
	];

	static readonly string[] QuoteRespTypeCodeSet =
	[
		"1", // Hit
		"2", // Counter
		"3", // Expired
		"4", // Cover
		"5", // DoneAway
		"6", // Pass
	];

	static readonly string[] PosTypeCodeSet =
	[
		"TQ", // TransactionQuantity
		"IAS", // IntraSpreadQty
		"IES", // InterSpreadQty
		"FIN", // EndOfDayQty
		"SOD", // StartOfDayQty
		"EX", // OptionExerciseQty
		"AS", // OptionAssignment
		"TX", // TransactionFromExercise
		"TA", // TransactionFromAssignment
		"PIT", // PitTradeQty
		"TRF", // TransferTradeQty
		"ETR", // ElectronicTradeQty
		"ALC", // AllocationTradeQty
		"PA", // AdjustmentQty
		"ASF", // AsOfTradeQty
		"DLV", // DeliveryQty
		"TOT", // TotalTransactionQty
		"XM", // CrossMarginQty
		"SPL", // IntegralSplit
	];

	static readonly string[] PosQtyStatusCodeSet =
	[
		"0", // Submitted
		"1", // Accepted
		"2", // Rejected
	];

	static readonly string[] PosAmtTypeCodeSet =
	[
		"FMTM", // FinalMarkToMarketAmount
		"IMTM", // IncrementalMarkToMarketAmount
		"TVAR", // TradeVariationAmount
		"SMTM", // StartOfDayMarkToMarketAmount
		"PREM", // PremiumAmount
		"CRES", // CashResidualAmount
		"CASH", // CashAmount
		"VADJ", // ValueAdjustedAmount
	];

	static readonly string[] PosTransTypeCodeSet =
	[
		"1", // Exercise
		"2", // DoNotExercise
		"3", // PositionAdjustment
		"4", // PositionChangeSubmission
		"5", // Pledge
	];

	static readonly string[] PosMaintActionCodeSet =
	[
		"1", // New
		"2", // Replace
		"3", // Cancel
	];

	static readonly string[] SettlSessIDCodeSet =
	[
		"ITD", // Intraday
		"RTH", // RegularTradingHours
		"ETH", // ElectronicTradingHours
	];

	static readonly string[] AdjustmentTypeCodeSet =
	[
		"0", // ProcessRequestAsMarginDisposition
		"1", // DeltaPlus
		"2", // DeltaMinus
		"3", // Final
	];

	static readonly string[] PosMaintStatusCodeSet =
	[
		"0", // Accepted
		"1", // AcceptedWithWarnings
		"2", // Rejected
		"3", // Completed
		"4", // CompletedWithWarnings
	];

	static readonly string[] PosMaintResultCodeSet =
	[
		"0", // SuccessfulCompletion
		"1", // Rejected
		"99", // Other
	];

	static readonly string[] PosReqTypeCodeSet =
	[
		"0", // Positions
		"1", // Trades
		"2", // Exercises
		"3", // Assignments
	];

	static readonly string[] ResponseTransportTypeCodeSet =
	[
		"0", // Inband
		"1", // OutOfBand
	];

	static readonly string[] PosReqResultCodeSet =
	[
		"0", // ValidRequest
		"1", // InvalidOrUnsupportedRequest
		"2", // NoPositionsFoundThatMatchCriteria
		"3", // NotAuthorizedToRequestPositions
		"4", // RequestForPositionNotSupported
		"99", // Other
	];

	static readonly string[] PosReqStatusCodeSet =
	[
		"0", // Completed
		"1", // CompletedWithWarnings
		"2", // Rejected
	];

	static readonly string[] SettlPriceTypeCodeSet =
	[
		"1", // Final
		"2", // Theoretical
	];

	static readonly string[] AssignmentMethodCodeSet =
	[
		"R", // Random
		"P", // ProRata
	];

	static readonly string[] ExerciseMethodCodeSet =
	[
		"A", // Automatic
		"M", // Manual
	];

	static readonly string[] TradeRequestResultCodeSet =
	[
		"0", // Successful
		"1", // InvalidOrUnknownInstrument
		"2", // InvalidTypeOfTradeRequested
		"3", // InvalidParties
		"4", // InvalidTransportTypeRequested
		"5", // InvalidDestinationRequested
		"8", // TradeRequestTypeNotSupported
		"9", // NotAuthorized
		"99", // Other
	];

	static readonly string[] TradeRequestStatusCodeSet =
	[
		"0", // Accepted
		"1", // Completed
		"2", // Rejected
	];

	static readonly string[] TradeReportRejectReasonCodeSet =
	[
		"0", // Successful
		"1", // InvalidPartyOnformation
		"2", // UnknownInstrument
		"3", // UnauthorizedToReportTrades
		"4", // InvalidTradeType
		"99", // Other
	];

	static readonly string[] SideMultiLegReportingTypeCodeSet =
	[
		"1", // SingleSecurity
		"2", // IndividualLegOfAMultilegSecurity
		"3", // MultilegSecurity
	];

	static readonly string[] TrdRegTimestampTypeCodeSet =
	[
		"1", // ExecutionTime
		"2", // TimeIn
		"3", // TimeOut
		"4", // BrokerReceipt
		"5", // BrokerExecution
	];

	static readonly string[] ConfirmTypeCodeSet =
	[
		"1", // Status
		"2", // Confirmation
		"3", // ConfirmationRequestRejected
	];

	static readonly string[] ConfirmRejReasonCodeSet =
	[
		"1", // MismatchedAccount
		"2", // MissingSettlementInstructions
		"99", // Other
	];

	static readonly string[] BookingTypeCodeSet =
	[
		"0", // RegularBooking
		"1", // CFD
		"2", // TotalReturnSwap
	];

	static readonly string[] AllocSettlInstTypeCodeSet =
	[
		"0", // UseDefaultInstructions
		"1", // DeriveFromParametersProvided
		"2", // FullDetailsProvided
		"3", // SSIDBIDsProvided
		"4", // PhoneForInstructions
	];

	static readonly string[] DlvyInstTypeCodeSet =
	[
		"S", // Securities
		"C", // Cash
	];

	static readonly string[] TerminationTypeCodeSet =
	[
		"1", // Overnight
		"2", // Term
		"3", // Flexible
		"4", // Open
	];

	static readonly string[] SettlInstReqRejCodeCodeSet =
	[
		"0", // UnableToProcessRequest
		"1", // UnknownAccount
		"2", // NoMatchingSettlementInstructionsFound
		"99", // Other
	];

	static readonly string[] AllocReportTypeCodeSet =
	[
		"3", // SellsideCalculatedUsingPreliminary
		"4", // SellsideCalculatedWithoutPreliminary
		"5", // WarehouseRecap
		"8", // RequestToIntermediary
	];

	static readonly string[] AllocCancReplaceReasonCodeSet =
	[
		"1", // OriginalDetailsIncomplete
		"2", // ChangeInUnderlyingOrderDetails
		"99", // Other
	];

	static readonly string[] AllocAccountTypeCodeSet =
	[
		"1", // CarriedCustomerSide
		"2", // CarriedNonCustomerSide
		"3", // HouseTrader
		"4", // FloorTrader
		"6", // CarriedNonCustomerSideCrossMargined
		"7", // HouseTraderCrossMargined
		"8", // JointBackOfficeAccount
	];

	static readonly string[] PartySubIDTypeCodeSet =
	[
		"1", // Firm
		"2", // Person
		"3", // System
		"4", // Application
		"5", // FullLegalNameOfFirm
		"6", // PostalAddress
		"7", // PhoneNumber
		"8", // EmailAddress
		"9", // ContactName
		"10", // SecuritiesAccountNumber
		"11", // RegistrationNumber
		"12", // RegisteredAddressForConfirmation
		"13", // RegulatoryStatus
		"14", // RegistrationName
		"15", // CashAccountNumber
		"16", // BIC
		"17", // CSDParticipantMemberCode
		"18", // RegisteredAddress
		"19", // FundAccountName
		"20", // TelexNumber
		"21", // FaxNumber
		"22", // SecuritiesAccountName
		"23", // CashAccountName
		"24", // Department
		"25", // LocationDesk
		"26", // PositionAccountType
	];

	static readonly string[] AllocIntermedReqTypeCodeSet =
	[
		"1", // PendingAccept
		"2", // PendingRelease
		"3", // PendingReversal
		"4", // Accept
		"5", // BlockLevelReject
		"6", // AccountLevelReject
	];

	static readonly string[] ApplQueueResolutionCodeSet =
	[
		"0", // NoActionTaken
		"1", // QueueFlushed
		"2", // OverlayLast
		"3", // EndSession
	];

	static readonly string[] ApplQueueActionCodeSet =
	[
		"0", // NoActionTaken
		"1", // QueueFlushed
		"2", // OverlayLast
		"3", // EndSession
	];

	static readonly string[] AvgPxIndicatorCodeSet =
	[
		"0", // NoAveragePricing
		"1", // Trade
		"2", // LastTrade
	];

	static readonly string[] TradeAllocIndicatorCodeSet =
	[
		"0", // AllocationNotRequired
		"1", // AllocationRequired
		"2", // UseAllocationProvidedWithTheTrade
	];

	static readonly string[] ExpirationCycleCodeSet =
	[
		"0", // ExpireOnTradingSessionClose
		"1", // ExpireOnTradingSessionOpen
	];

	static readonly string[] TrdTypeCodeSet =
	[
		"0", // RegularTrade
		"1", // BlockTrade
		"2", // EFP
		"3", // Transfer
		"4", // LateTrade
		"5", // TTrade
		"6", // WeightedAveragePriceTrade
		"7", // BunchedTrade
		"8", // LateBunchedTrade
		"9", // PriorReferencePriceTrade
		"10", // AfterHoursTrade
	];

	static readonly string[] PegMoveTypeCodeSet =
	[
		"0", // Floating
		"1", // Fixed
	];

	static readonly string[] PegOffsetTypeCodeSet =
	[
		"0", // Price
		"1", // BasisPoints
		"2", // Ticks
		"3", // PriceTier
	];

	static readonly string[] PegLimitTypeCodeSet =
	[
		"0", // OrBetter
		"1", // Strict
		"2", // OrWorse
	];

	static readonly string[] PegRoundDirectionCodeSet =
	[
		"1", // MoreAggressive
		"2", // MorePassive
	];

	static readonly string[] PegScopeCodeSet =
	[
		"1", // Local
		"2", // National
		"3", // Global
		"4", // NationalExcludingLocal
	];

	static readonly string[] DiscretionMoveTypeCodeSet =
	[
		"0", // Floating
		"1", // Fixed
	];

	static readonly string[] DiscretionOffsetTypeCodeSet =
	[
		"0", // Price
		"1", // BasisPoints
		"2", // Ticks
		"3", // PriceTier
	];

	static readonly string[] DiscretionLimitTypeCodeSet =
	[
		"0", // OrBetter
		"1", // Strict
		"2", // OrWorse
	];

	static readonly string[] DiscretionRoundDirectionCodeSet =
	[
		"1", // MoreAggressive
		"2", // MorePassive
	];

	static readonly string[] DiscretionScopeCodeSet =
	[
		"1", // Local
		"2", // National
		"3", // Global
		"4", // NationalExcludingLocal
	];

	static readonly string[] TargetStrategyCodeSet =
	[
		"1", // VWAP
		"2", // Participate
		"3", // MininizeMarketImpact
	];

	static readonly string[] LastLiquidityIndCodeSet =
	[
		"1", // AddedLiquidity
		"2", // RemovedLiquidity
		"3", // LiquidityRoutedOut
	];

	static readonly string[] PublishTrdIndicatorCodeSet =
	[
		"Y", // ReportTrade
		"N", // DoNotReportTrade
	];

	static readonly string[] ShortSaleReasonCodeSet =
	[
		"0", // DealerSoldShort
		"1", // DealerSoldShortExempt
		"2", // SellingCustomerSoldShort
		"3", // SellingCustomerSoldShortExempt
		"4", // QualifiedServiceRepresentative
		"5", // QSROrAGUContraSideSoldShortExempt
	];

	static readonly string[] QtyTypeCodeSet =
	[
		"0", // Units
		"1", // Contracts
	];

	static readonly string[] TradeReportTypeCodeSet =
	[
		"0", // Submit
		"1", // Alleged
		"2", // Accept
		"3", // Decline
		"4", // Addendum
		"5", // No
		"6", // TradeReportCancel
		"7", // LockedIn
	];

	static readonly string[] AllocNoOrdersTypeCodeSet =
	[
		"0", // NotSpecified
		"1", // ExplicitListProvided
	];

	static readonly string[] EventTypeCodeSet =
	[
		"1", // Put
		"2", // Call
		"3", // Tender
		"4", // SinkingFundCall
		"99", // Other
	];

	static readonly string[] InstrAttribTypeCodeSet =
	[
		"1", // Flat
		"2", // ZeroCoupon
		"3", // InterestBearing
		"4", // NoPeriodicPayments
		"5", // VariableRate
		"6", // LessFeeForPut
		"7", // SteppedCoupon
		"8", // CouponPeriod
		"9", // When
		"10", // OriginalIssueDiscount
		"11", // Callable
		"12", // EscrowedToMaturity
		"13", // EscrowedToRedemptionDate
		"14", // PreRefunded
		"15", // InDefault
		"16", // Unrated
		"17", // Taxable
		"18", // Indexed
		"19", // SubjectToAlternativeMinimumTax
		"20", // OriginalIssueDiscountPrice
		"21", // CallableBelowMaturityValue
		"22", // CallableWithoutNotice
		"99", // Text
	];

	static readonly string[] CPProgramCodeSet =
	[
		"1", // Program3a3
		"2", // Program42
		"99", // Other
	];

	static readonly string[] MiscFeeBasisCodeSet =
	[
		"0", // Absolute
		"1", // PerUnit
		"2", // Percentage
	];

	static readonly string[] LastFragmentCodeSet =
	[
		"Y", // LastMessage
		"N", // NotLastMessage
	];

	static readonly string[] CollAsgnReasonCodeSet =
	[
		"0", // Initial
		"1", // Scheduled
		"2", // TimeWarning
		"3", // MarginDeficiency
		"4", // MarginExcess
		"5", // ForwardCollateralDemand
		"6", // EventOfDefault
		"7", // AdverseTaxEvent
	];

	static readonly string[] CollInquiryQualifierCodeSet =
	[
		"0", // TradeDate
		"1", // GCInstrument
		"2", // CollateralInstrument
		"3", // SubstitutionEligible
		"4", // NotAssigned
		"5", // PartiallyAssigned
		"6", // FullyAssigned
		"7", // OutstandingTrades
	];

	static readonly string[] CollAsgnTransTypeCodeSet =
	[
		"0", // New
		"1", // Replace
		"2", // Cancel
		"3", // Release
		"4", // Reverse
	];

	static readonly string[] CollAsgnRespTypeCodeSet =
	[
		"0", // Received
		"1", // Accepted
		"2", // Declined
		"3", // Rejected
	];

	static readonly string[] CollAsgnRejectReasonCodeSet =
	[
		"0", // UnknownDeal
		"1", // UnknownOrInvalidInstrument
		"2", // UnauthorizedTransaction
		"3", // InsufficientCollateral
		"4", // InvalidTypeOfCollateral
		"5", // ExcessiveSubstitution
		"99", // Other
	];

	static readonly string[] CollStatusCodeSet =
	[
		"0", // Unassigned
		"1", // PartiallyAssigned
		"2", // AssignmentProposed
		"3", // Assigned
		"4", // Challenged
	];

	static readonly string[] DeliveryTypeCodeSet =
	[
		"0", // VersusPayment
		"1", // Free
		"2", // TriParty
		"3", // HoldInCustody
	];

	static readonly string[] UserRequestTypeCodeSet =
	[
		"1", // LogOnUser
		"2", // LogOffUser
		"3", // ChangePasswordForUser
		"4", // RequestIndividualUserStatus
	];

	static readonly string[] UserStatusCodeSet =
	[
		"1", // LoggedIn
		"2", // NotLoggedIn
		"3", // UserNotRecognised
		"4", // PasswordIncorrect
		"5", // PasswordChanged
		"6", // Other
	];

	static readonly string[] StatusValueCodeSet =
	[
		"1", // Connected
		"2", // NotConnectedUnexpected
		"3", // NotConnectedExpected
		"4", // InProcess
	];

	static readonly string[] NetworkRequestTypeCodeSet =
	[
		"1", // Snapshot
		"2", // Subscribe
		"4", // StopSubscribing
		"8", // LevelOfDetail
	];

	static readonly string[] NetworkStatusResponseTypeCodeSet =
	[
		"1", // Full
		"2", // IncrementalUpdate
	];

	static readonly string[] TrdRptStatusCodeSet =
	[
		"0", // Accepted
		"1", // Rejected
	];

	static readonly string[] AffirmStatusCodeSet =
	[
		"1", // Received
		"2", // ConfirmRejected
		"3", // Affirmed
	];

	static readonly string[] CollActionCodeSet =
	[
		"0", // Retain
		"1", // Add
		"2", // Remove
	];

	static readonly string[] CollInquiryStatusCodeSet =
	[
		"0", // Accepted
		"1", // AcceptedWithWarnings
		"2", // Completed
		"3", // CompletedWithWarnings
		"4", // Rejected
	];

	static readonly string[] CollInquiryResultCodeSet =
	[
		"0",  // Successful
		"1",  // InvalidOrUnknownInstrument
		"2",  // InvalidOrUnknownCollateralType
		"3",  // InvalidParties
		"4",  // InvalidTransportTypeRequested
		"5",  // InvalidDestinationRequested
		"6",  // NoCollateralFoundForTheTradeSpecified
		"7",  // NoCollateralFoundForTheOrderSpecified
		"8",  // CollateralInquiryTypeNotSupported
		"9",  // UnauthorizedForCollateralInquiry
		"99", // Other
	];

	public static string[]? Codes(int tag) => tag switch
	{
		  4 => AdvSideCodeSet,
		  5 => AdvTransTypeCodeSet,
		 13 => CommTypeCodeSet,
		 18 => ExecInstCodeSet,
		 21 => HandlInstCodeSet,
		 22 => SecurityIDSourceCodeSet,
		 25 => IOIQltyIndCodeSet,
		 27 => IOIQtyCodeSet,
		 28 => IOITransTypeCodeSet,
		 29 => LastCapacityCodeSet,
		 35 => MsgTypeCodeSet,
		 39 => OrdStatusCodeSet,
		 40 => OrdTypeCodeSet,
		 43 => PossDupFlagCodeSet,
		 54 => SideCodeSet,
		 59 => TimeInForceCodeSet,
		 61 => UrgencyCodeSet,
		 63 => SettlTypeCodeSet,
		 71 => AllocTransTypeCodeSet,
		 77 => PositionEffectCodeSet,
		 81 => ProcessCodeCodeSet,
		 87 => AllocStatusCodeSet,
		 88 => AllocRejCodeCodeSet,
		 94 => EmailTypeCodeSet,
		 97 => PossResendCodeSet,
		 98 => EncryptMethodCodeSet,
		102 => CxlRejReasonCodeSet,
		103 => OrdRejReasonCodeSet,
		104 => IOIQualifierCodeSet,
		113 => ReportToExchCodeSet,
		114 => LocateReqdCodeSet,
		121 => ForexReqCodeSet,
		123 => GapFillFlagCodeSet,
		127 => DKReasonCodeSet,
		130 => IOINaturalFlagCodeSet,
		139 => MiscFeeTypeCodeSet,
		141 => ResetSeqNumFlagCodeSet,
		150 => ExecTypeCodeSet,
		156 => SettlCurrFxRateCalcCodeSet,
		160 => SettlInstModeCodeSet,
		163 => SettlInstTransTypeCodeSet,
		165 => SettlInstSourceCodeSet,
		167 => SecurityTypeCodeSet,
		169 => StandInstDbTypeCodeSet,
		172 => SettlDeliveryTypeCodeSet,
		197 => AllocLinkTypeCodeSet,
		201 => PutOrCallCodeSet,
		203 => CoveredOrUncoveredCodeSet,
		208 => NotifyBrokerOfCreditCodeSet,
		209 => AllocHandlInstCodeSet,
		216 => RoutingTypeCodeSet,
		221 => BenchmarkCurveNameCodeSet,
		233 => StipulationTypeCodeSet,
		235 => YieldTypeCodeSet,
		258 => TradedFlatSwitchCodeSet,
		263 => SubscriptionRequestTypeCodeSet,
		265 => MDUpdateTypeCodeSet,
		266 => AggregatedBookCodeSet,
		269 => MDEntryTypeCodeSet,
		274 => TickDirectionCodeSet,
		276 => QuoteConditionCodeSet,
		277 => TradeConditionCodeSet,
		279 => MDUpdateActionCodeSet,
		281 => MDReqRejReasonCodeSet,
		285 => DeleteReasonCodeSet,
		286 => OpenCloseSettlFlagCodeSet,
		291 => FinancialStatusCodeSet,
		292 => CorporateActionCodeSet,
		297 => QuoteStatusCodeSet,
		298 => QuoteCancelTypeCodeSet,
		300 => QuoteRejectReasonCodeSet,
		301 => QuoteResponseLevelCodeSet,
		303 => QuoteRequestTypeCodeSet,
		305 => SecurityIDSourceCodeSet,
		321 => SecurityRequestTypeCodeSet,
		323 => SecurityResponseTypeCodeSet,
		325 => UnsolicitedIndicatorCodeSet,
		326 => SecurityTradingStatusCodeSet,
		327 => HaltReasonCodeSet,
		328 => InViewOfCommonCodeSet,
		329 => DueToRelatedCodeSet,
		334 => AdjustmentCodeSet,
		338 => TradSesMethodCodeSet,
		339 => TradSesModeCodeSet,
		340 => TradSesStatusCodeSet,
		347 => MessageEncodingCodeSet,
		368 => QuoteRejectReasonCodeSet,
		372 => MsgTypeCodeSet,
		373 => SessionRejectReasonCodeSet,
		374 => BidRequestTransTypeCodeSet,
		377 => SolicitedFlagCodeSet,
		378 => ExecRestatementReasonCodeSet,
		380 => BusinessRejectReasonCodeSet,
		385 => MsgDirectionCodeSet,
		388 => DiscretionInstCodeSet,
		394 => BidTypeCodeSet,
		399 => BidDescriptorTypeCodeSet,
		401 => SideValueIndCodeSet,
		409 => LiquidityIndTypeCodeSet,
		411 => ExchangeForPhysicalCodeSet,
		414 => ProgRptReqsCodeSet,
		416 => IncTaxIndCodeSet,
		418 => BidTradeTypeCodeSet,
		419 => BasisPxTypeCodeSet,
		423 => PriceTypeCodeSet,
		427 => GTBookingInstCodeSet,
		429 => ListStatusTypeCodeSet,
		430 => NetGrossIndCodeSet,
		431 => ListOrderStatusCodeSet,
		433 => ListExecInstTypeCodeSet,
		434 => CxlRejResponseToCodeSet,
		442 => MultiLegReportingTypeCodeSet,
		447 => PartyIDSourceCodeSet,
		452 => PartyRoleCodeSet,
		456 => SecurityIDSourceCodeSet,
		459 => SecurityIDSourceCodeSet,
		460 => ProductCodeSet,
		464 => TestMessageIndicatorCodeSet,
		468 => RoundingDirectionCodeSet,
		477 => DistribPaymentMethodCodeSet,
		480 => CancellationRightsCodeSet,
		481 => MoneyLaunderingStatusCodeSet,
		484 => ExecPriceTypeCodeSet,
		487 => TradeReportTransTypeCodeSet,
		492 => PaymentMethodCodeSet,
		495 => TaxAdvantageTypeCodeSet,
		497 => FundRenewWaivCodeSet,
		506 => RegistStatusCodeSet,
		507 => RegistRejReasonCodeCodeSet,
		514 => RegistTransTypeCodeSet,
		517 => OwnershipTypeCodeSet,
		519 => ContAmtTypeCodeSet,
		522 => OwnerTypeCodeSet,
		525 => PartyIDSourceCodeSet,
		528 => OrderCapacityCodeSet,
		529 => OrderRestrictionsCodeSet,
		530 => MassCancelRequestTypeCodeSet,
		531 => MassCancelResponseCodeSet,
		532 => MassCancelRejectReasonCodeSet,
		537 => QuoteTypeCodeSet,
		538 => PartyRoleCodeSet,
		544 => CashMarginCodeSet,
		546 => ScopeCodeSet,
		547 => MDImplicitDeleteCodeSet,
		549 => CrossTypeCodeSet,
		550 => CrossPrioritizationCodeSet,
		552 => NoSidesCodeSet,
		559 => SecurityListRequestTypeCodeSet,
		560 => SecurityRequestResultCodeSet,
		563 => MultiLegRptTypeReqCodeSet,
		564 => PositionEffectCodeSet,
		567 => TradSesStatusRejReasonCodeSet,
		569 => TradeRequestTypeCodeSet,
		570 => PreviouslyReportedCodeSet,
		573 => MatchStatusCodeSet,
		574 => MatchTypeCodeSet,
		575 => OddLotCodeSet,
		577 => ClearingInstructionCodeSet,
		581 => AccountTypeCodeSet,
		582 => CustOrderCapacityCodeSet,
		585 => MassStatusReqTypeCodeSet,
		587 => SettlTypeCodeSet,
		589 => DayBookingInstCodeSet,
		590 => BookingUnitCodeSet,
		591 => PreallocMethodCodeSet,
		603 => SecurityIDSourceCodeSet,
		606 => SecurityIDSourceCodeSet,
		626 => AllocTypeCodeSet,
		635 => ClearingFeeIndicatorCodeSet,
		636 => WorkingIndicatorCodeSet,
		638 => PriorityIndicatorCodeSet,
		650 => LegalConfirmCodeSet,
		658 => QuoteRequestRejectReasonCodeSet,
		660 => AcctIDSourceCodeSet,
		661 => AcctIDSourceCodeSet,
		665 => ConfirmStatusCodeSet,
		666 => ConfirmTransTypeCodeSet,
		668 => DeliveryFormCodeSet,
		674 => AcctIDSourceCodeSet,
		677 => BenchmarkCurveNameCodeSet,
		690 => LegSwapTypeCodeSet,
		692 => QuotePriceTypeCodeSet,
		694 => QuoteRespTypeCodeSet,
		703 => PosTypeCodeSet,
		706 => PosQtyStatusCodeSet,
		707 => PosAmtTypeCodeSet,
		709 => PosTransTypeCodeSet,
		712 => PosMaintActionCodeSet,
		716 => SettlSessIDCodeSet,
		718 => AdjustmentTypeCodeSet,
		722 => PosMaintStatusCodeSet,
		723 => PosMaintResultCodeSet,
		724 => PosReqTypeCodeSet,
		725 => ResponseTransportTypeCodeSet,
		728 => PosReqResultCodeSet,
		729 => PosReqStatusCodeSet,
		731 => SettlPriceTypeCodeSet,
		744 => AssignmentMethodCodeSet,
		747 => ExerciseMethodCodeSet,
		749 => TradeRequestResultCodeSet,
		750 => TradeRequestStatusCodeSet,
		751 => TradeReportRejectReasonCodeSet,
		752 => SideMultiLegReportingTypeCodeSet,
		758 => PartyIDSourceCodeSet,
		759 => PartyRoleCodeSet,
		761 => SecurityIDSourceCodeSet,
		770 => TrdRegTimestampTypeCodeSet,
		773 => ConfirmTypeCodeSet,
		774 => ConfirmRejReasonCodeSet,
		775 => BookingTypeCodeSet,
		776 => AllocRejCodeCodeSet,
		780 => AllocSettlInstTypeCodeSet,
		783 => PartyIDSourceCodeSet,
		784 => PartyRoleCodeSet,
		786 => PartySubIDTypeCodeSet,
		787 => DlvyInstTypeCodeSet,
		788 => TerminationTypeCodeSet,
		792 => SettlInstReqRejCodeCodeSet,
		794 => AllocReportTypeCodeSet,
		796 => AllocCancReplaceReasonCodeSet,
		798 => AllocAccountTypeCodeSet,
		803 => PartySubIDTypeCodeSet,
		805 => PartySubIDTypeCodeSet,
		807 => PartySubIDTypeCodeSet,
		808 => AllocIntermedReqTypeCodeSet,
		814 => ApplQueueResolutionCodeSet,
		815 => ApplQueueActionCodeSet,
		819 => AvgPxIndicatorCodeSet,
		826 => TradeAllocIndicatorCodeSet,
		827 => ExpirationCycleCodeSet,
		828 => TrdTypeCodeSet,
		835 => PegMoveTypeCodeSet,
		836 => PegOffsetTypeCodeSet,
		837 => PegLimitTypeCodeSet,
		838 => PegRoundDirectionCodeSet,
		840 => PegScopeCodeSet,
		841 => DiscretionMoveTypeCodeSet,
		842 => DiscretionOffsetTypeCodeSet,
		843 => DiscretionLimitTypeCodeSet,
		844 => DiscretionRoundDirectionCodeSet,
		846 => DiscretionScopeCodeSet,
		847 => TargetStrategyCodeSet,
		851 => LastLiquidityIndCodeSet,
		852 => PublishTrdIndicatorCodeSet,
		853 => ShortSaleReasonCodeSet,
		854 => QtyTypeCodeSet,
		856 => TradeReportTypeCodeSet,
		857 => AllocNoOrdersTypeCodeSet,
		865 => EventTypeCodeSet,
		871 => InstrAttribTypeCodeSet,
		875 => CPProgramCodeSet,
		888 => StipulationTypeCodeSet,
		891 => MiscFeeBasisCodeSet,
		893 => LastFragmentCodeSet,
		895 => CollAsgnReasonCodeSet,
		896 => CollInquiryQualifierCodeSet,
		903 => CollAsgnTransTypeCodeSet,
		905 => CollAsgnRespTypeCodeSet,
		906 => CollAsgnRejectReasonCodeSet,
		910 => CollStatusCodeSet,
		919 => DeliveryTypeCodeSet,
		924 => UserRequestTypeCodeSet,
		926 => UserStatusCodeSet,
		928 => StatusValueCodeSet,
		935 => NetworkRequestTypeCodeSet,
		937 => NetworkStatusResponseTypeCodeSet,
		939 => TrdRptStatusCodeSet,
		940 => AffirmStatusCodeSet,
		944 => CollActionCodeSet,
		945 => CollInquiryStatusCodeSet,
		946 => CollInquiryResultCodeSet,
		950 => PartyIDSourceCodeSet,
		951 => PartyRoleCodeSet,
		954 => PartySubIDTypeCodeSet,
		_   => null,
	};

	public static bool RequiresEncoding(int tag) => tag switch
	{
		349 => true,
		351 => true,
		353 => true,
		355 => true,
		357 => true,
		359 => true,
		361 => true,
		363 => true,
		365 => true,
		446 => true,
		619 => true,
		622 => true,
		_   => false,
	};
}
