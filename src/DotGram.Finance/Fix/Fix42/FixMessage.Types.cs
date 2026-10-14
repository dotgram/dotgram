using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.cs from the FIX 4.2 repository; not edited by hand.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

public abstract partial class FixMessage
{
	/// <summary>FIX 4.2 Advertisement, MsgType 7.</summary>
	public sealed partial class Advertisement : FixMessage
	{
		internal Advertisement(List<FixField> fields) : base("7", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.AdvId: if (AdvId is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AdvId = (FixField.Text)field; break;
					case FixTag.AdvTransType: if (AdvTransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AdvTransType = (FixField.Text)field; break;
					case FixTag.AdvRefID: if (AdvRefID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AdvRefID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.AdvSide: if (AdvSide is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AdvSide = (FixField.Character)field; break;
					case FixTag.Quantity: if (Shares is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Shares = (FixField.Decimal)field; break;
					case FixTag.Price: if (Price is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Price = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.URLLink: if (URLLink is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); URLLink = (FixField.Text)field; break;
					case FixTag.LastMkt: if (LastMkt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX AdvId, tag 2, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvId { get; internal set; }

		/// <summary>The FIX AdvTransType, tag 5, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvTransType { get; internal set; }

		/// <summary>The FIX AdvRefID, tag 3, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvRefID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX AdvSide, tag 4, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? AdvSide { get; internal set; }

		/// <summary>The FIX Shares, tag 53, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? Shares { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink { get; internal set; }

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Advertisement(context, this);
		}
	}

	/// <summary>FIX 4.2 Allocation, MsgType J.</summary>
	public sealed partial class Allocation : FixMessage
	{
		internal Allocation(List<FixField> fields) : base("J", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.AllocID: if (AllocID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocID = (FixField.Text)field; break;
					case FixTag.AllocTransType: if (AllocTransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocTransType = (FixField.Character)field; break;
					case FixTag.RefAllocID: if (RefAllocID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefAllocID = (FixField.Text)field; break;
					case FixTag.AllocLinkID: if (AllocLinkID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocLinkID = (FixField.Text)field; break;
					case FixTag.AllocLinkType: if (AllocLinkType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocLinkType = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (NoOrders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (NoOrdersGroupsStore is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroupsStore ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.OrderID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.SecondaryOrderID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SecondaryOrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SecondaryOrderID = (FixField.Text)field;
						break;
					case FixTag.ListID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ListID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ListID = (FixField.Text)field;
						break;
					case FixTag.WaveNo:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].WaveNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].WaveNo = (FixField.Text)field;
						break;
					case FixTag.NoExecs: if (NoExecs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoExecs = (FixField.Integer)field; break;
					case FixTag.LastQty:
						if (NoExecsGroupsStore is null && NoExecs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoExecs, field.Position, field, -1));
						(NoExecsGroupsStore ??= []).Add(new () { LastShares = (FixField.Decimal)field });
						break;
					case FixTag.ExecID:
						if (NoExecsGroupsStore is null || NoExecsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroupsStore![^1].ExecID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroupsStore!.Count - 1));
						else
							NoExecsGroupsStore![^1].ExecID = (FixField.Text)field;
						break;
					case FixTag.LastPx:
						if (NoExecsGroupsStore is null || NoExecsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroupsStore![^1].LastPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroupsStore!.Count - 1));
						else
							NoExecsGroupsStore![^1].LastPx = (FixField.Decimal)field;
						break;
					case FixTag.LastCapacity:
						if (NoExecsGroupsStore is null || NoExecsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroupsStore![^1].LastCapacity is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroupsStore!.Count - 1));
						else
							NoExecsGroupsStore![^1].LastCapacity = (FixField.Character)field;
						break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Quantity: if (Shares is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Shares = (FixField.Decimal)field; break;
					case FixTag.LastMkt: if (LastMkt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.AvgPx: if (AvgPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AvgPx = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.AvgPxPrecision: if (AvgPrxPrecision is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AvgPrxPrecision = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.SettlType: if (SettlmntTyp is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (FutSettDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate = (FixField.Date)field; break;
					case FixTag.GrossTradeAmt: if (GrossTradeAmt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GrossTradeAmt = (FixField.Decimal)field; break;
					case FixTag.NetMoney: if (NetMoney is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NetMoney = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (OpenClose is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OpenClose = (FixField.Character)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.NumDaysInterest: if (NumDaysInterest is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NumDaysInterest = (FixField.Integer)field; break;
					case FixTag.AccruedInterestRate: if (AccruedInterestRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AccruedInterestRate = (FixField.Decimal)field; break;
					case FixTag.NoAllocs: if (NoAllocs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (NoAllocsGroupsStore is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroupsStore ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocPrice:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocPrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocPrice = (FixField.Decimal)field;
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.ProcessCode:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].ProcessCode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].ProcessCode = (FixField.Character)field;
						break;
					case FixTag.BrokerOfCredit:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].BrokerOfCredit is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].BrokerOfCredit = (FixField.Text)field;
						break;
					case FixTag.NotifyBrokerOfCredit:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].NotifyBrokerOfCredit is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].NotifyBrokerOfCredit = (FixField.Boolean)field;
						break;
					case FixTag.AllocHandlInst:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocHandlInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocHandlInst = (FixField.Integer)field;
						break;
					case FixTag.AllocText:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocText = (FixField.Text)field;
						break;
					case FixTag.EncodedAllocTextLen:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].EncodedAllocTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].EncodedAllocTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedAllocText:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].EncodedAllocText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].EncodedAllocText = (FixField.Data)field;
						break;
					case FixTag.ExecBroker:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].ExecBroker is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].ExecBroker = (FixField.Text)field;
						break;
					case FixTag.ClientID:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].ClientID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].ClientID = (FixField.Text)field;
						break;
					case FixTag.Commission:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].Commission is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].Commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.AllocAvgPx:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocAvgPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocAvgPx = (FixField.Decimal)field;
						break;
					case FixTag.AllocNetMoney:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocNetMoney is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocNetMoney = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrAmt:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].SettlCurrAmt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].SettlCurrAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrency:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].SettlCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].SettlCurrency = (FixField.Text)field;
						break;
					case FixTag.SettlCurrFxRate:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].SettlCurrFxRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].SettlCurrFxRate = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrFxRateCalc:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].SettlCurrFxRateCalc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].SettlCurrFxRateCalc = (FixField.Character)field;
						break;
					case FixTag.AccruedInterestAmt:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AccruedInterestAmt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AccruedInterestAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlInstMode:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].SettlInstMode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].SettlInstMode = (FixField.Character)field;
						break;
					case FixTag.NoMiscFees:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].NoMiscFees is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].NoMiscFees = (FixField.Integer)field;
						break;
					case FixTag.MiscFeeAmt:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore is null && NoAllocsGroupsStore![^1].NoMiscFees is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMiscFees, field.Position, field, -1));
							(NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore ??= []).Add(new () { MiscFeeAmt = (FixField.Decimal)field });
						}
						break;
					case FixTag.MiscFeeCurr:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0 || NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore is null || NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore![^1].MiscFeeCurr is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore![^1].MiscFeeCurr = (FixField.Text)field;
						break;
					case FixTag.MiscFeeType:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0 || NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore is null || NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore![^1].MiscFeeType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].NoMiscFeesGroupsStore![^1].MiscFeeType = (FixField.Character)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID { get; internal set; }

		/// <summary>The FIX AllocTransType, tag 71, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? AllocTransType { get; internal set; }

		/// <summary>The FIX RefAllocID, tag 72, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefAllocID { get; internal set; }

		/// <summary>The FIX AllocLinkID, tag 196, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocLinkID { get; internal set; }

		/// <summary>The FIX AllocLinkType, tag 197, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocLinkType { get; internal set; }

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders { get; internal set; }

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Allocation.NoOrdersGroup>? NoOrdersGroups => NoOrdersGroupsStore;
		internal List<FixMessage.Allocation.NoOrdersGroup>? NoOrdersGroupsStore;

		/// <summary>The FIX NoExecs, tag 124, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoExecs { get; internal set; }

		/// <summary>The entries counted by NoExecs, tag 124; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Allocation.NoExecsGroup>? NoExecsGroups => NoExecsGroupsStore;
		internal List<FixMessage.Allocation.NoExecsGroup>? NoExecsGroupsStore;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Shares, tag 53, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? Shares { get; internal set; }

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? AvgPx { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX AvgPrxPrecision, tag 74, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AvgPrxPrecision { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp { get; internal set; }

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate { get; internal set; }

		/// <summary>The FIX GrossTradeAmt, tag 381, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? GrossTradeAmt { get; internal set; }

		/// <summary>The FIX NetMoney, tag 118, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? NetMoney { get; internal set; }

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX NumDaysInterest, tag 157, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumDaysInterest { get; internal set; }

		/// <summary>The FIX AccruedInterestRate, tag 158, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? AccruedInterestRate { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Allocation.NoAllocsGroup>? NoAllocsGroups => NoAllocsGroupsStore;
		internal List<FixMessage.Allocation.NoAllocsGroup>? NoAllocsGroupsStore;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup
		{
			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text ClOrdID { get; init; }

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID { get; internal set; }

			/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecondaryOrderID { get; internal set; }

			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ListID { get; internal set; }

			/// <summary>The FIX WaveNo, tag 105, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? WaveNo { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoExecs, tag 124.</summary>
		public sealed class NoExecsGroup
		{
			/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
			public required FixField.Decimal LastShares { get; init; }

			/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecID { get; internal set; }

			/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? LastPx { get; internal set; }

			/// <summary>The FIX LastCapacity, tag 29, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? LastCapacity { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup
		{
			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text AllocAccount { get; init; }

			/// <summary>The FIX AllocPrice, tag 366, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocPrice { get; internal set; }

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares { get; internal set; }

			/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? ProcessCode { get; internal set; }

			/// <summary>The FIX BrokerOfCredit, tag 92, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? BrokerOfCredit { get; internal set; }

			/// <summary>The FIX NotifyBrokerOfCredit, tag 208, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? NotifyBrokerOfCredit { get; internal set; }

			/// <summary>The FIX AllocHandlInst, tag 209, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? AllocHandlInst { get; internal set; }

			/// <summary>The FIX AllocText, tag 161, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? AllocText { get; internal set; }

			/// <summary>The FIX EncodedAllocTextLen, tag 360, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedAllocTextLen { get; internal set; }

			/// <summary>The FIX EncodedAllocText, tag 361, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedAllocText { get; internal set; }

			/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecBroker { get; internal set; }

			/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClientID { get; internal set; }

			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? Commission { get; internal set; }

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType { get; internal set; }

			/// <summary>The FIX AllocAvgPx, tag 153, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocAvgPx { get; internal set; }

			/// <summary>The FIX AllocNetMoney, tag 154, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocNetMoney { get; internal set; }

			/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? SettlCurrAmt { get; internal set; }

			/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? SettlCurrency { get; internal set; }

			/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? SettlCurrFxRate { get; internal set; }

			/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlCurrFxRateCalc { get; internal set; }

			/// <summary>The FIX AccruedInterestAmt, tag 159, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? AccruedInterestAmt { get; internal set; }

			/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlInstMode { get; internal set; }

			/// <summary>The FIX NoMiscFees, tag 136, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoMiscFees { get; internal set; }

			/// <summary>The entries counted by NoMiscFees, tag 136; null when the group is absent.</summary>
			public IReadOnlyList<FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup>? NoMiscFeesGroups => NoMiscFeesGroupsStore;
			internal List<FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup>? NoMiscFeesGroupsStore;

			/// <summary>One entry of the group counted by NoMiscFees, tag 136.</summary>
			public sealed class NoMiscFeesGroup
			{
				/// <summary>The FIX MiscFeeAmt, tag 137, wire type <c>Amt</c>; null when the field is absent.</summary>
				public required FixField.Decimal MiscFeeAmt { get; init; }

				/// <summary>The FIX MiscFeeCurr, tag 138, wire type <c>Currency</c>; null when the field is absent.</summary>
				public FixField.Text? MiscFeeCurr { get; internal set; }

				/// <summary>The FIX MiscFeeType, tag 139, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? MiscFeeType { get; internal set; }
			}
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Allocation(context, this);
		}
	}

	/// <summary>FIX 4.2 AllocationACK, MsgType P.</summary>
	public sealed partial class AllocationACK : FixMessage
	{
		internal AllocationACK(List<FixField> fields) : base("P", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.AllocID: if (AllocID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocID = (FixField.Text)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.AllocStatus: if (AllocStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocStatus = (FixField.Integer)field; break;
					case FixTag.AllocRejCode: if (AllocRejCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocRejCode = (FixField.Integer)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX AllocStatus, tag 87, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocStatus { get; internal set; }

		/// <summary>The FIX AllocRejCode, tag 88, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocRejCode { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.AllocationACK(context, this);
		}
	}

	/// <summary>FIX 4.2 BidRequest, MsgType k.</summary>
	public sealed partial class BidRequest : FixMessage
	{
		internal BidRequest(List<FixField> fields) : base("k", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.BidID: if (BidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (ClientBidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientBidID = (FixField.Text)field; break;
					case FixTag.BidRequestTransType: if (BidRequestTransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidRequestTransType = (FixField.Character)field; break;
					case FixTag.ListName: if (ListName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListName = (FixField.Text)field; break;
					case FixTag.TotNoRelatedSym: if (TotalNumSecurities is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotalNumSecurities = (FixField.Integer)field; break;
					case FixTag.BidType: if (BidType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidType = (FixField.Integer)field; break;
					case FixTag.NumTickets: if (NumTickets is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NumTickets = (FixField.Integer)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.SideValue1: if (SideValue1 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SideValue1 = (FixField.Decimal)field; break;
					case FixTag.SideValue2: if (SideValue2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SideValue2 = (FixField.Decimal)field; break;
					case FixTag.NoBidDescriptors: if (NoBidDescriptors is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoBidDescriptors = (FixField.Integer)field; break;
					case FixTag.BidDescriptorType:
						if (NoBidDescriptorsGroupsStore is null && NoBidDescriptors is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidDescriptors, field.Position, field, -1));
						(NoBidDescriptorsGroupsStore ??= []).Add(new () { BidDescriptorType = (FixField.Integer)field });
						break;
					case FixTag.BidDescriptor:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].BidDescriptor is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].BidDescriptor = (FixField.Text)field;
						break;
					case FixTag.SideValueInd:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].SideValueInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].SideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LiquidityValue:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].LiquidityValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].LiquidityValue = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityNumSecurities:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].LiquidityNumSecurities is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].LiquidityNumSecurities = (FixField.Integer)field;
						break;
					case FixTag.LiquidityPctLow:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].LiquidityPctLow is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].LiquidityPctLow = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityPctHigh:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].LiquidityPctHigh is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].LiquidityPctHigh = (FixField.Decimal)field;
						break;
					case FixTag.EFPTrackingError:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].EFPTrackingError is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].EFPTrackingError = (FixField.Decimal)field;
						break;
					case FixTag.FairValue:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].FairValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].FairValue = (FixField.Decimal)field;
						break;
					case FixTag.OutsideIndexPct:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].OutsideIndexPct is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].OutsideIndexPct = (FixField.Decimal)field;
						break;
					case FixTag.ValueOfFutures:
						if (NoBidDescriptorsGroupsStore is null || NoBidDescriptorsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroupsStore![^1].ValueOfFutures is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroupsStore!.Count - 1));
						else
							NoBidDescriptorsGroupsStore![^1].ValueOfFutures = (FixField.Decimal)field;
						break;
					case FixTag.NoBidComponents: if (NoBidComponents is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoBidComponents = (FixField.Integer)field; break;
					case FixTag.ListID:
						if (NoBidComponentsGroupsStore is null && NoBidComponents is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidComponents, field.Position, field, -1));
						(NoBidComponentsGroupsStore ??= []).Add(new () { ListID = (FixField.Text)field });
						break;
					case FixTag.Side:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.TradingSessionID:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.NetGrossInd:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].NetGrossInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].NetGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.Account:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Account is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Account = (FixField.Text)field;
						break;
					case FixTag.LiquidityIndType: if (LiquidityIndType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LiquidityIndType = (FixField.Integer)field; break;
					case FixTag.WtAverageLiquidity: if (WtAverageLiquidity is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); WtAverageLiquidity = (FixField.Decimal)field; break;
					case FixTag.ExchangeForPhysical: if (ExchangeForPhysical is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExchangeForPhysical = (FixField.Boolean)field; break;
					case FixTag.OutMainCntryUIndex: if (OutMainCntryUIndex is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OutMainCntryUIndex = (FixField.Decimal)field; break;
					case FixTag.CrossPercent: if (CrossPercent is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CrossPercent = (FixField.Decimal)field; break;
					case FixTag.ProgRptReqs: if (ProgRptReqs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ProgRptReqs = (FixField.Integer)field; break;
					case FixTag.ProgPeriodInterval: if (ProgPeriodInterval is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ProgPeriodInterval = (FixField.Integer)field; break;
					case FixTag.IncTaxInd: if (IncTaxInd is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IncTaxInd = (FixField.Integer)field; break;
					case FixTag.ForexReq: if (ForexReq is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ForexReq = (FixField.Boolean)field; break;
					case FixTag.NumBidders: if (NumBidders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NumBidders = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.BidTradeType: if (TradeType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeType = (FixField.Character)field; break;
					case FixTag.BasisPxType: if (BasisPxType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BasisPxType = (FixField.Character)field; break;
					case FixTag.StrikeTime: if (StrikeTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikeTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID { get; internal set; }

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID { get; internal set; }

		/// <summary>The FIX BidRequestTransType, tag 374, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? BidRequestTransType { get; internal set; }

		/// <summary>The FIX ListName, tag 392, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListName { get; internal set; }

		/// <summary>The FIX TotalNumSecurities, tag 393, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotalNumSecurities { get; internal set; }

		/// <summary>The FIX BidType, tag 394, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BidType { get; internal set; }

		/// <summary>The FIX NumTickets, tag 395, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumTickets { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX SideValue1, tag 396, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SideValue1 { get; internal set; }

		/// <summary>The FIX SideValue2, tag 397, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SideValue2 { get; internal set; }

		/// <summary>The FIX NoBidDescriptors, tag 398, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidDescriptors { get; internal set; }

		/// <summary>The entries counted by NoBidDescriptors, tag 398; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.BidRequest.NoBidDescriptorsGroup>? NoBidDescriptorsGroups => NoBidDescriptorsGroupsStore;
		internal List<FixMessage.BidRequest.NoBidDescriptorsGroup>? NoBidDescriptorsGroupsStore;

		/// <summary>The FIX NoBidComponents, tag 420, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidComponents { get; internal set; }

		/// <summary>The entries counted by NoBidComponents, tag 420; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.BidRequest.NoBidComponentsGroup>? NoBidComponentsGroups => NoBidComponentsGroupsStore;
		internal List<FixMessage.BidRequest.NoBidComponentsGroup>? NoBidComponentsGroupsStore;

		/// <summary>The FIX LiquidityIndType, tag 409, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LiquidityIndType { get; internal set; }

		/// <summary>The FIX WtAverageLiquidity, tag 410, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? WtAverageLiquidity { get; internal set; }

		/// <summary>The FIX ExchangeForPhysical, tag 411, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ExchangeForPhysical { get; internal set; }

		/// <summary>The FIX OutMainCntryUIndex, tag 412, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? OutMainCntryUIndex { get; internal set; }

		/// <summary>The FIX CrossPercent, tag 413, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CrossPercent { get; internal set; }

		/// <summary>The FIX ProgRptReqs, tag 414, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgRptReqs { get; internal set; }

		/// <summary>The FIX ProgPeriodInterval, tag 415, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgPeriodInterval { get; internal set; }

		/// <summary>The FIX IncTaxInd, tag 416, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? IncTaxInd { get; internal set; }

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq { get; internal set; }

		/// <summary>The FIX NumBidders, tag 417, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumBidders { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX TradeType, tag 418, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TradeType { get; internal set; }

		/// <summary>The FIX BasisPxType, tag 419, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? BasisPxType { get; internal set; }

		/// <summary>The FIX StrikeTime, tag 443, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? StrikeTime { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>One entry of the group counted by NoBidDescriptors, tag 398.</summary>
		public sealed class NoBidDescriptorsGroup
		{
			/// <summary>The FIX BidDescriptorType, tag 399, wire type <c>int</c>; null when the field is absent.</summary>
			public required FixField.Integer BidDescriptorType { get; init; }

			/// <summary>The FIX BidDescriptor, tag 400, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? BidDescriptor { get; internal set; }

			/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SideValueInd { get; internal set; }

			/// <summary>The FIX LiquidityValue, tag 404, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityValue { get; internal set; }

			/// <summary>The FIX LiquidityNumSecurities, tag 441, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? LiquidityNumSecurities { get; internal set; }

			/// <summary>The FIX LiquidityPctLow, tag 402, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityPctLow { get; internal set; }

			/// <summary>The FIX LiquidityPctHigh, tag 403, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityPctHigh { get; internal set; }

			/// <summary>The FIX EFPTrackingError, tag 405, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? EFPTrackingError { get; internal set; }

			/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? FairValue { get; internal set; }

			/// <summary>The FIX OutsideIndexPct, tag 407, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? OutsideIndexPct { get; internal set; }

			/// <summary>The FIX ValueOfFutures, tag 408, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? ValueOfFutures { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoBidComponents, tag 420.</summary>
		public sealed class NoBidComponentsGroup
		{
			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text ListID { get; init; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }

			/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NetGrossInd { get; internal set; }

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp { get; internal set; }

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate { get; internal set; }

			/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Account { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.BidRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 BidResponse, MsgType l.</summary>
	public sealed partial class BidResponse : FixMessage
	{
		internal BidResponse(List<FixField> fields) : base("l", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.BidID: if (BidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (ClientBidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientBidID = (FixField.Text)field; break;
					case FixTag.NoBidComponents: if (NoBidComponents is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoBidComponents = (FixField.Integer)field; break;
					case FixTag.Commission:
						if (NoBidComponentsGroupsStore is null && NoBidComponents is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidComponents, field.Position, field, -1));
						(NoBidComponentsGroupsStore ??= []).Add(new () { Commission = (FixField.Decimal)field });
						break;
					case FixTag.CommType:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.ListID:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].ListID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].ListID = (FixField.Text)field;
						break;
					case FixTag.Country:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Country is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Country = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.PriceType:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].PriceType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].PriceType = (FixField.Integer)field;
						break;
					case FixTag.FairValue:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].FairValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].FairValue = (FixField.Decimal)field;
						break;
					case FixTag.NetGrossInd:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].NetGrossInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].NetGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.TradingSessionID:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoBidComponentsGroupsStore is null || NoBidComponentsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroupsStore!.Count - 1));
						else
							NoBidComponentsGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID { get; internal set; }

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID { get; internal set; }

		/// <summary>The FIX NoBidComponents, tag 420, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidComponents { get; internal set; }

		/// <summary>The entries counted by NoBidComponents, tag 420; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.BidResponse.NoBidComponentsGroup>? NoBidComponentsGroups => NoBidComponentsGroupsStore;
		internal List<FixMessage.BidResponse.NoBidComponentsGroup>? NoBidComponentsGroupsStore;

		/// <summary>One entry of the group counted by NoBidComponents, tag 420.</summary>
		public sealed class NoBidComponentsGroup
		{
			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
			public required FixField.Decimal Commission { get; init; }

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType { get; internal set; }

			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ListID { get; internal set; }

			/// <summary>The FIX Country, tag 421, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Country { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price { get; internal set; }

			/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PriceType { get; internal set; }

			/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? FairValue { get; internal set; }

			/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NetGrossInd { get; internal set; }

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp { get; internal set; }

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.BidResponse(context, this);
		}
	}

	/// <summary>FIX 4.2 BusinessMessageReject, MsgType j.</summary>
	public sealed partial class BusinessMessageReject : FixMessage
	{
		internal BusinessMessageReject(List<FixField> fields) : base("j", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.RefSeqNum: if (RefSeqNum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefSeqNum = (FixField.Integer)field; break;
					case FixTag.RefMsgType: if (RefMsgType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefMsgType = (FixField.Text)field; break;
					case FixTag.BusinessRejectRefID: if (BusinessRejectRefID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BusinessRejectRefID = (FixField.Text)field; break;
					case FixTag.BusinessRejectReason: if (BusinessRejectReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BusinessRejectReason = (FixField.Integer)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX RefSeqNum, tag 45, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefSeqNum { get; internal set; }

		/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefMsgType { get; internal set; }

		/// <summary>The FIX BusinessRejectRefID, tag 379, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BusinessRejectRefID { get; internal set; }

		/// <summary>The FIX BusinessRejectReason, tag 380, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BusinessRejectReason { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.BusinessMessageReject(context, this);
		}
	}

	/// <summary>FIX 4.2 DontKnowTrade, MsgType Q.</summary>
	public sealed partial class DontKnowTrade : FixMessage
	{
		internal DontKnowTrade(List<FixField> fields) : base("Q", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ExecID: if (ExecID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecID = (FixField.Text)field; break;
					case FixTag.DKReason: if (DKReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DKReason = (FixField.Character)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.OrderQty: if (OrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (CashOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashOrderQty = (FixField.Decimal)field; break;
					case FixTag.LastQty: if (LastShares is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastShares = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (LastPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastPx = (FixField.Decimal)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecID { get; internal set; }

		/// <summary>The FIX DKReason, tag 127, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DKReason { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty { get; internal set; }

		/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastShares { get; internal set; }

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.DontKnowTrade(context, this);
		}
	}

	/// <summary>FIX 4.2 Email, MsgType C.</summary>
	public sealed partial class Email : FixMessage
	{
		internal Email(List<FixField> fields) : base("C", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.EmailThreadID: if (EmailThreadID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EmailThreadID = (FixField.Text)field; break;
					case FixTag.EmailType: if (EmailType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EmailType = (FixField.Character)field; break;
					case FixTag.OrigTime: if (OrigTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigTime = (FixField.Timestamp)field; break;
					case FixTag.Subject: if (Subject is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Subject = (FixField.Text)field; break;
					case FixTag.EncodedSubjectLen: if (EncodedSubjectLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSubjectLen = (FixField.Integer)field; break;
					case FixTag.EncodedSubject: if (EncodedSubject is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSubject = (FixField.Data)field; break;
					case FixTag.NoRoutingIDs: if (NoRoutingIDs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (NoRoutingIDsGroupsStore is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroupsStore ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroupsStore is null || NoRoutingIDsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroupsStore![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroupsStore!.Count - 1));
						else
							NoRoutingIDsGroupsStore![^1].RoutingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { RelatdSym = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.NoLinesOfText: if (LinesOfText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LinesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (LinesOfTextGroupsStore is null && LinesOfText is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoLinesOfText, field.Position, field, -1));
						(LinesOfTextGroupsStore ??= []).Add(new () { Text = (FixField.Text)field });
						break;
					case FixTag.EncodedTextLen:
						if (LinesOfTextGroupsStore is null || LinesOfTextGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroupsStore!.Count - 1));
						else
							LinesOfTextGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (LinesOfTextGroupsStore is null || LinesOfTextGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroupsStore!.Count - 1));
						else
							LinesOfTextGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;
					case FixTag.RawDataLength: if (RawDataLength is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (RawData is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawData = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX EmailThreadID, tag 164, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? EmailThreadID { get; internal set; }

		/// <summary>The FIX EmailType, tag 94, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? EmailType { get; internal set; }

		/// <summary>The FIX OrigTime, tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? OrigTime { get; internal set; }

		/// <summary>The FIX Subject, tag 147, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Subject { get; internal set; }

		/// <summary>The FIX EncodedSubjectLen, tag 356, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSubjectLen { get; internal set; }

		/// <summary>The FIX EncodedSubject, tag 357, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSubject { get; internal set; }

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs { get; internal set; }

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Email.NoRoutingIDsGroup>? NoRoutingIDsGroups => NoRoutingIDsGroupsStore;
		internal List<FixMessage.Email.NoRoutingIDsGroup>? NoRoutingIDsGroupsStore;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Email.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.Email.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText { get; internal set; }

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Email.LinesOfTextGroup>? LinesOfTextGroups => LinesOfTextGroupsStore;
		internal List<FixMessage.Email.LinesOfTextGroup>? LinesOfTextGroupsStore;

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength { get; internal set; }

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData { get; internal set; }

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup
		{
			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>; null when the field is absent.</summary>
			public required FixField.Integer RoutingType { get; init; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX RelatdSym, tag 46, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text RelatdSym { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }
		}

		/// <summary>One entry of the group counted by LinesOfText, tag 33.</summary>
		public sealed class LinesOfTextGroup
		{
			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Text { get; init; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Email(context, this);
		}
	}

	/// <summary>FIX 4.2 ExecutionReport, MsgType 8.</summary>
	public sealed partial class ExecutionReport : FixMessage
	{
		internal ExecutionReport(List<FixField> fields) : base("8", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.SecondaryOrderID: if (SecondaryOrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecondaryOrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (OrigClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigClOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.NoContraBrokers: if (NoContraBrokers is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoContraBrokers = (FixField.Integer)field; break;
					case FixTag.ContraBroker:
						if (NoContraBrokersGroupsStore is null && NoContraBrokers is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoContraBrokers, field.Position, field, -1));
						(NoContraBrokersGroupsStore ??= []).Add(new () { ContraBroker = (FixField.Text)field });
						break;
					case FixTag.ContraTrader:
						if (NoContraBrokersGroupsStore is null || NoContraBrokersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroupsStore![^1].ContraTrader is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroupsStore!.Count - 1));
						else
							NoContraBrokersGroupsStore![^1].ContraTrader = (FixField.Text)field;
						break;
					case FixTag.ContraTradeQty:
						if (NoContraBrokersGroupsStore is null || NoContraBrokersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroupsStore![^1].ContraTradeQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroupsStore!.Count - 1));
						else
							NoContraBrokersGroupsStore![^1].ContraTradeQty = (FixField.Decimal)field;
						break;
					case FixTag.ContraTradeTime:
						if (NoContraBrokersGroupsStore is null || NoContraBrokersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroupsStore![^1].ContraTradeTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroupsStore!.Count - 1));
						else
							NoContraBrokersGroupsStore![^1].ContraTradeTime = (FixField.Timestamp)field;
						break;
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.ExecID: if (ExecID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecID = (FixField.Text)field; break;
					case FixTag.ExecTransType: if (ExecTransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecTransType = (FixField.Character)field; break;
					case FixTag.ExecRefID: if (ExecRefID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecRefID = (FixField.Text)field; break;
					case FixTag.ExecType: if (ExecType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecType = (FixField.Character)field; break;
					case FixTag.OrdStatus: if (OrdStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdStatus = (FixField.Character)field; break;
					case FixTag.OrdRejReason: if (OrdRejReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdRejReason = (FixField.Integer)field; break;
					case FixTag.ExecRestatementReason: if (ExecRestatementReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecRestatementReason = (FixField.Integer)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.SettlType: if (SettlmntTyp is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (FutSettDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate = (FixField.Date)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.OrderQty: if (OrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (CashOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (OrdType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdType = (FixField.Character)field; break;
					case FixTag.Price: if (Price is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (StopPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StopPx = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (PegDifference is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (DiscretionInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (DiscretionOffset is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionOffset = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.ComplianceID: if (ComplianceID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ComplianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (SolicitedFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SolicitedFlag = (FixField.Boolean)field; break;
					case FixTag.TimeInForce: if (TimeInForce is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TimeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (EffectiveTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EffectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (ExpireDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (ExpireTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireTime = (FixField.Timestamp)field; break;
					case FixTag.ExecInst: if (ExecInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecInst = (FixField.Multiple)field; break;
					case FixTag.Rule80A: if (Rule80A is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Rule80A = (FixField.Character)field; break;
					case FixTag.LastQty: if (LastShares is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastShares = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (LastPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastPx = (FixField.Decimal)field; break;
					case FixTag.LastSpotRate: if (LastSpotRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastSpotRate = (FixField.Decimal)field; break;
					case FixTag.LastForwardPoints: if (LastForwardPoints is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastForwardPoints = (FixField.Decimal)field; break;
					case FixTag.LastMkt: if (LastMkt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.LastCapacity: if (LastCapacity is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastCapacity = (FixField.Character)field; break;
					case FixTag.LeavesQty: if (LeavesQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LeavesQty = (FixField.Decimal)field; break;
					case FixTag.CumQty: if (CumQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CumQty = (FixField.Decimal)field; break;
					case FixTag.AvgPx: if (AvgPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AvgPx = (FixField.Decimal)field; break;
					case FixTag.DayOrderQty: if (DayOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DayOrderQty = (FixField.Decimal)field; break;
					case FixTag.DayCumQty: if (DayCumQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DayCumQty = (FixField.Decimal)field; break;
					case FixTag.DayAvgPx: if (DayAvgPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DayAvgPx = (FixField.Decimal)field; break;
					case FixTag.GTBookingInst: if (GTBookingInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GTBookingInst = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.ReportToExch: if (ReportToExch is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ReportToExch = (FixField.Boolean)field; break;
					case FixTag.Commission: if (Commission is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (CommType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CommType = (FixField.Character)field; break;
					case FixTag.GrossTradeAmt: if (GrossTradeAmt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GrossTradeAmt = (FixField.Decimal)field; break;
					case FixTag.SettlCurrAmt: if (SettlCurrAmt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrAmt = (FixField.Decimal)field; break;
					case FixTag.SettlCurrency: if (SettlCurrency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrency = (FixField.Text)field; break;
					case FixTag.SettlCurrFxRate: if (SettlCurrFxRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrFxRate = (FixField.Decimal)field; break;
					case FixTag.SettlCurrFxRateCalc: if (SettlCurrFxRateCalc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrFxRateCalc = (FixField.Character)field; break;
					case FixTag.HandlInst: if (HandlInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HandlInst = (FixField.Character)field; break;
					case FixTag.MinQty: if (MinQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MinQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (MaxFloor is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxFloor = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (OpenClose is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OpenClose = (FixField.Character)field; break;
					case FixTag.MaxShow: if (MaxShow is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxShow = (FixField.Decimal)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (FutSettDate2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (OrderQty2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty2 = (FixField.Decimal)field; break;
					case FixTag.ClearingFirm: if (ClearingFirm is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (ClearingAccount is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingAccount = (FixField.Text)field; break;
					case FixTag.MultiLegReportingType: if (MultiLegReportingType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MultiLegReportingType = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecondaryOrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX NoContraBrokers, tag 382, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoContraBrokers { get; internal set; }

		/// <summary>The entries counted by NoContraBrokers, tag 382; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.ExecutionReport.NoContraBrokersGroup>? NoContraBrokersGroups => NoContraBrokersGroupsStore;
		internal List<FixMessage.ExecutionReport.NoContraBrokersGroup>? NoContraBrokersGroupsStore;

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecID { get; internal set; }

		/// <summary>The FIX ExecTransType, tag 20, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ExecTransType { get; internal set; }

		/// <summary>The FIX ExecRefID, tag 19, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecRefID { get; internal set; }

		/// <summary>The FIX ExecType, tag 150, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ExecType { get; internal set; }

		/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdStatus { get; internal set; }

		/// <summary>The FIX OrdRejReason, tag 103, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? OrdRejReason { get; internal set; }

		/// <summary>The FIX ExecRestatementReason, tag 378, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ExecRestatementReason { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp { get; internal set; }

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price { get; internal set; }

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx { get; internal set; }

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference { get; internal set; }

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst { get; internal set; }

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst { get; internal set; }

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A { get; internal set; }

		/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastShares { get; internal set; }

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx { get; internal set; }

		/// <summary>The FIX LastSpotRate, tag 194, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastSpotRate { get; internal set; }

		/// <summary>The FIX LastForwardPoints, tag 195, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastForwardPoints { get; internal set; }

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX LastCapacity, tag 29, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? LastCapacity { get; internal set; }

		/// <summary>The FIX LeavesQty, tag 151, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LeavesQty { get; internal set; }

		/// <summary>The FIX CumQty, tag 14, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CumQty { get; internal set; }

		/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? AvgPx { get; internal set; }

		/// <summary>The FIX DayOrderQty, tag 424, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayOrderQty { get; internal set; }

		/// <summary>The FIX DayCumQty, tag 425, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayCumQty { get; internal set; }

		/// <summary>The FIX DayAvgPx, tag 426, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayAvgPx { get; internal set; }

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX ReportToExch, tag 113, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ReportToExch { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType { get; internal set; }

		/// <summary>The FIX GrossTradeAmt, tag 381, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? GrossTradeAmt { get; internal set; }

		/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SettlCurrAmt { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency { get; internal set; }

		/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? SettlCurrFxRate { get; internal set; }

		/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlCurrFxRateCalc { get; internal set; }

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty { get; internal set; }

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor { get; internal set; }

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose { get; internal set; }

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 { get; internal set; }

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm { get; internal set; }

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount { get; internal set; }

		/// <summary>The FIX MultiLegReportingType, tag 442, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? MultiLegReportingType { get; internal set; }

		/// <summary>One entry of the group counted by NoContraBrokers, tag 382.</summary>
		public sealed class NoContraBrokersGroup
		{
			/// <summary>The FIX ContraBroker, tag 375, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text ContraBroker { get; init; }

			/// <summary>The FIX ContraTrader, tag 337, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ContraTrader { get; internal set; }

			/// <summary>The FIX ContraTradeQty, tag 437, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContraTradeQty { get; internal set; }

			/// <summary>The FIX ContraTradeTime, tag 438, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ContraTradeTime { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ExecutionReport(context, this);
		}
	}

	/// <summary>FIX 4.2 Heartbeat, MsgType 0.</summary>
	public sealed partial class Heartbeat : FixMessage
	{
		internal Heartbeat(List<FixField> fields) : base("0", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.TestReqID: if (TestReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TestReqID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX TestReqID, tag 112, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TestReqID { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Heartbeat(context, this);
		}
	}

	/// <summary>FIX 4.2 IndicationofInterest, MsgType 6.</summary>
	public sealed partial class IndicationofInterest : FixMessage
	{
		internal IndicationofInterest(List<FixField> fields) : base("6", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.IOIID: if (IOIid is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOIid = (FixField.Text)field; break;
					case FixTag.IOITransType: if (IOITransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOITransType = (FixField.Character)field; break;
					case FixTag.IOIRefID: if (IOIRefID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOIRefID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.IOIQty: if (IOIShares is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOIShares = (FixField.Text)field; break;
					case FixTag.Price: if (Price is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Price = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.ValidUntilTime: if (ValidUntilTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ValidUntilTime = (FixField.Timestamp)field; break;
					case FixTag.IOIQltyInd: if (IOIQltyInd is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOIQltyInd = (FixField.Character)field; break;
					case FixTag.IOINaturalFlag: if (IOINaturalFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOINaturalFlag = (FixField.Boolean)field; break;
					case FixTag.NoIOIQualifiers: if (NoIOIQualifiers is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoIOIQualifiers = (FixField.Integer)field; break;
					case FixTag.IOIQualifier:
						if (NoIOIQualifiersGroupsStore is null && NoIOIQualifiers is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoIOIQualifiers, field.Position, field, -1));
						(NoIOIQualifiersGroupsStore ??= []).Add(new () { IOIQualifier = (FixField.Character)field });
						break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.URLLink: if (URLLink is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); URLLink = (FixField.Text)field; break;
					case FixTag.NoRoutingIDs: if (NoRoutingIDs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (NoRoutingIDsGroupsStore is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroupsStore ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroupsStore is null || NoRoutingIDsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroupsStore![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroupsStore!.Count - 1));
						else
							NoRoutingIDsGroupsStore![^1].RoutingID = (FixField.Text)field;
						break;
					case FixTag.Spread: if (SpreadToBenchmark is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SpreadToBenchmark = (FixField.Decimal)field; break;
					case FixTag.Benchmark: if (Benchmark is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Benchmark = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIid { get; internal set; }

		/// <summary>The FIX IOITransType, tag 28, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? IOITransType { get; internal set; }

		/// <summary>The FIX IOIRefID, tag 26, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIRefID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX IOIShares, tag 27, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIShares { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ValidUntilTime { get; internal set; }

		/// <summary>The FIX IOIQltyInd, tag 25, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? IOIQltyInd { get; internal set; }

		/// <summary>The FIX IOINaturalFlag, tag 130, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? IOINaturalFlag { get; internal set; }

		/// <summary>The FIX NoIOIQualifiers, tag 199, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoIOIQualifiers { get; internal set; }

		/// <summary>The entries counted by NoIOIQualifiers, tag 199; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.IndicationofInterest.NoIOIQualifiersGroup>? NoIOIQualifiersGroups => NoIOIQualifiersGroupsStore;
		internal List<FixMessage.IndicationofInterest.NoIOIQualifiersGroup>? NoIOIQualifiersGroupsStore;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink { get; internal set; }

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs { get; internal set; }

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.IndicationofInterest.NoRoutingIDsGroup>? NoRoutingIDsGroups => NoRoutingIDsGroupsStore;
		internal List<FixMessage.IndicationofInterest.NoRoutingIDsGroup>? NoRoutingIDsGroupsStore;

		/// <summary>The FIX SpreadToBenchmark, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? SpreadToBenchmark { get; internal set; }

		/// <summary>The FIX Benchmark, tag 219, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Benchmark { get; internal set; }

		/// <summary>One entry of the group counted by NoIOIQualifiers, tag 199.</summary>
		public sealed class NoIOIQualifiersGroup
		{
			/// <summary>The FIX IOIQualifier, tag 104, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character IOIQualifier { get; init; }
		}

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup
		{
			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>; null when the field is absent.</summary>
			public required FixField.Integer RoutingType { get; init; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.IndicationofInterest(context, this);
		}
	}

	/// <summary>FIX 4.2 ListCancelRequest, MsgType K.</summary>
	public sealed partial class ListCancelRequest : FixMessage
	{
		internal ListCancelRequest(List<FixField> fields) : base("K", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ListCancelRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 ListExecute, MsgType L.</summary>
	public sealed partial class ListExecute : FixMessage
	{
		internal ListExecute(List<FixField> fields) : base("L", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (ClientBidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientBidID = (FixField.Text)field; break;
					case FixTag.BidID: if (BidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidID = (FixField.Text)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID { get; internal set; }

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ListExecute(context, this);
		}
	}

	/// <summary>FIX 4.2 ListStatus, MsgType N.</summary>
	public sealed partial class ListStatus : FixMessage
	{
		internal ListStatus(List<FixField> fields) : base("N", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.ListStatusType: if (ListStatusType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListStatusType = (FixField.Integer)field; break;
					case FixTag.NoRpts: if (NoRpts is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRpts = (FixField.Integer)field; break;
					case FixTag.ListOrderStatus: if (ListOrderStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListOrderStatus = (FixField.Integer)field; break;
					case FixTag.RptSeq: if (RptSeq is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RptSeq = (FixField.Integer)field; break;
					case FixTag.ListStatusText: if (ListStatusText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListStatusText = (FixField.Text)field; break;
					case FixTag.EncodedListStatusTextLen: if (EncodedListStatusTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedListStatusTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedListStatusText: if (EncodedListStatusText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedListStatusText = (FixField.Data)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.TotNoOrders: if (TotNoOrders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotNoOrders = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (NoOrders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (NoOrdersGroupsStore is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroupsStore ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.CumQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CumQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CumQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdStatus:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrdStatus is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrdStatus = (FixField.Character)field;
						break;
					case FixTag.LeavesQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].LeavesQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].LeavesQty = (FixField.Decimal)field;
						break;
					case FixTag.CxlQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CxlQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CxlQty = (FixField.Decimal)field;
						break;
					case FixTag.AvgPx:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].AvgPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].AvgPx = (FixField.Decimal)field;
						break;
					case FixTag.OrdRejReason:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrdRejReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrdRejReason = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX ListStatusType, tag 429, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ListStatusType { get; internal set; }

		/// <summary>The FIX NoRpts, tag 82, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRpts { get; internal set; }

		/// <summary>The FIX ListOrderStatus, tag 431, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ListOrderStatus { get; internal set; }

		/// <summary>The FIX RptSeq, tag 83, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RptSeq { get; internal set; }

		/// <summary>The FIX ListStatusText, tag 444, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListStatusText { get; internal set; }

		/// <summary>The FIX EncodedListStatusTextLen, tag 445, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedListStatusTextLen { get; internal set; }

		/// <summary>The FIX EncodedListStatusText, tag 446, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedListStatusText { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX TotNoOrders, tag 68, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoOrders { get; internal set; }

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders { get; internal set; }

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.ListStatus.NoOrdersGroup>? NoOrdersGroups => NoOrdersGroupsStore;
		internal List<FixMessage.ListStatus.NoOrdersGroup>? NoOrdersGroupsStore;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup
		{
			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text ClOrdID { get; init; }

			/// <summary>The FIX CumQty, tag 14, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CumQty { get; internal set; }

			/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdStatus { get; internal set; }

			/// <summary>The FIX LeavesQty, tag 151, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? LeavesQty { get; internal set; }

			/// <summary>The FIX CxlQty, tag 84, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CxlQty { get; internal set; }

			/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AvgPx { get; internal set; }

			/// <summary>The FIX OrdRejReason, tag 103, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? OrdRejReason { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ListStatus(context, this);
		}
	}

	/// <summary>FIX 4.2 ListStatusRequest, MsgType M.</summary>
	public sealed partial class ListStatusRequest : FixMessage
	{
		internal ListStatusRequest(List<FixField> fields) : base("M", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ListStatusRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 ListStrikePrice, MsgType m.</summary>
	public sealed partial class ListStrikePrice : FixMessage
	{
		internal ListStrikePrice(List<FixField> fields) : base("m", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.TotNoStrikes: if (TotNoStrikes is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotNoStrikes = (FixField.Integer)field; break;
					case FixTag.NoStrikes: if (NoStrikes is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoStrikes = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (NoStrikesGroupsStore is null && NoStrikes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoStrikes, field.Position, field, -1));
						(NoStrikesGroupsStore ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.ClOrdID:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].ClOrdID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].ClOrdID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoStrikesGroupsStore is null || NoStrikesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroupsStore!.Count - 1));
						else
							NoStrikesGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX TotNoStrikes, tag 422, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoStrikes { get; internal set; }

		/// <summary>The FIX NoStrikes, tag 428, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoStrikes { get; internal set; }

		/// <summary>The entries counted by NoStrikes, tag 428; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.ListStrikePrice.NoStrikesGroup>? NoStrikesGroups => NoStrikesGroupsStore;
		internal List<FixMessage.ListStrikePrice.NoStrikesGroup>? NoStrikesGroupsStore;

		/// <summary>One entry of the group counted by NoStrikes, tag 428.</summary>
		public sealed class NoStrikesGroup
		{
			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Symbol { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx { get; internal set; }

			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClOrdID { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price { get; internal set; }

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ListStrikePrice(context, this);
		}
	}

	/// <summary>FIX 4.2 Logon, MsgType A.</summary>
	public sealed partial class Logon : FixMessage
	{
		internal Logon(List<FixField> fields) : base("A", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.EncryptMethod: if (EncryptMethod is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncryptMethod = (FixField.Integer)field; break;
					case FixTag.HeartBtInt: if (HeartBtInt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HeartBtInt = (FixField.Integer)field; break;
					case FixTag.RawDataLength: if (RawDataLength is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (RawData is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawData = (FixField.Data)field; break;
					case FixTag.ResetSeqNumFlag: if (ResetSeqNumFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ResetSeqNumFlag = (FixField.Boolean)field; break;
					case FixTag.MaxMessageSize: if (MaxMessageSize is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxMessageSize = (FixField.Integer)field; break;
					case FixTag.NoMsgTypes: if (NoMsgTypes is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoMsgTypes = (FixField.Integer)field; break;
					case FixTag.RefMsgType:
						if (NoMsgTypesGroupsStore is null && NoMsgTypes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMsgTypes, field.Position, field, -1));
						(NoMsgTypesGroupsStore ??= []).Add(new () { RefMsgType = (FixField.Text)field });
						break;
					case FixTag.MsgDirection:
						if (NoMsgTypesGroupsStore is null || NoMsgTypesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMsgTypesGroupsStore![^1].MsgDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMsgTypesGroupsStore!.Count - 1));
						else
							NoMsgTypesGroupsStore![^1].MsgDirection = (FixField.Character)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX EncryptMethod, tag 98, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? EncryptMethod { get; internal set; }

		/// <summary>The FIX HeartBtInt, tag 108, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? HeartBtInt { get; internal set; }

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength { get; internal set; }

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData { get; internal set; }

		/// <summary>The FIX ResetSeqNumFlag, tag 141, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ResetSeqNumFlag { get; internal set; }

		/// <summary>The FIX MaxMessageSize, tag 383, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MaxMessageSize { get; internal set; }

		/// <summary>The FIX NoMsgTypes, tag 384, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMsgTypes { get; internal set; }

		/// <summary>The entries counted by NoMsgTypes, tag 384; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.Logon.NoMsgTypesGroup>? NoMsgTypesGroups => NoMsgTypesGroupsStore;
		internal List<FixMessage.Logon.NoMsgTypesGroup>? NoMsgTypesGroupsStore;

		/// <summary>One entry of the group counted by NoMsgTypes, tag 384.</summary>
		public sealed class NoMsgTypesGroup
		{
			/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text RefMsgType { get; init; }

			/// <summary>The FIX MsgDirection, tag 385, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? MsgDirection { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Logon(context, this);
		}
	}

	/// <summary>FIX 4.2 Logout, MsgType 5.</summary>
	public sealed partial class Logout : FixMessage
	{
		internal Logout(List<FixField> fields) : base("5", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Logout(context, this);
		}
	}

	/// <summary>FIX 4.2 MarketDataIncrementalRefresh, MsgType X.</summary>
	public sealed partial class MarketDataIncrementalRefresh : FixMessage
	{
		internal MarketDataIncrementalRefresh(List<FixField> fields) : base("X", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.MDReqID: if (MDReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDReqID = (FixField.Text)field; break;
					case FixTag.NoMDEntries: if (NoMDEntries is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoMDEntries = (FixField.Integer)field; break;
					case FixTag.MDUpdateAction:
						if (NoMDEntriesGroupsStore is null && NoMDEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntries, field.Position, field, -1));
						(NoMDEntriesGroupsStore ??= []).Add(new () { MDUpdateAction = (FixField.Character)field });
						break;
					case FixTag.DeleteReason:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].DeleteReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].DeleteReason = (FixField.Character)field;
						break;
					case FixTag.MDEntryType:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryType = (FixField.Character)field;
						break;
					case FixTag.MDEntryID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryRefID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryRefID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryRefID = (FixField.Text)field;
						break;
					case FixTag.Symbol:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.FinancialStatus:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].FinancialStatus is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].FinancialStatus = (FixField.Character)field;
						break;
					case FixTag.CorporateAction:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].CorporateAction is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].CorporateAction = (FixField.Character)field;
						break;
					case FixTag.MDEntryPx:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntrySize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TickDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDMkt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].QuoteCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].QuoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TradeCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryOriginator is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].LocationID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].LocationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].DeskID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].DeskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].OpenCloseSettleFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].OpenCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SellerDays is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].QuoteEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].QuoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryBuyer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntrySeller is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].NumberOfOrders is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].NumberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryPositionNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.TotalVolumeTraded:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TotalVolumeTraded is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TotalVolumeTraded = (FixField.Decimal)field;
						break;
					case FixTag.Text:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID { get; internal set; }

		/// <summary>The FIX NoMDEntries, tag 268, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntries { get; internal set; }

		/// <summary>The entries counted by NoMDEntries, tag 268; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup>? NoMDEntriesGroups => NoMDEntriesGroupsStore;
		internal List<FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup>? NoMDEntriesGroupsStore;

		/// <summary>One entry of the group counted by NoMDEntries, tag 268.</summary>
		public sealed class NoMDEntriesGroup
		{
			/// <summary>The FIX MDUpdateAction, tag 279, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character MDUpdateAction { get; init; }

			/// <summary>The FIX DeleteReason, tag 285, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? DeleteReason { get; internal set; }

			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? MDEntryType { get; internal set; }

			/// <summary>The FIX MDEntryID, tag 278, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryID { get; internal set; }

			/// <summary>The FIX MDEntryRefID, tag 280, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryRefID { get; internal set; }

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Symbol { get; internal set; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? FinancialStatus { get; internal set; }

			/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CorporateAction { get; internal set; }

			/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntryPx { get; internal set; }

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency { get; internal set; }

			/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntrySize { get; internal set; }

			/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDate</c>; null when the field is absent.</summary>
			public FixField.Date? MDEntryDate { get; internal set; }

			/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
			public FixField.Time? MDEntryTime { get; internal set; }

			/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TickDirection { get; internal set; }

			/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? MDMkt { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }

			/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? QuoteCondition { get; internal set; }

			/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? TradeCondition { get; internal set; }

			/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryOriginator { get; internal set; }

			/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? LocationID { get; internal set; }

			/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? DeskID { get; internal set; }

			/// <summary>The FIX OpenCloseSettleFlag, tag 286, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenCloseSettleFlag { get; internal set; }

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce { get; internal set; }

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate { get; internal set; }

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime { get; internal set; }

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty { get; internal set; }

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst { get; internal set; }

			/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SellerDays { get; internal set; }

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID { get; internal set; }

			/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteEntryID { get; internal set; }

			/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryBuyer { get; internal set; }

			/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntrySeller { get; internal set; }

			/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NumberOfOrders { get; internal set; }

			/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MDEntryPositionNo { get; internal set; }

			/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? TotalVolumeTraded { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.MarketDataIncrementalRefresh(context, this);
		}
	}

	/// <summary>FIX 4.2 MarketDataRequest, MsgType V.</summary>
	public sealed partial class MarketDataRequest : FixMessage
	{
		internal MarketDataRequest(List<FixField> fields) : base("V", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.MDReqID: if (MDReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDReqID = (FixField.Text)field; break;
					case FixTag.SubscriptionRequestType: if (SubscriptionRequestType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SubscriptionRequestType = (FixField.Character)field; break;
					case FixTag.MarketDepth: if (MarketDepth is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MarketDepth = (FixField.Integer)field; break;
					case FixTag.MDUpdateType: if (MDUpdateType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDUpdateType = (FixField.Integer)field; break;
					case FixTag.AggregatedBook: if (AggregatedBook is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AggregatedBook = (FixField.Boolean)field; break;
					case FixTag.NoMDEntryTypes: if (NoMDEntryTypes is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoMDEntryTypes = (FixField.Integer)field; break;
					case FixTag.MDEntryType:
						if (NoMDEntryTypesGroupsStore is null && NoMDEntryTypes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntryTypes, field.Position, field, -1));
						(NoMDEntryTypesGroupsStore ??= []).Add(new () { MDEntryType = (FixField.Character)field });
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TradingSessionID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID { get; internal set; }

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType { get; internal set; }

		/// <summary>The FIX MarketDepth, tag 264, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MarketDepth { get; internal set; }

		/// <summary>The FIX MDUpdateType, tag 265, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MDUpdateType { get; internal set; }

		/// <summary>The FIX AggregatedBook, tag 266, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? AggregatedBook { get; internal set; }

		/// <summary>The FIX NoMDEntryTypes, tag 267, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntryTypes { get; internal set; }

		/// <summary>The entries counted by NoMDEntryTypes, tag 267; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.MarketDataRequest.NoMDEntryTypesGroup>? NoMDEntryTypesGroups => NoMDEntryTypesGroupsStore;
		internal List<FixMessage.MarketDataRequest.NoMDEntryTypesGroup>? NoMDEntryTypesGroupsStore;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.MarketDataRequest.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.MarketDataRequest.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>One entry of the group counted by NoMDEntryTypes, tag 267.</summary>
		public sealed class NoMDEntryTypesGroup
		{
			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character MDEntryType { get; init; }
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Symbol { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.MarketDataRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 MarketDataRequestReject, MsgType Y.</summary>
	public sealed partial class MarketDataRequestReject : FixMessage
	{
		internal MarketDataRequestReject(List<FixField> fields) : base("Y", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.MDReqID: if (MDReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDReqID = (FixField.Text)field; break;
					case FixTag.MDReqRejReason: if (MDReqRejReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDReqRejReason = (FixField.Character)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID { get; internal set; }

		/// <summary>The FIX MDReqRejReason, tag 281, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? MDReqRejReason { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.MarketDataRequestReject(context, this);
		}
	}

	/// <summary>FIX 4.2 MarketDataSnapshotFullRefresh, MsgType W.</summary>
	public sealed partial class MarketDataSnapshotFullRefresh : FixMessage
	{
		internal MarketDataSnapshotFullRefresh(List<FixField> fields) : base("W", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.MDReqID: if (MDReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MDReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.FinancialStatus: if (FinancialStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FinancialStatus = (FixField.Character)field; break;
					case FixTag.CorporateAction: if (CorporateAction is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CorporateAction = (FixField.Character)field; break;
					case FixTag.TotalVolumeTraded: if (TotalVolumeTraded is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotalVolumeTraded = (FixField.Decimal)field; break;
					case FixTag.NoMDEntries: if (NoMDEntries is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoMDEntries = (FixField.Integer)field; break;
					case FixTag.MDEntryType:
						if (NoMDEntriesGroupsStore is null && NoMDEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntries, field.Position, field, -1));
						(NoMDEntriesGroupsStore ??= []).Add(new () { MDEntryType = (FixField.Character)field });
						break;
					case FixTag.MDEntryPx:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntrySize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TickDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDMkt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].QuoteCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].QuoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TradeCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryOriginator is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].LocationID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].LocationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].DeskID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].DeskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].OpenCloseSettleFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].OpenCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].SellerDays is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].SellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].QuoteEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].QuoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryBuyer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntrySeller is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].NumberOfOrders is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].NumberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].MDEntryPositionNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].MDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoMDEntriesGroupsStore is null || NoMDEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroupsStore!.Count - 1));
						else
							NoMDEntriesGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? FinancialStatus { get; internal set; }

		/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CorporateAction { get; internal set; }

		/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? TotalVolumeTraded { get; internal set; }

		/// <summary>The FIX NoMDEntries, tag 268, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntries { get; internal set; }

		/// <summary>The entries counted by NoMDEntries, tag 268; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>? NoMDEntriesGroups => NoMDEntriesGroupsStore;
		internal List<FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>? NoMDEntriesGroupsStore;

		/// <summary>One entry of the group counted by NoMDEntries, tag 268.</summary>
		public sealed class NoMDEntriesGroup
		{
			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
			public required FixField.Character MDEntryType { get; init; }

			/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntryPx { get; internal set; }

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency { get; internal set; }

			/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntrySize { get; internal set; }

			/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDate</c>; null when the field is absent.</summary>
			public FixField.Date? MDEntryDate { get; internal set; }

			/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
			public FixField.Time? MDEntryTime { get; internal set; }

			/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TickDirection { get; internal set; }

			/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? MDMkt { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }

			/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? QuoteCondition { get; internal set; }

			/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? TradeCondition { get; internal set; }

			/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryOriginator { get; internal set; }

			/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? LocationID { get; internal set; }

			/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? DeskID { get; internal set; }

			/// <summary>The FIX OpenCloseSettleFlag, tag 286, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenCloseSettleFlag { get; internal set; }

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce { get; internal set; }

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate { get; internal set; }

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime { get; internal set; }

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty { get; internal set; }

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst { get; internal set; }

			/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SellerDays { get; internal set; }

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID { get; internal set; }

			/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteEntryID { get; internal set; }

			/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryBuyer { get; internal set; }

			/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntrySeller { get; internal set; }

			/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NumberOfOrders { get; internal set; }

			/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MDEntryPositionNo { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.MarketDataSnapshotFullRefresh(context, this);
		}
	}

	/// <summary>FIX 4.2 MassQuote, MsgType i.</summary>
	public sealed partial class MassQuote : FixMessage
	{
		internal MassQuote(List<FixField> fields) : base("i", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteReqID: if (QuoteReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.QuoteResponseLevel: if (QuoteResponseLevel is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.DefBidSize: if (DefBidSize is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DefBidSize = (FixField.Decimal)field; break;
					case FixTag.DefOfferSize: if (DefOfferSize is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DefOfferSize = (FixField.Decimal)field; break;
					case FixTag.NoQuoteSets: if (NoQuoteSets is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoQuoteSets = (FixField.Integer)field; break;
					case FixTag.QuoteSetID:
						if (NoQuoteSetsGroupsStore is null && NoQuoteSets is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteSets, field.Position, field, -1));
						(NoQuoteSetsGroupsStore ??= []).Add(new () { QuoteSetID = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteSetValidUntilTime:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].QuoteSetValidUntilTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].QuoteSetValidUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].TotQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].TotQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null && NoQuoteSetsGroupsStore![^1].NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
							(NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore ??= []).Add(new () { QuoteEntryID = (FixField.Text)field });
						}
						break;
					case FixTag.Symbol:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.BidPx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidPx = (FixField.Decimal)field;
						break;
					case FixTag.OfferPx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferPx = (FixField.Decimal)field;
						break;
					case FixTag.BidSize:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidSize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidSize = (FixField.Decimal)field;
						break;
					case FixTag.OfferSize:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferSize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferSize = (FixField.Decimal)field;
						break;
					case FixTag.ValidUntilTime:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ValidUntilTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ValidUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.BidSpotRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidSpotRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.OfferSpotRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferSpotRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.BidForwardPoints:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidForwardPoints is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].BidForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.OfferForwardPoints:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferForwardPoints is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OfferForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.TransactTime:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.TradingSessionID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.SettlDate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Currency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel { get; internal set; }

		/// <summary>The FIX DefBidSize, tag 293, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DefBidSize { get; internal set; }

		/// <summary>The FIX DefOfferSize, tag 294, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DefOfferSize { get; internal set; }

		/// <summary>The FIX NoQuoteSets, tag 296, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteSets { get; internal set; }

		/// <summary>The entries counted by NoQuoteSets, tag 296; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.MassQuote.NoQuoteSetsGroup>? NoQuoteSetsGroups => NoQuoteSetsGroupsStore;
		internal List<FixMessage.MassQuote.NoQuoteSetsGroup>? NoQuoteSetsGroupsStore;

		/// <summary>One entry of the group counted by NoQuoteSets, tag 296.</summary>
		public sealed class NoQuoteSetsGroup
		{
			/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text QuoteSetID { get; init; }

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol { get; internal set; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID { get; internal set; }

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay { get; internal set; }

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall { get; internal set; }

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice { get; internal set; }

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute { get; internal set; }

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier { get; internal set; }

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange { get; internal set; }

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX QuoteSetValidUntilTime, tag 367, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? QuoteSetValidUntilTime { get; internal set; }

			/// <summary>The FIX TotQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? TotQuoteEntries { get; internal set; }

			/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoQuoteEntries { get; internal set; }

			/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
			public IReadOnlyList<FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => NoQuoteEntriesGroupsStore;
			internal List<FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroupsStore;

			/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
			public sealed class NoQuoteEntriesGroup
			{
				/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
				public required FixField.Text QuoteEntryID { get; init; }

				/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Symbol { get; internal set; }

				/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SymbolSfx { get; internal set; }

				/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityID { get; internal set; }

				/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? IDSource { get; internal set; }

				/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityType { get; internal set; }

				/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
				public FixField.MonthYear? MaturityMonthYear { get; internal set; }

				/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
				public FixField.Integer? MaturityDay { get; internal set; }

				/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? PutOrCall { get; internal set; }

				/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? StrikePrice { get; internal set; }

				/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OptAttribute { get; internal set; }

				/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? ContractMultiplier { get; internal set; }

				/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? CouponRate { get; internal set; }

				/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityExchange { get; internal set; }

				/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Issuer { get; internal set; }

				/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedIssuerLen { get; internal set; }

				/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedIssuer { get; internal set; }

				/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityDesc { get; internal set; }

				/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

				/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedSecurityDesc { get; internal set; }

				/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidPx { get; internal set; }

				/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferPx { get; internal set; }

				/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidSize { get; internal set; }

				/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferSize { get; internal set; }

				/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
				public FixField.Timestamp? ValidUntilTime { get; internal set; }

				/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidSpotRate { get; internal set; }

				/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferSpotRate { get; internal set; }

				/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidForwardPoints { get; internal set; }

				/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferForwardPoints { get; internal set; }

				/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
				public FixField.Timestamp? TransactTime { get; internal set; }

				/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? TradingSessionID { get; internal set; }

				/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
				public FixField.Date? FutSettDate { get; internal set; }

				/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OrdType { get; internal set; }

				/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
				public FixField.Date? FutSettDate2 { get; internal set; }

				/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? OrderQty2 { get; internal set; }

				/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
				public FixField.Text? Currency { get; internal set; }
			}
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.MassQuote(context, this);
		}
	}

	/// <summary>FIX 4.2 NewOrderList, MsgType E.</summary>
	public sealed partial class NewOrderList : FixMessage
	{
		internal NewOrderList(List<FixField> fields) : base("E", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.BidID: if (BidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (ClientBidID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientBidID = (FixField.Text)field; break;
					case FixTag.ProgRptReqs: if (ProgRptReqs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ProgRptReqs = (FixField.Integer)field; break;
					case FixTag.BidType: if (BidType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidType = (FixField.Integer)field; break;
					case FixTag.ProgPeriodInterval: if (ProgPeriodInterval is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ProgPeriodInterval = (FixField.Integer)field; break;
					case FixTag.ListExecInstType: if (ListExecInstType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListExecInstType = (FixField.Character)field; break;
					case FixTag.ListExecInst: if (ListExecInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListExecInst = (FixField.Text)field; break;
					case FixTag.EncodedListExecInstLen: if (EncodedListExecInstLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedListExecInstLen = (FixField.Integer)field; break;
					case FixTag.EncodedListExecInst: if (EncodedListExecInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedListExecInst = (FixField.Data)field; break;
					case FixTag.TotNoOrders: if (TotNoOrders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotNoOrders = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (NoOrders is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (NoOrdersGroupsStore is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroupsStore ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.ListSeqNo:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ListSeqNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ListSeqNo = (FixField.Integer)field;
						break;
					case FixTag.SettlInstMode:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SettlInstMode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SettlInstMode = (FixField.Character)field;
						break;
					case FixTag.ClientID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ClientID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ClientID = (FixField.Text)field;
						break;
					case FixTag.ExecBroker:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ExecBroker is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ExecBroker = (FixField.Text)field;
						break;
					case FixTag.Account:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Account is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Account = (FixField.Text)field;
						break;
					case FixTag.NoAllocs:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].NoAllocs is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].NoAllocs = (FixField.Integer)field;
						break;
					case FixTag.AllocAccount:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoOrdersGroupsStore![^1].NoAllocsGroupsStore is null && NoOrdersGroupsStore![^1].NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
							(NoOrdersGroupsStore![^1].NoAllocsGroupsStore ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						}
						break;
					case FixTag.AllocQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0 || NoOrdersGroupsStore![^1].NoAllocsGroupsStore is null || NoOrdersGroupsStore![^1].NoAllocsGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].NoAllocsGroupsStore![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore![^1].NoAllocsGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].NoAllocsGroupsStore![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.HandlInst:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].HandlInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].HandlInst = (FixField.Character)field;
						break;
					case FixTag.ExecInst:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.MinQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.MaxFloor:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].MaxFloor is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].MaxFloor = (FixField.Decimal)field;
						break;
					case FixTag.ExDestination:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ExDestination is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ExDestination = (FixField.Text)field;
						break;
					case FixTag.NoTradingSessions:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].NoTradingSessions is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].NoTradingSessions = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoOrdersGroupsStore![^1].NoTradingSessionsGroupsStore is null && NoOrdersGroupsStore![^1].NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
							(NoOrdersGroupsStore![^1].NoTradingSessionsGroupsStore ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
						}
						break;
					case FixTag.ProcessCode:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ProcessCode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ProcessCode = (FixField.Character)field;
						break;
					case FixTag.Symbol:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.SideValueInd:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SideValueInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LocateReqd:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].LocateReqd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].LocateReqd = (FixField.Boolean)field;
						break;
					case FixTag.TransactTime:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.OrderQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrderQty = (FixField.Decimal)field;
						break;
					case FixTag.CashOrderQty:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CashOrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CashOrderQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdType:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.StopPx:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].StopPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].StopPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.ComplianceID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ComplianceID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ComplianceID = (FixField.Text)field;
						break;
					case FixTag.SolicitedFlag:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SolicitedFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SolicitedFlag = (FixField.Boolean)field;
						break;
					case FixTag.IOIID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].IOIid is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].IOIid = (FixField.Text)field;
						break;
					case FixTag.QuoteID:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].QuoteID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].QuoteID = (FixField.Text)field;
						break;
					case FixTag.TimeInForce:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.EffectiveTime:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EffectiveTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EffectiveTime = (FixField.Timestamp)field;
						break;
					case FixTag.ExpireDate:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.GTBookingInst:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].GTBookingInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].GTBookingInst = (FixField.Integer)field;
						break;
					case FixTag.Commission:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Commission is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.Rule80A:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Rule80A is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Rule80A = (FixField.Character)field;
						break;
					case FixTag.ForexReq:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ForexReq is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ForexReq = (FixField.Boolean)field;
						break;
					case FixTag.SettlCurrency:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].SettlCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].SettlCurrency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;
					case FixTag.SettlDate2:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.PositionEffect:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].OpenClose is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].OpenClose = (FixField.Character)field;
						break;
					case FixTag.CoveredOrUncovered:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CoveredOrUncovered is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CoveredOrUncovered = (FixField.Integer)field;
						break;
					case FixTag.CustomerOrFirm:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].CustomerOrFirm is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].CustomerOrFirm = (FixField.Integer)field;
						break;
					case FixTag.MaxShow:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].MaxShow is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].MaxShow = (FixField.Decimal)field;
						break;
					case FixTag.PegOffsetValue:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].PegDifference is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].PegDifference = (FixField.Decimal)field;
						break;
					case FixTag.DiscretionInst:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].DiscretionInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].DiscretionInst = (FixField.Character)field;
						break;
					case FixTag.DiscretionOffsetValue:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].DiscretionOffset is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].DiscretionOffset = (FixField.Decimal)field;
						break;
					case FixTag.ClearingFirm:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ClearingFirm is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ClearingFirm = (FixField.Text)field;
						break;
					case FixTag.ClearingAccount:
						if (NoOrdersGroupsStore is null || NoOrdersGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroupsStore![^1].ClearingAccount is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroupsStore!.Count - 1));
						else
							NoOrdersGroupsStore![^1].ClearingAccount = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID { get; internal set; }

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID { get; internal set; }

		/// <summary>The FIX ProgRptReqs, tag 414, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgRptReqs { get; internal set; }

		/// <summary>The FIX BidType, tag 394, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BidType { get; internal set; }

		/// <summary>The FIX ProgPeriodInterval, tag 415, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgPeriodInterval { get; internal set; }

		/// <summary>The FIX ListExecInstType, tag 433, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ListExecInstType { get; internal set; }

		/// <summary>The FIX ListExecInst, tag 69, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListExecInst { get; internal set; }

		/// <summary>The FIX EncodedListExecInstLen, tag 352, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedListExecInstLen { get; internal set; }

		/// <summary>The FIX EncodedListExecInst, tag 353, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedListExecInst { get; internal set; }

		/// <summary>The FIX TotNoOrders, tag 68, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoOrders { get; internal set; }

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders { get; internal set; }

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.NewOrderList.NoOrdersGroup>? NoOrdersGroups => NoOrdersGroupsStore;
		internal List<FixMessage.NewOrderList.NoOrdersGroup>? NoOrdersGroupsStore;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup
		{
			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text ClOrdID { get; init; }

			/// <summary>The FIX ListSeqNo, tag 67, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? ListSeqNo { get; internal set; }

			/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlInstMode { get; internal set; }

			/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClientID { get; internal set; }

			/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecBroker { get; internal set; }

			/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Account { get; internal set; }

			/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoAllocs { get; internal set; }

			/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
			public IReadOnlyList<FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup>? NoAllocsGroups => NoAllocsGroupsStore;
			internal List<FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup>? NoAllocsGroupsStore;

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp { get; internal set; }

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate { get; internal set; }

			/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? HandlInst { get; internal set; }

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst { get; internal set; }

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty { get; internal set; }

			/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MaxFloor { get; internal set; }

			/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? ExDestination { get; internal set; }

			/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoTradingSessions { get; internal set; }

			/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
			public IReadOnlyList<FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup>? NoTradingSessionsGroups => NoTradingSessionsGroupsStore;
			internal List<FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup>? NoTradingSessionsGroupsStore;

			/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? ProcessCode { get; internal set; }

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Symbol { get; internal set; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SideValueInd { get; internal set; }

			/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? LocateReqd { get; internal set; }

			/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? TransactTime { get; internal set; }

			/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty { get; internal set; }

			/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CashOrderQty { get; internal set; }

			/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdType { get; internal set; }

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price { get; internal set; }

			/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StopPx { get; internal set; }

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency { get; internal set; }

			/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ComplianceID { get; internal set; }

			/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? SolicitedFlag { get; internal set; }

			/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IOIid { get; internal set; }

			/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteID { get; internal set; }

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce { get; internal set; }

			/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? EffectiveTime { get; internal set; }

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate { get; internal set; }

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime { get; internal set; }

			/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? GTBookingInst { get; internal set; }

			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? Commission { get; internal set; }

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType { get; internal set; }

			/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Rule80A { get; internal set; }

			/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? ForexReq { get; internal set; }

			/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? SettlCurrency { get; internal set; }

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text { get; internal set; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }

			/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate2 { get; internal set; }

			/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty2 { get; internal set; }

			/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenClose { get; internal set; }

			/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? CoveredOrUncovered { get; internal set; }

			/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? CustomerOrFirm { get; internal set; }

			/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MaxShow { get; internal set; }

			/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
			public FixField.Decimal? PegDifference { get; internal set; }

			/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? DiscretionInst { get; internal set; }

			/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
			public FixField.Decimal? DiscretionOffset { get; internal set; }

			/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClearingFirm { get; internal set; }

			/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClearingAccount { get; internal set; }

			/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
			public sealed class NoAllocsGroup
			{
				/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
				public required FixField.Text AllocAccount { get; init; }

				/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? AllocShares { get; internal set; }
			}

			/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
			public sealed class NoTradingSessionsGroup
			{
				/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
				public required FixField.Text TradingSessionID { get; init; }
			}
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.NewOrderList(context, this);
		}
	}

	/// <summary>FIX 4.2 NewOrderSingle, MsgType D.</summary>
	public sealed partial class NewOrderSingle : FixMessage
	{
		internal NewOrderSingle(List<FixField> fields) : base("D", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.NoAllocs: if (NoAllocs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (NoAllocsGroupsStore is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroupsStore ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType: if (SettlmntTyp is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (FutSettDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate = (FixField.Date)field; break;
					case FixTag.HandlInst: if (HandlInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HandlInst = (FixField.Character)field; break;
					case FixTag.ExecInst: if (ExecInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecInst = (FixField.Multiple)field; break;
					case FixTag.MinQty: if (MinQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MinQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (MaxFloor is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxFloor = (FixField.Decimal)field; break;
					case FixTag.ExDestination: if (ExDestination is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExDestination = (FixField.Text)field; break;
					case FixTag.NoTradingSessions: if (NoTradingSessions is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoTradingSessions = (FixField.Integer)field; break;
					case FixTag.TradingSessionID:
						if (NoTradingSessionsGroupsStore is null && NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
						(NoTradingSessionsGroupsStore ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
						break;
					case FixTag.ProcessCode: if (ProcessCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ProcessCode = (FixField.Character)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.PrevClosePx: if (PrevClosePx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PrevClosePx = (FixField.Decimal)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.LocateReqd: if (LocateReqd is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LocateReqd = (FixField.Boolean)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (OrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (CashOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (OrdType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdType = (FixField.Character)field; break;
					case FixTag.Price: if (Price is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (StopPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StopPx = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.ComplianceID: if (ComplianceID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ComplianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (SolicitedFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SolicitedFlag = (FixField.Boolean)field; break;
					case FixTag.IOIID: if (IOIid is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IOIid = (FixField.Text)field; break;
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.TimeInForce: if (TimeInForce is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TimeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (EffectiveTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EffectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (ExpireDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (ExpireTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireTime = (FixField.Timestamp)field; break;
					case FixTag.GTBookingInst: if (GTBookingInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GTBookingInst = (FixField.Integer)field; break;
					case FixTag.Commission: if (Commission is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (CommType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CommType = (FixField.Character)field; break;
					case FixTag.Rule80A: if (Rule80A is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Rule80A = (FixField.Character)field; break;
					case FixTag.ForexReq: if (ForexReq is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ForexReq = (FixField.Boolean)field; break;
					case FixTag.SettlCurrency: if (SettlCurrency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrency = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (FutSettDate2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (OrderQty2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty2 = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (OpenClose is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OpenClose = (FixField.Character)field; break;
					case FixTag.CoveredOrUncovered: if (CoveredOrUncovered is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CoveredOrUncovered = (FixField.Integer)field; break;
					case FixTag.CustomerOrFirm: if (CustomerOrFirm is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CustomerOrFirm = (FixField.Integer)field; break;
					case FixTag.MaxShow: if (MaxShow is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxShow = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (PegDifference is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (DiscretionInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (DiscretionOffset is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionOffset = (FixField.Decimal)field; break;
					case FixTag.ClearingFirm: if (ClearingFirm is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (ClearingAccount is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingAccount = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.NewOrderSingle.NoAllocsGroup>? NoAllocsGroups => NoAllocsGroupsStore;
		internal List<FixMessage.NewOrderSingle.NoAllocsGroup>? NoAllocsGroupsStore;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp { get; internal set; }

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate { get; internal set; }

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty { get; internal set; }

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor { get; internal set; }

		/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? ExDestination { get; internal set; }

		/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoTradingSessions { get; internal set; }

		/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.NewOrderSingle.NoTradingSessionsGroup>? NoTradingSessionsGroups => NoTradingSessionsGroupsStore;
		internal List<FixMessage.NewOrderSingle.NoTradingSessionsGroup>? NoTradingSessionsGroupsStore;

		/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ProcessCode { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? PrevClosePx { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? LocateReqd { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price { get; internal set; }

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag { get; internal set; }

		/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIid { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime { get; internal set; }

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType { get; internal set; }

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A { get; internal set; }

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 { get; internal set; }

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose { get; internal set; }

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CoveredOrUncovered { get; internal set; }

		/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CustomerOrFirm { get; internal set; }

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow { get; internal set; }

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference { get; internal set; }

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst { get; internal set; }

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset { get; internal set; }

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm { get; internal set; }

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount { get; internal set; }

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup
		{
			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text AllocAccount { get; init; }

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
		public sealed class NoTradingSessionsGroup
		{
			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text TradingSessionID { get; init; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.NewOrderSingle(context, this);
		}
	}

	/// <summary>FIX 4.2 News, MsgType B.</summary>
	public sealed partial class News : FixMessage
	{
		internal News(List<FixField> fields) : base("B", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrigTime: if (OrigTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigTime = (FixField.Timestamp)field; break;
					case FixTag.Urgency: if (Urgency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Urgency = (FixField.Character)field; break;
					case FixTag.Headline: if (Headline is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Headline = (FixField.Text)field; break;
					case FixTag.EncodedHeadlineLen: if (EncodedHeadlineLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedHeadlineLen = (FixField.Integer)field; break;
					case FixTag.EncodedHeadline: if (EncodedHeadline is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedHeadline = (FixField.Data)field; break;
					case FixTag.NoRoutingIDs: if (NoRoutingIDs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (NoRoutingIDsGroupsStore is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroupsStore ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroupsStore is null || NoRoutingIDsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroupsStore![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroupsStore!.Count - 1));
						else
							NoRoutingIDsGroupsStore![^1].RoutingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { RelatdSym = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.NoLinesOfText: if (LinesOfText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LinesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (LinesOfTextGroupsStore is null && LinesOfText is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoLinesOfText, field.Position, field, -1));
						(LinesOfTextGroupsStore ??= []).Add(new () { Text = (FixField.Text)field });
						break;
					case FixTag.EncodedTextLen:
						if (LinesOfTextGroupsStore is null || LinesOfTextGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroupsStore![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroupsStore!.Count - 1));
						else
							LinesOfTextGroupsStore![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (LinesOfTextGroupsStore is null || LinesOfTextGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroupsStore![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroupsStore!.Count - 1));
						else
							LinesOfTextGroupsStore![^1].EncodedText = (FixField.Data)field;
						break;
					case FixTag.URLLink: if (URLLink is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); URLLink = (FixField.Text)field; break;
					case FixTag.RawDataLength: if (RawDataLength is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (RawData is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RawData = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrigTime, tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? OrigTime { get; internal set; }

		/// <summary>The FIX Urgency, tag 61, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Urgency { get; internal set; }

		/// <summary>The FIX Headline, tag 148, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Headline { get; internal set; }

		/// <summary>The FIX EncodedHeadlineLen, tag 358, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedHeadlineLen { get; internal set; }

		/// <summary>The FIX EncodedHeadline, tag 359, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedHeadline { get; internal set; }

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs { get; internal set; }

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.News.NoRoutingIDsGroup>? NoRoutingIDsGroups => NoRoutingIDsGroupsStore;
		internal List<FixMessage.News.NoRoutingIDsGroup>? NoRoutingIDsGroupsStore;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.News.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.News.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText { get; internal set; }

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.News.LinesOfTextGroup>? LinesOfTextGroups => LinesOfTextGroupsStore;
		internal List<FixMessage.News.LinesOfTextGroup>? LinesOfTextGroupsStore;

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink { get; internal set; }

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength { get; internal set; }

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData { get; internal set; }

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup
		{
			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>; null when the field is absent.</summary>
			public required FixField.Integer RoutingType { get; init; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX RelatdSym, tag 46, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text RelatdSym { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }
		}

		/// <summary>One entry of the group counted by LinesOfText, tag 33.</summary>
		public sealed class LinesOfTextGroup
		{
			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Text { get; init; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen { get; internal set; }

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.News(context, this);
		}
	}

	/// <summary>FIX 4.2 OrderCancelReject, MsgType 9.</summary>
	public sealed partial class OrderCancelReject : FixMessage
	{
		internal OrderCancelReject(List<FixField> fields) : base("9", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.SecondaryOrderID: if (SecondaryOrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecondaryOrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (OrigClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigClOrdID = (FixField.Text)field; break;
					case FixTag.OrdStatus: if (OrdStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdStatus = (FixField.Character)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.CxlRejResponseTo: if (CxlRejResponseTo is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CxlRejResponseTo = (FixField.Character)field; break;
					case FixTag.CxlRejReason: if (CxlRejReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CxlRejReason = (FixField.Integer)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecondaryOrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID { get; internal set; }

		/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdStatus { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX CxlRejResponseTo, tag 434, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CxlRejResponseTo { get; internal set; }

		/// <summary>The FIX CxlRejReason, tag 102, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CxlRejReason { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.OrderCancelReject(context, this);
		}
	}

	/// <summary>FIX 4.2 OrderCancelReplaceRequest, MsgType G.</summary>
	public sealed partial class OrderCancelReplaceRequest : FixMessage
	{
		internal OrderCancelReplaceRequest(List<FixField> fields) : base("G", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (OrigClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigClOrdID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.NoAllocs: if (NoAllocs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (NoAllocsGroupsStore is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroupsStore ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroupsStore is null || NoAllocsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroupsStore![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroupsStore!.Count - 1));
						else
							NoAllocsGroupsStore![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType: if (SettlmntTyp is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (FutSettDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate = (FixField.Date)field; break;
					case FixTag.HandlInst: if (HandlInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HandlInst = (FixField.Character)field; break;
					case FixTag.ExecInst: if (ExecInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecInst = (FixField.Multiple)field; break;
					case FixTag.MinQty: if (MinQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MinQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (MaxFloor is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxFloor = (FixField.Decimal)field; break;
					case FixTag.ExDestination: if (ExDestination is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExDestination = (FixField.Text)field; break;
					case FixTag.NoTradingSessions: if (NoTradingSessions is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoTradingSessions = (FixField.Integer)field; break;
					case FixTag.TradingSessionID:
						if (NoTradingSessionsGroupsStore is null && NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
						(NoTradingSessionsGroupsStore ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
						break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (OrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (CashOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (OrdType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdType = (FixField.Character)field; break;
					case FixTag.Price: if (Price is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (StopPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StopPx = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (PegDifference is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (DiscretionInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (DiscretionOffset is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DiscretionOffset = (FixField.Decimal)field; break;
					case FixTag.ComplianceID: if (ComplianceID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ComplianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (SolicitedFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SolicitedFlag = (FixField.Boolean)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.TimeInForce: if (TimeInForce is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TimeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (EffectiveTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EffectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (ExpireDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (ExpireTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExpireTime = (FixField.Timestamp)field; break;
					case FixTag.GTBookingInst: if (GTBookingInst is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GTBookingInst = (FixField.Integer)field; break;
					case FixTag.Commission: if (Commission is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (CommType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CommType = (FixField.Character)field; break;
					case FixTag.Rule80A: if (Rule80A is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Rule80A = (FixField.Character)field; break;
					case FixTag.ForexReq: if (ForexReq is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ForexReq = (FixField.Boolean)field; break;
					case FixTag.SettlCurrency: if (SettlCurrency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlCurrency = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (FutSettDate2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (OrderQty2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty2 = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (OpenClose is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OpenClose = (FixField.Character)field; break;
					case FixTag.CoveredOrUncovered: if (CoveredOrUncovered is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CoveredOrUncovered = (FixField.Integer)field; break;
					case FixTag.CustomerOrFirm: if (CustomerOrFirm is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CustomerOrFirm = (FixField.Integer)field; break;
					case FixTag.MaxShow: if (MaxShow is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaxShow = (FixField.Decimal)field; break;
					case FixTag.LocateReqd: if (LocateReqd is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LocateReqd = (FixField.Boolean)field; break;
					case FixTag.ClearingFirm: if (ClearingFirm is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (ClearingAccount is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClearingAccount = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs { get; internal set; }

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.OrderCancelReplaceRequest.NoAllocsGroup>? NoAllocsGroups => NoAllocsGroupsStore;
		internal List<FixMessage.OrderCancelReplaceRequest.NoAllocsGroup>? NoAllocsGroupsStore;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp { get; internal set; }

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate { get; internal set; }

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst { get; internal set; }

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst { get; internal set; }

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty { get; internal set; }

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor { get; internal set; }

		/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? ExDestination { get; internal set; }

		/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoTradingSessions { get; internal set; }

		/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup>? NoTradingSessionsGroups => NoTradingSessionsGroupsStore;
		internal List<FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup>? NoTradingSessionsGroupsStore;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType { get; internal set; }

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price { get; internal set; }

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx { get; internal set; }

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference { get; internal set; }

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst { get; internal set; }

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime { get; internal set; }

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate { get; internal set; }

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime { get; internal set; }

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst { get; internal set; }

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission { get; internal set; }

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType { get; internal set; }

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A { get; internal set; }

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq { get; internal set; }

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 { get; internal set; }

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose { get; internal set; }

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CoveredOrUncovered { get; internal set; }

		/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CustomerOrFirm { get; internal set; }

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow { get; internal set; }

		/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? LocateReqd { get; internal set; }

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm { get; internal set; }

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount { get; internal set; }

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup
		{
			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text AllocAccount { get; init; }

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares { get; internal set; }
		}

		/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
		public sealed class NoTradingSessionsGroup
		{
			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text TradingSessionID { get; init; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.OrderCancelReplaceRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 OrderCancelRequest, MsgType F.</summary>
	public sealed partial class OrderCancelRequest : FixMessage
	{
		internal OrderCancelRequest(List<FixField> fields) : base("F", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrigClOrdID: if (OrigClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrigClOrdID = (FixField.Text)field; break;
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.ListID: if (ListID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ListID = (FixField.Text)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (OrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (CashOrderQty is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashOrderQty = (FixField.Decimal)field; break;
					case FixTag.ComplianceID: if (ComplianceID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ComplianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (SolicitedFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SolicitedFlag = (FixField.Boolean)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID { get; internal set; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty { get; internal set; }

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty { get; internal set; }

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID { get; internal set; }

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.OrderCancelRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 OrderStatusRequest, MsgType H.</summary>
	public sealed partial class OrderStatusRequest : FixMessage
	{
		internal OrderStatusRequest(List<FixField> fields) : base("H", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.Account: if (Account is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Account = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.OrderStatusRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 Quote, MsgType S.</summary>
	public sealed partial class Quote : FixMessage
	{
		internal Quote(List<FixField> fields) : base("S", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteReqID: if (QuoteReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.QuoteResponseLevel: if (QuoteResponseLevel is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.BidPx: if (BidPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidPx = (FixField.Decimal)field; break;
					case FixTag.OfferPx: if (OfferPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OfferPx = (FixField.Decimal)field; break;
					case FixTag.BidSize: if (BidSize is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidSize = (FixField.Decimal)field; break;
					case FixTag.OfferSize: if (OfferSize is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OfferSize = (FixField.Decimal)field; break;
					case FixTag.ValidUntilTime: if (ValidUntilTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ValidUntilTime = (FixField.Timestamp)field; break;
					case FixTag.BidSpotRate: if (BidSpotRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidSpotRate = (FixField.Decimal)field; break;
					case FixTag.OfferSpotRate: if (OfferSpotRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OfferSpotRate = (FixField.Decimal)field; break;
					case FixTag.BidForwardPoints: if (BidForwardPoints is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BidForwardPoints = (FixField.Decimal)field; break;
					case FixTag.OfferForwardPoints: if (OfferForwardPoints is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OfferForwardPoints = (FixField.Decimal)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.SettlDate: if (FutSettDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate = (FixField.Date)field; break;
					case FixTag.OrdType: if (OrdType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrdType = (FixField.Character)field; break;
					case FixTag.SettlDate2: if (FutSettDate2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FutSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (OrderQty2 is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderQty2 = (FixField.Decimal)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidPx { get; internal set; }

		/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferPx { get; internal set; }

		/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidSize { get; internal set; }

		/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferSize { get; internal set; }

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ValidUntilTime { get; internal set; }

		/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidSpotRate { get; internal set; }

		/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferSpotRate { get; internal set; }

		/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidForwardPoints { get; internal set; }

		/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferForwardPoints { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate { get; internal set; }

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType { get; internal set; }

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 { get; internal set; }

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Quote(context, this);
		}
	}

	/// <summary>FIX 4.2 QuoteAcknowledgement, MsgType b.</summary>
	public sealed partial class QuoteAcknowledgement : FixMessage
	{
		internal QuoteAcknowledgement(List<FixField> fields) : base("b", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteReqID: if (QuoteReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.QuoteStatus: if (QuoteAckStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteAckStatus = (FixField.Integer)field; break;
					case FixTag.QuoteRejectReason: if (QuoteRejectReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteRejectReason = (FixField.Integer)field; break;
					case FixTag.QuoteResponseLevel: if (QuoteResponseLevel is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.NoQuoteSets: if (NoQuoteSets is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoQuoteSets = (FixField.Integer)field; break;
					case FixTag.QuoteSetID:
						if (NoQuoteSetsGroupsStore is null && NoQuoteSets is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteSets, field.Position, field, -1));
						(NoQuoteSetsGroupsStore ??= []).Add(new () { QuoteSetID = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].TotQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].TotQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null && NoQuoteSetsGroupsStore![^1].NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
							(NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore ??= []).Add(new () { QuoteEntryID = (FixField.Text)field });
						}
						break;
					case FixTag.Symbol:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteEntryRejectReason:
						if (NoQuoteSetsGroupsStore is null || NoQuoteSetsGroupsStore.Count == 0 || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore is null || NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].QuoteEntryRejectReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteSetsGroupsStore![^1].NoQuoteEntriesGroupsStore![^1].QuoteEntryRejectReason = (FixField.Integer)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX QuoteAckStatus, tag 297, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteAckStatus { get; internal set; }

		/// <summary>The FIX QuoteRejectReason, tag 300, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteRejectReason { get; internal set; }

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX NoQuoteSets, tag 296, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteSets { get; internal set; }

		/// <summary>The entries counted by NoQuoteSets, tag 296; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup>? NoQuoteSetsGroups => NoQuoteSetsGroupsStore;
		internal List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup>? NoQuoteSetsGroupsStore;

		/// <summary>One entry of the group counted by NoQuoteSets, tag 296.</summary>
		public sealed class NoQuoteSetsGroup
		{
			/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text QuoteSetID { get; init; }

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol { get; internal set; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID { get; internal set; }

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay { get; internal set; }

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall { get; internal set; }

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice { get; internal set; }

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute { get; internal set; }

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier { get; internal set; }

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange { get; internal set; }

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX TotQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? TotQuoteEntries { get; internal set; }

			/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoQuoteEntries { get; internal set; }

			/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
			public IReadOnlyList<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => NoQuoteEntriesGroupsStore;
			internal List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroupsStore;

			/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
			public sealed class NoQuoteEntriesGroup
			{
				/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
				public required FixField.Text QuoteEntryID { get; init; }

				/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Symbol { get; internal set; }

				/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SymbolSfx { get; internal set; }

				/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityID { get; internal set; }

				/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? IDSource { get; internal set; }

				/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityType { get; internal set; }

				/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
				public FixField.MonthYear? MaturityMonthYear { get; internal set; }

				/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
				public FixField.Integer? MaturityDay { get; internal set; }

				/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? PutOrCall { get; internal set; }

				/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? StrikePrice { get; internal set; }

				/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OptAttribute { get; internal set; }

				/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? ContractMultiplier { get; internal set; }

				/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? CouponRate { get; internal set; }

				/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityExchange { get; internal set; }

				/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Issuer { get; internal set; }

				/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedIssuerLen { get; internal set; }

				/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedIssuer { get; internal set; }

				/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityDesc { get; internal set; }

				/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

				/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedSecurityDesc { get; internal set; }

				/// <summary>The FIX QuoteEntryRejectReason, tag 368, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? QuoteEntryRejectReason { get; internal set; }
			}
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.QuoteAcknowledgement(context, this);
		}
	}

	/// <summary>FIX 4.2 QuoteCancel, MsgType Z.</summary>
	public sealed partial class QuoteCancel : FixMessage
	{
		internal QuoteCancel(List<FixField> fields) : base("Z", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteReqID: if (QuoteReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.QuoteCancelType: if (QuoteCancelType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteCancelType = (FixField.Integer)field; break;
					case FixTag.QuoteResponseLevel: if (QuoteResponseLevel is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.NoQuoteEntries: if (NoQuoteEntries is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoQuoteEntries = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (NoQuoteEntriesGroupsStore is null && NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
						(NoQuoteEntriesGroupsStore ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteEntriesGroupsStore is null || NoQuoteEntriesGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroupsStore![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroupsStore!.Count - 1));
						else
							NoQuoteEntriesGroupsStore![^1].UnderlyingSymbol = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID { get; internal set; }

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX QuoteCancelType, tag 298, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteCancelType { get; internal set; }

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteEntries { get; internal set; }

		/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.QuoteCancel.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => NoQuoteEntriesGroupsStore;
		internal List<FixMessage.QuoteCancel.NoQuoteEntriesGroup>? NoQuoteEntriesGroupsStore;

		/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
		public sealed class NoQuoteEntriesGroup
		{
			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Symbol { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.QuoteCancel(context, this);
		}
	}

	/// <summary>FIX 4.2 QuoteRequest, MsgType R.</summary>
	public sealed partial class QuoteRequest : FixMessage
	{
		internal QuoteRequest(List<FixField> fields) : base("R", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteReqID: if (QuoteReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteReqID = (FixField.Text)field; break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.QuoteRequestType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].QuoteRequestType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].QuoteRequestType = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.OrderQty:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OrderQty = (FixField.Decimal)field;
						break;
					case FixTag.SettlDate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.ExpireTime:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.TransactTime:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.Currency:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Currency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.QuoteRequest.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.QuoteRequest.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text Symbol { get; init; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx { get; internal set; }

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID { get; internal set; }

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource { get; internal set; }

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType { get; internal set; }

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear { get; internal set; }

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay { get; internal set; }

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall { get; internal set; }

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice { get; internal set; }

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute { get; internal set; }

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier { get; internal set; }

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate { get; internal set; }

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange { get; internal set; }

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer { get; internal set; }

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer { get; internal set; }

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc { get; internal set; }

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx { get; internal set; }

			/// <summary>The FIX QuoteRequestType, tag 303, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? QuoteRequestType { get; internal set; }

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty { get; internal set; }

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate { get; internal set; }

			/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdType { get; internal set; }

			/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate2 { get; internal set; }

			/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty2 { get; internal set; }

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime { get; internal set; }

			/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? TransactTime { get; internal set; }

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.QuoteRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 QuoteStatusRequest, MsgType a.</summary>
	public sealed partial class QuoteStatusRequest : FixMessage
	{
		internal QuoteStatusRequest(List<FixField> fields) : base("a", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.QuoteID: if (QuoteID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); QuoteID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.QuoteStatusRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 Reject, MsgType 3.</summary>
	public sealed partial class Reject : FixMessage
	{
		internal Reject(List<FixField> fields) : base("3", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.RefSeqNum: if (RefSeqNum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefSeqNum = (FixField.Integer)field; break;
					case FixTag.RefTagID: if (RefTagID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefTagID = (FixField.Integer)field; break;
					case FixTag.RefMsgType: if (RefMsgType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); RefMsgType = (FixField.Text)field; break;
					case FixTag.SessionRejectReason: if (SessionRejectReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SessionRejectReason = (FixField.Integer)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX RefSeqNum, tag 45, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefSeqNum { get; internal set; }

		/// <summary>The FIX RefTagID, tag 371, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefTagID { get; internal set; }

		/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefMsgType { get; internal set; }

		/// <summary>The FIX SessionRejectReason, tag 373, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SessionRejectReason { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.Reject(context, this);
		}
	}

	/// <summary>FIX 4.2 ResendRequest, MsgType 2.</summary>
	public sealed partial class ResendRequest : FixMessage
	{
		internal ResendRequest(List<FixField> fields) : base("2", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.BeginSeqNo: if (BeginSeqNo is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BeginSeqNo = (FixField.Integer)field; break;
					case FixTag.EndSeqNo: if (EndSeqNo is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EndSeqNo = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX BeginSeqNo, tag 7, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BeginSeqNo { get; internal set; }

		/// <summary>The FIX EndSeqNo, tag 16, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? EndSeqNo { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.ResendRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 SecurityDefinition, MsgType d.</summary>
	public sealed partial class SecurityDefinition : FixMessage
	{
		internal SecurityDefinition(List<FixField> fields) : base("d", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.SecurityReqID: if (SecurityReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityReqID = (FixField.Text)field; break;
					case FixTag.SecurityResponseID: if (SecurityResponseID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityResponseID = (FixField.Text)field; break;
					case FixTag.SecurityResponseType: if (SecurityResponseType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityResponseType = (FixField.Integer)field; break;
					case FixTag.TotNoRelatedSym: if (TotalNumSecurities is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotalNumSecurities = (FixField.Integer)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.UnderlyingSymbol:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { UnderlyingSymbol = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].RatioQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].RatioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingCurrency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX SecurityReqID, tag 320, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityReqID { get; internal set; }

		/// <summary>The FIX SecurityResponseID, tag 322, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityResponseID { get; internal set; }

		/// <summary>The FIX SecurityResponseType, tag 323, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityResponseType { get; internal set; }

		/// <summary>The FIX TotalNumSecurities, tag 393, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotalNumSecurities { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.SecurityDefinition.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.SecurityDefinition.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text UnderlyingSymbol { get; init; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID { get; internal set; }

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay { get; internal set; }

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall { get; internal set; }

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice { get; internal set; }

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute { get; internal set; }

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier { get; internal set; }

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange { get; internal set; }

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX RatioQty, tag 319, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? RatioQty { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingCurrency { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SecurityDefinition(context, this);
		}
	}

	/// <summary>FIX 4.2 SecurityDefinitionRequest, MsgType c.</summary>
	public sealed partial class SecurityDefinitionRequest : FixMessage
	{
		internal SecurityDefinitionRequest(List<FixField> fields) : base("c", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.SecurityReqID: if (SecurityReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityReqID = (FixField.Text)field; break;
					case FixTag.SecurityRequestType: if (SecurityRequestType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityRequestType = (FixField.Integer)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.UnderlyingSymbol:
						if (NoRelatedSymGroupsStore is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroupsStore ??= []).Add(new () { UnderlyingSymbol = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].RatioQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].RatioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].Side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (NoRelatedSymGroupsStore is null || NoRelatedSymGroupsStore.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroupsStore![^1].UnderlyingCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroupsStore!.Count - 1));
						else
							NoRelatedSymGroupsStore![^1].UnderlyingCurrency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX SecurityReqID, tag 320, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityReqID { get; internal set; }

		/// <summary>The FIX SecurityRequestType, tag 321, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityRequestType { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public IReadOnlyList<FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup>? NoRelatedSymGroups => NoRelatedSymGroupsStore;
		internal List<FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup>? NoRelatedSymGroupsStore;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup
		{
			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public required FixField.Text UnderlyingSymbol { get; init; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID { get; internal set; }

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear { get; internal set; }

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay { get; internal set; }

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall { get; internal set; }

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice { get; internal set; }

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute { get; internal set; }

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier { get; internal set; }

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange { get; internal set; }

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer { get; internal set; }

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen { get; internal set; }

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc { get; internal set; }

			/// <summary>The FIX RatioQty, tag 319, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? RatioQty { get; internal set; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side { get; internal set; }

			/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingCurrency { get; internal set; }
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SecurityDefinitionRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 SecurityStatus, MsgType f.</summary>
	public sealed partial class SecurityStatus : FixMessage
	{
		internal SecurityStatus(List<FixField> fields) : base("f", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.SecurityStatusReqID: if (SecurityStatusReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityStatusReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.UnsolicitedIndicator: if (UnsolicitedIndicator is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); UnsolicitedIndicator = (FixField.Boolean)field; break;
					case FixTag.SecurityTradingStatus: if (SecurityTradingStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityTradingStatus = (FixField.Integer)field; break;
					case FixTag.FinancialStatus: if (FinancialStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); FinancialStatus = (FixField.Character)field; break;
					case FixTag.CorporateAction: if (CorporateAction is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CorporateAction = (FixField.Character)field; break;
					case FixTag.HaltReason: if (HaltReason is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HaltReason = (FixField.Character)field; break;
					case FixTag.InViewOfCommon: if (InViewOfCommon is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); InViewOfCommon = (FixField.Boolean)field; break;
					case FixTag.DueToRelated: if (DueToRelated is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); DueToRelated = (FixField.Boolean)field; break;
					case FixTag.BuyVolume: if (BuyVolume is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); BuyVolume = (FixField.Decimal)field; break;
					case FixTag.SellVolume: if (SellVolume is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SellVolume = (FixField.Decimal)field; break;
					case FixTag.HighPx: if (HighPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); HighPx = (FixField.Decimal)field; break;
					case FixTag.LowPx: if (LowPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LowPx = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (LastPx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastPx = (FixField.Decimal)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.Adjustment: if (Adjustment is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Adjustment = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX SecurityStatusReqID, tag 324, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityStatusReqID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX UnsolicitedIndicator, tag 325, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? UnsolicitedIndicator { get; internal set; }

		/// <summary>The FIX SecurityTradingStatus, tag 326, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityTradingStatus { get; internal set; }

		/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? FinancialStatus { get; internal set; }

		/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CorporateAction { get; internal set; }

		/// <summary>The FIX HaltReason, tag 327, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HaltReason { get; internal set; }

		/// <summary>The FIX InViewOfCommon, tag 328, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? InViewOfCommon { get; internal set; }

		/// <summary>The FIX DueToRelated, tag 329, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? DueToRelated { get; internal set; }

		/// <summary>The FIX BuyVolume, tag 330, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? BuyVolume { get; internal set; }

		/// <summary>The FIX SellVolume, tag 331, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? SellVolume { get; internal set; }

		/// <summary>The FIX HighPx, tag 332, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? HighPx { get; internal set; }

		/// <summary>The FIX LowPx, tag 333, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LowPx { get; internal set; }

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX Adjustment, tag 334, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? Adjustment { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SecurityStatus(context, this);
		}
	}

	/// <summary>FIX 4.2 SecurityStatusRequest, MsgType e.</summary>
	public sealed partial class SecurityStatusRequest : FixMessage
	{
		internal SecurityStatusRequest(List<FixField> fields) : base("e", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.SecurityStatusReqID: if (SecurityStatusReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityStatusReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (Symbol is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (SymbolSfx is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SymbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (SecurityID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (IDSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); IDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (MaturityMonthYear is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (MaturityDay is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); MaturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (PutOrCall is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); PutOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (StrikePrice is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StrikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (OptAttribute is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OptAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (ContractMultiplier is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ContractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (CouponRate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CouponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (SecurityExchange is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (Issuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (EncodedIssuerLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (EncodedIssuer is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (SecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (EncodedSecurityDescLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (EncodedSecurityDesc is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (Currency is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Currency = (FixField.Text)field; break;
					case FixTag.SubscriptionRequestType: if (SubscriptionRequestType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SubscriptionRequestType = (FixField.Character)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX SecurityStatusReqID, tag 324, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityStatusReqID { get; internal set; }

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol { get; internal set; }

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx { get; internal set; }

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID { get; internal set; }

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear { get; internal set; }

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay { get; internal set; }

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall { get; internal set; }

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice { get; internal set; }

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute { get; internal set; }

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier { get; internal set; }

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate { get; internal set; }

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange { get; internal set; }

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer { get; internal set; }

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen { get; internal set; }

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer { get; internal set; }

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc { get; internal set; }

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen { get; internal set; }

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc { get; internal set; }

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency { get; internal set; }

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SecurityStatusRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 SequenceReset, MsgType 4.</summary>
	public sealed partial class SequenceReset : FixMessage
	{
		internal SequenceReset(List<FixField> fields) : base("4", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.GapFillFlag: if (GapFillFlag is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); GapFillFlag = (FixField.Boolean)field; break;
					case FixTag.NewSeqNo: if (NewSeqNo is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NewSeqNo = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX GapFillFlag, tag 123, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? GapFillFlag { get; internal set; }

		/// <summary>The FIX NewSeqNo, tag 36, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NewSeqNo { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SequenceReset(context, this);
		}
	}

	/// <summary>FIX 4.2 SettlementInstructions, MsgType T.</summary>
	public sealed partial class SettlementInstructions : FixMessage
	{
		internal SettlementInstructions(List<FixField> fields) : base("T", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.SettlInstID: if (SettlInstID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstID = (FixField.Text)field; break;
					case FixTag.SettlInstTransType: if (SettlInstTransType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstTransType = (FixField.Character)field; break;
					case FixTag.SettlInstRefID: if (SettlInstRefID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstRefID = (FixField.Text)field; break;
					case FixTag.SettlInstMode: if (SettlInstMode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstMode = (FixField.Character)field; break;
					case FixTag.SettlInstSource: if (SettlInstSource is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstSource = (FixField.Character)field; break;
					case FixTag.AllocAccount: if (AllocAccount is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocAccount = (FixField.Text)field; break;
					case FixTag.SettlLocation: if (SettlLocation is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlLocation = (FixField.Text)field; break;
					case FixTag.TradeDate: if (TradeDate is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradeDate = (FixField.Date)field; break;
					case FixTag.AllocID: if (AllocID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); AllocID = (FixField.Text)field; break;
					case FixTag.LastMkt: if (LastMkt is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.Side: if (Side is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Side = (FixField.Character)field; break;
					case FixTag.SecurityType: if (SecurityType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecurityType = (FixField.Text)field; break;
					case FixTag.EffectiveTime: if (EffectiveTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EffectiveTime = (FixField.Timestamp)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.ClientID: if (ClientID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (ExecBroker is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ExecBroker = (FixField.Text)field; break;
					case FixTag.StandInstDbType: if (StandInstDbType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StandInstDbType = (FixField.Integer)field; break;
					case FixTag.StandInstDbName: if (StandInstDbName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StandInstDbName = (FixField.Text)field; break;
					case FixTag.StandInstDbID: if (StandInstDbID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); StandInstDbID = (FixField.Text)field; break;
					case FixTag.SettlDeliveryType: if (SettlDeliveryType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlDeliveryType = (FixField.Integer)field; break;
					case FixTag.SettlDepositoryCode: if (SettlDepositoryCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlDepositoryCode = (FixField.Text)field; break;
					case FixTag.SettlBrkrCode: if (SettlBrkrCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlBrkrCode = (FixField.Text)field; break;
					case FixTag.SettlInstCode: if (SettlInstCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SettlInstCode = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentName: if (SecuritySettlAgentName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentCode: if (SecuritySettlAgentCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentCode = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentAcctNum: if (SecuritySettlAgentAcctNum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentAcctNum = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentAcctName: if (SecuritySettlAgentAcctName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentAcctName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentContactName: if (SecuritySettlAgentContactName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentContactName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentContactPhone: if (SecuritySettlAgentContactPhone is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SecuritySettlAgentContactPhone = (FixField.Text)field; break;
					case FixTag.CashSettlAgentName: if (CashSettlAgentName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentCode: if (CashSettlAgentCode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentCode = (FixField.Text)field; break;
					case FixTag.CashSettlAgentAcctNum: if (CashSettlAgentAcctNum is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentAcctNum = (FixField.Text)field; break;
					case FixTag.CashSettlAgentAcctName: if (CashSettlAgentAcctName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentAcctName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentContactName: if (CashSettlAgentContactName is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentContactName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentContactPhone: if (CashSettlAgentContactPhone is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); CashSettlAgentContactPhone = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX SettlInstID, tag 162, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstID { get; internal set; }

		/// <summary>The FIX SettlInstTransType, tag 163, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstTransType { get; internal set; }

		/// <summary>The FIX SettlInstRefID, tag 214, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstRefID { get; internal set; }

		/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstMode { get; internal set; }

		/// <summary>The FIX SettlInstSource, tag 165, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstSource { get; internal set; }

		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocAccount { get; internal set; }

		/// <summary>The FIX SettlLocation, tag 166, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlLocation { get; internal set; }

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate { get; internal set; }

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID { get; internal set; }

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side { get; internal set; }

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType { get; internal set; }

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime { get; internal set; }

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime { get; internal set; }

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID { get; internal set; }

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker { get; internal set; }

		/// <summary>The FIX StandInstDbType, tag 169, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? StandInstDbType { get; internal set; }

		/// <summary>The FIX StandInstDbName, tag 170, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? StandInstDbName { get; internal set; }

		/// <summary>The FIX StandInstDbID, tag 171, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? StandInstDbID { get; internal set; }

		/// <summary>The FIX SettlDeliveryType, tag 172, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SettlDeliveryType { get; internal set; }

		/// <summary>The FIX SettlDepositoryCode, tag 173, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlDepositoryCode { get; internal set; }

		/// <summary>The FIX SettlBrkrCode, tag 174, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlBrkrCode { get; internal set; }

		/// <summary>The FIX SettlInstCode, tag 175, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstCode { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentName, tag 176, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentName { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentCode, tag 177, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentCode { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentAcctNum, tag 178, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentAcctNum { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentAcctName, tag 179, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentAcctName { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentContactName, tag 180, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentContactName { get; internal set; }

		/// <summary>The FIX SecuritySettlAgentContactPhone, tag 181, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentContactPhone { get; internal set; }

		/// <summary>The FIX CashSettlAgentName, tag 182, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentName { get; internal set; }

		/// <summary>The FIX CashSettlAgentCode, tag 183, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentCode { get; internal set; }

		/// <summary>The FIX CashSettlAgentAcctNum, tag 184, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentAcctNum { get; internal set; }

		/// <summary>The FIX CashSettlAgentAcctName, tag 185, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentAcctName { get; internal set; }

		/// <summary>The FIX CashSettlAgentContactName, tag 186, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentContactName { get; internal set; }

		/// <summary>The FIX CashSettlAgentContactPhone, tag 187, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentContactPhone { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.SettlementInstructions(context, this);
		}
	}

	/// <summary>FIX 4.2 TestRequest, MsgType 1.</summary>
	public sealed partial class TestRequest : FixMessage
	{
		internal TestRequest(List<FixField> fields) : base("1", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.TestReqID: if (TestReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TestReqID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX TestReqID, tag 112, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TestReqID { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.TestRequest(context, this);
		}
	}

	/// <summary>FIX 4.2 TradingSessionStatus, MsgType h.</summary>
	public sealed partial class TradingSessionStatus : FixMessage
	{
		internal TradingSessionStatus(List<FixField> fields) : base("h", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.TradSesReqID: if (TradSesReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesReqID = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.TradSesMethod: if (TradSesMethod is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesMethod = (FixField.Integer)field; break;
					case FixTag.TradSesMode: if (TradSesMode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesMode = (FixField.Integer)field; break;
					case FixTag.UnsolicitedIndicator: if (UnsolicitedIndicator is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); UnsolicitedIndicator = (FixField.Boolean)field; break;
					case FixTag.TradSesStatus: if (TradSesStatus is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesStatus = (FixField.Integer)field; break;
					case FixTag.TradSesStartTime: if (TradSesStartTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesStartTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesOpenTime: if (TradSesOpenTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesOpenTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesPreCloseTime: if (TradSesPreCloseTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesPreCloseTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesCloseTime: if (TradSesCloseTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesCloseTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesEndTime: if (TradSesEndTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesEndTime = (FixField.Timestamp)field; break;
					case FixTag.TotalVolumeTraded: if (TotalVolumeTraded is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TotalVolumeTraded = (FixField.Decimal)field; break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX TradSesReqID, tag 335, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradSesReqID { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX TradSesMethod, tag 338, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMethod { get; internal set; }

		/// <summary>The FIX TradSesMode, tag 339, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMode { get; internal set; }

		/// <summary>The FIX UnsolicitedIndicator, tag 325, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? UnsolicitedIndicator { get; internal set; }

		/// <summary>The FIX TradSesStatus, tag 340, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesStatus { get; internal set; }

		/// <summary>The FIX TradSesStartTime, tag 341, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesStartTime { get; internal set; }

		/// <summary>The FIX TradSesOpenTime, tag 342, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesOpenTime { get; internal set; }

		/// <summary>The FIX TradSesPreCloseTime, tag 343, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesPreCloseTime { get; internal set; }

		/// <summary>The FIX TradSesCloseTime, tag 344, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesCloseTime { get; internal set; }

		/// <summary>The FIX TradSesEndTime, tag 345, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesEndTime { get; internal set; }

		/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? TotalVolumeTraded { get; internal set; }

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text { get; internal set; }

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen { get; internal set; }

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.TradingSessionStatus(context, this);
		}
	}

	/// <summary>FIX 4.2 TradingSessionStatusRequest, MsgType g.</summary>
	public sealed partial class TradingSessionStatusRequest : FixMessage
	{
		internal TradingSessionStatusRequest(List<FixField> fields) : base("g", fields)
		{
			foreach (var field in fields)
			{
				switch (field.Tag)
				{
					case FixTag.TradSesReqID: if (TradSesReqID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesReqID = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (TradingSessionID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradingSessionID = (FixField.Text)field; break;
					case FixTag.TradSesMethod: if (TradSesMethod is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesMethod = (FixField.Integer)field; break;
					case FixTag.TradSesMode: if (TradSesMode is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TradSesMode = (FixField.Integer)field; break;
					case FixTag.SubscriptionRequestType: if (SubscriptionRequestType is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); SubscriptionRequestType = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

						break;
				}
			}
		}

		/// <summary>The FIX TradSesReqID, tag 335, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradSesReqID { get; internal set; }

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID { get; internal set; }

		/// <summary>The FIX TradSesMethod, tag 338, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMethod { get; internal set; }

		/// <summary>The FIX TradSesMode, tag 339, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMode { get; internal set; }

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType { get; internal set; }

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context)
		{
			context.Validators.TradingSessionStatusRequest(context, this);
		}
	}
}
