using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// The repeating groups FIX 4.4 describes, each a class whose instances are one entry.
/// </summary>
/// <remarks>
/// One class per repeating component of FIX 4.4, held in a container the messages reach through
/// `using static`: a message names its entries with a property of the same name, which a nested
/// type of its own base class could not survive, and a container keeps ninety-two names out of the
/// namespace.
/// </remarks>
public static class FixGroup
{
	/// <summary>One entry of the FIX 4.4 AffectedOrdGrp, counted by NoAffectedOrders, tag 534.</summary>
	public sealed class AffectedOrdGrp
	{
		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.OrigClOrdID               OrigClOrdID              { get; init; }

		/// <summary>The FIX AffectedOrderID, tag 535, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AffectedOrderID?          AffectedOrderID          { get; internal set; }

		/// <summary>The FIX AffectedSecondaryOrderID, tag 536, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AffectedSecondaryOrderID? AffectedSecondaryOrderID { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 AllocAckGrp, counted by NoAllocs, tag 78.</summary>
	public sealed class AllocAckGrp
	{
		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AllocAccount            AllocAccount           { get; init; }

		/// <summary>The FIX AllocAcctIDSource, tag 661, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocAcctIDSource?      AllocAcctIDSource      { get; internal set; }

		/// <summary>The FIX AllocPrice, tag 366, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.AllocPrice?             AllocPrice             { get; internal set; }

		/// <summary>The FIX IndividualAllocID, tag 467, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocID?      IndividualAllocID      { get; internal set; }

		/// <summary>The FIX IndividualAllocRejCode, tag 776, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocRejCode? IndividualAllocRejCode { get; internal set; }

		/// <summary>The FIX AllocText, tag 161, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AllocText?              AllocText              { get; internal set; }

		/// <summary>The FIX EncodedAllocTextLen, tag 360, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedAllocTextLen?    EncodedAllocTextLen    { get; internal set; }

		/// <summary>The FIX EncodedAllocText, tag 361, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedAllocText?       EncodedAllocText       { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 AllocGrp, counted by NoAllocs, tag 78.</summary>
	public sealed class AllocGrp : ICommissionData, ISettlInstructionsData
	{
		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AllocAccount             AllocAccount            { get; init; }

		/// <summary>The FIX AllocAcctIDSource, tag 661, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocAcctIDSource?       AllocAcctIDSource       { get; internal set; }

		/// <summary>The FIX MatchStatus, tag 573, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.MatchStatus?             MatchStatus             { get; internal set; }

		/// <summary>The FIX AllocPrice, tag 366, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.AllocPrice?              AllocPrice              { get; internal set; }

		/// <summary>The FIX AllocQty, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.AllocQty?                AllocQty                { get; internal set; }

		/// <summary>The FIX IndividualAllocID, tag 467, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocID?       IndividualAllocID       { get; internal set; }

		/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.ProcessCode?             ProcessCode             { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?        NoNestedPartyIDs        { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?              NestedParties           { get; internal set; }

		/// <summary>The FIX NotifyBrokerOfCredit, tag 208, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.NotifyBrokerOfCredit?    NotifyBrokerOfCredit    { get; internal set; }

		/// <summary>The FIX AllocHandlInst, tag 209, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocHandlInst?          AllocHandlInst          { get; internal set; }

		/// <summary>The FIX AllocText, tag 161, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AllocText?               AllocText               { get; internal set; }

		/// <summary>The FIX EncodedAllocTextLen, tag 360, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedAllocTextLen?     EncodedAllocTextLen     { get; internal set; }

		/// <summary>The FIX EncodedAllocText, tag 361, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedAllocText?        EncodedAllocText        { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.Commission?              Commission              { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CommType?                CommType                { get; internal set; }

		/// <summary>The FIX CommCurrency, tag 479, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CommCurrency?            CommCurrency            { get; internal set; }

		/// <summary>The FIX FundRenewWaiv, tag 497, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.FundRenewWaiv?           FundRenewWaiv           { get; internal set; }

		/// <summary>The FIX AllocAvgPx, tag 153, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.AllocAvgPx?              AllocAvgPx              { get; internal set; }

		/// <summary>The FIX AllocNetMoney, tag 154, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.AllocNetMoney?           AllocNetMoney           { get; internal set; }

		/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrAmt?            SettlCurrAmt            { get; internal set; }

		/// <summary>The FIX AllocSettlCurrAmt, tag 737, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlCurrAmt?       AllocSettlCurrAmt       { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrency?           SettlCurrency           { get; internal set; }

		/// <summary>The FIX AllocSettlCurrency, tag 736, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlCurrency?      AllocSettlCurrency      { get; internal set; }

		/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrFxRate?         SettlCurrFxRate         { get; internal set; }

		/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrFxRateCalc?     SettlCurrFxRateCalc     { get; internal set; }

		/// <summary>The FIX AllocAccruedInterestAmt, tag 742, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.AllocAccruedInterestAmt? AllocAccruedInterestAmt { get; internal set; }

		/// <summary>The FIX AllocInterestAtMaturity, tag 741, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.AllocInterestAtMaturity? AllocInterestAtMaturity { get; internal set; }

		/// <summary>The FIX NoMiscFees, tag 136; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoMiscFees?              NoMiscFees              { get; internal set; }

		/// <summary>The entries counted by NoMiscFees, tag 136; null when the group is absent.</summary>
		public          List<MiscFeesGrp>?                MiscFeesGrp             { get; internal set; }

		/// <summary>The FIX NoClearingInstructions, tag 576; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoClearingInstructions?  NoClearingInstructions  { get; internal set; }

		/// <summary>The entries counted by NoClearingInstructions, tag 576; null when the group is absent.</summary>
		public          List<ClrInstGrp>?                 ClrInstGrp              { get; internal set; }

		/// <summary>The FIX AllocSettlInstType, tag 780, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlInstType?      AllocSettlInstType      { get; internal set; }

		/// <summary>The FIX SettlDeliveryType, tag 172, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SettlDeliveryType?       SettlDeliveryType       { get; internal set; }

		/// <summary>The FIX StandInstDbType, tag 169, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbType?         StandInstDbType         { get; internal set; }

		/// <summary>The FIX StandInstDbName, tag 170, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbName?         StandInstDbName         { get; internal set; }

		/// <summary>The FIX StandInstDbID, tag 171, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbID?           StandInstDbID           { get; internal set; }

		/// <summary>The FIX NoDlvyInst, tag 85; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoDlvyInst?              NoDlvyInst              { get; internal set; }

		/// <summary>The entries counted by NoDlvyInst, tag 85; null when the group is absent.</summary>
		public          List<DlvyInstGrp>?                DlvyInstGrp             { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 AttrbGrp, counted by NoInstrAttrib, tag 870.</summary>
	public sealed class AttrbGrp
	{
		/// <summary>The FIX InstrAttribType, tag 871, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.InstrAttribType   InstrAttribType  { get; init; }

		/// <summary>The FIX InstrAttribValue, tag 872, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrAttribValue? InstrAttribValue { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 BidCompReqGrp, counted by NoBidComponents, tag 420.</summary>
	public sealed class BidCompReqGrp
	{
		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ListID               ListID              { get; init; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                Side                { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?    TradingSessionID    { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID? TradingSessionSubID { get; internal set; }

		/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NetGrossInd?         NetGrossInd         { get; internal set; }

		/// <summary>The FIX SettlType, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlType?           SettlType           { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?           SettlDate           { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?             Account             { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?        AcctIDSource        { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 BidCompRspGrp, counted by NoBidComponents, tag 420.</summary>
	public sealed class BidCompRspGrp : ICommissionData
	{
		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public required FixField.Commission           Commission          { get; init; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CommType?            CommType            { get; internal set; }

		/// <summary>The FIX CommCurrency, tag 479, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CommCurrency?        CommCurrency        { get; internal set; }

		/// <summary>The FIX FundRenewWaiv, tag 497, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.FundRenewWaiv?       FundRenewWaiv       { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ListID?              ListID              { get; internal set; }

		/// <summary>The FIX Country, tag 421, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.Country?             Country             { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                Side                { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price?               Price               { get; internal set; }

		/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PriceType?           PriceType           { get; internal set; }

		/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.FairValue?           FairValue           { get; internal set; }

		/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NetGrossInd?         NetGrossInd         { get; internal set; }

		/// <summary>The FIX SettlType, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlType?           SettlType           { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?           SettlDate           { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?    TradingSessionID    { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID? TradingSessionSubID { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                Text                { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?      EncodedTextLen      { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?         EncodedText         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 BidDescReqGrp, counted by NoBidDescriptors, tag 398.</summary>
	public sealed class BidDescReqGrp
	{
		/// <summary>The FIX BidDescriptorType, tag 399, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.BidDescriptorType       BidDescriptorType      { get; init; }

		/// <summary>The FIX BidDescriptor, tag 400, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BidDescriptor?          BidDescriptor          { get; internal set; }

		/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SideValueInd?           SideValueInd           { get; internal set; }

		/// <summary>The FIX LiquidityValue, tag 404, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.LiquidityValue?         LiquidityValue         { get; internal set; }

		/// <summary>The FIX LiquidityNumSecurities, tag 441, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LiquidityNumSecurities? LiquidityNumSecurities { get; internal set; }

		/// <summary>The FIX LiquidityPctLow, tag 402, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LiquidityPctLow?        LiquidityPctLow        { get; internal set; }

		/// <summary>The FIX LiquidityPctHigh, tag 403, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LiquidityPctHigh?       LiquidityPctHigh       { get; internal set; }

		/// <summary>The FIX EFPTrackingError, tag 405, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.EFPTrackingError?       EFPTrackingError       { get; internal set; }

		/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.FairValue?              FairValue              { get; internal set; }

		/// <summary>The FIX OutsideIndexPct, tag 407, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OutsideIndexPct?        OutsideIndexPct        { get; internal set; }

		/// <summary>The FIX ValueOfFutures, tag 408, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.ValueOfFutures?         ValueOfFutures         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ClrInstGrp, counted by NoClearingInstructions, tag 576.</summary>
	public sealed class ClrInstGrp
	{
		/// <summary>The FIX ClearingInstruction, tag 577, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.ClearingInstruction ClearingInstruction { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 CollInqQualGrp, counted by NoCollInquiryQualifier, tag 938.</summary>
	public sealed class CollInqQualGrp
	{
		/// <summary>The FIX CollInquiryQualifier, tag 896, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.CollInquiryQualifier CollInquiryQualifier { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 CompIDReqGrp, counted by NoCompIDs, tag 936.</summary>
	public sealed class CompIDReqGrp
	{
		/// <summary>The FIX RefCompID, tag 930, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.RefCompID   RefCompID  { get; init; }

		/// <summary>The FIX RefSubID, tag 931, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RefSubID?   RefSubID   { get; internal set; }

		/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocationID? LocationID { get; internal set; }

		/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.DeskID?     DeskID     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 CompIDStatGrp, counted by NoCompIDs, tag 936.</summary>
	public sealed class CompIDStatGrp
	{
		/// <summary>The FIX RefCompID, tag 930, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.RefCompID    RefCompID   { get; init; }

		/// <summary>The FIX RefSubID, tag 931, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RefSubID?    RefSubID    { get; internal set; }

		/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocationID?  LocationID  { get; internal set; }

		/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.DeskID?      DeskID      { get; internal set; }

		/// <summary>The FIX StatusValue, tag 928, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.StatusValue? StatusValue { get; internal set; }

		/// <summary>The FIX StatusText, tag 929, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StatusText?  StatusText  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ContAmtGrp, counted by NoContAmts, tag 518.</summary>
	public sealed class ContAmtGrp
	{
		/// <summary>The FIX ContAmtType, tag 519, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.ContAmtType   ContAmtType  { get; init; }

		/// <summary>The FIX ContAmtValue, tag 520, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContAmtValue? ContAmtValue { get; internal set; }

		/// <summary>The FIX ContAmtCurr, tag 521, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.ContAmtCurr?  ContAmtCurr  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ContraGrp, counted by NoContraBrokers, tag 382.</summary>
	public sealed class ContraGrp
	{
		/// <summary>The FIX ContraBroker, tag 375, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ContraBroker     ContraBroker    { get; init; }

		/// <summary>The FIX ContraTrader, tag 337, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ContraTrader?    ContraTrader    { get; internal set; }

		/// <summary>The FIX ContraTradeQty, tag 437, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.ContraTradeQty?  ContraTradeQty  { get; internal set; }

		/// <summary>The FIX ContraTradeTime, tag 438, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ContraTradeTime? ContraTradeTime { get; internal set; }

		/// <summary>The FIX ContraLegRefID, tag 655, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ContraLegRefID?  ContraLegRefID  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 CpctyConfGrp, counted by NoCapacities, tag 862.</summary>
	public sealed class CpctyConfGrp
	{
		/// <summary>The FIX OrderCapacity, tag 528, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.OrderCapacity      OrderCapacity     { get; init; }

		/// <summary>The FIX OrderRestrictions, tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OrderRestrictions? OrderRestrictions { get; internal set; }

		/// <summary>The FIX OrderCapacityQty, tag 863, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderCapacityQty?  OrderCapacityQty  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 DlvyInstGrp, counted by NoDlvyInst, tag 85.</summary>
	public sealed class DlvyInstGrp
	{
		/// <summary>The FIX SettlInstSource, tag 165, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.SettlInstSource  SettlInstSource { get; init; }

		/// <summary>The FIX DlvyInstType, tag 787, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.DlvyInstType?    DlvyInstType    { get; internal set; }

		/// <summary>The FIX NoSettlPartyIDs, tag 781; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSettlPartyIDs? NoSettlPartyIDs { get; internal set; }

		/// <summary>The entries counted by NoSettlPartyIDs, tag 781; null when the group is absent.</summary>
		public          List<SettlParties>?       SettlParties    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 EvntGrp, counted by NoEvents, tag 864.</summary>
	public sealed class EvntGrp
	{
		/// <summary>The FIX EventType, tag 865, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.EventType  EventType { get; init; }

		/// <summary>The FIX EventDate, tag 866, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.EventDate? EventDate { get; internal set; }

		/// <summary>The FIX EventPx, tag 867, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.EventPx?   EventPx   { get; internal set; }

		/// <summary>The FIX EventText, tag 868, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.EventText? EventText { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ExecAllocGrp, counted by NoExecs, tag 124.</summary>
	public sealed class ExecAllocGrp
	{
		/// <summary>The FIX LastQty, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
		public required FixField.LastQty          LastQty         { get; init; }

		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ExecID?          ExecID          { get; internal set; }

		/// <summary>The FIX SecondaryExecID, tag 527, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryExecID? SecondaryExecID { get; internal set; }

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LastPx?          LastPx          { get; internal set; }

		/// <summary>The FIX LastParPx, tag 669, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LastParPx?       LastParPx       { get; internal set; }

		/// <summary>The FIX LastCapacity, tag 29, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LastCapacity?    LastCapacity    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ExecCollGrp, counted by NoExecs, tag 124.</summary>
	public sealed class ExecCollGrp
	{
		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ExecID ExecID { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 ExecsGrp, counted by NoExecs, tag 124.</summary>
	public sealed class ExecsGrp
	{
		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ExecID? ExecID { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 Hop, counted by NoHops, tag 627.</summary>
	public sealed class Hop
	{
		/// <summary>The FIX HopCompID, tag 628, wire type <c>String</c>; it opens the entry.</summary>
		public required FixField.HopCompID       HopCompID      { get; init; }

		/// <summary>The FIX HopSendingTime, tag 629, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.HopSendingTime? HopSendingTime { get; internal set; }

		/// <summary>The FIX HopRefID, tag 630, wire type <c>SeqNum</c>; null when the field is absent.</summary>
		public          FixField.HopRefID?       HopRefID       { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 IOIQualGrp, counted by NoIOIQualifiers, tag 199.</summary>
	public sealed class IOIQualGrp
	{
		/// <summary>The FIX IOIQualifier, tag 104, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.IOIQualifier IOIQualifier { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class InstrmtGrp : IInstrument
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtLegExecGrp, counted by NoLegs, tag 555.</summary>
	public sealed class InstrmtLegExecGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX LegPositionEffect, tag 564, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegPositionEffect?             LegPositionEffect             { get; internal set; }

		/// <summary>The FIX LegCoveredOrUncovered, tag 565, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegCoveredOrUncovered?         LegCoveredOrUncovered         { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }

		/// <summary>The FIX LegRefID, tag 654, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRefID?                      LegRefID                      { get; internal set; }

		/// <summary>The FIX LegPrice, tag 566, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegPrice?                      LegPrice                      { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }

		/// <summary>The FIX LegLastPx, tag 637, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegLastPx?                     LegLastPx                     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtLegGrp, counted by NoLegs, tag 555.</summary>
	public sealed class InstrmtLegGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtLegIOIGrp, counted by NoLegs, tag 555.</summary>
	public sealed class InstrmtLegIOIGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegIOIQty, tag 682, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIOIQty?                     LegIOIQty                     { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtLegSecListGrp, counted by NoLegs, tag 555.</summary>
	public sealed class InstrmtLegSecListGrp : IInstrumentLeg, ILegBenchmarkCurveData
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveCurrency, tag 676, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveCurrency?     LegBenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveName, tag 677, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveName?         LegBenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurvePoint, tag 678, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurvePoint?        LegBenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX LegBenchmarkPrice, tag 679, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPrice?             LegBenchmarkPrice             { get; internal set; }

		/// <summary>The FIX LegBenchmarkPriceType, tag 680, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPriceType?         LegBenchmarkPriceType         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtMDReqGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class InstrmtMDReqGrp : IInstrument
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 InstrmtStrkPxGrp, counted by NoStrikes, tag 428.</summary>
	public sealed class InstrmtStrkPxGrp : IInstrument
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegOrdGrp, counted by NoLegs, tag 555.</summary>
	public sealed class LegOrdGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX NoLegAllocs, tag 670; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegAllocs?                   NoLegAllocs                   { get; internal set; }

		/// <summary>The entries counted by NoLegAllocs, tag 670; null when the group is absent.</summary>
		public          List<LegPreAllocGrp>?                   LegPreAllocGrp                { get; internal set; }

		/// <summary>The FIX LegPositionEffect, tag 564, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegPositionEffect?             LegPositionEffect             { get; internal set; }

		/// <summary>The FIX LegCoveredOrUncovered, tag 565, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegCoveredOrUncovered?         LegCoveredOrUncovered         { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }

		/// <summary>The FIX LegRefID, tag 654, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRefID?                      LegRefID                      { get; internal set; }

		/// <summary>The FIX LegPrice, tag 566, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegPrice?                      LegPrice                      { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegPreAllocGrp, counted by NoLegAllocs, tag 670.</summary>
	public sealed class LegPreAllocGrp
	{
		/// <summary>The FIX LegAllocAccount, tag 671, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegAllocAccount       LegAllocAccount      { get; init; }

		/// <summary>The FIX LegIndividualAllocID, tag 672, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIndividualAllocID? LegIndividualAllocID { get; internal set; }

		/// <summary>The FIX NoNested2PartyIDs, tag 756; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested2PartyIDs?    NoNested2PartyIDs    { get; internal set; }

		/// <summary>The entries counted by NoNested2PartyIDs, tag 756; null when the group is absent.</summary>
		public          List<NestedParties2>?          NestedParties2       { get; internal set; }

		/// <summary>The FIX LegAllocQty, tag 673, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegAllocQty?          LegAllocQty          { get; internal set; }

		/// <summary>The FIX LegAllocAcctIDSource, tag 674, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegAllocAcctIDSource? LegAllocAcctIDSource { get; internal set; }

		/// <summary>The FIX LegSettlCurrency, tag 675, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegSettlCurrency?     LegSettlCurrency     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegQuotGrp, counted by NoLegs, tag 555.</summary>
	public sealed class LegQuotGrp : IInstrumentLeg, ILegBenchmarkCurveData
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }

		/// <summary>The FIX LegPriceType, tag 686, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegPriceType?                  LegPriceType                  { get; internal set; }

		/// <summary>The FIX LegBidPx, tag 681, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegBidPx?                      LegBidPx                      { get; internal set; }

		/// <summary>The FIX LegOfferPx, tag 684, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegOfferPx?                    LegOfferPx                    { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveCurrency, tag 676, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveCurrency?     LegBenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveName, tag 677, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveName?         LegBenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurvePoint, tag 678, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurvePoint?        LegBenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX LegBenchmarkPrice, tag 679, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPrice?             LegBenchmarkPrice             { get; internal set; }

		/// <summary>The FIX LegBenchmarkPriceType, tag 680, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPriceType?         LegBenchmarkPriceType         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegQuotStatGrp, counted by NoLegs, tag 555.</summary>
	public sealed class LegQuotStatGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegSecAltIDGrp, counted by NoLegSecurityAltID, tag 604.</summary>
	public sealed class LegSecAltIDGrp
	{
		/// <summary>The FIX LegSecurityAltID, tag 605, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSecurityAltID        LegSecurityAltID       { get; init; }

		/// <summary>The FIX LegSecurityAltIDSource, tag 606, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityAltIDSource? LegSecurityAltIDSource { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LegStipulations, counted by NoLegStipulations, tag 683.</summary>
	public sealed class LegStipulations
	{
		/// <summary>The FIX LegStipulationType, tag 688, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegStipulationType   LegStipulationType  { get; init; }

		/// <summary>The FIX LegStipulationValue, tag 689, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStipulationValue? LegStipulationValue { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 LinesOfTextGrp, counted by LinesOfText, tag 33.</summary>
	public sealed class LinesOfTextGrp
	{
		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Text            Text           { get; init; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?    EncodedText    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 ListOrdGrp, counted by NoOrders, tag 73.</summary>
	public sealed class ListOrdGrp : ICommissionData, IDiscretionInstructions, IInstrument, IOrderQtyData, IPegInstructions, ISpreadOrBenchmarkCurveData, IYieldData
	{
		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ClOrdID                     ClOrdID                    { get; init; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?           SecondaryClOrdID           { get; internal set; }

		/// <summary>The FIX ListSeqNo, tag 67, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.ListSeqNo?                  ListSeqNo                  { get; internal set; }

		/// <summary>The FIX ClOrdLinkID, tag 583, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdLinkID?                ClOrdLinkID                { get; internal set; }

		/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlInstMode?              SettlInstMode              { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?                 NoPartyIDs                 { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                       Parties                    { get; internal set; }

		/// <summary>The FIX TradeOriginationDate, tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeOriginationDate?       TradeOriginationDate       { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeDate?                  TradeDate                  { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?                    Account                    { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?               AcctIDSource               { get; internal set; }

		/// <summary>The FIX AccountType, tag 581, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AccountType?                AccountType                { get; internal set; }

		/// <summary>The FIX DayBookingInst, tag 589, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.DayBookingInst?             DayBookingInst             { get; internal set; }

		/// <summary>The FIX BookingUnit, tag 590, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.BookingUnit?                BookingUnit                { get; internal set; }

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AllocID?                    AllocID                    { get; internal set; }

		/// <summary>The FIX PreallocMethod, tag 591, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PreallocMethod?             PreallocMethod             { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoAllocs?                   NoAllocs                   { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public          List<PreAllocGrp>?                   PreAllocGrp                { get; internal set; }

		/// <summary>The FIX SettlType, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlType?                  SettlType                  { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?                  SettlDate                  { get; internal set; }

		/// <summary>The FIX CashMargin, tag 544, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CashMargin?                 CashMargin                 { get; internal set; }

		/// <summary>The FIX ClearingFeeIndicator, tag 635, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClearingFeeIndicator?       ClearingFeeIndicator       { get; internal set; }

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.HandlInst?                  HandlInst                  { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.ExecInst?                   ExecInst                   { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MinQty?                     MinQty                     { get; internal set; }

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MaxFloor?                   MaxFloor                   { get; internal set; }

		/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.ExDestination?              ExDestination              { get; internal set; }

		/// <summary>The FIX NoTradingSessions, tag 386; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoTradingSessions?          NoTradingSessions          { get; internal set; }

		/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
		public          List<TrdgSesGrp>?                    TrdgSesGrp                 { get; internal set; }

		/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.ProcessCode?                ProcessCode                { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Symbol?                     Symbol                     { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.PrevClosePx?                PrevClosePx                { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                       Side                       { get; internal set; }

		/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SideValueInd?               SideValueInd               { get; internal set; }

		/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.LocateReqd?                 LocateReqd                 { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime?               TransactTime               { get; internal set; }

		/// <summary>The FIX NoStipulations, tag 232; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoStipulations?             NoStipulations             { get; internal set; }

		/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
		public          List<Stipulations>?                  Stipulations               { get; internal set; }

		/// <summary>The FIX QtyType, tag 854, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QtyType?                    QtyType                    { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?                   OrderQty                   { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CashOrderQty?               CashOrderQty               { get; internal set; }

		/// <summary>The FIX OrderPercent, tag 516, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OrderPercent?               OrderPercent               { get; internal set; }

		/// <summary>The FIX RoundingDirection, tag 468, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.RoundingDirection?          RoundingDirection          { get; internal set; }

		/// <summary>The FIX RoundingModulus, tag 469, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.RoundingModulus?            RoundingModulus            { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                    OrdType                    { get; internal set; }

		/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PriceType?                  PriceType                  { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price?                      Price                      { get; internal set; }

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StopPx?                     StopPx                     { get; internal set; }

		/// <summary>The FIX Spread, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.Spread?                     Spread                     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveCurrency, tag 220, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveCurrency?     BenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveName, tag 221, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveName?         BenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX BenchmarkCurvePoint, tag 222, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurvePoint?        BenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX BenchmarkPrice, tag 662, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPrice?             BenchmarkPrice             { get; internal set; }

		/// <summary>The FIX BenchmarkPriceType, tag 663, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPriceType?         BenchmarkPriceType         { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityID, tag 699, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityID?        BenchmarkSecurityID        { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityIDSource, tag 761, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityIDSource?  BenchmarkSecurityIDSource  { get; internal set; }

		/// <summary>The FIX YieldType, tag 235, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.YieldType?                  YieldType                  { get; internal set; }

		/// <summary>The FIX Yield, tag 236, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.Yield?                      Yield                      { get; internal set; }

		/// <summary>The FIX YieldCalcDate, tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldCalcDate?              YieldCalcDate              { get; internal set; }

		/// <summary>The FIX YieldRedemptionDate, tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionDate?        YieldRedemptionDate        { get; internal set; }

		/// <summary>The FIX YieldRedemptionPrice, tag 697, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPrice?       YieldRedemptionPrice       { get; internal set; }

		/// <summary>The FIX YieldRedemptionPriceType, tag 698, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPriceType?   YieldRedemptionPriceType   { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ComplianceID?               ComplianceID               { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.SolicitedFlag?              SolicitedFlag              { get; internal set; }

		/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IOIid?                      IOIid                      { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.QuoteID?                    QuoteID                    { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.TimeInForce?                TimeInForce                { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.EffectiveTime?              EffectiveTime              { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.ExpireDate?                 ExpireDate                 { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?                 ExpireTime                 { get; internal set; }

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.GTBookingInst?              GTBookingInst              { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.Commission?                 Commission                 { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CommType?                   CommType                   { get; internal set; }

		/// <summary>The FIX CommCurrency, tag 479, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CommCurrency?               CommCurrency               { get; internal set; }

		/// <summary>The FIX FundRenewWaiv, tag 497, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.FundRenewWaiv?              FundRenewWaiv              { get; internal set; }

		/// <summary>The FIX OrderCapacity, tag 528, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrderCapacity?              OrderCapacity              { get; internal set; }

		/// <summary>The FIX OrderRestrictions, tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OrderRestrictions?          OrderRestrictions          { get; internal set; }

		/// <summary>The FIX CustOrderCapacity, tag 582, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CustOrderCapacity?          CustOrderCapacity          { get; internal set; }

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.ForexReq?                   ForexReq                   { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrency?              SettlCurrency              { get; internal set; }

		/// <summary>The FIX BookingType, tag 775, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BookingType?                BookingType                { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                       Text                       { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?             EncodedTextLen             { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?                EncodedText                { get; internal set; }

		/// <summary>The FIX SettlDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate2?                 SettlDate2                 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty2?                  OrderQty2                  { get; internal set; }

		/// <summary>The FIX Price2, tag 640, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price2?                     Price2                     { get; internal set; }

		/// <summary>The FIX PositionEffect, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PositionEffect?             PositionEffect             { get; internal set; }

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CoveredOrUncovered?         CoveredOrUncovered         { get; internal set; }

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MaxShow?                    MaxShow                    { get; internal set; }

		/// <summary>The FIX PegOffsetValue, tag 211, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.PegOffsetValue?             PegOffsetValue             { get; internal set; }

		/// <summary>The FIX PegMoveType, tag 835, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PegMoveType?                PegMoveType                { get; internal set; }

		/// <summary>The FIX PegOffsetType, tag 836, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PegOffsetType?              PegOffsetType              { get; internal set; }

		/// <summary>The FIX PegLimitType, tag 837, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PegLimitType?               PegLimitType               { get; internal set; }

		/// <summary>The FIX PegRoundDirection, tag 838, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PegRoundDirection?          PegRoundDirection          { get; internal set; }

		/// <summary>The FIX PegScope, tag 840, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PegScope?                   PegScope                   { get; internal set; }

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.DiscretionInst?             DiscretionInst             { get; internal set; }

		/// <summary>The FIX DiscretionOffsetValue, tag 389, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.DiscretionOffsetValue?      DiscretionOffsetValue      { get; internal set; }

		/// <summary>The FIX DiscretionMoveType, tag 841, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DiscretionMoveType?         DiscretionMoveType         { get; internal set; }

		/// <summary>The FIX DiscretionOffsetType, tag 842, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DiscretionOffsetType?       DiscretionOffsetType       { get; internal set; }

		/// <summary>The FIX DiscretionLimitType, tag 843, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DiscretionLimitType?        DiscretionLimitType        { get; internal set; }

		/// <summary>The FIX DiscretionRoundDirection, tag 844, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DiscretionRoundDirection?   DiscretionRoundDirection   { get; internal set; }

		/// <summary>The FIX DiscretionScope, tag 846, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DiscretionScope?            DiscretionScope            { get; internal set; }

		/// <summary>The FIX TargetStrategy, tag 847, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TargetStrategy?             TargetStrategy             { get; internal set; }

		/// <summary>The FIX TargetStrategyParameters, tag 848, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TargetStrategyParameters?   TargetStrategyParameters   { get; internal set; }

		/// <summary>The FIX ParticipationRate, tag 849, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.ParticipationRate?          ParticipationRate          { get; internal set; }

		/// <summary>The FIX Designation, tag 494, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Designation?                Designation                { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 MDFullGrp, counted by NoMDEntries, tag 268.</summary>
	public sealed class MDFullGrp
	{
		/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.MDEntryType          MDEntryType         { get; init; }

		/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.MDEntryPx?           MDEntryPx           { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?            Currency            { get; internal set; }

		/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MDEntrySize?         MDEntrySize         { get; internal set; }

		/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDateOnly</c>; null when the field is absent.</summary>
		public          FixField.MDEntryDate?         MDEntryDate         { get; internal set; }

		/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
		public          FixField.MDEntryTime?         MDEntryTime         { get; internal set; }

		/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.TickDirection?       TickDirection       { get; internal set; }

		/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.MDMkt?               MDMkt               { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?    TradingSessionID    { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID? TradingSessionSubID { get; internal set; }

		/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.QuoteCondition?      QuoteCondition      { get; internal set; }

		/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.TradeCondition?      TradeCondition      { get; internal set; }

		/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryOriginator?   MDEntryOriginator   { get; internal set; }

		/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocationID?          LocationID          { get; internal set; }

		/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.DeskID?              DeskID              { get; internal set; }

		/// <summary>The FIX OpenCloseSettlFlag, tag 286, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OpenCloseSettlFlag?  OpenCloseSettlFlag  { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.TimeInForce?         TimeInForce         { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.ExpireDate?          ExpireDate          { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?          ExpireTime          { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MinQty?              MinQty              { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.ExecInst?            ExecInst            { get; internal set; }

		/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SellerDays?          SellerDays          { get; internal set; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrderID?             OrderID             { get; internal set; }

		/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.QuoteEntryID?        QuoteEntryID        { get; internal set; }

		/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryBuyer?        MDEntryBuyer        { get; internal set; }

		/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntrySeller?       MDEntrySeller       { get; internal set; }

		/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NumberOfOrders?      NumberOfOrders      { get; internal set; }

		/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.MDEntryPositionNo?   MDEntryPositionNo   { get; internal set; }

		/// <summary>The FIX Scope, tag 546, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.Scope?               Scope               { get; internal set; }

		/// <summary>The FIX PriceDelta, tag 811, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.PriceDelta?          PriceDelta          { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                Text                { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?      EncodedTextLen      { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?         EncodedText         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 MDIncGrp, counted by NoMDEntries, tag 268.</summary>
	public sealed class MDIncGrp : IInstrument
	{
		/// <summary>The FIX MDUpdateAction, tag 279, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.MDUpdateAction              MDUpdateAction             { get; init; }

		/// <summary>The FIX DeleteReason, tag 285, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.DeleteReason?               DeleteReason               { get; internal set; }

		/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.MDEntryType?                MDEntryType                { get; internal set; }

		/// <summary>The FIX MDEntryID, tag 278, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryID?                  MDEntryID                  { get; internal set; }

		/// <summary>The FIX MDEntryRefID, tag 280, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryRefID?               MDEntryRefID               { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Symbol?                     Symbol                     { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }

		/// <summary>The FIX FinancialStatus, tag 291, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.FinancialStatus?            FinancialStatus            { get; internal set; }

		/// <summary>The FIX CorporateAction, tag 292, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.CorporateAction?            CorporateAction            { get; internal set; }

		/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.MDEntryPx?                  MDEntryPx                  { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MDEntrySize?                MDEntrySize                { get; internal set; }

		/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDateOnly</c>; null when the field is absent.</summary>
		public          FixField.MDEntryDate?                MDEntryDate                { get; internal set; }

		/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
		public          FixField.MDEntryTime?                MDEntryTime                { get; internal set; }

		/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.TickDirection?              TickDirection              { get; internal set; }

		/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.MDMkt?                      MDMkt                      { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.QuoteCondition?             QuoteCondition             { get; internal set; }

		/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.TradeCondition?             TradeCondition             { get; internal set; }

		/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryOriginator?          MDEntryOriginator          { get; internal set; }

		/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocationID?                 LocationID                 { get; internal set; }

		/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.DeskID?                     DeskID                     { get; internal set; }

		/// <summary>The FIX OpenCloseSettlFlag, tag 286, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OpenCloseSettlFlag?         OpenCloseSettlFlag         { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.TimeInForce?                TimeInForce                { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.ExpireDate?                 ExpireDate                 { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?                 ExpireTime                 { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MinQty?                     MinQty                     { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.ExecInst?                   ExecInst                   { get; internal set; }

		/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SellerDays?                 SellerDays                 { get; internal set; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrderID?                    OrderID                    { get; internal set; }

		/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.QuoteEntryID?               QuoteEntryID               { get; internal set; }

		/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntryBuyer?               MDEntryBuyer               { get; internal set; }

		/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MDEntrySeller?              MDEntrySeller              { get; internal set; }

		/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NumberOfOrders?             NumberOfOrders             { get; internal set; }

		/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.MDEntryPositionNo?          MDEntryPositionNo          { get; internal set; }

		/// <summary>The FIX Scope, tag 546, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.Scope?                      Scope                      { get; internal set; }

		/// <summary>The FIX PriceDelta, tag 811, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.PriceDelta?                 PriceDelta                 { get; internal set; }

		/// <summary>The FIX NetChgPrevDay, tag 451, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.NetChgPrevDay?              NetChgPrevDay              { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                       Text                       { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?             EncodedTextLen             { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?                EncodedText                { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 MDReqGrp, counted by NoMDEntryTypes, tag 267.</summary>
	public sealed class MDReqGrp
	{
		/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.MDEntryType MDEntryType { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 MDRjctGrp, counted by NoAltMDSource, tag 816.</summary>
	public sealed class MDRjctGrp
	{
		/// <summary>The FIX AltMDSourceID, tag 817, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AltMDSourceID AltMDSourceID { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 MiscFeesGrp, counted by NoMiscFees, tag 136.</summary>
	public sealed class MiscFeesGrp
	{
		/// <summary>The FIX MiscFeeAmt, tag 137, wire type <c>Amt</c>; null when the field is absent.</summary>
		public required FixField.MiscFeeAmt    MiscFeeAmt   { get; init; }

		/// <summary>The FIX MiscFeeCurr, tag 138, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.MiscFeeCurr?  MiscFeeCurr  { get; internal set; }

		/// <summary>The FIX MiscFeeType, tag 139, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.MiscFeeType?  MiscFeeType  { get; internal set; }

		/// <summary>The FIX MiscFeeBasis, tag 891, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.MiscFeeBasis? MiscFeeBasis { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 MsgTypeGrp, counted by NoMsgTypes, tag 384.</summary>
	/// <remarks>
	/// The repository writes this group inline in Logon rather than as a component of its own, so
	/// it has no component name there; MsgTypeGrp is the name this package has always used for it.
	/// </remarks>
	public sealed class MsgTypeGrp
	{
		/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.RefMsgType    RefMsgType   { get; init; }

		/// <summary>The FIX MsgDirection, tag 385, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.MsgDirection? MsgDirection { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NestedParties, counted by NoNestedPartyIDs, tag 539.</summary>
	public sealed class NestedParties
	{
		/// <summary>The FIX NestedPartyID, tag 524, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.NestedPartyID        NestedPartyID       { get; init; }

		/// <summary>The FIX NestedPartyIDSource, tag 525, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.NestedPartyIDSource? NestedPartyIDSource { get; internal set; }

		/// <summary>The FIX NestedPartyRole, tag 538, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NestedPartyRole?     NestedPartyRole     { get; internal set; }

		/// <summary>The FIX NoNestedPartySubIDs, tag 804; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartySubIDs? NoNestedPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNestedPartySubIDs, tag 804; null when the group is absent.</summary>
		public          List<NstdPtysSubGrp>?         NstdPtysSubGrp      { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NestedParties2, counted by NoNested2PartyIDs, tag 756.</summary>
	public sealed class NestedParties2
	{
		/// <summary>The FIX Nested2PartyID, tag 757, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Nested2PartyID        Nested2PartyID       { get; init; }

		/// <summary>The FIX Nested2PartyIDSource, tag 758, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Nested2PartyIDSource? Nested2PartyIDSource { get; internal set; }

		/// <summary>The FIX Nested2PartyRole, tag 759, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Nested2PartyRole?     Nested2PartyRole     { get; internal set; }

		/// <summary>The FIX NoNested2PartySubIDs, tag 806; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested2PartySubIDs? NoNested2PartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNested2PartySubIDs, tag 806; null when the group is absent.</summary>
		public          List<NstdPtys2SubGrp>?         NstdPtys2SubGrp      { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NestedParties3, counted by NoNested3PartyIDs, tag 948.</summary>
	public sealed class NestedParties3
	{
		/// <summary>The FIX Nested3PartyID, tag 949, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Nested3PartyID        Nested3PartyID       { get; init; }

		/// <summary>The FIX Nested3PartyIDSource, tag 950, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Nested3PartyIDSource? Nested3PartyIDSource { get; internal set; }

		/// <summary>The FIX Nested3PartyRole, tag 951, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Nested3PartyRole?     Nested3PartyRole     { get; internal set; }

		/// <summary>The FIX NoNested3PartySubIDs, tag 952; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested3PartySubIDs? NoNested3PartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoNested3PartySubIDs, tag 952; null when the group is absent.</summary>
		public          List<NstdPtys3SubGrp>?         NstdPtys3SubGrp      { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NstdPtys2SubGrp, counted by NoNested2PartySubIDs, tag 806.</summary>
	public sealed class NstdPtys2SubGrp
	{
		/// <summary>The FIX Nested2PartySubID, tag 760, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Nested2PartySubID      Nested2PartySubID     { get; init; }

		/// <summary>The FIX Nested2PartySubIDType, tag 807, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Nested2PartySubIDType? Nested2PartySubIDType { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NstdPtys3SubGrp, counted by NoNested3PartySubIDs, tag 952.</summary>
	public sealed class NstdPtys3SubGrp
	{
		/// <summary>The FIX Nested3PartySubID, tag 953, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Nested3PartySubID      Nested3PartySubID     { get; init; }

		/// <summary>The FIX Nested3PartySubIDType, tag 954, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Nested3PartySubIDType? Nested3PartySubIDType { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 NstdPtysSubGrp, counted by NoNestedPartySubIDs, tag 804.</summary>
	public sealed class NstdPtysSubGrp
	{
		/// <summary>The FIX NestedPartySubID, tag 545, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.NestedPartySubID      NestedPartySubID     { get; init; }

		/// <summary>The FIX NestedPartySubIDType, tag 805, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NestedPartySubIDType? NestedPartySubIDType { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 OrdAllocGrp, counted by NoOrders, tag 73.</summary>
	public sealed class OrdAllocGrp
	{
		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ClOrdID            ClOrdID           { get; init; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrderID?           OrderID           { get; internal set; }

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryOrderID?  SecondaryOrderID  { get; internal set; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?  SecondaryClOrdID  { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ListID?            ListID            { get; internal set; }

		/// <summary>The FIX NoNested2PartyIDs, tag 756; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested2PartyIDs? NoNested2PartyIDs { get; internal set; }

		/// <summary>The entries counted by NoNested2PartyIDs, tag 756; null when the group is absent.</summary>
		public          List<NestedParties2>?       NestedParties2    { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?          OrderQty          { get; internal set; }

		/// <summary>The FIX OrderAvgPx, tag 799, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.OrderAvgPx?        OrderAvgPx        { get; internal set; }

		/// <summary>The FIX OrderBookingQty, tag 800, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderBookingQty?   OrderBookingQty   { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 OrdListStatGrp, counted by NoOrders, tag 73.</summary>
	public sealed class OrdListStatGrp
	{
		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.ClOrdID           ClOrdID          { get; init; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID? SecondaryClOrdID { get; internal set; }

		/// <summary>The FIX CumQty, tag 14, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CumQty?           CumQty           { get; internal set; }

		/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdStatus?        OrdStatus        { get; internal set; }

		/// <summary>The FIX WorkingIndicator, tag 636, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.WorkingIndicator? WorkingIndicator { get; internal set; }

		/// <summary>The FIX LeavesQty, tag 151, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LeavesQty?        LeavesQty        { get; internal set; }

		/// <summary>The FIX CxlQty, tag 84, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CxlQty?           CxlQty           { get; internal set; }

		/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.AvgPx?            AvgPx            { get; internal set; }

		/// <summary>The FIX OrdRejReason, tag 103, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.OrdRejReason?     OrdRejReason     { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?             Text             { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?   EncodedTextLen   { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?      EncodedText      { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 Parties, counted by NoPartyIDs, tag 453.</summary>
	public sealed class Parties
	{
		/// <summary>The FIX PartyID, tag 448, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.PartyID        PartyID       { get; init; }

		/// <summary>The FIX PartyIDSource, tag 447, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PartyIDSource? PartyIDSource { get; internal set; }

		/// <summary>The FIX PartyRole, tag 452, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PartyRole?     PartyRole     { get; internal set; }

		/// <summary>The FIX NoPartySubIDs, tag 802; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartySubIDs? NoPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoPartySubIDs, tag 802; null when the group is absent.</summary>
		public          List<PtysSubGrp>?       PtysSubGrp    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PosUndInstrmtGrp, counted by NoUnderlyings, tag 711.</summary>
	public sealed class PosUndInstrmtGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingSymbol                      UnderlyingSymbol                     { get; init; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }

		/// <summary>The FIX UnderlyingSettlPrice, tag 732, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSettlPrice?                 UnderlyingSettlPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingSettlPriceType, tag 733, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSettlPriceType?             UnderlyingSettlPriceType             { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PositionAmountData, counted by NoPosAmt, tag 753.</summary>
	public sealed class PositionAmountData
	{
		/// <summary>The FIX PosAmtType, tag 707, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.PosAmtType PosAmtType { get; init; }

		/// <summary>The FIX PosAmt, tag 708, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.PosAmt?    PosAmt     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PositionQty, counted by NoPositions, tag 702.</summary>
	public sealed class PositionQty
	{
		/// <summary>The FIX PosType, tag 703, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.PosType           PosType          { get; init; }

		/// <summary>The FIX LongQty, tag 704, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LongQty?          LongQty          { get; internal set; }

		/// <summary>The FIX ShortQty, tag 705, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.ShortQty?         ShortQty         { get; internal set; }

		/// <summary>The FIX PosQtyStatus, tag 706, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PosQtyStatus?     PosQtyStatus     { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs? NoNestedPartyIDs { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?       NestedParties    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PreAllocGrp, counted by NoAllocs, tag 78.</summary>
	public sealed class PreAllocGrp
	{
		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AllocAccount        AllocAccount       { get; init; }

		/// <summary>The FIX AllocAcctIDSource, tag 661, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocAcctIDSource?  AllocAcctIDSource  { get; internal set; }

		/// <summary>The FIX AllocSettlCurrency, tag 736, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlCurrency? AllocSettlCurrency { get; internal set; }

		/// <summary>The FIX IndividualAllocID, tag 467, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocID?  IndividualAllocID  { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?   NoNestedPartyIDs   { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?         NestedParties      { get; internal set; }

		/// <summary>The FIX AllocQty, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.AllocQty?           AllocQty           { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PreAllocMlegGrp, counted by NoAllocs, tag 78.</summary>
	public sealed class PreAllocMlegGrp
	{
		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AllocAccount        AllocAccount       { get; init; }

		/// <summary>The FIX AllocAcctIDSource, tag 661, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocAcctIDSource?  AllocAcctIDSource  { get; internal set; }

		/// <summary>The FIX AllocSettlCurrency, tag 736, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlCurrency? AllocSettlCurrency { get; internal set; }

		/// <summary>The FIX IndividualAllocID, tag 467, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocID?  IndividualAllocID  { get; internal set; }

		/// <summary>The FIX NoNested3PartyIDs, tag 948; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested3PartyIDs?  NoNested3PartyIDs  { get; internal set; }

		/// <summary>The entries counted by NoNested3PartyIDs, tag 948; null when the group is absent.</summary>
		public          List<NestedParties3>?        NestedParties3     { get; internal set; }

		/// <summary>The FIX AllocQty, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.AllocQty?           AllocQty           { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 PtysSubGrp, counted by NoPartySubIDs, tag 802.</summary>
	public sealed class PtysSubGrp
	{
		/// <summary>The FIX PartySubID, tag 523, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.PartySubID      PartySubID     { get; init; }

		/// <summary>The FIX PartySubIDType, tag 803, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PartySubIDType? PartySubIDType { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotCxlEntriesGrp, counted by NoQuoteEntries, tag 295.</summary>
	public sealed class QuotCxlEntriesGrp : IFinancingDetails, IInstrument
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX AgreementDesc, tag 913, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementDesc?              AgreementDesc              { get; internal set; }

		/// <summary>The FIX AgreementID, tag 914, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementID?                AgreementID                { get; internal set; }

		/// <summary>The FIX AgreementDate, tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.AgreementDate?              AgreementDate              { get; internal set; }

		/// <summary>The FIX AgreementCurrency, tag 918, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AgreementCurrency?          AgreementCurrency          { get; internal set; }

		/// <summary>The FIX TerminationType, tag 788, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TerminationType?            TerminationType            { get; internal set; }

		/// <summary>The FIX StartDate, tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.StartDate?                  StartDate                  { get; internal set; }

		/// <summary>The FIX EndDate, tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.EndDate?                    EndDate                    { get; internal set; }

		/// <summary>The FIX DeliveryType, tag 919, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryType?               DeliveryType               { get; internal set; }

		/// <summary>The FIX MarginRatio, tag 898, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MarginRatio?                MarginRatio                { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotEntryAckGrp, counted by NoQuoteEntries, tag 295.</summary>
	public sealed class QuotEntryAckGrp : IInstrument
	{
		/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.QuoteEntryID                QuoteEntryID               { get; init; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Symbol?                     Symbol                     { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }

		/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BidPx?                      BidPx                      { get; internal set; }

		/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.OfferPx?                    OfferPx                    { get; internal set; }

		/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.BidSize?                    BidSize                    { get; internal set; }

		/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OfferSize?                  OfferSize                  { get; internal set; }

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ValidUntilTime?             ValidUntilTime             { get; internal set; }

		/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BidSpotRate?                BidSpotRate                { get; internal set; }

		/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.OfferSpotRate?              OfferSpotRate              { get; internal set; }

		/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.BidForwardPoints?           BidForwardPoints           { get; internal set; }

		/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.OfferForwardPoints?         OfferForwardPoints         { get; internal set; }

		/// <summary>The FIX MidPx, tag 631, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.MidPx?                      MidPx                      { get; internal set; }

		/// <summary>The FIX BidYield, tag 632, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.BidYield?                   BidYield                   { get; internal set; }

		/// <summary>The FIX MidYield, tag 633, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MidYield?                   MidYield                   { get; internal set; }

		/// <summary>The FIX OfferYield, tag 634, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OfferYield?                 OfferYield                 { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime?               TransactTime               { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?                  SettlDate                  { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                    OrdType                    { get; internal set; }

		/// <summary>The FIX SettlDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate2?                 SettlDate2                 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty2?                  OrderQty2                  { get; internal set; }

		/// <summary>The FIX BidForwardPoints2, tag 642, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.BidForwardPoints2?          BidForwardPoints2          { get; internal set; }

		/// <summary>The FIX OfferForwardPoints2, tag 643, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.OfferForwardPoints2?        OfferForwardPoints2        { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX QuoteEntryRejectReason, tag 368, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteEntryRejectReason?     QuoteEntryRejectReason     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotEntryGrp, counted by NoQuoteEntries, tag 295.</summary>
	public sealed class QuotEntryGrp : IInstrument
	{
		/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.QuoteEntryID                QuoteEntryID               { get; init; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Symbol?                     Symbol                     { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }

		/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BidPx?                      BidPx                      { get; internal set; }

		/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.OfferPx?                    OfferPx                    { get; internal set; }

		/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.BidSize?                    BidSize                    { get; internal set; }

		/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OfferSize?                  OfferSize                  { get; internal set; }

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ValidUntilTime?             ValidUntilTime             { get; internal set; }

		/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BidSpotRate?                BidSpotRate                { get; internal set; }

		/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.OfferSpotRate?              OfferSpotRate              { get; internal set; }

		/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.BidForwardPoints?           BidForwardPoints           { get; internal set; }

		/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.OfferForwardPoints?         OfferForwardPoints         { get; internal set; }

		/// <summary>The FIX MidPx, tag 631, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.MidPx?                      MidPx                      { get; internal set; }

		/// <summary>The FIX BidYield, tag 632, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.BidYield?                   BidYield                   { get; internal set; }

		/// <summary>The FIX MidYield, tag 633, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MidYield?                   MidYield                   { get; internal set; }

		/// <summary>The FIX OfferYield, tag 634, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OfferYield?                 OfferYield                 { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime?               TransactTime               { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?                  SettlDate                  { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                    OrdType                    { get; internal set; }

		/// <summary>The FIX SettlDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate2?                 SettlDate2                 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty2?                  OrderQty2                  { get; internal set; }

		/// <summary>The FIX BidForwardPoints2, tag 642, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.BidForwardPoints2?          BidForwardPoints2          { get; internal set; }

		/// <summary>The FIX OfferForwardPoints2, tag 643, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.OfferForwardPoints2?        OfferForwardPoints2        { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotQualGrp, counted by NoQuoteQualifiers, tag 735.</summary>
	public sealed class QuotQualGrp
	{
		/// <summary>The FIX QuoteQualifier, tag 695, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.QuoteQualifier QuoteQualifier { get; init; }
	}

	/// <summary>One entry of the FIX 4.4 QuotReqGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class QuotReqGrp : IFinancingDetails, IInstrument, IOrderQtyData, ISpreadOrBenchmarkCurveData, IYieldData
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX AgreementDesc, tag 913, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementDesc?              AgreementDesc              { get; internal set; }

		/// <summary>The FIX AgreementID, tag 914, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementID?                AgreementID                { get; internal set; }

		/// <summary>The FIX AgreementDate, tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.AgreementDate?              AgreementDate              { get; internal set; }

		/// <summary>The FIX AgreementCurrency, tag 918, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AgreementCurrency?          AgreementCurrency          { get; internal set; }

		/// <summary>The FIX TerminationType, tag 788, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TerminationType?            TerminationType            { get; internal set; }

		/// <summary>The FIX StartDate, tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.StartDate?                  StartDate                  { get; internal set; }

		/// <summary>The FIX EndDate, tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.EndDate?                    EndDate                    { get; internal set; }

		/// <summary>The FIX DeliveryType, tag 919, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryType?               DeliveryType               { get; internal set; }

		/// <summary>The FIX MarginRatio, tag 898, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MarginRatio?                MarginRatio                { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.PrevClosePx?                PrevClosePx                { get; internal set; }

		/// <summary>The FIX QuoteRequestType, tag 303, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteRequestType?           QuoteRequestType           { get; internal set; }

		/// <summary>The FIX QuoteType, tag 537, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteType?                  QuoteType                  { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX TradeOriginationDate, tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeOriginationDate?       TradeOriginationDate       { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                       Side                       { get; internal set; }

		/// <summary>The FIX QtyType, tag 854, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QtyType?                    QtyType                    { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?                   OrderQty                   { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CashOrderQty?               CashOrderQty               { get; internal set; }

		/// <summary>The FIX OrderPercent, tag 516, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OrderPercent?               OrderPercent               { get; internal set; }

		/// <summary>The FIX RoundingDirection, tag 468, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.RoundingDirection?          RoundingDirection          { get; internal set; }

		/// <summary>The FIX RoundingModulus, tag 469, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.RoundingModulus?            RoundingModulus            { get; internal set; }

		/// <summary>The FIX SettlType, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlType?                  SettlType                  { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?                  SettlDate                  { get; internal set; }

		/// <summary>The FIX SettlDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate2?                 SettlDate2                 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty2?                  OrderQty2                  { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX NoStipulations, tag 232; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoStipulations?             NoStipulations             { get; internal set; }

		/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
		public          List<Stipulations>?                  Stipulations               { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?                    Account                    { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?               AcctIDSource               { get; internal set; }

		/// <summary>The FIX AccountType, tag 581, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AccountType?                AccountType                { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<QuotReqLegsGrp>?                QuotReqLegsGrp             { get; internal set; }

		/// <summary>The FIX NoQuoteQualifiers, tag 735; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoQuoteQualifiers?          NoQuoteQualifiers          { get; internal set; }

		/// <summary>The entries counted by NoQuoteQualifiers, tag 735; null when the group is absent.</summary>
		public          List<QuotQualGrp>?                   QuotQualGrp                { get; internal set; }

		/// <summary>The FIX QuotePriceType, tag 692, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuotePriceType?             QuotePriceType             { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                    OrdType                    { get; internal set; }

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ValidUntilTime?             ValidUntilTime             { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?                 ExpireTime                 { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime?               TransactTime               { get; internal set; }

		/// <summary>The FIX Spread, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.Spread?                     Spread                     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveCurrency, tag 220, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveCurrency?     BenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveName, tag 221, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveName?         BenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX BenchmarkCurvePoint, tag 222, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurvePoint?        BenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX BenchmarkPrice, tag 662, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPrice?             BenchmarkPrice             { get; internal set; }

		/// <summary>The FIX BenchmarkPriceType, tag 663, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPriceType?         BenchmarkPriceType         { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityID, tag 699, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityID?        BenchmarkSecurityID        { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityIDSource, tag 761, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityIDSource?  BenchmarkSecurityIDSource  { get; internal set; }

		/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PriceType?                  PriceType                  { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price?                      Price                      { get; internal set; }

		/// <summary>The FIX Price2, tag 640, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price2?                     Price2                     { get; internal set; }

		/// <summary>The FIX YieldType, tag 235, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.YieldType?                  YieldType                  { get; internal set; }

		/// <summary>The FIX Yield, tag 236, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.Yield?                      Yield                      { get; internal set; }

		/// <summary>The FIX YieldCalcDate, tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldCalcDate?              YieldCalcDate              { get; internal set; }

		/// <summary>The FIX YieldRedemptionDate, tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionDate?        YieldRedemptionDate        { get; internal set; }

		/// <summary>The FIX YieldRedemptionPrice, tag 697, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPrice?       YieldRedemptionPrice       { get; internal set; }

		/// <summary>The FIX YieldRedemptionPriceType, tag 698, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPriceType?   YieldRedemptionPriceType   { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?                 NoPartyIDs                 { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                       Parties                    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotReqLegsGrp, counted by NoLegs, tag 555.</summary>
	public sealed class QuotReqLegsGrp : IInstrumentLeg, ILegBenchmarkCurveData
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveCurrency, tag 676, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveCurrency?     LegBenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurveName, tag 677, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurveName?         LegBenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX LegBenchmarkCurvePoint, tag 678, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkCurvePoint?        LegBenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX LegBenchmarkPrice, tag 679, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPrice?             LegBenchmarkPrice             { get; internal set; }

		/// <summary>The FIX LegBenchmarkPriceType, tag 680, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegBenchmarkPriceType?         LegBenchmarkPriceType         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotReqRjctGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class QuotReqRjctGrp : IFinancingDetails, IInstrument, IOrderQtyData, ISpreadOrBenchmarkCurveData, IYieldData
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX AgreementDesc, tag 913, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementDesc?              AgreementDesc              { get; internal set; }

		/// <summary>The FIX AgreementID, tag 914, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementID?                AgreementID                { get; internal set; }

		/// <summary>The FIX AgreementDate, tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.AgreementDate?              AgreementDate              { get; internal set; }

		/// <summary>The FIX AgreementCurrency, tag 918, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AgreementCurrency?          AgreementCurrency          { get; internal set; }

		/// <summary>The FIX TerminationType, tag 788, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TerminationType?            TerminationType            { get; internal set; }

		/// <summary>The FIX StartDate, tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.StartDate?                  StartDate                  { get; internal set; }

		/// <summary>The FIX EndDate, tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.EndDate?                    EndDate                    { get; internal set; }

		/// <summary>The FIX DeliveryType, tag 919, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryType?               DeliveryType               { get; internal set; }

		/// <summary>The FIX MarginRatio, tag 898, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MarginRatio?                MarginRatio                { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.PrevClosePx?                PrevClosePx                { get; internal set; }

		/// <summary>The FIX QuoteRequestType, tag 303, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteRequestType?           QuoteRequestType           { get; internal set; }

		/// <summary>The FIX QuoteType, tag 537, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteType?                  QuoteType                  { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX TradeOriginationDate, tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeOriginationDate?       TradeOriginationDate       { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                       Side                       { get; internal set; }

		/// <summary>The FIX QtyType, tag 854, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QtyType?                    QtyType                    { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?                   OrderQty                   { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CashOrderQty?               CashOrderQty               { get; internal set; }

		/// <summary>The FIX OrderPercent, tag 516, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OrderPercent?               OrderPercent               { get; internal set; }

		/// <summary>The FIX RoundingDirection, tag 468, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.RoundingDirection?          RoundingDirection          { get; internal set; }

		/// <summary>The FIX RoundingModulus, tag 469, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.RoundingModulus?            RoundingModulus            { get; internal set; }

		/// <summary>The FIX SettlType, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlType?                  SettlType                  { get; internal set; }

		/// <summary>The FIX SettlDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate?                  SettlDate                  { get; internal set; }

		/// <summary>The FIX SettlDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.SettlDate2?                 SettlDate2                 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty2?                  OrderQty2                  { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX NoStipulations, tag 232; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoStipulations?             NoStipulations             { get; internal set; }

		/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
		public          List<Stipulations>?                  Stipulations               { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?                    Account                    { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?               AcctIDSource               { get; internal set; }

		/// <summary>The FIX AccountType, tag 581, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AccountType?                AccountType                { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<QuotReqLegsGrp>?                QuotReqLegsGrp             { get; internal set; }

		/// <summary>The FIX NoQuoteQualifiers, tag 735; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoQuoteQualifiers?          NoQuoteQualifiers          { get; internal set; }

		/// <summary>The entries counted by NoQuoteQualifiers, tag 735; null when the group is absent.</summary>
		public          List<QuotQualGrp>?                   QuotQualGrp                { get; internal set; }

		/// <summary>The FIX QuotePriceType, tag 692, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuotePriceType?             QuotePriceType             { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                    OrdType                    { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?                 ExpireTime                 { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime?               TransactTime               { get; internal set; }

		/// <summary>The FIX Spread, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.Spread?                     Spread                     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveCurrency, tag 220, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveCurrency?     BenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveName, tag 221, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveName?         BenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX BenchmarkCurvePoint, tag 222, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurvePoint?        BenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX BenchmarkPrice, tag 662, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPrice?             BenchmarkPrice             { get; internal set; }

		/// <summary>The FIX BenchmarkPriceType, tag 663, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPriceType?         BenchmarkPriceType         { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityID, tag 699, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityID?        BenchmarkSecurityID        { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityIDSource, tag 761, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityIDSource?  BenchmarkSecurityIDSource  { get; internal set; }

		/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PriceType?                  PriceType                  { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price?                      Price                      { get; internal set; }

		/// <summary>The FIX Price2, tag 640, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price2?                     Price2                     { get; internal set; }

		/// <summary>The FIX YieldType, tag 235, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.YieldType?                  YieldType                  { get; internal set; }

		/// <summary>The FIX Yield, tag 236, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.Yield?                      Yield                      { get; internal set; }

		/// <summary>The FIX YieldCalcDate, tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldCalcDate?              YieldCalcDate              { get; internal set; }

		/// <summary>The FIX YieldRedemptionDate, tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionDate?        YieldRedemptionDate        { get; internal set; }

		/// <summary>The FIX YieldRedemptionPrice, tag 697, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPrice?       YieldRedemptionPrice       { get; internal set; }

		/// <summary>The FIX YieldRedemptionPriceType, tag 698, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPriceType?   YieldRedemptionPriceType   { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?                 NoPartyIDs                 { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                       Parties                    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotSetAckGrp, counted by NoQuoteSets, tag 296.</summary>
	public sealed class QuotSetAckGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.QuoteSetID                            QuoteSetID                           { get; init; }

		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbol?                     UnderlyingSymbol                     { get; internal set; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }

		/// <summary>The FIX TotNoQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TotNoQuoteEntries?                    TotNoQuoteEntries                    { get; internal set; }

		/// <summary>The FIX LastFragment, tag 893, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.LastFragment?                         LastFragment                         { get; internal set; }

		/// <summary>The FIX NoQuoteEntries, tag 295; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoQuoteEntries?                       NoQuoteEntries                       { get; internal set; }

		/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
		public          List<QuotEntryAckGrp>?                         QuotEntryAckGrp                      { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 QuotSetGrp, counted by NoQuoteSets, tag 296.</summary>
	public sealed class QuotSetGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.QuoteSetID                            QuoteSetID                           { get; init; }

		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbol?                     UnderlyingSymbol                     { get; internal set; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }

		/// <summary>The FIX QuoteSetValidUntilTime, tag 367, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.QuoteSetValidUntilTime?               QuoteSetValidUntilTime               { get; internal set; }

		/// <summary>The FIX TotNoQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TotNoQuoteEntries?                    TotNoQuoteEntries                    { get; internal set; }

		/// <summary>The FIX LastFragment, tag 893, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.LastFragment?                         LastFragment                         { get; internal set; }

		/// <summary>The FIX NoQuoteEntries, tag 295; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoQuoteEntries?                       NoQuoteEntries                       { get; internal set; }

		/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
		public          List<QuotEntryGrp>?                            QuotEntryGrp                         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 RFQReqGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class RFQReqGrp : IInstrument
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.PrevClosePx?                PrevClosePx                { get; internal set; }

		/// <summary>The FIX QuoteRequestType, tag 303, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteRequestType?           QuoteRequestType           { get; internal set; }

		/// <summary>The FIX QuoteType, tag 537, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QuoteType?                  QuoteType                  { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 RelSymDerivSecGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class RelSymDerivSecGrp : IInstrument, IInstrumentExtension
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX ExpirationCycle, tag 827, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.ExpirationCycle?            ExpirationCycle            { get; internal set; }

		/// <summary>The FIX DeliveryForm, tag 668, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryForm?               DeliveryForm               { get; internal set; }

		/// <summary>The FIX PctAtRisk, tag 869, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.PctAtRisk?                  PctAtRisk                  { get; internal set; }

		/// <summary>The FIX NoInstrAttrib, tag 870; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoInstrAttrib?              NoInstrAttrib              { get; internal set; }

		/// <summary>The entries counted by NoInstrAttrib, tag 870; null when the group is absent.</summary>
		public          List<AttrbGrp>?                      AttrbGrp                   { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegGrp>?                 InstrmtLegGrp              { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                       Text                       { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?             EncodedTextLen             { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?                EncodedText                { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 RgstDistInstGrp, counted by NoDistribInsts, tag 510.</summary>
	public sealed class RgstDistInstGrp
	{
		/// <summary>The FIX DistribPaymentMethod, tag 477, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.DistribPaymentMethod        DistribPaymentMethod       { get; init; }

		/// <summary>The FIX DistribPercentage, tag 512, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.DistribPercentage?          DistribPercentage          { get; internal set; }

		/// <summary>The FIX CashDistribCurr, tag 478, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CashDistribCurr?            CashDistribCurr            { get; internal set; }

		/// <summary>The FIX CashDistribAgentName, tag 498, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CashDistribAgentName?       CashDistribAgentName       { get; internal set; }

		/// <summary>The FIX CashDistribAgentCode, tag 499, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CashDistribAgentCode?       CashDistribAgentCode       { get; internal set; }

		/// <summary>The FIX CashDistribAgentAcctNumber, tag 500, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CashDistribAgentAcctNumber? CashDistribAgentAcctNumber { get; internal set; }

		/// <summary>The FIX CashDistribPayRef, tag 501, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CashDistribPayRef?          CashDistribPayRef          { get; internal set; }

		/// <summary>The FIX CashDistribAgentAcctName, tag 502, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CashDistribAgentAcctName?   CashDistribAgentAcctName   { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 RgstDtlsGrp, counted by NoRegistDtls, tag 473.</summary>
	public sealed class RgstDtlsGrp
	{
		/// <summary>The FIX RegistDtls, tag 509, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.RegistDtls                  RegistDtls                 { get; init; }

		/// <summary>The FIX RegistEmail, tag 511, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RegistEmail?                RegistEmail                { get; internal set; }

		/// <summary>The FIX MailingDtls, tag 474, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MailingDtls?                MailingDtls                { get; internal set; }

		/// <summary>The FIX MailingInst, tag 482, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.MailingInst?                MailingInst                { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?           NoNestedPartyIDs           { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                 NestedParties              { get; internal set; }

		/// <summary>The FIX OwnerType, tag 522, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.OwnerType?                  OwnerType                  { get; internal set; }

		/// <summary>The FIX DateOfBirth, tag 486, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DateOfBirth?                DateOfBirth                { get; internal set; }

		/// <summary>The FIX InvestorCountryOfResidence, tag 475, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.InvestorCountryOfResidence? InvestorCountryOfResidence { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 RoutingGrp, counted by NoRoutingIDs, tag 215.</summary>
	public sealed class RoutingGrp
	{
		/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>; null when the field is absent.</summary>
		public required FixField.RoutingType RoutingType { get; init; }

		/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RoutingID?  RoutingID   { get; internal set; }
	}

	/// <summary>
	/// One alternative identifier of an instrument: an entry of SecAltIDGrp, counted by
	/// NoSecurityAltID, tag 454.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The group says the same instrument by other names — an ISIN beside a Bloomberg ticker beside
	/// an exchange symbol — so an entry is one name and where that name comes from. It sits inside
	/// the <c>Instrument</c> block, which is not a group of its own but part of a message's body,
	/// and so the same entries appear in every message that carries an instrument.
	/// </para>
	/// <para>
	/// <see cref="SecurityAltID"/> opens an entry and is therefore <c>required</c>: the wire marks
	/// no boundary between entries, and the group's first field is what tells one from the next.
	/// That is the protocol's rule about how repeating groups are encoded, and not something the
	/// repository's tables say — there tag 455 is marked optional like the rest.
	/// </para>
	/// </remarks>
	public sealed class SecAltIDGrp
	{
		/// <summary>
		/// The FIX SecurityAltID, tag 455, wire type <c>String</c>: the alternative identifier
		/// itself, and the field that opens the entry, so it is always present.
		/// </summary>
		public required FixField.SecurityAltID        SecurityAltID       { get; init; }

		/// <summary>
		/// The FIX SecurityAltIDSource, tag 456, wire type <c>String</c>: what kind of identifier
		/// <see cref="SecurityAltID"/> is; null when the counterparty did not say.
		/// </summary>
		public          FixField.SecurityAltIDSource? SecurityAltIDSource { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SecListGrp, counted by NoRelatedSym, tag 146.</summary>
	public sealed class SecListGrp : IFinancingDetails, IInstrument, IInstrumentExtension, ISpreadOrBenchmarkCurveData, IYieldData
	{
		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.Symbol                      Symbol                     { get; init; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SymbolSfx?                  SymbolSfx                  { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityID?                 SecurityID                 { get; internal set; }

		/// <summary>The FIX SecurityIDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityIDSource?           SecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoSecurityAltID, tag 454; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSecurityAltID?            NoSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoSecurityAltID, tag 454; null when the group is absent.</summary>
		public          List<SecAltIDGrp>?                   SecAltIDGrp                { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?                    Product                    { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?                    CFICode                    { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?               SecurityType               { get; internal set; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType?            SecuritySubType            { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.MaturityMonthYear?          MaturityMonthYear          { get; internal set; }

		/// <summary>The FIX MaturityDate, tag 541, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.MaturityDate?               MaturityDate               { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PutOrCall?                  PutOrCall                  { get; internal set; }

		/// <summary>The FIX CouponPaymentDate, tag 224, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CouponPaymentDate?          CouponPaymentDate          { get; internal set; }

		/// <summary>The FIX IssueDate, tag 225, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.IssueDate?                  IssueDate                  { get; internal set; }

		/// <summary>The FIX RepoCollateralSecurityType, tag 239, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.RepoCollateralSecurityType? RepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX RepurchaseTerm, tag 226, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseTerm?             RepurchaseTerm             { get; internal set; }

		/// <summary>The FIX RepurchaseRate, tag 227, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.RepurchaseRate?             RepurchaseRate             { get; internal set; }

		/// <summary>The FIX Factor, tag 228, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.Factor?                     Factor                     { get; internal set; }

		/// <summary>The FIX CreditRating, tag 255, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CreditRating?               CreditRating               { get; internal set; }

		/// <summary>The FIX InstrRegistry, tag 543, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.InstrRegistry?              InstrRegistry              { get; internal set; }

		/// <summary>The FIX CountryOfIssue, tag 470, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.CountryOfIssue?             CountryOfIssue             { get; internal set; }

		/// <summary>The FIX StateOrProvinceOfIssue, tag 471, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StateOrProvinceOfIssue?     StateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LocaleOfIssue, tag 472, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LocaleOfIssue?              LocaleOfIssue              { get; internal set; }

		/// <summary>The FIX RedemptionDate, tag 240, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.RedemptionDate?             RedemptionDate             { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.StrikePrice?                StrikePrice                { get; internal set; }

		/// <summary>The FIX StrikeCurrency, tag 947, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.StrikeCurrency?             StrikeCurrency             { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OptAttribute?               OptAttribute               { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.ContractMultiplier?         ContractMultiplier         { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.CouponRate?                 CouponRate                 { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.SecurityExchange?           SecurityExchange           { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Issuer?                     Issuer                     { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuerLen?           EncodedIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedIssuer?              EncodedIssuer              { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityDesc?               SecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDescLen?     EncodedSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedSecurityDesc?        EncodedSecurityDesc        { get; internal set; }

		/// <summary>The FIX Pool, tag 691, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Pool?                       Pool                       { get; internal set; }

		/// <summary>The FIX ContractSettlMonth, tag 667, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.ContractSettlMonth?         ContractSettlMonth         { get; internal set; }

		/// <summary>The FIX CPProgram, tag 875, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CPProgram?                  CPProgram                  { get; internal set; }

		/// <summary>The FIX CPRegType, tag 876, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CPRegType?                  CPRegType                  { get; internal set; }

		/// <summary>The FIX NoEvents, tag 864; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoEvents?                   NoEvents                   { get; internal set; }

		/// <summary>The entries counted by NoEvents, tag 864; null when the group is absent.</summary>
		public          List<EvntGrp>?                       EvntGrp                    { get; internal set; }

		/// <summary>The FIX DatedDate, tag 873, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.DatedDate?                  DatedDate                  { get; internal set; }

		/// <summary>The FIX InterestAccrualDate, tag 874, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.InterestAccrualDate?        InterestAccrualDate        { get; internal set; }

		/// <summary>The FIX DeliveryForm, tag 668, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryForm?               DeliveryForm               { get; internal set; }

		/// <summary>The FIX PctAtRisk, tag 869, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.PctAtRisk?                  PctAtRisk                  { get; internal set; }

		/// <summary>The FIX NoInstrAttrib, tag 870; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoInstrAttrib?              NoInstrAttrib              { get; internal set; }

		/// <summary>The entries counted by NoInstrAttrib, tag 870; null when the group is absent.</summary>
		public          List<AttrbGrp>?                      AttrbGrp                   { get; internal set; }

		/// <summary>The FIX AgreementDesc, tag 913, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementDesc?              AgreementDesc              { get; internal set; }

		/// <summary>The FIX AgreementID, tag 914, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AgreementID?                AgreementID                { get; internal set; }

		/// <summary>The FIX AgreementDate, tag 915, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.AgreementDate?              AgreementDate              { get; internal set; }

		/// <summary>The FIX AgreementCurrency, tag 918, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AgreementCurrency?          AgreementCurrency          { get; internal set; }

		/// <summary>The FIX TerminationType, tag 788, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TerminationType?            TerminationType            { get; internal set; }

		/// <summary>The FIX StartDate, tag 916, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.StartDate?                  StartDate                  { get; internal set; }

		/// <summary>The FIX EndDate, tag 917, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.EndDate?                    EndDate                    { get; internal set; }

		/// <summary>The FIX DeliveryType, tag 919, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.DeliveryType?               DeliveryType               { get; internal set; }

		/// <summary>The FIX MarginRatio, tag 898, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.MarginRatio?                MarginRatio                { get; internal set; }

		/// <summary>The FIX NoUnderlyings, tag 711; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyings?              NoUnderlyings              { get; internal set; }

		/// <summary>The entries counted by NoUnderlyings, tag 711; null when the group is absent.</summary>
		public          List<UndInstrmtGrp>?                 UndInstrmtGrp              { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                   Currency                   { get; internal set; }

		/// <summary>The FIX NoStipulations, tag 232; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoStipulations?             NoStipulations             { get; internal set; }

		/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
		public          List<Stipulations>?                  Stipulations               { get; internal set; }

		/// <summary>The FIX NoLegs, tag 555; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegs?                     NoLegs                     { get; internal set; }

		/// <summary>The entries counted by NoLegs, tag 555; null when the group is absent.</summary>
		public          List<InstrmtLegSecListGrp>?          InstrmtLegSecListGrp       { get; internal set; }

		/// <summary>The FIX Spread, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public          FixField.Spread?                     Spread                     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveCurrency, tag 220, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveCurrency?     BenchmarkCurveCurrency     { get; internal set; }

		/// <summary>The FIX BenchmarkCurveName, tag 221, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurveName?         BenchmarkCurveName         { get; internal set; }

		/// <summary>The FIX BenchmarkCurvePoint, tag 222, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkCurvePoint?        BenchmarkCurvePoint        { get; internal set; }

		/// <summary>The FIX BenchmarkPrice, tag 662, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPrice?             BenchmarkPrice             { get; internal set; }

		/// <summary>The FIX BenchmarkPriceType, tag 663, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkPriceType?         BenchmarkPriceType         { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityID, tag 699, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityID?        BenchmarkSecurityID        { get; internal set; }

		/// <summary>The FIX BenchmarkSecurityIDSource, tag 761, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.BenchmarkSecurityIDSource?  BenchmarkSecurityIDSource  { get; internal set; }

		/// <summary>The FIX YieldType, tag 235, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.YieldType?                  YieldType                  { get; internal set; }

		/// <summary>The FIX Yield, tag 236, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.Yield?                      Yield                      { get; internal set; }

		/// <summary>The FIX YieldCalcDate, tag 701, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldCalcDate?              YieldCalcDate              { get; internal set; }

		/// <summary>The FIX YieldRedemptionDate, tag 696, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionDate?        YieldRedemptionDate        { get; internal set; }

		/// <summary>The FIX YieldRedemptionPrice, tag 697, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPrice?       YieldRedemptionPrice       { get; internal set; }

		/// <summary>The FIX YieldRedemptionPriceType, tag 698, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.YieldRedemptionPriceType?   YieldRedemptionPriceType   { get; internal set; }

		/// <summary>The FIX RoundLot, tag 561, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.RoundLot?                   RoundLot                   { get; internal set; }

		/// <summary>The FIX MinTradeVol, tag 562, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.MinTradeVol?                MinTradeVol                { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?           TradingSessionID           { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?        TradingSessionSubID        { get; internal set; }

		/// <summary>The FIX ExpirationCycle, tag 827, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.ExpirationCycle?            ExpirationCycle            { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                       Text                       { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?             EncodedTextLen             { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?                EncodedText                { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SecTypesGrp, counted by NoSecurityTypes, tag 558.</summary>
	public sealed class SecTypesGrp
	{
		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.SecurityType     SecurityType    { get; init; }

		/// <summary>The FIX SecuritySubType, tag 762, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecuritySubType? SecuritySubType { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?         Product         { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?         CFICode         { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SettlInstGrp, counted by NoSettlInst, tag 778.</summary>
	public sealed class SettlInstGrp : ISettlInstructionsData
	{
		/// <summary>The FIX SettlInstID, tag 162, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.SettlInstID         SettlInstID        { get; init; }

		/// <summary>The FIX SettlInstTransType, tag 163, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlInstTransType? SettlInstTransType { get; internal set; }

		/// <summary>The FIX SettlInstRefID, tag 214, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SettlInstRefID?     SettlInstRefID     { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?         NoPartyIDs         { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?               Parties            { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?               Side               { get; internal set; }

		/// <summary>The FIX Product, tag 460, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.Product?            Product            { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecurityType?       SecurityType       { get; internal set; }

		/// <summary>The FIX CFICode, tag 461, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CFICode?            CFICode            { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.EffectiveTime?      EffectiveTime      { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.ExpireTime?         ExpireTime         { get; internal set; }

		/// <summary>The FIX LastUpdateTime, tag 779, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.LastUpdateTime?     LastUpdateTime     { get; internal set; }

		/// <summary>The FIX SettlDeliveryType, tag 172, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SettlDeliveryType?  SettlDeliveryType  { get; internal set; }

		/// <summary>The FIX StandInstDbType, tag 169, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbType?    StandInstDbType    { get; internal set; }

		/// <summary>The FIX StandInstDbName, tag 170, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbName?    StandInstDbName    { get; internal set; }

		/// <summary>The FIX StandInstDbID, tag 171, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StandInstDbID?      StandInstDbID      { get; internal set; }

		/// <summary>The FIX NoDlvyInst, tag 85; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoDlvyInst?         NoDlvyInst         { get; internal set; }

		/// <summary>The entries counted by NoDlvyInst, tag 85; null when the group is absent.</summary>
		public          List<DlvyInstGrp>?           DlvyInstGrp        { get; internal set; }

		/// <summary>The FIX PaymentMethod, tag 492, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.PaymentMethod?      PaymentMethod      { get; internal set; }

		/// <summary>The FIX PaymentRef, tag 476, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.PaymentRef?         PaymentRef         { get; internal set; }

		/// <summary>The FIX CardHolderName, tag 488, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CardHolderName?     CardHolderName     { get; internal set; }

		/// <summary>The FIX CardNumber, tag 489, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CardNumber?         CardNumber         { get; internal set; }

		/// <summary>The FIX CardStartDate, tag 503, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CardStartDate?      CardStartDate      { get; internal set; }

		/// <summary>The FIX CardExpDate, tag 490, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.CardExpDate?        CardExpDate        { get; internal set; }

		/// <summary>The FIX CardIssNum, tag 491, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.CardIssNum?         CardIssNum         { get; internal set; }

		/// <summary>The FIX PaymentDate, tag 504, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.PaymentDate?        PaymentDate        { get; internal set; }

		/// <summary>The FIX PaymentRemitterID, tag 505, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.PaymentRemitterID?  PaymentRemitterID  { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SettlParties, counted by NoSettlPartyIDs, tag 781.</summary>
	public sealed class SettlParties
	{
		/// <summary>The FIX SettlPartyID, tag 782, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.SettlPartyID        SettlPartyID       { get; init; }

		/// <summary>The FIX SettlPartyIDSource, tag 783, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlPartyIDSource? SettlPartyIDSource { get; internal set; }

		/// <summary>The FIX SettlPartyRole, tag 784, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SettlPartyRole?     SettlPartyRole     { get; internal set; }

		/// <summary>The FIX NoSettlPartySubIDs, tag 801; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoSettlPartySubIDs? NoSettlPartySubIDs { get; internal set; }

		/// <summary>The entries counted by NoSettlPartySubIDs, tag 801; null when the group is absent.</summary>
		public          List<SettlPtysSubGrp>?       SettlPtysSubGrp    { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SettlPtysSubGrp, counted by NoSettlPartySubIDs, tag 801.</summary>
	public sealed class SettlPtysSubGrp
	{
		/// <summary>The FIX SettlPartySubID, tag 785, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.SettlPartySubID      SettlPartySubID     { get; init; }

		/// <summary>The FIX SettlPartySubIDType, tag 786, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SettlPartySubIDType? SettlPartySubIDType { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SideCrossOrdCxlGrp, counted by NoSides, tag 552.</summary>
	public sealed class SideCrossOrdCxlGrp : IOrderQtyData
	{
		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.Side                  Side                 { get; init; }

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrigClOrdID?          OrigClOrdID          { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdID?              ClOrdID              { get; internal set; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?     SecondaryClOrdID     { get; internal set; }

		/// <summary>The FIX ClOrdLinkID, tag 583, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdLinkID?          ClOrdLinkID          { get; internal set; }

		/// <summary>The FIX OrigOrdModTime, tag 586, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.OrigOrdModTime?       OrigOrdModTime       { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?           NoPartyIDs           { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                 Parties              { get; internal set; }

		/// <summary>The FIX TradeOriginationDate, tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeOriginationDate? TradeOriginationDate { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeDate?            TradeDate            { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?             OrderQty             { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CashOrderQty?         CashOrderQty         { get; internal set; }

		/// <summary>The FIX OrderPercent, tag 516, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OrderPercent?         OrderPercent         { get; internal set; }

		/// <summary>The FIX RoundingDirection, tag 468, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.RoundingDirection?    RoundingDirection    { get; internal set; }

		/// <summary>The FIX RoundingModulus, tag 469, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.RoundingModulus?      RoundingModulus      { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ComplianceID?         ComplianceID         { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                 Text                 { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?       EncodedTextLen       { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?          EncodedText          { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 SideCrossOrdModGrp, counted by NoSides, tag 552.</summary>
	public sealed class SideCrossOrdModGrp : ICommissionData, IOrderQtyData
	{
		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.Side                  Side                 { get; init; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdID?              ClOrdID              { get; internal set; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?     SecondaryClOrdID     { get; internal set; }

		/// <summary>The FIX ClOrdLinkID, tag 583, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdLinkID?          ClOrdLinkID          { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?           NoPartyIDs           { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                 Parties              { get; internal set; }

		/// <summary>The FIX TradeOriginationDate, tag 229, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeOriginationDate? TradeOriginationDate { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.TradeDate?            TradeDate            { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?              Account              { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?         AcctIDSource         { get; internal set; }

		/// <summary>The FIX AccountType, tag 581, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AccountType?          AccountType          { get; internal set; }

		/// <summary>The FIX DayBookingInst, tag 589, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.DayBookingInst?       DayBookingInst       { get; internal set; }

		/// <summary>The FIX BookingUnit, tag 590, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.BookingUnit?          BookingUnit          { get; internal set; }

		/// <summary>The FIX PreallocMethod, tag 591, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PreallocMethod?       PreallocMethod       { get; internal set; }

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AllocID?              AllocID              { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoAllocs?             NoAllocs             { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public          List<PreAllocGrp>?             PreAllocGrp          { get; internal set; }

		/// <summary>The FIX QtyType, tag 854, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.QtyType?              QtyType              { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.OrderQty?             OrderQty             { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.CashOrderQty?         CashOrderQty         { get; internal set; }

		/// <summary>The FIX OrderPercent, tag 516, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.OrderPercent?         OrderPercent         { get; internal set; }

		/// <summary>The FIX RoundingDirection, tag 468, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.RoundingDirection?    RoundingDirection    { get; internal set; }

		/// <summary>The FIX RoundingModulus, tag 469, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.RoundingModulus?      RoundingModulus      { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.Commission?           Commission           { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CommType?             CommType             { get; internal set; }

		/// <summary>The FIX CommCurrency, tag 479, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CommCurrency?         CommCurrency         { get; internal set; }

		/// <summary>The FIX FundRenewWaiv, tag 497, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.FundRenewWaiv?        FundRenewWaiv        { get; internal set; }

		/// <summary>The FIX OrderCapacity, tag 528, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrderCapacity?        OrderCapacity        { get; internal set; }

		/// <summary>The FIX OrderRestrictions, tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OrderRestrictions?    OrderRestrictions    { get; internal set; }

		/// <summary>The FIX CustOrderCapacity, tag 582, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CustOrderCapacity?    CustOrderCapacity    { get; internal set; }

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.ForexReq?             ForexReq             { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrency?        SettlCurrency        { get; internal set; }

		/// <summary>The FIX BookingType, tag 775, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.BookingType?          BookingType          { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                 Text                 { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?       EncodedTextLen       { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?          EncodedText          { get; internal set; }

		/// <summary>The FIX PositionEffect, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PositionEffect?       PositionEffect       { get; internal set; }

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CoveredOrUncovered?   CoveredOrUncovered   { get; internal set; }

		/// <summary>The FIX CashMargin, tag 544, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CashMargin?           CashMargin           { get; internal set; }

		/// <summary>The FIX ClearingFeeIndicator, tag 635, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClearingFeeIndicator? ClearingFeeIndicator { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.SolicitedFlag?        SolicitedFlag        { get; internal set; }

		/// <summary>The FIX SideComplianceID, tag 659, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SideComplianceID?     SideComplianceID     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 Stipulations, counted by NoStipulations, tag 232.</summary>
	public sealed class Stipulations
	{
		/// <summary>The FIX StipulationType, tag 233, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.StipulationType   StipulationType  { get; init; }

		/// <summary>The FIX StipulationValue, tag 234, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.StipulationValue? StipulationValue { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdAllocGrp, counted by NoAllocs, tag 78.</summary>
	public sealed class TrdAllocGrp
	{
		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.AllocAccount        AllocAccount       { get; init; }

		/// <summary>The FIX AllocAcctIDSource, tag 661, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AllocAcctIDSource?  AllocAcctIDSource  { get; internal set; }

		/// <summary>The FIX AllocSettlCurrency, tag 736, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.AllocSettlCurrency? AllocSettlCurrency { get; internal set; }

		/// <summary>The FIX IndividualAllocID, tag 467, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.IndividualAllocID?  IndividualAllocID  { get; internal set; }

		/// <summary>The FIX NoNested2PartyIDs, tag 756; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNested2PartyIDs?  NoNested2PartyIDs  { get; internal set; }

		/// <summary>The entries counted by NoNested2PartyIDs, tag 756; null when the group is absent.</summary>
		public          List<NestedParties2>?        NestedParties2     { get; internal set; }

		/// <summary>The FIX AllocQty, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.AllocQty?           AllocQty           { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdCapDtGrp, counted by NoDates, tag 580.</summary>
	public sealed class TrdCapDtGrp
	{
		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public required FixField.TradeDate     TradeDate    { get; init; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransactTime? TransactTime { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdCapRptSideGrp, counted by NoSides, tag 552.</summary>
	public sealed class TrdCapRptSideGrp : ICommissionData
	{
		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public required FixField.Side                       Side                      { get; init; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrderID?                   OrderID                   { get; internal set; }

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryOrderID?          SecondaryOrderID          { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdID?                   ClOrdID                   { get; internal set; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?          SecondaryClOrdID          { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ListID?                    ListID                    { get; internal set; }

		/// <summary>The FIX NoPartyIDs, tag 453; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoPartyIDs?                NoPartyIDs                { get; internal set; }

		/// <summary>The entries counted by NoPartyIDs, tag 453; null when the group is absent.</summary>
		public          List<Parties>?                      Parties                   { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Account?                   Account                   { get; internal set; }

		/// <summary>The FIX AcctIDSource, tag 660, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AcctIDSource?              AcctIDSource              { get; internal set; }

		/// <summary>The FIX AccountType, tag 581, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.AccountType?               AccountType               { get; internal set; }

		/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.ProcessCode?               ProcessCode               { get; internal set; }

		/// <summary>The FIX OddLot, tag 575, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.OddLot?                    OddLot                    { get; internal set; }

		/// <summary>The FIX NoClearingInstructions, tag 576; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoClearingInstructions?    NoClearingInstructions    { get; internal set; }

		/// <summary>The entries counted by NoClearingInstructions, tag 576; null when the group is absent.</summary>
		public          List<ClrInstGrp>?                   ClrInstGrp                { get; internal set; }

		/// <summary>The FIX TradeInputSource, tag 578, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradeInputSource?          TradeInputSource          { get; internal set; }

		/// <summary>The FIX TradeInputDevice, tag 579, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradeInputDevice?          TradeInputDevice          { get; internal set; }

		/// <summary>The FIX OrderInputDevice, tag 821, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.OrderInputDevice?          OrderInputDevice          { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                  Currency                  { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ComplianceID?              ComplianceID              { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public          FixField.SolicitedFlag?             SolicitedFlag             { get; internal set; }

		/// <summary>The FIX OrderCapacity, tag 528, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrderCapacity?             OrderCapacity             { get; internal set; }

		/// <summary>The FIX OrderRestrictions, tag 529, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.OrderRestrictions?         OrderRestrictions         { get; internal set; }

		/// <summary>The FIX CustOrderCapacity, tag 582, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CustOrderCapacity?         CustOrderCapacity         { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.OrdType?                   OrdType                   { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public          FixField.ExecInst?                  ExecInst                  { get; internal set; }

		/// <summary>The FIX TransBkdTime, tag 483, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public          FixField.TransBkdTime?              TransBkdTime              { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionID?          TradingSessionID          { get; internal set; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID?       TradingSessionSubID       { get; internal set; }

		/// <summary>The FIX TimeBracket, tag 943, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TimeBracket?               TimeBracket               { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.Commission?                Commission                { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.CommType?                  CommType                  { get; internal set; }

		/// <summary>The FIX CommCurrency, tag 479, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.CommCurrency?              CommCurrency              { get; internal set; }

		/// <summary>The FIX FundRenewWaiv, tag 497, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.FundRenewWaiv?             FundRenewWaiv             { get; internal set; }

		/// <summary>The FIX GrossTradeAmt, tag 381, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.GrossTradeAmt?             GrossTradeAmt             { get; internal set; }

		/// <summary>The FIX NumDaysInterest, tag 157, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.NumDaysInterest?           NumDaysInterest           { get; internal set; }

		/// <summary>The FIX ExDate, tag 230, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.ExDate?                    ExDate                    { get; internal set; }

		/// <summary>The FIX AccruedInterestRate, tag 158, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.AccruedInterestRate?       AccruedInterestRate       { get; internal set; }

		/// <summary>The FIX AccruedInterestAmt, tag 159, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.AccruedInterestAmt?        AccruedInterestAmt        { get; internal set; }

		/// <summary>The FIX InterestAtMaturity, tag 738, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.InterestAtMaturity?        InterestAtMaturity        { get; internal set; }

		/// <summary>The FIX EndAccruedInterestAmt, tag 920, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.EndAccruedInterestAmt?     EndAccruedInterestAmt     { get; internal set; }

		/// <summary>The FIX StartCash, tag 921, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.StartCash?                 StartCash                 { get; internal set; }

		/// <summary>The FIX EndCash, tag 922, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.EndCash?                   EndCash                   { get; internal set; }

		/// <summary>The FIX Concession, tag 238, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.Concession?                Concession                { get; internal set; }

		/// <summary>The FIX TotalTakedown, tag 237, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.TotalTakedown?             TotalTakedown             { get; internal set; }

		/// <summary>The FIX NetMoney, tag 118, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.NetMoney?                  NetMoney                  { get; internal set; }

		/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrAmt?              SettlCurrAmt              { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrency?             SettlCurrency             { get; internal set; }

		/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrFxRate?           SettlCurrFxRate           { get; internal set; }

		/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.SettlCurrFxRateCalc?       SettlCurrFxRateCalc       { get; internal set; }

		/// <summary>The FIX PositionEffect, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PositionEffect?            PositionEffect            { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                      Text                      { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?            EncodedTextLen            { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?               EncodedText               { get; internal set; }

		/// <summary>The FIX SideMultiLegReportingType, tag 752, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.SideMultiLegReportingType? SideMultiLegReportingType { get; internal set; }

		/// <summary>The FIX NoContAmts, tag 518; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoContAmts?                NoContAmts                { get; internal set; }

		/// <summary>The entries counted by NoContAmts, tag 518; null when the group is absent.</summary>
		public          List<ContAmtGrp>?                   ContAmtGrp                { get; internal set; }

		/// <summary>The FIX NoStipulations, tag 232; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoStipulations?            NoStipulations            { get; internal set; }

		/// <summary>The entries counted by NoStipulations, tag 232; null when the group is absent.</summary>
		public          List<Stipulations>?                 Stipulations              { get; internal set; }

		/// <summary>The FIX NoMiscFees, tag 136; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoMiscFees?                NoMiscFees                { get; internal set; }

		/// <summary>The entries counted by NoMiscFees, tag 136; null when the group is absent.</summary>
		public          List<MiscFeesGrp>?                  MiscFeesGrp               { get; internal set; }

		/// <summary>The FIX ExchangeRule, tag 825, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ExchangeRule?              ExchangeRule              { get; internal set; }

		/// <summary>The FIX TradeAllocIndicator, tag 826, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TradeAllocIndicator?       TradeAllocIndicator       { get; internal set; }

		/// <summary>The FIX PreallocMethod, tag 591, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.PreallocMethod?            PreallocMethod            { get; internal set; }

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.AllocID?                   AllocID                   { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoAllocs?                  NoAllocs                  { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public          List<TrdAllocGrp>?                  TrdAllocGrp               { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdCollGrp, counted by NoTrades, tag 897.</summary>
	public sealed class TrdCollGrp
	{
		/// <summary>The FIX TradeReportID, tag 571, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.TradeReportID           TradeReportID          { get; init; }

		/// <summary>The FIX SecondaryTradeReportID, tag 818, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryTradeReportID? SecondaryTradeReportID { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdInstrmtLegGrp, counted by NoLegs, tag 555.</summary>
	public sealed class TrdInstrmtLegGrp : IInstrumentLeg
	{
		/// <summary>The FIX LegSymbol, tag 600, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.LegSymbol                      LegSymbol                     { get; init; }

		/// <summary>The FIX LegSymbolSfx, tag 601, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSymbolSfx?                  LegSymbolSfx                  { get; internal set; }

		/// <summary>The FIX LegSecurityID, tag 602, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityID?                 LegSecurityID                 { get; internal set; }

		/// <summary>The FIX LegSecurityIDSource, tag 603, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityIDSource?           LegSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoLegSecurityAltID, tag 604; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegSecurityAltID?            NoLegSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoLegSecurityAltID, tag 604; null when the group is absent.</summary>
		public          List<LegSecAltIDGrp>?                   LegSecAltIDGrp                { get; internal set; }

		/// <summary>The FIX LegProduct, tag 607, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegProduct?                    LegProduct                    { get; internal set; }

		/// <summary>The FIX LegCFICode, tag 608, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCFICode?                    LegCFICode                    { get; internal set; }

		/// <summary>The FIX LegSecurityType, tag 609, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityType?               LegSecurityType               { get; internal set; }

		/// <summary>The FIX LegSecuritySubType, tag 764, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecuritySubType?            LegSecuritySubType            { get; internal set; }

		/// <summary>The FIX LegMaturityMonthYear, tag 610, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityMonthYear?          LegMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX LegMaturityDate, tag 611, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegMaturityDate?               LegMaturityDate               { get; internal set; }

		/// <summary>The FIX LegCouponPaymentDate, tag 248, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegCouponPaymentDate?          LegCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX LegIssueDate, tag 249, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegIssueDate?                  LegIssueDate                  { get; internal set; }

		/// <summary>The FIX LegRepoCollateralSecurityType, tag 250, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRepoCollateralSecurityType? LegRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX LegRepurchaseTerm, tag 251, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseTerm?             LegRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX LegRepurchaseRate, tag 252, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegRepurchaseRate?             LegRepurchaseRate             { get; internal set; }

		/// <summary>The FIX LegFactor, tag 253, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegFactor?                     LegFactor                     { get; internal set; }

		/// <summary>The FIX LegCreditRating, tag 257, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegCreditRating?               LegCreditRating               { get; internal set; }

		/// <summary>The FIX LegInstrRegistry, tag 599, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegInstrRegistry?              LegInstrRegistry              { get; internal set; }

		/// <summary>The FIX LegCountryOfIssue, tag 596, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.LegCountryOfIssue?             LegCountryOfIssue             { get; internal set; }

		/// <summary>The FIX LegStateOrProvinceOfIssue, tag 597, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegStateOrProvinceOfIssue?     LegStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX LegLocaleOfIssue, tag 598, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegLocaleOfIssue?              LegLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX LegRedemptionDate, tag 254, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegRedemptionDate?             LegRedemptionDate             { get; internal set; }

		/// <summary>The FIX LegStrikePrice, tag 612, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegStrikePrice?                LegStrikePrice                { get; internal set; }

		/// <summary>The FIX LegStrikeCurrency, tag 942, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegStrikeCurrency?             LegStrikeCurrency             { get; internal set; }

		/// <summary>The FIX LegOptAttribute, tag 613, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegOptAttribute?               LegOptAttribute               { get; internal set; }

		/// <summary>The FIX LegContractMultiplier, tag 614, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegContractMultiplier?         LegContractMultiplier         { get; internal set; }

		/// <summary>The FIX LegCouponRate, tag 615, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.LegCouponRate?                 LegCouponRate                 { get; internal set; }

		/// <summary>The FIX LegSecurityExchange, tag 616, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityExchange?           LegSecurityExchange           { get; internal set; }

		/// <summary>The FIX LegIssuer, tag 617, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegIssuer?                     LegIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedLegIssuerLen, tag 618, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuerLen?           EncodedLegIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedLegIssuer, tag 619, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegIssuer?              EncodedLegIssuer              { get; internal set; }

		/// <summary>The FIX LegSecurityDesc, tag 620, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegSecurityDesc?               LegSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDescLen, tag 621, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDescLen?     EncodedLegSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedLegSecurityDesc, tag 622, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedLegSecurityDesc?        EncodedLegSecurityDesc        { get; internal set; }

		/// <summary>The FIX LegRatioQty, tag 623, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.LegRatioQty?                   LegRatioQty                   { get; internal set; }

		/// <summary>The FIX LegSide, tag 624, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSide?                       LegSide                       { get; internal set; }

		/// <summary>The FIX LegCurrency, tag 556, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.LegCurrency?                   LegCurrency                   { get; internal set; }

		/// <summary>The FIX LegPool, tag 740, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegPool?                       LegPool                       { get; internal set; }

		/// <summary>The FIX LegDatedDate, tag 739, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegDatedDate?                  LegDatedDate                  { get; internal set; }

		/// <summary>The FIX LegContractSettlMonth, tag 955, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.LegContractSettlMonth?         LegContractSettlMonth         { get; internal set; }

		/// <summary>The FIX LegInterestAccrualDate, tag 956, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegInterestAccrualDate?        LegInterestAccrualDate        { get; internal set; }

		/// <summary>The FIX LegQty, tag 687, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.LegQty?                        LegQty                        { get; internal set; }

		/// <summary>The FIX LegSwapType, tag 690, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegSwapType?                   LegSwapType                   { get; internal set; }

		/// <summary>The FIX NoLegStipulations, tag 683; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoLegStipulations?             NoLegStipulations             { get; internal set; }

		/// <summary>The entries counted by NoLegStipulations, tag 683; null when the group is absent.</summary>
		public          List<LegStipulations>?                  LegStipulations               { get; internal set; }

		/// <summary>The FIX LegPositionEffect, tag 564, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegPositionEffect?             LegPositionEffect             { get; internal set; }

		/// <summary>The FIX LegCoveredOrUncovered, tag 565, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.LegCoveredOrUncovered?         LegCoveredOrUncovered         { get; internal set; }

		/// <summary>The FIX NoNestedPartyIDs, tag 539; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoNestedPartyIDs?              NoNestedPartyIDs              { get; internal set; }

		/// <summary>The entries counted by NoNestedPartyIDs, tag 539; null when the group is absent.</summary>
		public          List<NestedParties>?                    NestedParties                 { get; internal set; }

		/// <summary>The FIX LegRefID, tag 654, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.LegRefID?                      LegRefID                      { get; internal set; }

		/// <summary>The FIX LegPrice, tag 566, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegPrice?                      LegPrice                      { get; internal set; }

		/// <summary>The FIX LegSettlType, tag 587, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.LegSettlType?                  LegSettlType                  { get; internal set; }

		/// <summary>The FIX LegSettlDate, tag 588, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.LegSettlDate?                  LegSettlDate                  { get; internal set; }

		/// <summary>The FIX LegLastPx, tag 637, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.LegLastPx?                     LegLastPx                     { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdRegTimestamps, counted by NoTrdRegTimestamps, tag 768.</summary>
	public sealed class TrdRegTimestamps
	{
		/// <summary>The FIX TrdRegTimestamp, tag 769, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public required FixField.TrdRegTimestamp        TrdRegTimestamp       { get; init; }

		/// <summary>The FIX TrdRegTimestampType, tag 770, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.TrdRegTimestampType?   TrdRegTimestampType   { get; internal set; }

		/// <summary>The FIX TrdRegTimestampOrigin, tag 771, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TrdRegTimestampOrigin? TrdRegTimestampOrigin { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 TrdgSesGrp, counted by NoTradingSessions, tag 386.</summary>
	public sealed class TrdgSesGrp
	{
		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.TradingSessionID     TradingSessionID    { get; init; }

		/// <summary>The FIX TradingSessionSubID, tag 625, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.TradingSessionSubID? TradingSessionSubID { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 UndInstrmtCollGrp, counted by NoUnderlyings, tag 711.</summary>
	public sealed class UndInstrmtCollGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingSymbol                      UnderlyingSymbol                     { get; init; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }

		/// <summary>The FIX CollAction, tag 944, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.CollAction?                           CollAction                           { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 UndInstrmtGrp, counted by NoUnderlyings, tag 711.</summary>
	public sealed class UndInstrmtGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingSymbol                      UnderlyingSymbol                     { get; init; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 UndInstrmtStrkPxGrp, counted by NoUnderlyings, tag 711.</summary>
	public sealed class UndInstrmtStrkPxGrp : IUnderlyingInstrument
	{
		/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingSymbol                      UnderlyingSymbol                     { get; init; }

		/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSymbolSfx?                  UnderlyingSymbolSfx                  { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityID?                 UnderlyingSecurityID                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityIDSource?           UnderlyingSecurityIDSource           { get; internal set; }

		/// <summary>The FIX NoUnderlyingSecurityAltID, tag 457; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingSecurityAltID?            NoUnderlyingSecurityAltID            { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingSecurityAltID, tag 457; null when the group is absent.</summary>
		public          List<UndSecAltIDGrp>?                          UndSecAltIDGrp                       { get; internal set; }

		/// <summary>The FIX UnderlyingProduct, tag 462, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingProduct?                    UnderlyingProduct                    { get; internal set; }

		/// <summary>The FIX UnderlyingCFICode, tag 463, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCFICode?                    UnderlyingCFICode                    { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityType?               UnderlyingSecurityType               { get; internal set; }

		/// <summary>The FIX UnderlyingSecuritySubType, tag 763, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecuritySubType?            UnderlyingSecuritySubType            { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityMonthYear?          UnderlyingMaturityMonthYear          { get; internal set; }

		/// <summary>The FIX UnderlyingMaturityDate, tag 542, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingMaturityDate?               UnderlyingMaturityDate               { get; internal set; }

		/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPutOrCall?                  UnderlyingPutOrCall                  { get; internal set; }

		/// <summary>The FIX UnderlyingCouponPaymentDate, tag 241, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponPaymentDate?          UnderlyingCouponPaymentDate          { get; internal set; }

		/// <summary>The FIX UnderlyingIssueDate, tag 242, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssueDate?                  UnderlyingIssueDate                  { get; internal set; }

		/// <summary>The FIX UnderlyingRepoCollateralSecurityType, tag 243, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepoCollateralSecurityType? UnderlyingRepoCollateralSecurityType { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseTerm, tag 244, wire type <c>int</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseTerm?             UnderlyingRepurchaseTerm             { get; internal set; }

		/// <summary>The FIX UnderlyingRepurchaseRate, tag 245, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRepurchaseRate?             UnderlyingRepurchaseRate             { get; internal set; }

		/// <summary>The FIX UnderlyingFactor, tag 246, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingFactor?                     UnderlyingFactor                     { get; internal set; }

		/// <summary>The FIX UnderlyingCreditRating, tag 256, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCreditRating?               UnderlyingCreditRating               { get; internal set; }

		/// <summary>The FIX UnderlyingInstrRegistry, tag 595, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingInstrRegistry?              UnderlyingInstrRegistry              { get; internal set; }

		/// <summary>The FIX UnderlyingCountryOfIssue, tag 592, wire type <c>Country</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCountryOfIssue?             UnderlyingCountryOfIssue             { get; internal set; }

		/// <summary>The FIX UnderlyingStateOrProvinceOfIssue, tag 593, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStateOrProvinceOfIssue?     UnderlyingStateOrProvinceOfIssue     { get; internal set; }

		/// <summary>The FIX UnderlyingLocaleOfIssue, tag 594, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingLocaleOfIssue?              UnderlyingLocaleOfIssue              { get; internal set; }

		/// <summary>The FIX UnderlyingRedemptionDate, tag 247, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingRedemptionDate?             UnderlyingRedemptionDate             { get; internal set; }

		/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikePrice?                UnderlyingStrikePrice                { get; internal set; }

		/// <summary>The FIX UnderlyingStrikeCurrency, tag 941, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStrikeCurrency?             UnderlyingStrikeCurrency             { get; internal set; }

		/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingOptAttribute?               UnderlyingOptAttribute               { get; internal set; }

		/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingContractMultiplier?         UnderlyingContractMultiplier         { get; internal set; }

		/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>Percentage</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCouponRate?                 UnderlyingCouponRate                 { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityExchange?           UnderlyingSecurityExchange           { get; internal set; }

		/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingIssuer?                     UnderlyingIssuer                     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuerLen?           EncodedUnderlyingIssuerLen           { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingIssuer?              EncodedUnderlyingIssuer              { get; internal set; }

		/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityDesc?               UnderlyingSecurityDesc               { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDescLen?     EncodedUnderlyingSecurityDescLen     { get; internal set; }

		/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedUnderlyingSecurityDesc?        EncodedUnderlyingSecurityDesc        { get; internal set; }

		/// <summary>The FIX UnderlyingCPProgram, tag 877, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPProgram?                  UnderlyingCPProgram                  { get; internal set; }

		/// <summary>The FIX UnderlyingCPRegType, tag 878, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCPRegType?                  UnderlyingCPRegType                  { get; internal set; }

		/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrency?                   UnderlyingCurrency                   { get; internal set; }

		/// <summary>The FIX UnderlyingQty, tag 879, wire type <c>Qty</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingQty?                        UnderlyingQty                        { get; internal set; }

		/// <summary>The FIX UnderlyingPx, tag 810, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingPx?                         UnderlyingPx                         { get; internal set; }

		/// <summary>The FIX UnderlyingDirtyPrice, tag 882, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingDirtyPrice?                 UnderlyingDirtyPrice                 { get; internal set; }

		/// <summary>The FIX UnderlyingEndPrice, tag 883, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndPrice?                   UnderlyingEndPrice                   { get; internal set; }

		/// <summary>The FIX UnderlyingStartValue, tag 884, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStartValue?                 UnderlyingStartValue                 { get; internal set; }

		/// <summary>The FIX UnderlyingCurrentValue, tag 885, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingCurrentValue?               UnderlyingCurrentValue               { get; internal set; }

		/// <summary>The FIX UnderlyingEndValue, tag 886, wire type <c>Amt</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingEndValue?                   UnderlyingEndValue                   { get; internal set; }

		/// <summary>The FIX NoUnderlyingStips, tag 887; how many entries of the group it counts follow in this entry.</summary>
		public          FixField.NoUnderlyingStips?                    NoUnderlyingStips                    { get; internal set; }

		/// <summary>The entries counted by NoUnderlyingStips, tag 887; null when the group is absent.</summary>
		public          List<UnderlyingStipulations>?                  UnderlyingStipulations               { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.PrevClosePx?                          PrevClosePx                          { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.ClOrdID?                              ClOrdID                              { get; internal set; }

		/// <summary>The FIX SecondaryClOrdID, tag 526, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.SecondaryClOrdID?                     SecondaryClOrdID                     { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public          FixField.Side?                                 Side                                 { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public          FixField.Price?                                Price                                { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public          FixField.Currency?                             Currency                             { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.Text?                                 Text                                 { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public          FixField.EncodedTextLen?                       EncodedTextLen                       { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public          FixField.EncodedText?                          EncodedText                          { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 UndSecAltIDGrp, counted by NoUnderlyingSecurityAltID, tag 457.</summary>
	public sealed class UndSecAltIDGrp
	{
		/// <summary>The FIX UnderlyingSecurityAltID, tag 458, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingSecurityAltID        UnderlyingSecurityAltID       { get; init; }

		/// <summary>The FIX UnderlyingSecurityAltIDSource, tag 459, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingSecurityAltIDSource? UnderlyingSecurityAltIDSource { get; internal set; }
	}

	/// <summary>One entry of the FIX 4.4 UnderlyingStipulations, counted by NoUnderlyingStips, tag 887.</summary>
	public sealed class UnderlyingStipulations
	{
		/// <summary>The FIX UnderlyingStipType, tag 888, wire type <c>String</c>; null when the field is absent.</summary>
		public required FixField.UnderlyingStipType   UnderlyingStipType  { get; init; }

		/// <summary>The FIX UnderlyingStipValue, tag 889, wire type <c>String</c>; null when the field is absent.</summary>
		public          FixField.UnderlyingStipValue? UnderlyingStipValue { get; internal set; }
	}
}
