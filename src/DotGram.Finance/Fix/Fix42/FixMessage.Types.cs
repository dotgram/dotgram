using System.Collections.Generic;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand.

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
						if (NoOrdersGroups is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroups ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.OrderID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.SecondaryOrderID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SecondaryOrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SecondaryOrderID = (FixField.Text)field;
						break;
					case FixTag.ListID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ListID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ListID = (FixField.Text)field;
						break;
					case FixTag.WaveNo:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].WaveNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].WaveNo = (FixField.Text)field;
						break;
					case FixTag.NoExecs: if (NoExecs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoExecs = (FixField.Integer)field; break;
					case FixTag.LastQty:
						if (NoExecsGroups is null && NoExecs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoExecs, field.Position, field, -1));
						(NoExecsGroups ??= []).Add(new () { LastShares = (FixField.Decimal)field });
						break;
					case FixTag.ExecID:
						if (NoExecsGroups is null || NoExecsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroups![^1].ExecID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroups!.Count - 1));
						else
							NoExecsGroups![^1].ExecID = (FixField.Text)field;
						break;
					case FixTag.LastPx:
						if (NoExecsGroups is null || NoExecsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroups![^1].LastPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroups!.Count - 1));
						else
							NoExecsGroups![^1].LastPx = (FixField.Decimal)field;
						break;
					case FixTag.LastCapacity:
						if (NoExecsGroups is null || NoExecsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoExecsGroups![^1].LastCapacity is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoExecsGroups!.Count - 1));
						else
							NoExecsGroups![^1].LastCapacity = (FixField.Character)field;
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
						if (NoAllocsGroups is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroups ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocPrice:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocPrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocPrice = (FixField.Decimal)field;
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.ProcessCode:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].ProcessCode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].ProcessCode = (FixField.Character)field;
						break;
					case FixTag.BrokerOfCredit:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].BrokerOfCredit is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].BrokerOfCredit = (FixField.Text)field;
						break;
					case FixTag.NotifyBrokerOfCredit:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].NotifyBrokerOfCredit is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].NotifyBrokerOfCredit = (FixField.Boolean)field;
						break;
					case FixTag.AllocHandlInst:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocHandlInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocHandlInst = (FixField.Integer)field;
						break;
					case FixTag.AllocText:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocText = (FixField.Text)field;
						break;
					case FixTag.EncodedAllocTextLen:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].EncodedAllocTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].EncodedAllocTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedAllocText:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].EncodedAllocText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].EncodedAllocText = (FixField.Data)field;
						break;
					case FixTag.ExecBroker:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].ExecBroker is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].ExecBroker = (FixField.Text)field;
						break;
					case FixTag.ClientID:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].ClientID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].ClientID = (FixField.Text)field;
						break;
					case FixTag.Commission:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].Commission is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].Commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.AllocAvgPx:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocAvgPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocAvgPx = (FixField.Decimal)field;
						break;
					case FixTag.AllocNetMoney:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocNetMoney is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocNetMoney = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrAmt:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].SettlCurrAmt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].SettlCurrAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrency:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].SettlCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].SettlCurrency = (FixField.Text)field;
						break;
					case FixTag.SettlCurrFxRate:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].SettlCurrFxRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].SettlCurrFxRate = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrFxRateCalc:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].SettlCurrFxRateCalc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].SettlCurrFxRateCalc = (FixField.Character)field;
						break;
					case FixTag.AccruedInterestAmt:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AccruedInterestAmt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AccruedInterestAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlInstMode:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].SettlInstMode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].SettlInstMode = (FixField.Character)field;
						break;
					case FixTag.NoMiscFees:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].NoMiscFees is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].NoMiscFees = (FixField.Integer)field;
						break;
					case FixTag.MiscFeeAmt:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoAllocsGroups![^1].NoMiscFeesGroups is null && NoAllocsGroups![^1].NoMiscFees is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMiscFees, field.Position, field, -1));
							(NoAllocsGroups![^1].NoMiscFeesGroups ??= []).Add(new () { MiscFeeAmt = (FixField.Decimal)field });
						}
						break;
					case FixTag.MiscFeeCurr:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0 || NoAllocsGroups![^1].NoMiscFeesGroups is null || NoAllocsGroups![^1].NoMiscFeesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].NoMiscFeesGroups![^1].MiscFeeCurr is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups![^1].NoMiscFeesGroups!.Count - 1));
						else
							NoAllocsGroups![^1].NoMiscFeesGroups![^1].MiscFeeCurr = (FixField.Text)field;
						break;
					case FixTag.MiscFeeType:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0 || NoAllocsGroups![^1].NoMiscFeesGroups is null || NoAllocsGroups![^1].NoMiscFeesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].NoMiscFeesGroups![^1].MiscFeeType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups![^1].NoMiscFeesGroups!.Count - 1));
						else
							NoAllocsGroups![^1].NoMiscFeesGroups![^1].MiscFeeType = (FixField.Character)field;
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
		public List<FixMessage.Allocation.NoOrdersGroup>? NoOrdersGroups { get; internal set; }

		/// <summary>The FIX NoExecs, tag 124, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoExecs { get; internal set; }

		/// <summary>The entries counted by NoExecs, tag 124; null when the group is absent.</summary>
		public List<FixMessage.Allocation.NoExecsGroup>? NoExecsGroups { get; internal set; }

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
		public List<FixMessage.Allocation.NoAllocsGroup>? NoAllocsGroups { get; internal set; }

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
			public List<FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup>? NoMiscFeesGroups { get; internal set; }

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
						if (NoBidDescriptorsGroups is null && NoBidDescriptors is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidDescriptors, field.Position, field, -1));
						(NoBidDescriptorsGroups ??= []).Add(new () { BidDescriptorType = (FixField.Integer)field });
						break;
					case FixTag.BidDescriptor:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].BidDescriptor is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].BidDescriptor = (FixField.Text)field;
						break;
					case FixTag.SideValueInd:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].SideValueInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].SideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LiquidityValue:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].LiquidityValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].LiquidityValue = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityNumSecurities:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].LiquidityNumSecurities is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].LiquidityNumSecurities = (FixField.Integer)field;
						break;
					case FixTag.LiquidityPctLow:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].LiquidityPctLow is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].LiquidityPctLow = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityPctHigh:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].LiquidityPctHigh is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].LiquidityPctHigh = (FixField.Decimal)field;
						break;
					case FixTag.EFPTrackingError:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].EFPTrackingError is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].EFPTrackingError = (FixField.Decimal)field;
						break;
					case FixTag.FairValue:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].FairValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].FairValue = (FixField.Decimal)field;
						break;
					case FixTag.OutsideIndexPct:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].OutsideIndexPct is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].OutsideIndexPct = (FixField.Decimal)field;
						break;
					case FixTag.ValueOfFutures:
						if (NoBidDescriptorsGroups is null || NoBidDescriptorsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidDescriptorsGroups![^1].ValueOfFutures is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidDescriptorsGroups!.Count - 1));
						else
							NoBidDescriptorsGroups![^1].ValueOfFutures = (FixField.Decimal)field;
						break;
					case FixTag.NoBidComponents: if (NoBidComponents is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoBidComponents = (FixField.Integer)field; break;
					case FixTag.ListID:
						if (NoBidComponentsGroups is null && NoBidComponents is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidComponents, field.Position, field, -1));
						(NoBidComponentsGroups ??= []).Add(new () { ListID = (FixField.Text)field });
						break;
					case FixTag.Side:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.TradingSessionID:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.NetGrossInd:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].NetGrossInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].NetGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.Account:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Account is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Account = (FixField.Text)field;
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
		public List<FixMessage.BidRequest.NoBidDescriptorsGroup>? NoBidDescriptorsGroups { get; internal set; }

		/// <summary>The FIX NoBidComponents, tag 420, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidComponents { get; internal set; }

		/// <summary>The entries counted by NoBidComponents, tag 420; null when the group is absent.</summary>
		public List<FixMessage.BidRequest.NoBidComponentsGroup>? NoBidComponentsGroups { get; internal set; }

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
						if (NoBidComponentsGroups is null && NoBidComponents is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoBidComponents, field.Position, field, -1));
						(NoBidComponentsGroups ??= []).Add(new () { Commission = (FixField.Decimal)field });
						break;
					case FixTag.CommType:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.ListID:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].ListID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].ListID = (FixField.Text)field;
						break;
					case FixTag.Country:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Country is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Country = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.PriceType:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].PriceType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].PriceType = (FixField.Integer)field;
						break;
					case FixTag.FairValue:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].FairValue is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].FairValue = (FixField.Decimal)field;
						break;
					case FixTag.NetGrossInd:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].NetGrossInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].NetGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.TradingSessionID:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoBidComponentsGroups is null || NoBidComponentsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoBidComponentsGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoBidComponentsGroups!.Count - 1));
						else
							NoBidComponentsGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.BidResponse.NoBidComponentsGroup>? NoBidComponentsGroups { get; internal set; }

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
						if (NoRoutingIDsGroups is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroups ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroups is null || NoRoutingIDsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroups![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroups!.Count - 1));
						else
							NoRoutingIDsGroups![^1].RoutingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { RelatdSym = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.OrderID: if (OrderID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); OrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (ClOrdID is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); ClOrdID = (FixField.Text)field; break;
					case FixTag.NoLinesOfText: if (LinesOfText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LinesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (LinesOfTextGroups is null && LinesOfText is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoLinesOfText, field.Position, field, -1));
						(LinesOfTextGroups ??= []).Add(new () { Text = (FixField.Text)field });
						break;
					case FixTag.EncodedTextLen:
						if (LinesOfTextGroups is null || LinesOfTextGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroups!.Count - 1));
						else
							LinesOfTextGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (LinesOfTextGroups is null || LinesOfTextGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroups!.Count - 1));
						else
							LinesOfTextGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.Email.NoRoutingIDsGroup>? NoRoutingIDsGroups { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.Email.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID { get; internal set; }

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID { get; internal set; }

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText { get; internal set; }

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public List<FixMessage.Email.LinesOfTextGroup>? LinesOfTextGroups { get; internal set; }

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
						if (NoContraBrokersGroups is null && NoContraBrokers is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoContraBrokers, field.Position, field, -1));
						(NoContraBrokersGroups ??= []).Add(new () { ContraBroker = (FixField.Text)field });
						break;
					case FixTag.ContraTrader:
						if (NoContraBrokersGroups is null || NoContraBrokersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroups![^1].ContraTrader is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroups!.Count - 1));
						else
							NoContraBrokersGroups![^1].ContraTrader = (FixField.Text)field;
						break;
					case FixTag.ContraTradeQty:
						if (NoContraBrokersGroups is null || NoContraBrokersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroups![^1].ContraTradeQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroups!.Count - 1));
						else
							NoContraBrokersGroups![^1].ContraTradeQty = (FixField.Decimal)field;
						break;
					case FixTag.ContraTradeTime:
						if (NoContraBrokersGroups is null || NoContraBrokersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoContraBrokersGroups![^1].ContraTradeTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoContraBrokersGroups!.Count - 1));
						else
							NoContraBrokersGroups![^1].ContraTradeTime = (FixField.Timestamp)field;
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
		public List<FixMessage.ExecutionReport.NoContraBrokersGroup>? NoContraBrokersGroups { get; internal set; }

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
						if (NoIOIQualifiersGroups is null && NoIOIQualifiers is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoIOIQualifiers, field.Position, field, -1));
						(NoIOIQualifiersGroups ??= []).Add(new () { IOIQualifier = (FixField.Character)field });
						break;
					case FixTag.Text: if (Text is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); Text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (EncodedTextLen is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (EncodedText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); EncodedText = (FixField.Data)field; break;
					case FixTag.TransactTime: if (TransactTime is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); TransactTime = (FixField.Timestamp)field; break;
					case FixTag.URLLink: if (URLLink is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); URLLink = (FixField.Text)field; break;
					case FixTag.NoRoutingIDs: if (NoRoutingIDs is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (NoRoutingIDsGroups is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroups ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroups is null || NoRoutingIDsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroups![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroups!.Count - 1));
						else
							NoRoutingIDsGroups![^1].RoutingID = (FixField.Text)field;
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
		public List<FixMessage.IndicationofInterest.NoIOIQualifiersGroup>? NoIOIQualifiersGroups { get; internal set; }

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
		public List<FixMessage.IndicationofInterest.NoRoutingIDsGroup>? NoRoutingIDsGroups { get; internal set; }

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
						if (NoOrdersGroups is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroups ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.CumQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CumQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CumQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdStatus:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrdStatus is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrdStatus = (FixField.Character)field;
						break;
					case FixTag.LeavesQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].LeavesQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].LeavesQty = (FixField.Decimal)field;
						break;
					case FixTag.CxlQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CxlQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CxlQty = (FixField.Decimal)field;
						break;
					case FixTag.AvgPx:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].AvgPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].AvgPx = (FixField.Decimal)field;
						break;
					case FixTag.OrdRejReason:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrdRejReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrdRejReason = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.ListStatus.NoOrdersGroup>? NoOrdersGroups { get; internal set; }

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
						if (NoStrikesGroups is null && NoStrikes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoStrikes, field.Position, field, -1));
						(NoStrikesGroups ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.ClOrdID:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].ClOrdID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].ClOrdID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoStrikesGroups is null || NoStrikesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoStrikesGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoStrikesGroups!.Count - 1));
						else
							NoStrikesGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.ListStrikePrice.NoStrikesGroup>? NoStrikesGroups { get; internal set; }

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
						if (NoMsgTypesGroups is null && NoMsgTypes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMsgTypes, field.Position, field, -1));
						(NoMsgTypesGroups ??= []).Add(new () { RefMsgType = (FixField.Text)field });
						break;
					case FixTag.MsgDirection:
						if (NoMsgTypesGroups is null || NoMsgTypesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMsgTypesGroups![^1].MsgDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMsgTypesGroups!.Count - 1));
						else
							NoMsgTypesGroups![^1].MsgDirection = (FixField.Character)field;
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
		public List<FixMessage.Logon.NoMsgTypesGroup>? NoMsgTypesGroups { get; internal set; }

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
						if (NoMDEntriesGroups is null && NoMDEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntries, field.Position, field, -1));
						(NoMDEntriesGroups ??= []).Add(new () { MDUpdateAction = (FixField.Character)field });
						break;
					case FixTag.DeleteReason:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].DeleteReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].DeleteReason = (FixField.Character)field;
						break;
					case FixTag.MDEntryType:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryType = (FixField.Character)field;
						break;
					case FixTag.MDEntryID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryRefID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryRefID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryRefID = (FixField.Text)field;
						break;
					case FixTag.Symbol:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.FinancialStatus:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].FinancialStatus is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].FinancialStatus = (FixField.Character)field;
						break;
					case FixTag.CorporateAction:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].CorporateAction is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].CorporateAction = (FixField.Character)field;
						break;
					case FixTag.MDEntryPx:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntrySize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TickDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDMkt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].QuoteCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].QuoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TradeCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryOriginator is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].LocationID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].LocationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].DeskID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].DeskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].OpenCloseSettleFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].OpenCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SellerDays is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].QuoteEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].QuoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryBuyer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntrySeller is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].NumberOfOrders is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].NumberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryPositionNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.TotalVolumeTraded:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TotalVolumeTraded is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TotalVolumeTraded = (FixField.Decimal)field;
						break;
					case FixTag.Text:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup>? NoMDEntriesGroups { get; internal set; }

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
						if (NoMDEntryTypesGroups is null && NoMDEntryTypes is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntryTypes, field.Position, field, -1));
						(NoMDEntryTypesGroups ??= []).Add(new () { MDEntryType = (FixField.Character)field });
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TradingSessionID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].TradingSessionID = (FixField.Text)field;
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
		public List<FixMessage.MarketDataRequest.NoMDEntryTypesGroup>? NoMDEntryTypesGroups { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.MarketDataRequest.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

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
						if (NoMDEntriesGroups is null && NoMDEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoMDEntries, field.Position, field, -1));
						(NoMDEntriesGroups ??= []).Add(new () { MDEntryType = (FixField.Character)field });
						break;
					case FixTag.MDEntryPx:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntrySize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TickDirection is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDMkt is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].QuoteCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].QuoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TradeCondition is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryOriginator is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].LocationID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].LocationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].DeskID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].DeskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].OpenCloseSettleFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].OpenCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].SellerDays is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].SellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].OrderID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].OrderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].QuoteEntryID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].QuoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryBuyer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntrySeller is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].NumberOfOrders is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].NumberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].MDEntryPositionNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].MDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoMDEntriesGroups is null || NoMDEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoMDEntriesGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoMDEntriesGroups!.Count - 1));
						else
							NoMDEntriesGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>? NoMDEntriesGroups { get; internal set; }

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
						if (NoQuoteSetsGroups is null && NoQuoteSets is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteSets, field.Position, field, -1));
						(NoQuoteSetsGroups ??= []).Add(new () { QuoteSetID = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteSetValidUntilTime:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].QuoteSetValidUntilTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].QuoteSetValidUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].TotQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].TotQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null && NoQuoteSetsGroups![^1].NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
							(NoQuoteSetsGroups![^1].NoQuoteEntriesGroups ??= []).Add(new () { QuoteEntryID = (FixField.Text)field });
						}
						break;
					case FixTag.Symbol:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.BidPx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidPx = (FixField.Decimal)field;
						break;
					case FixTag.OfferPx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferPx = (FixField.Decimal)field;
						break;
					case FixTag.BidSize:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidSize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidSize = (FixField.Decimal)field;
						break;
					case FixTag.OfferSize:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferSize is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferSize = (FixField.Decimal)field;
						break;
					case FixTag.ValidUntilTime:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ValidUntilTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ValidUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.BidSpotRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidSpotRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.OfferSpotRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferSpotRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.BidForwardPoints:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidForwardPoints is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].BidForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.OfferForwardPoints:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferForwardPoints is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OfferForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.TransactTime:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.TradingSessionID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.SettlDate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Currency = (FixField.Text)field;
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
		public List<FixMessage.MassQuote.NoQuoteSetsGroup>? NoQuoteSetsGroups { get; internal set; }

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
			public List<FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups { get; internal set; }

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
						if (NoOrdersGroups is null && NoOrders is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoOrders, field.Position, field, -1));
						(NoOrdersGroups ??= []).Add(new () { ClOrdID = (FixField.Text)field });
						break;
					case FixTag.ListSeqNo:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ListSeqNo is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ListSeqNo = (FixField.Integer)field;
						break;
					case FixTag.SettlInstMode:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SettlInstMode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SettlInstMode = (FixField.Character)field;
						break;
					case FixTag.ClientID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ClientID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ClientID = (FixField.Text)field;
						break;
					case FixTag.ExecBroker:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ExecBroker is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ExecBroker = (FixField.Text)field;
						break;
					case FixTag.Account:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Account is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Account = (FixField.Text)field;
						break;
					case FixTag.NoAllocs:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].NoAllocs is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].NoAllocs = (FixField.Integer)field;
						break;
					case FixTag.AllocAccount:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoOrdersGroups![^1].NoAllocsGroups is null && NoOrdersGroups![^1].NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
							(NoOrdersGroups![^1].NoAllocsGroups ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						}
						break;
					case FixTag.AllocQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0 || NoOrdersGroups![^1].NoAllocsGroups is null || NoOrdersGroups![^1].NoAllocsGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].NoAllocsGroups![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups![^1].NoAllocsGroups!.Count - 1));
						else
							NoOrdersGroups![^1].NoAllocsGroups![^1].AllocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SettlmntTyp is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SettlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.HandlInst:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].HandlInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].HandlInst = (FixField.Character)field;
						break;
					case FixTag.ExecInst:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ExecInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ExecInst = (FixField.Multiple)field;
						break;
					case FixTag.MinQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].MinQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].MinQty = (FixField.Decimal)field;
						break;
					case FixTag.MaxFloor:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].MaxFloor is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].MaxFloor = (FixField.Decimal)field;
						break;
					case FixTag.ExDestination:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ExDestination is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ExDestination = (FixField.Text)field;
						break;
					case FixTag.NoTradingSessions:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].NoTradingSessions is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].NoTradingSessions = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoOrdersGroups![^1].NoTradingSessionsGroups is null && NoOrdersGroups![^1].NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
							(NoOrdersGroups![^1].NoTradingSessionsGroups ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
						}
						break;
					case FixTag.ProcessCode:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ProcessCode is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ProcessCode = (FixField.Character)field;
						break;
					case FixTag.Symbol:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.SideValueInd:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SideValueInd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LocateReqd:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].LocateReqd is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].LocateReqd = (FixField.Boolean)field;
						break;
					case FixTag.TransactTime:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.OrderQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrderQty = (FixField.Decimal)field;
						break;
					case FixTag.CashOrderQty:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CashOrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CashOrderQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdType:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Price is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Price = (FixField.Decimal)field;
						break;
					case FixTag.StopPx:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].StopPx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].StopPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Currency = (FixField.Text)field;
						break;
					case FixTag.ComplianceID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ComplianceID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ComplianceID = (FixField.Text)field;
						break;
					case FixTag.SolicitedFlag:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SolicitedFlag is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SolicitedFlag = (FixField.Boolean)field;
						break;
					case FixTag.IOIID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].IOIid is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].IOIid = (FixField.Text)field;
						break;
					case FixTag.QuoteID:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].QuoteID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].QuoteID = (FixField.Text)field;
						break;
					case FixTag.TimeInForce:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].TimeInForce is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].TimeInForce = (FixField.Character)field;
						break;
					case FixTag.EffectiveTime:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EffectiveTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EffectiveTime = (FixField.Timestamp)field;
						break;
					case FixTag.ExpireDate:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ExpireDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ExpireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.GTBookingInst:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].GTBookingInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].GTBookingInst = (FixField.Integer)field;
						break;
					case FixTag.Commission:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Commission is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CommType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CommType = (FixField.Character)field;
						break;
					case FixTag.Rule80A:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Rule80A is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Rule80A = (FixField.Character)field;
						break;
					case FixTag.ForexReq:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ForexReq is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ForexReq = (FixField.Boolean)field;
						break;
					case FixTag.SettlCurrency:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].SettlCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].SettlCurrency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].Text is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].Text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].EncodedText = (FixField.Data)field;
						break;
					case FixTag.SettlDate2:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.PositionEffect:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].OpenClose is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].OpenClose = (FixField.Character)field;
						break;
					case FixTag.CoveredOrUncovered:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CoveredOrUncovered is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CoveredOrUncovered = (FixField.Integer)field;
						break;
					case FixTag.CustomerOrFirm:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].CustomerOrFirm is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].CustomerOrFirm = (FixField.Integer)field;
						break;
					case FixTag.MaxShow:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].MaxShow is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].MaxShow = (FixField.Decimal)field;
						break;
					case FixTag.PegOffsetValue:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].PegDifference is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].PegDifference = (FixField.Decimal)field;
						break;
					case FixTag.DiscretionInst:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].DiscretionInst is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].DiscretionInst = (FixField.Character)field;
						break;
					case FixTag.DiscretionOffsetValue:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].DiscretionOffset is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].DiscretionOffset = (FixField.Decimal)field;
						break;
					case FixTag.ClearingFirm:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ClearingFirm is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ClearingFirm = (FixField.Text)field;
						break;
					case FixTag.ClearingAccount:
						if (NoOrdersGroups is null || NoOrdersGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoOrdersGroups![^1].ClearingAccount is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoOrdersGroups!.Count - 1));
						else
							NoOrdersGroups![^1].ClearingAccount = (FixField.Text)field;
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
		public List<FixMessage.NewOrderList.NoOrdersGroup>? NoOrdersGroups { get; internal set; }

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
			public List<FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup>? NoAllocsGroups { get; internal set; }

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
			public List<FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup>? NoTradingSessionsGroups { get; internal set; }

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
						if (NoAllocsGroups is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroups ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocShares = (FixField.Decimal)field;
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
						if (NoTradingSessionsGroups is null && NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
						(NoTradingSessionsGroups ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
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
		public List<FixMessage.NewOrderSingle.NoAllocsGroup>? NoAllocsGroups { get; internal set; }

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
		public List<FixMessage.NewOrderSingle.NoTradingSessionsGroup>? NoTradingSessionsGroups { get; internal set; }

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
						if (NoRoutingIDsGroups is null && NoRoutingIDs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRoutingIDs, field.Position, field, -1));
						(NoRoutingIDsGroups ??= []).Add(new () { RoutingType = (FixField.Integer)field });
						break;
					case FixTag.RoutingID:
						if (NoRoutingIDsGroups is null || NoRoutingIDsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRoutingIDsGroups![^1].RoutingID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRoutingIDsGroups!.Count - 1));
						else
							NoRoutingIDsGroups![^1].RoutingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (NoRelatedSym is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); NoRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { RelatdSym = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.NoLinesOfText: if (LinesOfText is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); LinesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (LinesOfTextGroups is null && LinesOfText is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoLinesOfText, field.Position, field, -1));
						(LinesOfTextGroups ??= []).Add(new () { Text = (FixField.Text)field });
						break;
					case FixTag.EncodedTextLen:
						if (LinesOfTextGroups is null || LinesOfTextGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroups![^1].EncodedTextLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroups!.Count - 1));
						else
							LinesOfTextGroups![^1].EncodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (LinesOfTextGroups is null || LinesOfTextGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (LinesOfTextGroups![^1].EncodedText is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, LinesOfTextGroups!.Count - 1));
						else
							LinesOfTextGroups![^1].EncodedText = (FixField.Data)field;
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
		public List<FixMessage.News.NoRoutingIDsGroup>? NoRoutingIDsGroups { get; internal set; }

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym { get; internal set; }

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.News.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText { get; internal set; }

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public List<FixMessage.News.LinesOfTextGroup>? LinesOfTextGroups { get; internal set; }

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
						if (NoAllocsGroups is null && NoAllocs is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoAllocs, field.Position, field, -1));
						(NoAllocsGroups ??= []).Add(new () { AllocAccount = (FixField.Text)field });
						break;
					case FixTag.AllocQty:
						if (NoAllocsGroups is null || NoAllocsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoAllocsGroups![^1].AllocShares is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoAllocsGroups!.Count - 1));
						else
							NoAllocsGroups![^1].AllocShares = (FixField.Decimal)field;
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
						if (NoTradingSessionsGroups is null && NoTradingSessions is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoTradingSessions, field.Position, field, -1));
						(NoTradingSessionsGroups ??= []).Add(new () { TradingSessionID = (FixField.Text)field });
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
		public List<FixMessage.OrderCancelReplaceRequest.NoAllocsGroup>? NoAllocsGroups { get; internal set; }

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
		public List<FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup>? NoTradingSessionsGroups { get; internal set; }

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
						if (NoQuoteSetsGroups is null && NoQuoteSets is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteSets, field.Position, field, -1));
						(NoQuoteSetsGroups ??= []).Add(new () { QuoteSetID = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].TotQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].TotQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntries is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else
						{
							if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null && NoQuoteSetsGroups![^1].NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
							(NoQuoteSetsGroups![^1].NoQuoteEntriesGroups ??= []).Add(new () { QuoteEntryID = (FixField.Text)field });
						}
						break;
					case FixTag.Symbol:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Symbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteEntryRejectReason:
						if (NoQuoteSetsGroups is null || NoQuoteSetsGroups.Count == 0 || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups is null || NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].QuoteEntryRejectReason is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteSetsGroups![^1].NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteSetsGroups![^1].NoQuoteEntriesGroups![^1].QuoteEntryRejectReason = (FixField.Integer)field;
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
		public List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup>? NoQuoteSetsGroups { get; internal set; }

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
			public List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups { get; internal set; }

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
						if (NoQuoteEntriesGroups is null && NoQuoteEntries is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoQuoteEntries, field.Position, field, -1));
						(NoQuoteEntriesGroups ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSymbol:
						if (NoQuoteEntriesGroups is null || NoQuoteEntriesGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoQuoteEntriesGroups![^1].UnderlyingSymbol is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoQuoteEntriesGroups!.Count - 1));
						else
							NoQuoteEntriesGroups![^1].UnderlyingSymbol = (FixField.Text)field;
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
		public List<FixMessage.QuoteCancel.NoQuoteEntriesGroup>? NoQuoteEntriesGroups { get; internal set; }

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
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { Symbol = (FixField.Text)field });
						break;
					case FixTag.SymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SymbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].IDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].IDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].MaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].MaturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].PutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].PutOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].StrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].StrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OptAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].ContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].ContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].CouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].CouponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Issuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].SecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].SecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].PrevClosePx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].PrevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.QuoteRequestType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].QuoteRequestType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].QuoteRequestType = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].TradingSessionID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].TradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.OrderQty:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OrderQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OrderQty = (FixField.Decimal)field;
						break;
					case FixTag.SettlDate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].FutSettDate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].FutSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OrdType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OrdType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].FutSettDate2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].FutSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].OrderQty2 is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].OrderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.ExpireTime:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].ExpireTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].ExpireTime = (FixField.Timestamp)field;
						break;
					case FixTag.TransactTime:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].TransactTime is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].TransactTime = (FixField.Timestamp)field;
						break;
					case FixTag.Currency:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Currency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Currency = (FixField.Text)field;
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
		public List<FixMessage.QuoteRequest.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

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
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { UnderlyingSymbol = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].RatioQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].RatioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingCurrency = (FixField.Text)field;
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
		public List<FixMessage.SecurityDefinition.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

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
						if (NoRelatedSymGroups is null && NoRelatedSym is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.NoRelatedSym, field.Position, field, -1));
						(NoRelatedSymGroups ??= []).Add(new () { UnderlyingSymbol = (FixField.Text)field });
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSymbolSfx is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityID is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingIDSource is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityType is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingMaturityMonthYear is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingMaturityDay is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingPutOrCall is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingStrikePrice is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingOptAttribute is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingContractMultiplier is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingCouponRate is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityExchange is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingIssuerLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingIssuer is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDescLen is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDesc is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].EncodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].RatioQty is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].RatioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].Side is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].Side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (NoRelatedSymGroups is null || NoRelatedSymGroups.Count == 0)
							AddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));
						else if (NoRelatedSymGroups![^1].UnderlyingCurrency is not null)
							AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, NoRelatedSymGroups!.Count - 1));
						else
							NoRelatedSymGroups![^1].UnderlyingCurrency = (FixField.Text)field;
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
		public List<FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup>? NoRelatedSymGroups { get; internal set; }

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
