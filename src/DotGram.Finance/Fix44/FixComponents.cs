using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace DotGram.Finance.Fix44;

// Written by generate.py from the FIX 4.4 repository; not edited by hand.
//
// The components FIX 4.4 reuses across message types, as the shape a message or a group entry has
// when it carries one. A component is written into its carrier field by field, because that is
// what it is on the wire; the interface is those same fields read as the one thing they are, so
// that what is asked of a component is written once and asked of every carrier. A group inside a
// component is the same group wherever the component is carried, so its entry is a class nested
// in the interface: IInstrument.NoSecurityAltIDGroup.

/// <summary>The FIX 4.4 CommissionData block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 DiscretionInstructions block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 FinancingDetails block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 Instrument block, wherever it is carried.</summary>
public interface IInstrument
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

	/// <summary>The FIX PutOrCall, tag 201.</summary>
	FixField.Integer? PutOrCall { get; }

	/// <summary>The FIX CouponPaymentDate, tag 224.</summary>
	FixField.Date? CouponPaymentDate { get; }

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

	/// <summary>The FIX OptAttribute, tag 206.</summary>
	FixField.Character? OptAttribute { get; }

	/// <summary>The FIX ContractMultiplier, tag 231.</summary>
	FixField.Decimal? ContractMultiplier { get; }

	/// <summary>The FIX CouponRate, tag 223.</summary>
	FixField.Decimal? CouponRate { get; }

	/// <summary>The FIX SecurityExchange, tag 207.</summary>
	FixField.Text? SecurityExchange { get; }

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

		/// <summary>The FIX EventPx, tag 867, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? EventPx { get; internal set; }

		/// <summary>The FIX EventText, tag 868, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? EventText { get; internal set; }
	}
}

/// <summary>The FIX 4.4 InstrumentExtension block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 InstrumentLeg block, wherever it is carried.</summary>
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

	/// <summary>One entry of the group counted by NoLegSecurityAltID, tag 604.</summary>
	public sealed class NoLegSecurityAltIDGroup
	{
		/// <summary>The FIX LegSecurityAltID, tag 605, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text LegSecurityAltID { get; init; }

		/// <summary>The FIX LegSecurityAltIDSource, tag 606, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? LegSecurityAltIDSource { get; internal set; }
	}
}

/// <summary>The FIX 4.4 LegBenchmarkCurveData block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 LegStipulations repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 NestedParties repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 NestedParties2 repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 NestedParties3 repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 OrderQtyData block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 Parties repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 PegInstructions block, wherever it is carried.</summary>
public interface IPegInstructions
{
	/// <summary>The FIX PegOffsetValue, tag 211.</summary>
	FixField.Decimal? PegOffsetValue { get; }

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
}

/// <summary>The FIX 4.4 PositionAmountData repeating component, wherever it is carried.</summary>
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
	}
}

/// <summary>The FIX 4.4 PositionQty repeating component, wherever it is carried.</summary>
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

		/// <summary>The FIX NoNestedPartyIDs, tag 539, wire type <c>NumInGroup</c>; null when the field is absent.</summary>
		public FixField.Integer? NoNestedPartyIDs { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public List<INestedParties.NoNestedPartyIDsGroup>? NoNestedPartyIDsGroups { get; internal set; }
	}
}

/// <summary>The FIX 4.4 SettlInstructionsData block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 SettlParties repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 SpreadOrBenchmarkCurveData block, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 Stipulations repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 TrdRegTimestamps repeating component, wherever it is carried.</summary>
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
	}
}

/// <summary>The FIX 4.4 UnderlyingInstrument block, wherever it is carried.</summary>
public interface IUnderlyingInstrument : IUnderlyingStipulations
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

	/// <summary>The FIX UnderlyingPutOrCall, tag 315.</summary>
	FixField.Integer? UnderlyingPutOrCall { get; }

	/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241.</summary>
	FixField.Date? UnderlyingCouponPaymentDate { get; }

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

	/// <summary>The FIX UnderlyingCurrency, tag 318.</summary>
	FixField.Text? UnderlyingCurrency { get; }

	/// <summary>The FIX UnderlyingQty, tag 879.</summary>
	FixField.Decimal? UnderlyingQty { get; }

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

	/// <summary>One entry of the group counted by NoUnderlyingSecurityAltID, tag 457.</summary>
	public sealed class NoUnderlyingSecurityAltIDGroup
	{
		/// <summary>The FIX UnderlyingSecurityAltID, tag 458, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text UnderlyingSecurityAltID { get; init; }

		/// <summary>The FIX UnderlyingSecurityAltIDSource, tag 459, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? UnderlyingSecurityAltIDSource { get; internal set; }
	}
}

/// <summary>The FIX 4.4 UnderlyingStipulations repeating component, wherever it is carried.</summary>
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

/// <summary>The FIX 4.4 YieldData block, wherever it is carried.</summary>
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
