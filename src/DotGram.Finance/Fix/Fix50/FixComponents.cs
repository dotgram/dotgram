using System;

// ReSharper disable InconsistentNaming

namespace DotGram.Finance.Fix.Fix50;

// Written by generate.py from the FIX 5.0 SP2 repository; not edited by hand.
//
// The components FIX 5.0 SP2 reuses across message types, as the shape a message or a group entry has
// when it carries one. A component is written into its carrier field by field, because that is
// what it is on the wire; the interface is those same fields read as the one thing they are, so
// that what is asked of a component is written once and asked of every carrier. A group inside a
// component is the same group wherever the component is carried, so its entry is a class nested
// in the interface: IInstrument.NoSecurityAltIDGroup.

/// <summary>The FIX 5.0 SP2 ApplicationSequenceControl block, wherever it is carried.</summary>
public interface IApplicationSequenceControl
{
	/// <summary>The FIX ApplID, tag 1180.</summary>
	FixField.Text? ApplID { get; }

	/// <summary>The FIX ApplSeqNum, tag 1181.</summary>
	FixField.Integer? ApplSeqNum { get; }

	/// <summary>The FIX ApplLastSeqNum, tag 1350.</summary>
	FixField.Integer? ApplLastSeqNum { get; }

	/// <summary>The FIX ApplResendFlag, tag 1352.</summary>
	FixField.Boolean? ApplResendFlag { get; }
}

/// <summary>The FIX 5.0 SP2 CommissionData block, wherever it is carried.</summary>
public interface ICommissionData
{
	/// <summary>The FIX Commission, tag 12.</summary>
	FixField.Decimal? Commission { get; }

	/// <summary>The FIX CommType, tag 13.</summary>
	FixField.Character? CommType { get; }

	/// <summary>The FIX CommCurrency, tag 479.</summary>
	FixField.Text? CommCurrency { get; }

	/// <summary>The FIX FundRenewWaiv, tag 497.</summary>
	FixField.Character? FundRenewWaiv { get; }
}

/// <summary>The FIX 5.0 SP2 DiscretionInstructions block, wherever it is carried.</summary>
public interface IDiscretionInstructions
{
	/// <summary>The FIX DiscretionInst, tag 388.</summary>
	FixField.Character? DiscretionInst { get; }

	/// <summary>The FIX DiscretionOffsetValue, tag 389.</summary>
	FixField.Decimal? DiscretionOffsetValue { get; }

	/// <summary>The FIX DiscretionMoveType, tag 841.</summary>
	FixField.Integer? DiscretionMoveType { get; }

	/// <summary>The FIX DiscretionOffsetType, tag 842.</summary>
	FixField.Integer? DiscretionOffsetType { get; }

	/// <summary>The FIX DiscretionLimitType, tag 843.</summary>
	FixField.Integer? DiscretionLimitType { get; }

	/// <summary>The FIX DiscretionRoundDirection, tag 844.</summary>
	FixField.Integer? DiscretionRoundDirection { get; }

	/// <summary>The FIX DiscretionScope, tag 846.</summary>
	FixField.Integer? DiscretionScope { get; }
}

/// <summary>The FIX 5.0 SP2 DisplayInstruction block, wherever it is carried.</summary>
public interface IDisplayInstruction
{
	/// <summary>The FIX DisplayQty, tag 1138.</summary>
	FixField.Decimal? DisplayQty { get; }

	/// <summary>The FIX SecondaryDisplayQty, tag 1082.</summary>
	FixField.Decimal? SecondaryDisplayQty { get; }

	/// <summary>The FIX DisplayWhen, tag 1083.</summary>
	FixField.Character? DisplayWhen { get; }

	/// <summary>The FIX DisplayMethod, tag 1084.</summary>
	FixField.Character? DisplayMethod { get; }

	/// <summary>The FIX DisplayLowQty, tag 1085.</summary>
	FixField.Decimal? DisplayLowQty { get; }

	/// <summary>The FIX DisplayHighQty, tag 1086.</summary>
	FixField.Decimal? DisplayHighQty { get; }

	/// <summary>The FIX DisplayMinIncr, tag 1087.</summary>
	FixField.Decimal? DisplayMinIncr { get; }

	/// <summary>The FIX RefreshQty, tag 1088.</summary>
	FixField.Decimal? RefreshQty { get; }
}

/// <summary>The FIX 5.0 SP2 ExpirationQty repeating component, wherever it is carried.</summary>
public interface IExpirationQty
{
	/// <summary>The FIX NoExpiration, tag 981.</summary>
	FixField.Integer? NoExpiration { get; }

	/// <summary>The entries counted by NoExpiration, tag 981; null when the group is absent.</summary>
	List<IExpirationQty.NoExpirationGroup>? NoExpirationGroups { get; }

	/// <summary>One entry of the group counted by NoExpiration, tag 981.</summary>
	public sealed class NoExpirationGroup
	{
		/// <summary>The FIX ExpirationQtyType, tag 982, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer ExpirationQtyType { get; init; }

		/// <summary>The FIX ExpQty, tag 983, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? ExpQty { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 FinancingDetails block, wherever it is carried.</summary>
public interface IFinancingDetails
{
	/// <summary>The FIX AgreementDesc, tag 913.</summary>
	FixField.Text? AgreementDesc { get; }

	/// <summary>The FIX AgreementID, tag 914.</summary>
	FixField.Text? AgreementID { get; }

	/// <summary>The FIX AgreementDate, tag 915.</summary>
	FixField.Date? AgreementDate { get; }

	/// <summary>The FIX AgreementCurrency, tag 918.</summary>
	FixField.Text? AgreementCurrency { get; }

	/// <summary>The FIX TerminationType, tag 788.</summary>
	FixField.Integer? TerminationType { get; }

	/// <summary>The FIX StartDate, tag 916.</summary>
	FixField.Date? StartDate { get; }

	/// <summary>The FIX EndDate, tag 917.</summary>
	FixField.Date? EndDate { get; }

	/// <summary>The FIX DeliveryType, tag 919.</summary>
	FixField.Integer? DeliveryType { get; }

	/// <summary>The FIX MarginRatio, tag 898.</summary>
	FixField.Decimal? MarginRatio { get; }
}

/// <summary>The FIX 5.0 SP2 Instrument block, wherever it is carried.</summary>
public interface IInstrument : IInstrumentParties
{
	/// <summary>The FIX Symbol, tag 55.</summary>
	FixField.Text? Symbol { get; }

	/// <summary>The FIX SymbolSfx, tag 65.</summary>
	FixField.Text? SymbolSfx { get; }

	/// <summary>The FIX SecurityID, tag 48.</summary>
	FixField.Text? SecurityID { get; }

	/// <summary>The FIX SecurityIDSource, tag 22.</summary>
	FixField.Text? SecurityIDSource { get; }

	/// <summary>The FIX NoSecurityAltID, tag 454.</summary>
	FixField.Integer? NoSecurityAltID { get; }

	/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
	List<IInstrument.NoSecurityAltIDGroup>? NoSecurityAltIDGroups { get; }

	/// <summary>The FIX Product, tag 460.</summary>
	FixField.Integer? Product { get; }

	/// <summary>The FIX ProductComplex, tag 1227.</summary>
	FixField.Text? ProductComplex { get; }

	/// <summary>The FIX SecurityGroup, tag 1151.</summary>
	FixField.Text? SecurityGroup { get; }

	/// <summary>The FIX CFICode, tag 461.</summary>
	FixField.Text? CFICode { get; }

	/// <summary>The FIX SecurityType, tag 167.</summary>
	FixField.Text? SecurityType { get; }

	/// <summary>The FIX SecuritySubType, tag 762.</summary>
	FixField.Text? SecuritySubType { get; }

	/// <summary>The FIX MaturityMonthYear, tag 200.</summary>
	FixField.MonthYear? MaturityMonthYear { get; }

	/// <summary>The FIX MaturityDate, tag 541.</summary>
	FixField.Date? MaturityDate { get; }

	/// <summary>The FIX MaturityTime, tag 1079.</summary>
	FixField.ZonedTime? MaturityTime { get; }

	/// <summary>The FIX SettleOnOpenFlag, tag 966.</summary>
	FixField.Text? SettleOnOpenFlag { get; }

	/// <summary>The FIX InstrmtAssignmentMethod, tag 1049.</summary>
	FixField.Character? InstrmtAssignmentMethod { get; }

	/// <summary>The FIX SecurityStatus, tag 965.</summary>
	FixField.Text? SecurityStatus { get; }

	/// <summary>The FIX CouponPaymentDate, tag 224.</summary>
	FixField.Date? CouponPaymentDate { get; }

	/// <summary>The FIX RestructuringType, tag 1449.</summary>
	FixField.Text? RestructuringType { get; }

	/// <summary>The FIX Seniority, tag 1450.</summary>
	FixField.Text? Seniority { get; }

	/// <summary>The FIX NotionalPercentageOutstanding, tag 1451.</summary>
	FixField.Decimal? NotionalPercentageOutstanding { get; }

	/// <summary>The FIX OriginalNotionalPercentageOutstanding, tag 1452.</summary>
	FixField.Decimal? OriginalNotionalPercentageOutstanding { get; }

	/// <summary>The FIX AttachmentPoint, tag 1457.</summary>
	FixField.Decimal? AttachmentPoint { get; }

	/// <summary>The FIX DetachmentPoint, tag 1458.</summary>
	FixField.Decimal? DetachmentPoint { get; }

	/// <summary>The FIX IssueDate, tag 225.</summary>
	FixField.Date? IssueDate { get; }

	/// <summary>The FIX RepoCollateralSecurityType, tag 239.</summary>
	FixField.Text? RepoCollateralSecurityType { get; }

	/// <summary>The FIX RepurchaseTerm, tag 226.</summary>
	FixField.Integer? RepurchaseTerm { get; }

	/// <summary>The FIX RepurchaseRate, tag 227.</summary>
	FixField.Decimal? RepurchaseRate { get; }

	/// <summary>The FIX Factor, tag 228.</summary>
	FixField.Decimal? Factor { get; }

	/// <summary>The FIX CreditRating, tag 255.</summary>
	FixField.Text? CreditRating { get; }

	/// <summary>The FIX InstrRegistry, tag 543.</summary>
	FixField.Text? InstrRegistry { get; }

	/// <summary>The FIX CountryOfIssue, tag 470.</summary>
	FixField.Text? CountryOfIssue { get; }

	/// <summary>The FIX StateOrProvinceOfIssue, tag 471.</summary>
	FixField.Text? StateOrProvinceOfIssue { get; }

	/// <summary>The FIX LocaleOfIssue, tag 472.</summary>
	FixField.Text? LocaleOfIssue { get; }

	/// <summary>The FIX RedemptionDate, tag 240.</summary>
	FixField.Date? RedemptionDate { get; }

	/// <summary>The FIX StrikePrice, tag 202.</summary>
	FixField.Decimal? StrikePrice { get; }

	/// <summary>The FIX StrikeCurrency, tag 947.</summary>
	FixField.Text? StrikeCurrency { get; }

	/// <summary>The FIX StrikeMultiplier, tag 967.</summary>
	FixField.Decimal? StrikeMultiplier { get; }

	/// <summary>The FIX StrikeValue, tag 968.</summary>
	FixField.Decimal? StrikeValue { get; }

	/// <summary>The FIX StrikePriceDeterminationMethod, tag 1478.</summary>
	FixField.Integer? StrikePriceDeterminationMethod { get; }

	/// <summary>The FIX StrikePriceBoundaryMethod, tag 1479.</summary>
	FixField.Integer? StrikePriceBoundaryMethod { get; }

	/// <summary>The FIX StrikePriceBoundaryPrecision, tag 1480.</summary>
	FixField.Decimal? StrikePriceBoundaryPrecision { get; }

	/// <summary>The FIX UnderlyingPriceDeterminationMethod, tag 1481.</summary>
	FixField.Integer? UnderlyingPriceDeterminationMethod { get; }

	/// <summary>The FIX OptAttribute, tag 206.</summary>
	FixField.Character? OptAttribute { get; }

	/// <summary>The FIX ContractMultiplier, tag 231.</summary>
	FixField.Decimal? ContractMultiplier { get; }

	/// <summary>The FIX ContractMultiplierUnit, tag 1435.</summary>
	FixField.Integer? ContractMultiplierUnit { get; }

	/// <summary>The FIX FlowScheduleType, tag 1439.</summary>
	FixField.Integer? FlowScheduleType { get; }

	/// <summary>The FIX MinPriceIncrement, tag 969.</summary>
	FixField.Decimal? MinPriceIncrement { get; }

	/// <summary>The FIX MinPriceIncrementAmount, tag 1146.</summary>
	FixField.Decimal? MinPriceIncrementAmount { get; }

	/// <summary>The FIX UnitOfMeasure, tag 996.</summary>
	FixField.Text? UnitOfMeasure { get; }

	/// <summary>The FIX UnitOfMeasureQty, tag 1147.</summary>
	FixField.Decimal? UnitOfMeasureQty { get; }

	/// <summary>The FIX PriceUnitOfMeasure, tag 1191.</summary>
	FixField.Text? PriceUnitOfMeasure { get; }

	/// <summary>The FIX PriceUnitOfMeasureQty, tag 1192.</summary>
	FixField.Decimal? PriceUnitOfMeasureQty { get; }

	/// <summary>The FIX SettlMethod, tag 1193.</summary>
	FixField.Character? SettlMethod { get; }

	/// <summary>The FIX ExerciseStyle, tag 1194.</summary>
	FixField.Integer? ExerciseStyle { get; }

	/// <summary>The FIX OptPayoutType, tag 1482.</summary>
	FixField.Integer? OptPayoutType { get; }

	/// <summary>The FIX OptPayoutAmount, tag 1195.</summary>
	FixField.Decimal? OptPayoutAmount { get; }

	/// <summary>The FIX PriceQuoteMethod, tag 1196.</summary>
	FixField.Text? PriceQuoteMethod { get; }

	/// <summary>The FIX ValuationMethod, tag 1197.</summary>
	FixField.Text? ValuationMethod { get; }

	/// <summary>The FIX ListMethod, tag 1198.</summary>
	FixField.Integer? ListMethod { get; }

	/// <summary>The FIX CapPrice, tag 1199.</summary>
	FixField.Decimal? CapPrice { get; }

	/// <summary>The FIX FloorPrice, tag 1200.</summary>
	FixField.Decimal? FloorPrice { get; }

	/// <summary>The FIX PutOrCall, tag 201.</summary>
	FixField.Integer? PutOrCall { get; }

	/// <summary>The FIX FlexibleIndicator, tag 1244.</summary>
	FixField.Boolean? FlexibleIndicator { get; }

	/// <summary>The FIX FlexProductEligibilityIndicator, tag 1242.</summary>
	FixField.Boolean? FlexProductEligibilityIndicator { get; }

	/// <summary>The FIX TimeUnit, tag 997.</summary>
	FixField.Text? TimeUnit { get; }

	/// <summary>The FIX CouponRate, tag 223.</summary>
	FixField.Decimal? CouponRate { get; }

	/// <summary>The FIX SecurityExchange, tag 207.</summary>
	FixField.Text? SecurityExchange { get; }

	/// <summary>The FIX PositionLimit, tag 970.</summary>
	FixField.Integer? PositionLimit { get; }

	/// <summary>The FIX NTPositionLimit, tag 971.</summary>
	FixField.Integer? NTPositionLimit { get; }

	/// <summary>The FIX Issuer, tag 106.</summary>
	FixField.Text? Issuer { get; }

	/// <summary>The FIX EncodedIssuerLen, tag 348.</summary>
	FixField.Integer? EncodedIssuerLen { get; }

	/// <summary>The FIX EncodedIssuer, tag 349.</summary>
	FixField.Data? EncodedIssuer { get; }

	/// <summary>The FIX SecurityDesc, tag 107.</summary>
	FixField.Text? SecurityDesc { get; }

	/// <summary>The FIX EncodedSecurityDescLen, tag 350.</summary>
	FixField.Integer? EncodedSecurityDescLen { get; }

	/// <summary>The FIX EncodedSecurityDesc, tag 351.</summary>
	FixField.Data? EncodedSecurityDesc { get; }

	/// <summary>The FIX SecurityXMLLen, tag 1184.</summary>
	FixField.Integer? SecurityXMLLen { get; }

	/// <summary>The FIX SecurityXML, tag 1185.</summary>
	FixField.Data? SecurityXML { get; }

	/// <summary>The FIX SecurityXMLSchema, tag 1186.</summary>
	FixField.Text? SecurityXMLSchema { get; }

	/// <summary>The FIX Pool, tag 691.</summary>
	FixField.Text? Pool { get; }

	/// <summary>The FIX ContractSettlMonth, tag 667.</summary>
	FixField.MonthYear? ContractSettlMonth { get; }

	/// <summary>The FIX CPProgram, tag 875.</summary>
	FixField.Integer? CPProgram { get; }

	/// <summary>The FIX CPRegType, tag 876.</summary>
	FixField.Text? CPRegType { get; }

	/// <summary>The FIX NoEvents, tag 864.</summary>
	FixField.Integer? NoEvents { get; }

	/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
	List<IInstrument.NoEventsGroup>? NoEventsGroups { get; }

	/// <summary>The FIX DatedDate, tag 873.</summary>
	FixField.Date? DatedDate { get; }

	/// <summary>The FIX InterestAccrualDate, tag 874.</summary>
	FixField.Date? InterestAccrualDate { get; }

	/// <summary>The FIX NoComplexEvents, tag 1483.</summary>
	FixField.Integer? NoComplexEvents { get; }

	/// <summary>The entries counted by NoComplexEvents, tag 1483; null when the group is absent.</summary>
	List<IInstrument.NoComplexEventsGroup>? NoComplexEventsGroups { get; }

	/// <summary>One entry of the group counted by NoSecurityAltID, tag 454.</summary>
	public sealed class NoSecurityAltIDGroup
	{
		/// <summary>The FIX SecurityAltID, tag 455, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text SecurityAltID { get; init; }

		/// <summary>The FIX SecurityAltIDSource, tag 456, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityAltIDSource { get; internal set; }
	}

	/// <summary>One entry of the group counted by NoEvents, tag 864.</summary>
	public sealed class NoEventsGroup
	{
		/// <summary>The FIX EventType, tag 865, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer EventType { get; init; }

		/// <summary>The FIX EventDate, tag 866, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? EventDate { get; internal set; }

		/// <summary>The FIX EventTime, tag 1145, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EventTime { get; internal set; }

		/// <summary>The FIX EventPx, tag 867, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? EventPx { get; internal set; }

		/// <summary>The FIX EventText, tag 868, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? EventText { get; internal set; }
	}

	/// <summary>One entry of the group counted by NoComplexEvents, tag 1483.</summary>
	public sealed class NoComplexEventsGroup
	{
		/// <summary>The FIX ComplexEventType, tag 1484, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer ComplexEventType { get; init; }

		/// <summary>The FIX ComplexOptPayoutAmount, tag 1485, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? ComplexOptPayoutAmount { get; internal set; }

		/// <summary>The FIX ComplexEventPrice, tag 1486, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? ComplexEventPrice { get; internal set; }

		/// <summary>The FIX ComplexEventPriceBoundaryMethod, tag 1487, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ComplexEventPriceBoundaryMethod { get; internal set; }

		/// <summary>The FIX ComplexEventPriceBoundaryPrecision, tag 1488, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public FixField.Decimal? ComplexEventPriceBoundaryPrecision { get; internal set; }

		/// <summary>The FIX ComplexEventPriceTimeType, tag 1489, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ComplexEventPriceTimeType { get; internal set; }

		/// <summary>The FIX ComplexEventCondition, tag 1490, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ComplexEventCondition { get; internal set; }

		/// <summary>The FIX NoComplexEventDates, tag 1491, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoComplexEventDates { get; internal set; }

		/// <summary>The entries counted by NoComplexEventDates, tag 1491; null when the group is absent.</summary>
		public List<IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup>? NoComplexEventDatesGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoComplexEventDates, tag 1491.</summary>
		public sealed class NoComplexEventDatesGroup
		{
			/// <summary>The FIX ComplexEventStartDate, tag 1492, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public required FixField.Timestamp ComplexEventStartDate { get; init; }

			/// <summary>The FIX ComplexEventEndDate, tag 1493, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ComplexEventEndDate { get; internal set; }

			/// <summary>The FIX NoComplexEventTimes, tag 1494, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
			public FixField.Integer? NoComplexEventTimes { get; internal set; }

			/// <summary>The entries counted by NoComplexEventTimes, tag 1494; null when the group is absent.</summary>
			public List<IInstrument.NoComplexEventsGroup.NoComplexEventDatesGroup.NoComplexEventTimesGroup>? NoComplexEventTimesGroups { get; internal set; }

			/// <summary>One entry of the group counted by NoComplexEventTimes, tag 1494.</summary>
			public sealed class NoComplexEventTimesGroup
			{
				/// <summary>The FIX ComplexEventStartTime, tag 1495, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
				public required FixField.Time ComplexEventStartTime { get; init; }

				/// <summary>The FIX ComplexEventEndTime, tag 1496, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
				public FixField.Time? ComplexEventEndTime { get; internal set; }
			}
		}
	}
}

/// <summary>The FIX 5.0 SP2 InstrumentExtension block, wherever it is carried.</summary>
public interface IInstrumentExtension
{
	/// <summary>The FIX DeliveryForm, tag 668.</summary>
	FixField.Integer? DeliveryForm { get; }

	/// <summary>The FIX PctAtRisk, tag 869.</summary>
	FixField.Decimal? PctAtRisk { get; }

	/// <summary>The FIX NoInstrAttrib, tag 870.</summary>
	FixField.Integer? NoInstrAttrib { get; }

	/// <summary>The entries counted by NoInstrAttrib, tag 870; null when the group is absent.</summary>
	List<IInstrumentExtension.NoInstrAttribGroup>? NoInstrAttribGroups { get; }

	/// <summary>One entry of the group counted by NoInstrAttrib, tag 870.</summary>
	public sealed class NoInstrAttribGroup
	{
		/// <summary>The FIX InstrAttribType, tag 871, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer InstrAttribType { get; init; }

		/// <summary>The FIX InstrAttribValue, tag 872, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? InstrAttribValue { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 InstrumentLeg block, wherever it is carried.</summary>
public interface IInstrumentLeg
{
	/// <summary>The FIX LegSymbol, tag 600.</summary>
	FixField.Text? LegSymbol { get; }

	/// <summary>The FIX LegSymbolSfx, tag 601.</summary>
	FixField.Text? LegSymbolSfx { get; }

	/// <summary>The FIX LegSecurityID, tag 602.</summary>
	FixField.Text? LegSecurityID { get; }

	/// <summary>The FIX LegSecurityIDSource, tag 603.</summary>
	FixField.Text? LegSecurityIDSource { get; }

	/// <summary>The FIX NoLegSecurityAltID, tag 604.</summary>
	FixField.Integer? NoLegSecurityAltID { get; }

	/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
	List<IInstrumentLeg.NoLegSecurityAltIDGroup>? NoLegSecurityAltIDGroups { get; }

	/// <summary>The FIX LegProduct, tag 607.</summary>
	FixField.Integer? LegProduct { get; }

	/// <summary>The FIX LegCFICode, tag 608.</summary>
	FixField.Text? LegCFICode { get; }

	/// <summary>The FIX LegSecurityType, tag 609.</summary>
	FixField.Text? LegSecurityType { get; }

	/// <summary>The FIX LegSecuritySubType, tag 764.</summary>
	FixField.Text? LegSecuritySubType { get; }

	/// <summary>The FIX LegMaturityMonthYear, tag 610.</summary>
	FixField.MonthYear? LegMaturityMonthYear { get; }

	/// <summary>The FIX LegMaturityDate, tag 611.</summary>
	FixField.Date? LegMaturityDate { get; }

	/// <summary>The FIX LegMaturityTime, tag 1212.</summary>
	FixField.ZonedTime? LegMaturityTime { get; }

	/// <summary>The FIX LegCouponPaymentDate, tag 248.</summary>
	FixField.Date? LegCouponPaymentDate { get; }

	/// <summary>The FIX LegIssueDate, tag 249.</summary>
	FixField.Date? LegIssueDate { get; }

	/// <summary>The FIX LegRepoCollateralSecurityType, tag 250.</summary>
	FixField.Text? LegRepoCollateralSecurityType { get; }

	/// <summary>The FIX LegRepurchaseTerm, tag 251.</summary>
	FixField.Integer? LegRepurchaseTerm { get; }

	/// <summary>The FIX LegRepurchaseRate, tag 252.</summary>
	FixField.Decimal? LegRepurchaseRate { get; }

	/// <summary>The FIX LegFactor, tag 253.</summary>
	FixField.Decimal? LegFactor { get; }

	/// <summary>The FIX LegCreditRating, tag 257.</summary>
	FixField.Text? LegCreditRating { get; }

	/// <summary>The FIX LegInstrRegistry, tag 599.</summary>
	FixField.Text? LegInstrRegistry { get; }

	/// <summary>The FIX LegCountryOfIssue, tag 596.</summary>
	FixField.Text? LegCountryOfIssue { get; }

	/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597.</summary>
	FixField.Text? LegStateOrProvinceOfIssue { get; }

	/// <summary>The FIX LegLocaleOfIssue, tag 598.</summary>
	FixField.Text? LegLocaleOfIssue { get; }

	/// <summary>The FIX LegRedemptionDate, tag 254.</summary>
	FixField.Date? LegRedemptionDate { get; }

	/// <summary>The FIX LegStrikePrice, tag 612.</summary>
	FixField.Decimal? LegStrikePrice { get; }

	/// <summary>The FIX LegStrikeCurrency, tag 942.</summary>
	FixField.Text? LegStrikeCurrency { get; }

	/// <summary>The FIX LegOptAttribute, tag 613.</summary>
	FixField.Character? LegOptAttribute { get; }

	/// <summary>The FIX LegContractMultiplier, tag 614.</summary>
	FixField.Decimal? LegContractMultiplier { get; }

	/// <summary>The FIX LegContractMultiplierUnit, tag 1436.</summary>
	FixField.Integer? LegContractMultiplierUnit { get; }

	/// <summary>The FIX LegFlowScheduleType, tag 1440.</summary>
	FixField.Integer? LegFlowScheduleType { get; }

	/// <summary>The FIX LegUnitOfMeasure, tag 999.</summary>
	FixField.Text? LegUnitOfMeasure { get; }

	/// <summary>The FIX LegUnitOfMeasureQty, tag 1224.</summary>
	FixField.Decimal? LegUnitOfMeasureQty { get; }

	/// <summary>The FIX LegPriceUnitOfMeasure, tag 1421.</summary>
	FixField.Text? LegPriceUnitOfMeasure { get; }

	/// <summary>The FIX LegPriceUnitOfMeasureQty, tag 1422.</summary>
	FixField.Decimal? LegPriceUnitOfMeasureQty { get; }

	/// <summary>The FIX LegTimeUnit, tag 1001.</summary>
	FixField.Text? LegTimeUnit { get; }

	/// <summary>The FIX LegExerciseStyle, tag 1420.</summary>
	FixField.Integer? LegExerciseStyle { get; }

	/// <summary>The FIX LegCouponRate, tag 615.</summary>
	FixField.Decimal? LegCouponRate { get; }

	/// <summary>The FIX LegSecurityExchange, tag 616.</summary>
	FixField.Text? LegSecurityExchange { get; }

	/// <summary>The FIX LegIssuer, tag 617.</summary>
	FixField.Text? LegIssuer { get; }

	/// <summary>The FIX EncodedLegIssuerLen, tag 618.</summary>
	FixField.Integer? EncodedLegIssuerLen { get; }

	/// <summary>The FIX EncodedLegIssuer, tag 619.</summary>
	FixField.Data? EncodedLegIssuer { get; }

	/// <summary>The FIX LegSecurityDesc, tag 620.</summary>
	FixField.Text? LegSecurityDesc { get; }

	/// <summary>The FIX EncodedLegSecurityDescLen, tag 621.</summary>
	FixField.Integer? EncodedLegSecurityDescLen { get; }

	/// <summary>The FIX EncodedLegSecurityDesc, tag 622.</summary>
	FixField.Data? EncodedLegSecurityDesc { get; }

	/// <summary>The FIX LegRatioQty, tag 623.</summary>
	FixField.Decimal? LegRatioQty { get; }

	/// <summary>The FIX LegSide, tag 624.</summary>
	FixField.Character? LegSide { get; }

	/// <summary>The FIX LegCurrency, tag 556.</summary>
	FixField.Text? LegCurrency { get; }

	/// <summary>The FIX LegPool, tag 740.</summary>
	FixField.Text? LegPool { get; }

	/// <summary>The FIX LegDatedDate, tag 739.</summary>
	FixField.Date? LegDatedDate { get; }

	/// <summary>The FIX LegContractSettlMonth, tag 955.</summary>
	FixField.MonthYear? LegContractSettlMonth { get; }

	/// <summary>The FIX LegInterestAccrualDate, tag 956.</summary>
	FixField.Date? LegInterestAccrualDate { get; }

	/// <summary>The FIX LegPutOrCall, tag 1358.</summary>
	FixField.Integer? LegPutOrCall { get; }

	/// <summary>The FIX LegOptionRatio, tag 1017.</summary>
	FixField.Decimal? LegOptionRatio { get; }

	/// <summary>The FIX LegPrice, tag 566.</summary>
	FixField.Decimal? LegPrice { get; }

	/// <summary>One entry of the group counted by NoLegSecurityAltID, tag 604.</summary>
	public sealed class NoLegSecurityAltIDGroup
	{
		/// <summary>The FIX LegSecurityAltID, tag 605, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text LegSecurityAltID { get; init; }

		/// <summary>The FIX LegSecurityAltIDSource, tag 606, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? LegSecurityAltIDSource { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 InstrumentParties repeating component, wherever it is carried.</summary>
public interface IInstrumentParties
{
	/// <summary>The FIX NoInstrumentParties, tag 1018.</summary>
	FixField.Integer? NoInstrumentParties { get; }

	/// <summary>The entries counted by NoInstrumentParties, tag 1018; null when the group is absent.</summary>
	List<IInstrumentParties.NoInstrumentPartiesGroup>? NoInstrumentPartiesGroups { get; }

	/// <summary>One entry of the group counted by NoInstrumentParties, tag 1018.</summary>
	public sealed class NoInstrumentPartiesGroup
	{
		/// <summary>The FIX InstrumentPartyID, tag 1019, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text InstrumentPartyID { get; init; }

		/// <summary>The FIX InstrumentPartyIDSource, tag 1050, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? InstrumentPartyIDSource { get; internal set; }

		/// <summary>The FIX InstrumentPartyRole, tag 1051, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? InstrumentPartyRole { get; internal set; }

		/// <summary>The FIX NoInstrumentPartySubIDs, tag 1052, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoInstrumentPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoInstrumentPartySubIDs, tag 1052; null when the group is absent.</summary>
		public List<IInstrumentParties.NoInstrumentPartiesGroup.NoInstrumentPartySubIDsGroup>? NoInstrumentPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoInstrumentPartySubIDs, tag 1052.</summary>
		public sealed class NoInstrumentPartySubIDsGroup
		{
			/// <summary>The FIX InstrumentPartySubID, tag 1053, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text InstrumentPartySubID { get; init; }

			/// <summary>The FIX InstrumentPartySubIDType, tag 1054, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? InstrumentPartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 LegBenchmarkCurveData block, wherever it is carried.</summary>
public interface ILegBenchmarkCurveData
{
	/// <summary>The FIX LegBenchmarkCurveCurrency, tag 676.</summary>
	FixField.Text? LegBenchmarkCurveCurrency { get; }

	/// <summary>The FIX LegBenchmarkCurveName, tag 677.</summary>
	FixField.Text? LegBenchmarkCurveName { get; }

	/// <summary>The FIX LegBenchmarkCurvePoint, tag 678.</summary>
	FixField.Text? LegBenchmarkCurvePoint { get; }

	/// <summary>The FIX LegBenchmarkPrice, tag 679.</summary>
	FixField.Decimal? LegBenchmarkPrice { get; }

	/// <summary>The FIX LegBenchmarkPriceType, tag 680.</summary>
	FixField.Integer? LegBenchmarkPriceType { get; }
}

/// <summary>The FIX 5.0 SP2 LegStipulations repeating component, wherever it is carried.</summary>
public interface ILegStipulations
{
	/// <summary>The FIX NoLegStipulations, tag 683.</summary>
	FixField.Integer? NoLegStipulations { get; }

	/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
	List<ILegStipulations.NoLegStipulationsGroup>? NoLegStipulationsGroups { get; }

	/// <summary>One entry of the group counted by NoLegStipulations, tag 683.</summary>
	public sealed class NoLegStipulationsGroup
	{
		/// <summary>The FIX LegStipulationType, tag 688, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text LegStipulationType { get; init; }

		/// <summary>The FIX LegStipulationValue, tag 689, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? LegStipulationValue { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 NestedParties repeating component, wherever it is carried.</summary>
public interface INestedParties
{
	/// <summary>The FIX NoNestedPartyIDs, tag 539.</summary>
	FixField.Integer? NoNestedPartyIDs { get; }

	/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
	List<INestedParties.NoNestedPartyIDsGroup>? NoNestedPartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoNestedPartyIDs, tag 539.</summary>
	public sealed class NoNestedPartyIDsGroup
	{
		/// <summary>The FIX NestedPartyID, tag 524, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text NestedPartyID { get; init; }

		/// <summary>The FIX NestedPartyIDSource, tag 525, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? NestedPartyIDSource { get; internal set; }

		/// <summary>The FIX NestedPartyRole, tag 538, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NestedPartyRole { get; internal set; }

		/// <summary>The FIX NoNestedPartySubIDs, tag 804, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNestedPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNestedPartySubIDs, tag 804; null when the group is absent.</summary>
		public List<INestedParties.NoNestedPartyIDsGroup.NoNestedPartySubIDsGroup>? NoNestedPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoNestedPartySubIDs, tag 804.</summary>
		public sealed class NoNestedPartySubIDsGroup
		{
			/// <summary>The FIX NestedPartySubID, tag 545, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text NestedPartySubID { get; init; }

			/// <summary>The FIX NestedPartySubIDType, tag 805, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NestedPartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 NestedParties2 repeating component, wherever it is carried.</summary>
public interface INestedParties2
{
	/// <summary>The FIX NoNested2PartyIDs, tag 756.</summary>
	FixField.Integer? NoNested2PartyIDs { get; }

	/// <summary>The entries counted by NoNested2PartyIDs, tag 756; null when the group is absent.</summary>
	List<INestedParties2.NoNested2PartyIDsGroup>? NoNested2PartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoNested2PartyIDs, tag 756.</summary>
	public sealed class NoNested2PartyIDsGroup
	{
		/// <summary>The FIX Nested2PartyID, tag 757, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text Nested2PartyID { get; init; }

		/// <summary>The FIX Nested2PartyIDSource, tag 758, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Nested2PartyIDSource { get; internal set; }

		/// <summary>The FIX Nested2PartyRole, tag 759, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? Nested2PartyRole { get; internal set; }

		/// <summary>The FIX NoNested2PartySubIDs, tag 806, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNested2PartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNested2PartySubIDs, tag 806; null when the group is absent.</summary>
		public List<INestedParties2.NoNested2PartyIDsGroup.NoNested2PartySubIDsGroup>? NoNested2PartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoNested2PartySubIDs, tag 806.</summary>
		public sealed class NoNested2PartySubIDsGroup
		{
			/// <summary>The FIX Nested2PartySubID, tag 760, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Nested2PartySubID { get; init; }

			/// <summary>The FIX Nested2PartySubIDType, tag 807, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? Nested2PartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 NestedParties3 repeating component, wherever it is carried.</summary>
public interface INestedParties3
{
	/// <summary>The FIX NoNested3PartyIDs, tag 948.</summary>
	FixField.Integer? NoNested3PartyIDs { get; }

	/// <summary>The entries counted by NoNested3PartyIDs, tag 948; null when the group is absent.</summary>
	List<INestedParties3.NoNested3PartyIDsGroup>? NoNested3PartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoNested3PartyIDs, tag 948.</summary>
	public sealed class NoNested3PartyIDsGroup
	{
		/// <summary>The FIX Nested3PartyID, tag 949, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text Nested3PartyID { get; init; }

		/// <summary>The FIX Nested3PartyIDSource, tag 950, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Nested3PartyIDSource { get; internal set; }

		/// <summary>The FIX Nested3PartyRole, tag 951, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? Nested3PartyRole { get; internal set; }

		/// <summary>The FIX NoNested3PartySubIDs, tag 952, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNested3PartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNested3PartySubIDs, tag 952; null when the group is absent.</summary>
		public List<INestedParties3.NoNested3PartyIDsGroup.NoNested3PartySubIDsGroup>? NoNested3PartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoNested3PartySubIDs, tag 952.</summary>
		public sealed class NoNested3PartySubIDsGroup
		{
			/// <summary>The FIX Nested3PartySubID, tag 953, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Nested3PartySubID { get; init; }

			/// <summary>The FIX Nested3PartySubIDType, tag 954, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? Nested3PartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 NestedParties4 repeating component, wherever it is carried.</summary>
public interface INestedParties4
{
	/// <summary>The FIX NoNested4PartyIDs, tag 1414.</summary>
	FixField.Integer? NoNested4PartyIDs { get; }

	/// <summary>The entries counted by NoNested4PartyIDs, tag 1414; null when the group is absent.</summary>
	List<INestedParties4.NoNested4PartyIDsGroup>? NoNested4PartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoNested4PartyIDs, tag 1414.</summary>
	public sealed class NoNested4PartyIDsGroup
	{
		/// <summary>The FIX Nested4PartyID, tag 1415, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text Nested4PartyID { get; init; }

		/// <summary>The FIX Nested4PartyIDSource, tag 1416, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Nested4PartyIDSource { get; internal set; }

		/// <summary>The FIX Nested4PartyRole, tag 1417, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? Nested4PartyRole { get; internal set; }

		/// <summary>The FIX NoNested4PartySubIDs, tag 1413, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNested4PartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNested4PartySubIDs, tag 1413; null when the group is absent.</summary>
		public List<INestedParties4.NoNested4PartyIDsGroup.NoNested4PartySubIDsGroup>? NoNested4PartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoNested4PartySubIDs, tag 1413.</summary>
		public sealed class NoNested4PartySubIDsGroup
		{
			/// <summary>The FIX Nested4PartySubID, tag 1412, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Nested4PartySubID { get; init; }

			/// <summary>The FIX Nested4PartySubIDType, tag 1411, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? Nested4PartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 OrderQtyData block, wherever it is carried.</summary>
public interface IOrderQtyData
{
	/// <summary>The FIX OrderQty, tag 38.</summary>
	FixField.Decimal? OrderQty { get; }

	/// <summary>The FIX CashOrderQty, tag 152.</summary>
	FixField.Decimal? CashOrderQty { get; }

	/// <summary>The FIX OrderPercent, tag 516.</summary>
	FixField.Decimal? OrderPercent { get; }

	/// <summary>The FIX RoundingDirection, tag 468.</summary>
	FixField.Character? RoundingDirection { get; }

	/// <summary>The FIX RoundingModulus, tag 469.</summary>
	FixField.Decimal? RoundingModulus { get; }
}

/// <summary>The FIX 5.0 SP2 Parties repeating component, wherever it is carried.</summary>
public interface IParties
{
	/// <summary>The FIX NoPartyIDs, tag 453.</summary>
	FixField.Integer? NoPartyIDs { get; }

	/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
	List<IParties.NoPartyIDsGroup>? NoPartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoPartyIDs, tag 453.</summary>
	public sealed class NoPartyIDsGroup
	{
		/// <summary>The FIX PartyID, tag 448, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text PartyID { get; init; }

		/// <summary>The FIX PartyIDSource, tag 447, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? PartyIDSource { get; internal set; }

		/// <summary>The FIX PartyRole, tag 452, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PartyRole { get; internal set; }

		/// <summary>The FIX NoPartySubIDs, tag 802, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoPartySubIDs, tag 802; null when the group is absent.</summary>
		public List<IParties.NoPartyIDsGroup.NoPartySubIDsGroup>? NoPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoPartySubIDs, tag 802.</summary>
		public sealed class NoPartySubIDsGroup
		{
			/// <summary>The FIX PartySubID, tag 523, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text PartySubID { get; init; }

			/// <summary>The FIX PartySubIDType, tag 803, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 PegInstructions block, wherever it is carried.</summary>
public interface IPegInstructions
{
	/// <summary>The FIX PegOffsetValue, tag 211.</summary>
	FixField.Decimal? PegOffsetValue { get; }

	/// <summary>The FIX PegPriceType, tag 1094.</summary>
	FixField.Integer? PegPriceType { get; }

	/// <summary>The FIX PegMoveType, tag 835.</summary>
	FixField.Integer? PegMoveType { get; }

	/// <summary>The FIX PegOffsetType, tag 836.</summary>
	FixField.Integer? PegOffsetType { get; }

	/// <summary>The FIX PegLimitType, tag 837.</summary>
	FixField.Integer? PegLimitType { get; }

	/// <summary>The FIX PegRoundDirection, tag 838.</summary>
	FixField.Integer? PegRoundDirection { get; }

	/// <summary>The FIX PegScope, tag 840.</summary>
	FixField.Integer? PegScope { get; }

	/// <summary>The FIX PegSecurityIDSource, tag 1096.</summary>
	FixField.Text? PegSecurityIDSource { get; }

	/// <summary>The FIX PegSecurityID, tag 1097.</summary>
	FixField.Text? PegSecurityID { get; }

	/// <summary>The FIX PegSymbol, tag 1098.</summary>
	FixField.Text? PegSymbol { get; }

	/// <summary>The FIX PegSecurityDesc, tag 1099.</summary>
	FixField.Text? PegSecurityDesc { get; }
}

/// <summary>The FIX 5.0 SP2 PositionAmountData repeating component, wherever it is carried.</summary>
public interface IPositionAmountData
{
	/// <summary>The FIX NoPosAmt, tag 753.</summary>
	FixField.Integer? NoPosAmt { get; }

	/// <summary>The entries counted by NoPosAmt, tag 753; null when the group is absent.</summary>
	List<IPositionAmountData.NoPosAmtGroup>? NoPosAmtGroups { get; }

	/// <summary>One entry of the group counted by NoPosAmt, tag 753.</summary>
	public sealed class NoPosAmtGroup
	{
		/// <summary>The FIX PosAmtType, tag 707, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text PosAmtType { get; init; }

		/// <summary>The FIX PosAmt, tag 708, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? PosAmt { get; internal set; }

		/// <summary>The FIX PositionCurrency, tag 1055, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? PositionCurrency { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 PositionQty repeating component, wherever it is carried.</summary>
public interface IPositionQty
{
	/// <summary>The FIX NoPositions, tag 702.</summary>
	FixField.Integer? NoPositions { get; }

	/// <summary>The entries counted by NoPositions, tag 702; null when the group is absent.</summary>
	List<IPositionQty.NoPositionsGroup>? NoPositionsGroups { get; }

	/// <summary>One entry of the group counted by NoPositions, tag 702.</summary>
	public sealed class NoPositionsGroup : INestedParties
	{
		/// <summary>The FIX PosType, tag 703, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text PosType { get; init; }

		/// <summary>The FIX LongQty, tag 704, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LongQty { get; internal set; }

		/// <summary>The FIX ShortQty, tag 705, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? ShortQty { get; internal set; }

		/// <summary>The FIX PosQtyStatus, tag 706, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PosQtyStatus { get; internal set; }

		/// <summary>The FIX QuantityDate, tag 976, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? QuantityDate { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNestedPartyIDs { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public List<INestedParties.NoNestedPartyIDsGroup>? NoNestedPartyIDsGroups { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 RateSource repeating component, wherever it is carried.</summary>
public interface IRateSource
{
	/// <summary>The FIX NoRateSources, tag 1445.</summary>
	FixField.Integer? NoRateSources { get; }

	/// <summary>The entries counted by NoRateSources, tag 1445; null when the group is absent.</summary>
	List<IRateSource.NoRateSourcesGroup>? NoRateSourcesGroups { get; }

	/// <summary>One entry of the group counted by NoRateSources, tag 1445.</summary>
	public sealed class NoRateSourcesGroup
	{
		/// <summary>The FIX RateSource, tag 1446, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer RateSource { get; init; }

		/// <summary>The FIX RateSourceType, tag 1447, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RateSourceType { get; internal set; }

		/// <summary>The FIX ReferencePage, tag 1448, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ReferencePage { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 RootParties repeating component, wherever it is carried.</summary>
public interface IRootParties
{
	/// <summary>The FIX NoRootPartyIDs, tag 1116.</summary>
	FixField.Integer? NoRootPartyIDs { get; }

	/// <summary>The entries counted by NoRootPartyIDs, tag 1116; null when the group is absent.</summary>
	List<IRootParties.NoRootPartyIDsGroup>? NoRootPartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoRootPartyIDs, tag 1116.</summary>
	public sealed class NoRootPartyIDsGroup
	{
		/// <summary>The FIX RootPartyID, tag 1117, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text RootPartyID { get; init; }

		/// <summary>The FIX RootPartyIDSource, tag 1118, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? RootPartyIDSource { get; internal set; }

		/// <summary>The FIX RootPartyRole, tag 1119, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RootPartyRole { get; internal set; }

		/// <summary>The FIX NoRootPartySubIDs, tag 1120, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRootPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoRootPartySubIDs, tag 1120; null when the group is absent.</summary>
		public List<IRootParties.NoRootPartyIDsGroup.NoRootPartySubIDsGroup>? NoRootPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoRootPartySubIDs, tag 1120.</summary>
		public sealed class NoRootPartySubIDsGroup
		{
			/// <summary>The FIX RootPartySubID, tag 1121, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text RootPartySubID { get; init; }

			/// <summary>The FIX RootPartySubIDType, tag 1122, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? RootPartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 SecurityTradingRules block, wherever it is carried.</summary>
public interface ISecurityTradingRules
{
	/// <summary>The FIX NoTickRules, tag 1205.</summary>
	FixField.Integer? NoTickRules { get; }

	/// <summary>The entries counted by NoTickRules, tag 1205; null when the group is absent.</summary>
	List<ISecurityTradingRules.NoTickRulesGroup>? NoTickRulesGroups { get; }

	/// <summary>The FIX NoLotTypeRules, tag 1234.</summary>
	FixField.Integer? NoLotTypeRules { get; }

	/// <summary>The entries counted by NoLotTypeRules, tag 1234; null when the group is absent.</summary>
	List<ISecurityTradingRules.NoLotTypeRulesGroup>? NoLotTypeRulesGroups { get; }

	/// <summary>The FIX PriceLimitType, tag 1306.</summary>
	FixField.Integer? PriceLimitType { get; }

	/// <summary>The FIX LowLimitPrice, tag 1148.</summary>
	FixField.Decimal? LowLimitPrice { get; }

	/// <summary>The FIX HighLimitPrice, tag 1149.</summary>
	FixField.Decimal? HighLimitPrice { get; }

	/// <summary>The FIX TradingReferencePrice, tag 1150.</summary>
	FixField.Decimal? TradingReferencePrice { get; }

	/// <summary>The FIX ExpirationCycle, tag 827.</summary>
	FixField.Integer? ExpirationCycle { get; }

	/// <summary>The FIX MinTradeVol, tag 562.</summary>
	FixField.Decimal? MinTradeVol { get; }

	/// <summary>The FIX MaxTradeVol, tag 1140.</summary>
	FixField.Decimal? MaxTradeVol { get; }

	/// <summary>The FIX MaxPriceVariation, tag 1143.</summary>
	FixField.Decimal? MaxPriceVariation { get; }

	/// <summary>The FIX ImpliedMarketIndicator, tag 1144.</summary>
	FixField.Integer? ImpliedMarketIndicator { get; }

	/// <summary>The FIX TradingCurrency, tag 1245.</summary>
	FixField.Text? TradingCurrency { get; }

	/// <summary>The FIX RoundLot, tag 561.</summary>
	FixField.Decimal? RoundLot { get; }

	/// <summary>The FIX MultilegModel, tag 1377.</summary>
	FixField.Integer? MultilegModel { get; }

	/// <summary>The FIX MultilegPriceMethod, tag 1378.</summary>
	FixField.Integer? MultilegPriceMethod { get; }

	/// <summary>The FIX PriceType, tag 423.</summary>
	FixField.Integer? PriceType { get; }

	/// <summary>The FIX NoTradingSessionRules, tag 1309.</summary>
	FixField.Integer? NoTradingSessionRules { get; }

	/// <summary>The entries counted by NoTradingSessionRules, tag 1309; null when the group is absent.</summary>
	List<ISecurityTradingRules.NoTradingSessionRulesGroup>? NoTradingSessionRulesGroups { get; }

	/// <summary>The FIX NoNestedInstrAttrib, tag 1312.</summary>
	FixField.Integer? NoNestedInstrAttrib { get; }

	/// <summary>The entries counted by NoNestedInstrAttrib, tag 1312; null when the group is absent.</summary>
	List<ISecurityTradingRules.NoNestedInstrAttribGroup>? NoNestedInstrAttribGroups { get; }

	/// <summary>One entry of the group counted by NoTickRules, tag 1205.</summary>
	public sealed class NoTickRulesGroup
	{
		/// <summary>The FIX StartTickPriceRange, tag 1206, wire type <c>Price</c>; null when the field is absent.</summary>
		public required FixField.Decimal StartTickPriceRange { get; init; }

		/// <summary>The FIX EndTickPriceRange, tag 1207, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? EndTickPriceRange { get; internal set; }

		/// <summary>The FIX TickIncrement, tag 1208, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? TickIncrement { get; internal set; }

		/// <summary>The FIX TickRuleType, tag 1209, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TickRuleType { get; internal set; }
	}

	/// <summary>One entry of the group counted by NoLotTypeRules, tag 1234.</summary>
	public sealed class NoLotTypeRulesGroup
	{
		/// <summary>The FIX LotType, tag 1093, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.Character LotType { get; init; }

		/// <summary>The FIX MinLotSize, tag 1231, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinLotSize { get; internal set; }
	}

	/// <summary>One entry of the group counted by NoTradingSessionRules, tag 1309.</summary>
	public sealed class NoTradingSessionRulesGroup
	{
		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text TradingSessionID { get; init; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionSubID { get; internal set; }

		/// <summary>The FIX NoOrdTypeRules, tag 1237, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrdTypeRules { get; internal set; }

		/// <summary>The entries counted by NoOrdTypeRules, tag 1237; null when the group is absent.</summary>
		public List<ISecurityTradingRules.NoTradingSessionRulesGroup.NoOrdTypeRulesGroup>? NoOrdTypeRulesGroups { get; internal set; }

		/// <summary>The FIX NoTimeInForceRules, tag 1239, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoTimeInForceRules { get; internal set; }

		/// <summary>The entries counted by NoTimeInForceRules, tag 1239; null when the group is absent.</summary>
		public List<ISecurityTradingRules.NoTradingSessionRulesGroup.NoTimeInForceRulesGroup>? NoTimeInForceRulesGroups { get; internal set; }

		/// <summary>The FIX NoExecInstRules, tag 1232, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoExecInstRules { get; internal set; }

		/// <summary>The entries counted by NoExecInstRules, tag 1232; null when the group is absent.</summary>
		public List<ISecurityTradingRules.NoTradingSessionRulesGroup.NoExecInstRulesGroup>? NoExecInstRulesGroups { get; internal set; }

		/// <summary>The FIX NoMatchRules, tag 1235, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMatchRules { get; internal set; }

		/// <summary>The entries counted by NoMatchRules, tag 1235; null when the group is absent.</summary>
		public List<ISecurityTradingRules.NoTradingSessionRulesGroup.NoMatchRulesGroup>? NoMatchRulesGroups { get; internal set; }

		/// <summary>The FIX NoMDFeedTypes, tag 1141, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDFeedTypes { get; internal set; }

		/// <summary>The entries counted by NoMDFeedTypes, tag 1141; null when the group is absent.</summary>
		public List<ISecurityTradingRules.NoTradingSessionRulesGroup.NoMDFeedTypesGroup>? NoMDFeedTypesGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoOrdTypeRules, tag 1237.</summary>
		public sealed class NoOrdTypeRulesGroup
		{
			/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character OrdType { get; init; }
		}

		/// <summary>One entry of the group counted by NoTimeInForceRules, tag 1239.</summary>
		public sealed class NoTimeInForceRulesGroup
		{
			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character TimeInForce { get; init; }
		}

		/// <summary>One entry of the group counted by NoExecInstRules, tag 1232.</summary>
		public sealed class NoExecInstRulesGroup
		{
			/// <summary>The FIX ExecInstValue, tag 1308, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character ExecInstValue { get; init; }
		}

		/// <summary>One entry of the group counted by NoMatchRules, tag 1235.</summary>
		public sealed class NoMatchRulesGroup
		{
			/// <summary>The FIX MatchAlgorithm, tag 1142, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text MatchAlgorithm { get; init; }

			/// <summary>The FIX MatchType, tag 574, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MatchType { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoMDFeedTypes, tag 1141.</summary>
		public sealed class NoMDFeedTypesGroup
		{
			/// <summary>The FIX MDFeedType, tag 1022, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text MDFeedType { get; init; }

			/// <summary>The FIX MarketDepth, tag 264, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MarketDepth { get; internal set; }

			/// <summary>The FIX MDBookType, tag 1021, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MDBookType { get; internal set; }
		}
	}

	/// <summary>One entry of the group counted by NoNestedInstrAttrib, tag 1312.</summary>
	public sealed class NoNestedInstrAttribGroup
	{
		/// <summary>The FIX NestedInstrAttribType, tag 1210, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.Integer NestedInstrAttribType { get; init; }

		/// <summary>The FIX NestedInstrAttribValue, tag 1211, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? NestedInstrAttribValue { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 SettlInstructionsData block, wherever it is carried.</summary>
public interface ISettlInstructionsData
{
	/// <summary>The FIX SettlDeliveryType, tag 172.</summary>
	FixField.Integer? SettlDeliveryType { get; }

	/// <summary>The FIX StandInstDbType, tag 169.</summary>
	FixField.Integer? StandInstDbType { get; }

	/// <summary>The FIX StandInstDbName, tag 170.</summary>
	FixField.Text? StandInstDbName { get; }

	/// <summary>The FIX StandInstDbID, tag 171.</summary>
	FixField.Text? StandInstDbID { get; }

	/// <summary>The FIX NoDlvyInst, tag 85.</summary>
	FixField.Integer? NoDlvyInst { get; }

	/// <summary>The entries counted by NoDlvyInst, tag 85; null when the group is absent.</summary>
	List<ISettlInstructionsData.NoDlvyInstGroup>? NoDlvyInstGroups { get; }

	/// <summary>One entry of the group counted by NoDlvyInst, tag 85.</summary>
	public sealed class NoDlvyInstGroup : ISettlParties
	{
		/// <summary>The FIX SettlInstSource, tag 165, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.Character SettlInstSource { get; init; }

		/// <summary>The FIX DlvyInstType, tag 787, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DlvyInstType { get; internal set; }

		/// <summary>The FIX NoSettlPartyIDs, tag 781, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoSettlPartyIDs { get; internal set; }

		/// <summary>The entries counted by NoSettlPartyIDs, tag 781; null when the group is absent.</summary>
		public List<ISettlParties.NoSettlPartyIDsGroup>? NoSettlPartyIDsGroups { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 SettlParties repeating component, wherever it is carried.</summary>
public interface ISettlParties
{
	/// <summary>The FIX NoSettlPartyIDs, tag 781.</summary>
	FixField.Integer? NoSettlPartyIDs { get; }

	/// <summary>The entries counted by NoSettlPartyIDs, tag 781; null when the group is absent.</summary>
	List<ISettlParties.NoSettlPartyIDsGroup>? NoSettlPartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoSettlPartyIDs, tag 781.</summary>
	public sealed class NoSettlPartyIDsGroup
	{
		/// <summary>The FIX SettlPartyID, tag 782, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text SettlPartyID { get; init; }

		/// <summary>The FIX SettlPartyIDSource, tag 783, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlPartyIDSource { get; internal set; }

		/// <summary>The FIX SettlPartyRole, tag 784, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SettlPartyRole { get; internal set; }

		/// <summary>The FIX NoSettlPartySubIDs, tag 801, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoSettlPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoSettlPartySubIDs, tag 801; null when the group is absent.</summary>
		public List<ISettlParties.NoSettlPartyIDsGroup.NoSettlPartySubIDsGroup>? NoSettlPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoSettlPartySubIDs, tag 801.</summary>
		public sealed class NoSettlPartySubIDsGroup
		{
			/// <summary>The FIX SettlPartySubID, tag 785, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text SettlPartySubID { get; init; }

			/// <summary>The FIX SettlPartySubIDType, tag 786, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SettlPartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 SideTrdRegTS repeating component, wherever it is carried.</summary>
public interface ISideTrdRegTS
{
	/// <summary>The FIX NoSideTrdRegTS, tag 1016.</summary>
	FixField.Integer? NoSideTrdRegTS { get; }

	/// <summary>The entries counted by NoSideTrdRegTS, tag 1016; null when the group is absent.</summary>
	List<ISideTrdRegTS.NoSideTrdRegTSGroup>? NoSideTrdRegTSGroups { get; }

	/// <summary>One entry of the group counted by NoSideTrdRegTS, tag 1016.</summary>
	public sealed class NoSideTrdRegTSGroup
	{
		/// <summary>The FIX SideTrdRegTimestamp, tag 1012, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public required FixField.Timestamp SideTrdRegTimestamp { get; init; }

		/// <summary>The FIX SideTrdRegTimestampType, tag 1013, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SideTrdRegTimestampType { get; internal set; }

		/// <summary>The FIX SideTrdRegTimestampSrc, tag 1014, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SideTrdRegTimestampSrc { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 SpreadOrBenchmarkCurveData block, wherever it is carried.</summary>
public interface ISpreadOrBenchmarkCurveData
{
	/// <summary>The FIX Spread, tag 218.</summary>
	FixField.Decimal? Spread { get; }

	/// <summary>The FIX BenchmarkCurveCurrency, tag 220.</summary>
	FixField.Text? BenchmarkCurveCurrency { get; }

	/// <summary>The FIX BenchmarkCurveName, tag 221.</summary>
	FixField.Text? BenchmarkCurveName { get; }

	/// <summary>The FIX BenchmarkCurvePoint, tag 222.</summary>
	FixField.Text? BenchmarkCurvePoint { get; }

	/// <summary>The FIX BenchmarkPrice, tag 662.</summary>
	FixField.Decimal? BenchmarkPrice { get; }

	/// <summary>The FIX BenchmarkPriceType, tag 663.</summary>
	FixField.Integer? BenchmarkPriceType { get; }

	/// <summary>The FIX BenchmarkSecurityID, tag 699.</summary>
	FixField.Text? BenchmarkSecurityID { get; }

	/// <summary>The FIX BenchmarkSecurityIDSource, tag 761.</summary>
	FixField.Text? BenchmarkSecurityIDSource { get; }
}

/// <summary>The FIX 5.0 SP2 Stipulations repeating component, wherever it is carried.</summary>
public interface IStipulations
{
	/// <summary>The FIX NoStipulations, tag 232.</summary>
	FixField.Integer? NoStipulations { get; }

	/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
	List<IStipulations.NoStipulationsGroup>? NoStipulationsGroups { get; }

	/// <summary>One entry of the group counted by NoStipulations, tag 232.</summary>
	public sealed class NoStipulationsGroup
	{
		/// <summary>The FIX StipulationType, tag 233, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text StipulationType { get; init; }

		/// <summary>The FIX StipulationValue, tag 234, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? StipulationValue { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 TargetParties repeating component, wherever it is carried.</summary>
public interface ITargetParties
{
	/// <summary>The FIX NoTargetPartyIDs, tag 1461.</summary>
	FixField.Integer? NoTargetPartyIDs { get; }

	/// <summary>The entries counted by NoTargetPartyIDs, tag 1461; null when the group is absent.</summary>
	List<ITargetParties.NoTargetPartyIDsGroup>? NoTargetPartyIDsGroups { get; }

	/// <summary>One entry of the group counted by NoTargetPartyIDs, tag 1461.</summary>
	public sealed class NoTargetPartyIDsGroup
	{
		/// <summary>The FIX TargetPartyID, tag 1462, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text TargetPartyID { get; init; }

		/// <summary>The FIX TargetPartyIDSource, tag 1463, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TargetPartyIDSource { get; internal set; }

		/// <summary>The FIX TargetPartyRole, tag 1464, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TargetPartyRole { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 TrdRegTimestamps repeating component, wherever it is carried.</summary>
public interface ITrdRegTimestamps
{
	/// <summary>The FIX NoTrdRegTimestamps, tag 768.</summary>
	FixField.Integer? NoTrdRegTimestamps { get; }

	/// <summary>The entries counted by NoTrdRegTimestamps, tag 768; null when the group is absent.</summary>
	List<ITrdRegTimestamps.NoTrdRegTimestampsGroup>? NoTrdRegTimestampsGroups { get; }

	/// <summary>One entry of the group counted by NoTrdRegTimestamps, tag 768.</summary>
	public sealed class NoTrdRegTimestampsGroup
	{
		/// <summary>The FIX TrdRegTimestamp, tag 769, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public required FixField.Timestamp TrdRegTimestamp { get; init; }

		/// <summary>The FIX TrdRegTimestampType, tag 770, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TrdRegTimestampType { get; internal set; }

		/// <summary>The FIX TrdRegTimestampOrigin, tag 771, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TrdRegTimestampOrigin { get; internal set; }

		/// <summary>The FIX DeskType, tag 1033, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? DeskType { get; internal set; }

		/// <summary>The FIX DeskTypeSource, tag 1034, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? DeskTypeSource { get; internal set; }

		/// <summary>The FIX DeskOrderHandlingInst, tag 1035, wire type <c>MultipleStringValue</c>; null when the field is absent.</summary>
		public FixField.Multiple? DeskOrderHandlingInst { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 TriggeringInstruction block, wherever it is carried.</summary>
public interface ITriggeringInstruction
{
	/// <summary>The FIX TriggerType, tag 1100.</summary>
	FixField.Character? TriggerType { get; }

	/// <summary>The FIX TriggerAction, tag 1101.</summary>
	FixField.Character? TriggerAction { get; }

	/// <summary>The FIX TriggerPrice, tag 1102.</summary>
	FixField.Decimal? TriggerPrice { get; }

	/// <summary>The FIX TriggerSymbol, tag 1103.</summary>
	FixField.Text? TriggerSymbol { get; }

	/// <summary>The FIX TriggerSecurityID, tag 1104.</summary>
	FixField.Text? TriggerSecurityID { get; }

	/// <summary>The FIX TriggerSecurityIDSource, tag 1105.</summary>
	FixField.Text? TriggerSecurityIDSource { get; }

	/// <summary>The FIX TriggerSecurityDesc, tag 1106.</summary>
	FixField.Text? TriggerSecurityDesc { get; }

	/// <summary>The FIX TriggerPriceType, tag 1107.</summary>
	FixField.Character? TriggerPriceType { get; }

	/// <summary>The FIX TriggerPriceTypeScope, tag 1108.</summary>
	FixField.Character? TriggerPriceTypeScope { get; }

	/// <summary>The FIX TriggerPriceDirection, tag 1109.</summary>
	FixField.Character? TriggerPriceDirection { get; }

	/// <summary>The FIX TriggerNewPrice, tag 1110.</summary>
	FixField.Decimal? TriggerNewPrice { get; }

	/// <summary>The FIX TriggerOrderType, tag 1111.</summary>
	FixField.Character? TriggerOrderType { get; }

	/// <summary>The FIX TriggerNewQty, tag 1112.</summary>
	FixField.Decimal? TriggerNewQty { get; }

	/// <summary>The FIX TriggerTradingSessionID, tag 1113.</summary>
	FixField.Text? TriggerTradingSessionID { get; }

	/// <summary>The FIX TriggerTradingSessionSubID, tag 1114.</summary>
	FixField.Text? TriggerTradingSessionSubID { get; }
}

/// <summary>The FIX 5.0 SP2 UnderlyingAmount repeating component, wherever it is carried.</summary>
public interface IUnderlyingAmount
{
	/// <summary>The FIX NoUnderlyingAmounts, tag 984.</summary>
	FixField.Integer? NoUnderlyingAmounts { get; }

	/// <summary>The entries counted by NoUnderlyingAmounts, tag 984; null when the group is absent.</summary>
	List<IUnderlyingAmount.NoUnderlyingAmountsGroup>? NoUnderlyingAmountsGroups { get; }

	/// <summary>One entry of the group counted by NoUnderlyingAmounts, tag 984.</summary>
	public sealed class NoUnderlyingAmountsGroup
	{
		/// <summary>The FIX UnderlyingPayAmount, tag 985, wire type <c>Amt</c>; null when the field is absent.</summary>
		public required FixField.Decimal UnderlyingPayAmount { get; init; }

		/// <summary>The FIX UnderlyingCollectAmount, tag 986, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? UnderlyingCollectAmount { get; internal set; }

		/// <summary>The FIX UnderlyingSettlementDate, tag 987, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? UnderlyingSettlementDate { get; internal set; }

		/// <summary>The FIX UnderlyingSettlementStatus, tag 988, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? UnderlyingSettlementStatus { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 UnderlyingInstrument block, wherever it is carried.</summary>
public interface IUnderlyingInstrument : IUnderlyingStipulations, IUndlyInstrumentParties
{
	/// <summary>The FIX UnderlyingSymbol, tag 311.</summary>
	FixField.Text? UnderlyingSymbol { get; }

	/// <summary>The FIX UnderlyingSymbolSfx, tag 312.</summary>
	FixField.Text? UnderlyingSymbolSfx { get; }

	/// <summary>The FIX UnderlyingSecurityID, tag 309.</summary>
	FixField.Text? UnderlyingSecurityID { get; }

	/// <summary>The FIX UnderlyingSecurityIDSource, tag 305.</summary>
	FixField.Text? UnderlyingSecurityIDSource { get; }

	/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457.</summary>
	FixField.Integer? NoUnderlyingSecurityAltID { get; }

	/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
	List<IUnderlyingInstrument.NoUnderlyingSecurityAltIDGroup>? NoUnderlyingSecurityAltIDGroups { get; }

	/// <summary>The FIX UnderlyingProduct, tag 462.</summary>
	FixField.Integer? UnderlyingProduct { get; }

	/// <summary>The FIX UnderlyingCFICode, tag 463.</summary>
	FixField.Text? UnderlyingCFICode { get; }

	/// <summary>The FIX UnderlyingSecurityType, tag 310.</summary>
	FixField.Text? UnderlyingSecurityType { get; }

	/// <summary>The FIX UnderlyingSecuritySubType, tag 763.</summary>
	FixField.Text? UnderlyingSecuritySubType { get; }

	/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313.</summary>
	FixField.MonthYear? UnderlyingMaturityMonthYear { get; }

	/// <summary>The FIX UnderlyingMaturityDate, tag 542.</summary>
	FixField.Date? UnderlyingMaturityDate { get; }

	/// <summary>The FIX UnderlyingMaturityTime, tag 1213.</summary>
	FixField.ZonedTime? UnderlyingMaturityTime { get; }

	/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241.</summary>
	FixField.Date? UnderlyingCouponPaymentDate { get; }

	/// <summary>The FIX UnderlyingRestructuringType, tag 1453.</summary>
	FixField.Text? UnderlyingRestructuringType { get; }

	/// <summary>The FIX UnderlyingSeniority, tag 1454.</summary>
	FixField.Text? UnderlyingSeniority { get; }

	/// <summary>The FIX UnderlyingNotionalPercentageOutstanding, tag 1455.</summary>
	FixField.Decimal? UnderlyingNotionalPercentageOutstanding { get; }

	/// <summary>The FIX UnderlyingOriginalNotionalPercentageOutstanding, tag 1456.</summary>
	FixField.Decimal? UnderlyingOriginalNotionalPercentageOutstanding { get; }

	/// <summary>The FIX UnderlyingAttachmentPoint, tag 1459.</summary>
	FixField.Decimal? UnderlyingAttachmentPoint { get; }

	/// <summary>The FIX UnderlyingDetachmentPoint, tag 1460.</summary>
	FixField.Decimal? UnderlyingDetachmentPoint { get; }

	/// <summary>The FIX UnderlyingIssueDate, tag 242.</summary>
	FixField.Date? UnderlyingIssueDate { get; }

	/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243.</summary>
	FixField.Text? UnderlyingRepoCollateralSecurityType { get; }

	/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244.</summary>
	FixField.Integer? UnderlyingRepurchaseTerm { get; }

	/// <summary>The FIX UnderlyingRepurchaseRate, tag 245.</summary>
	FixField.Decimal? UnderlyingRepurchaseRate { get; }

	/// <summary>The FIX UnderlyingFactor, tag 246.</summary>
	FixField.Decimal? UnderlyingFactor { get; }

	/// <summary>The FIX UnderlyingCreditRating, tag 256.</summary>
	FixField.Text? UnderlyingCreditRating { get; }

	/// <summary>The FIX UnderlyingInstrRegistry, tag 595.</summary>
	FixField.Text? UnderlyingInstrRegistry { get; }

	/// <summary>The FIX UnderlyingCountryOfIssue, tag 592.</summary>
	FixField.Text? UnderlyingCountryOfIssue { get; }

	/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593.</summary>
	FixField.Text? UnderlyingStateOrProvinceOfIssue { get; }

	/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594.</summary>
	FixField.Text? UnderlyingLocaleOfIssue { get; }

	/// <summary>The FIX UnderlyingRedemptionDate, tag 247.</summary>
	FixField.Date? UnderlyingRedemptionDate { get; }

	/// <summary>The FIX UnderlyingStrikePrice, tag 316.</summary>
	FixField.Decimal? UnderlyingStrikePrice { get; }

	/// <summary>The FIX UnderlyingStrikeCurrency, tag 941.</summary>
	FixField.Text? UnderlyingStrikeCurrency { get; }

	/// <summary>The FIX UnderlyingOptAttribute, tag 317.</summary>
	FixField.Character? UnderlyingOptAttribute { get; }

	/// <summary>The FIX UnderlyingContractMultiplier, tag 436.</summary>
	FixField.Decimal? UnderlyingContractMultiplier { get; }

	/// <summary>The FIX UnderlyingContractMultiplierUnit, tag 1437.</summary>
	FixField.Integer? UnderlyingContractMultiplierUnit { get; }

	/// <summary>The FIX UnderlyingFlowScheduleType, tag 1441.</summary>
	FixField.Integer? UnderlyingFlowScheduleType { get; }

	/// <summary>The FIX UnderlyingUnitOfMeasure, tag 998.</summary>
	FixField.Text? UnderlyingUnitOfMeasure { get; }

	/// <summary>The FIX UnderlyingUnitOfMeasureQty, tag 1423.</summary>
	FixField.Decimal? UnderlyingUnitOfMeasureQty { get; }

	/// <summary>The FIX UnderlyingPriceUnitOfMeasure, tag 1424.</summary>
	FixField.Text? UnderlyingPriceUnitOfMeasure { get; }

	/// <summary>The FIX UnderlyingPriceUnitOfMeasureQty, tag 1425.</summary>
	FixField.Decimal? UnderlyingPriceUnitOfMeasureQty { get; }

	/// <summary>The FIX UnderlyingTimeUnit, tag 1000.</summary>
	FixField.Text? UnderlyingTimeUnit { get; }

	/// <summary>The FIX UnderlyingExerciseStyle, tag 1419.</summary>
	FixField.Integer? UnderlyingExerciseStyle { get; }

	/// <summary>The FIX UnderlyingCouponRate, tag 435.</summary>
	FixField.Decimal? UnderlyingCouponRate { get; }

	/// <summary>The FIX UnderlyingSecurityExchange, tag 308.</summary>
	FixField.Text? UnderlyingSecurityExchange { get; }

	/// <summary>The FIX UnderlyingIssuer, tag 306.</summary>
	FixField.Text? UnderlyingIssuer { get; }

	/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362.</summary>
	FixField.Integer? EncodedUnderlyingIssuerLen { get; }

	/// <summary>The FIX EncodedUnderlyingIssuer, tag 363.</summary>
	FixField.Data? EncodedUnderlyingIssuer { get; }

	/// <summary>The FIX UnderlyingSecurityDesc, tag 307.</summary>
	FixField.Text? UnderlyingSecurityDesc { get; }

	/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364.</summary>
	FixField.Integer? EncodedUnderlyingSecurityDescLen { get; }

	/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365.</summary>
	FixField.Data? EncodedUnderlyingSecurityDesc { get; }

	/// <summary>The FIX UnderlyingCPProgram, tag 877.</summary>
	FixField.Text? UnderlyingCPProgram { get; }

	/// <summary>The FIX UnderlyingCPRegType, tag 878.</summary>
	FixField.Text? UnderlyingCPRegType { get; }

	/// <summary>The FIX UnderlyingAllocationPercent, tag 972.</summary>
	FixField.Decimal? UnderlyingAllocationPercent { get; }

	/// <summary>The FIX UnderlyingCurrency, tag 318.</summary>
	FixField.Text? UnderlyingCurrency { get; }

	/// <summary>The FIX UnderlyingQty, tag 879.</summary>
	FixField.Decimal? UnderlyingQty { get; }

	/// <summary>The FIX UnderlyingSettlementType, tag 975.</summary>
	FixField.Integer? UnderlyingSettlementType { get; }

	/// <summary>The FIX UnderlyingCashAmount, tag 973.</summary>
	FixField.Decimal? UnderlyingCashAmount { get; }

	/// <summary>The FIX UnderlyingCashType, tag 974.</summary>
	FixField.Text? UnderlyingCashType { get; }

	/// <summary>The FIX UnderlyingPx, tag 810.</summary>
	FixField.Decimal? UnderlyingPx { get; }

	/// <summary>The FIX UnderlyingDirtyPrice, tag 882.</summary>
	FixField.Decimal? UnderlyingDirtyPrice { get; }

	/// <summary>The FIX UnderlyingEndPrice, tag 883.</summary>
	FixField.Decimal? UnderlyingEndPrice { get; }

	/// <summary>The FIX UnderlyingStartValue, tag 884.</summary>
	FixField.Decimal? UnderlyingStartValue { get; }

	/// <summary>The FIX UnderlyingCurrentValue, tag 885.</summary>
	FixField.Decimal? UnderlyingCurrentValue { get; }

	/// <summary>The FIX UnderlyingEndValue, tag 886.</summary>
	FixField.Decimal? UnderlyingEndValue { get; }

	/// <summary>The FIX UnderlyingAdjustedQuantity, tag 1044.</summary>
	FixField.Decimal? UnderlyingAdjustedQuantity { get; }

	/// <summary>The FIX UnderlyingFXRate, tag 1045.</summary>
	FixField.Decimal? UnderlyingFXRate { get; }

	/// <summary>The FIX UnderlyingFXRateCalc, tag 1046.</summary>
	FixField.Character? UnderlyingFXRateCalc { get; }

	/// <summary>The FIX UnderlyingCapValue, tag 1038.</summary>
	FixField.Decimal? UnderlyingCapValue { get; }

	/// <summary>The FIX UnderlyingSettlMethod, tag 1039.</summary>
	FixField.Text? UnderlyingSettlMethod { get; }

	/// <summary>The FIX UnderlyingPutOrCall, tag 315.</summary>
	FixField.Integer? UnderlyingPutOrCall { get; }

	/// <summary>One entry of the group counted by NoUnderlyingSecurityAltID, tag 457.</summary>
	public sealed class NoUnderlyingSecurityAltIDGroup
	{
		/// <summary>The FIX UnderlyingSecurityAltID, tag 458, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text UnderlyingSecurityAltID { get; init; }

		/// <summary>The FIX UnderlyingSecurityAltIDSource, tag 459, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? UnderlyingSecurityAltIDSource { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 UnderlyingStipulations repeating component, wherever it is carried.</summary>
public interface IUnderlyingStipulations
{
	/// <summary>The FIX NoUnderlyingStips, tag 887.</summary>
	FixField.Integer? NoUnderlyingStips { get; }

	/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
	List<IUnderlyingStipulations.NoUnderlyingStipsGroup>? NoUnderlyingStipsGroups { get; }

	/// <summary>One entry of the group counted by NoUnderlyingStips, tag 887.</summary>
	public sealed class NoUnderlyingStipsGroup
	{
		/// <summary>The FIX UnderlyingStipType, tag 888, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text UnderlyingStipType { get; init; }

		/// <summary>The FIX UnderlyingStipValue, tag 889, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? UnderlyingStipValue { get; internal set; }
	}
}

/// <summary>The FIX 5.0 SP2 UndlyInstrumentParties repeating component, wherever it is carried.</summary>
public interface IUndlyInstrumentParties
{
	/// <summary>The FIX NoUndlyInstrumentParties, tag 1058.</summary>
	FixField.Integer? NoUndlyInstrumentParties { get; }

	/// <summary>The entries counted by NoUndlyInstrumentParties, tag 1058; null when the group is absent.</summary>
	List<IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup>? NoUndlyInstrumentPartiesGroups { get; }

	/// <summary>One entry of the group counted by NoUndlyInstrumentParties, tag 1058.</summary>
	public sealed class NoUndlyInstrumentPartiesGroup
	{
		/// <summary>The FIX UnderlyingInstrumentPartyID, tag 1059, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text UnderlyingInstrumentPartyID { get; init; }

		/// <summary>The FIX UnderlyingInstrumentPartyIDSource, tag 1060, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? UnderlyingInstrumentPartyIDSource { get; internal set; }

		/// <summary>The FIX UnderlyingInstrumentPartyRole, tag 1061, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? UnderlyingInstrumentPartyRole { get; internal set; }

		/// <summary>The FIX NoUndlyInstrumentPartySubIDs, tag 1062, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoUndlyInstrumentPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoUndlyInstrumentPartySubIDs, tag 1062; null when the group is absent.</summary>
		public List<IUndlyInstrumentParties.NoUndlyInstrumentPartiesGroup.NoUndlyInstrumentPartySubIDsGroup>? NoUndlyInstrumentPartySubIDsGroups { get; internal set; }

		/// <summary>One entry of the group counted by NoUndlyInstrumentPartySubIDs, tag 1062.</summary>
		public sealed class NoUndlyInstrumentPartySubIDsGroup
		{
			/// <summary>The FIX UnderlyingInstrumentPartySubID, tag 1063, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text UnderlyingInstrumentPartySubID { get; init; }

			/// <summary>The FIX UnderlyingInstrumentPartySubIDType, tag 1064, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingInstrumentPartySubIDType { get; internal set; }
		}
	}
}

/// <summary>The FIX 5.0 SP2 YieldData block, wherever it is carried.</summary>
public interface IYieldData
{
	/// <summary>The FIX YieldType, tag 235.</summary>
	FixField.Text? YieldType { get; }

	/// <summary>The FIX Yield, tag 236.</summary>
	FixField.Decimal? Yield { get; }

	/// <summary>The FIX YieldCalcDate, tag 701.</summary>
	FixField.Date? YieldCalcDate { get; }

	/// <summary>The FIX YieldRedemptionDate, tag 696.</summary>
	FixField.Date? YieldRedemptionDate { get; }

	/// <summary>The FIX YieldRedemptionPrice, tag 697.</summary>
	FixField.Decimal? YieldRedemptionPrice { get; }

	/// <summary>The FIX YieldRedemptionPriceType, tag 698.</summary>
	FixField.Integer? YieldRedemptionPriceType { get; }
}
