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
					case FixTag.AdvId: if (_advId is not null) Duplicate(field); _advId = (FixField.Text)field; break;
					case FixTag.AdvTransType: if (_advTransType is not null) Duplicate(field); _advTransType = (FixField.Text)field; break;
					case FixTag.AdvRefID: if (_advRefID is not null) Duplicate(field); _advRefID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.AdvSide: if (_advSide is not null) Duplicate(field); _advSide = (FixField.Character)field; break;
					case FixTag.Quantity: if (_shares is not null) Duplicate(field); _shares = (FixField.Decimal)field; break;
					case FixTag.Price: if (_price is not null) Duplicate(field); _price = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.URLLink: if (_uRLLink is not null) Duplicate(field); _uRLLink = (FixField.Text)field; break;
					case FixTag.LastMkt: if (_lastMkt is not null) Duplicate(field); _lastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX AdvId, tag 2, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvId => _advId;
		internal FixField.Text? _advId;

		/// <summary>The FIX AdvTransType, tag 5, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvTransType => _advTransType;
		internal FixField.Text? _advTransType;

		/// <summary>The FIX AdvRefID, tag 3, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AdvRefID => _advRefID;
		internal FixField.Text? _advRefID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX AdvSide, tag 4, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? AdvSide => _advSide;
		internal FixField.Character? _advSide;

		/// <summary>The FIX Shares, tag 53, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? Shares => _shares;
		internal FixField.Decimal? _shares;

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price => _price;
		internal FixField.Decimal? _price;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink => _uRLLink;
		internal FixField.Text? _uRLLink;

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt => _lastMkt;
		internal FixField.Text? _lastMkt;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Advertisement(context, check, this);
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
					case FixTag.AllocID: if (_allocID is not null) Duplicate(field); _allocID = (FixField.Text)field; break;
					case FixTag.AllocTransType: if (_allocTransType is not null) Duplicate(field); _allocTransType = (FixField.Character)field; break;
					case FixTag.RefAllocID: if (_refAllocID is not null) Duplicate(field); _refAllocID = (FixField.Text)field; break;
					case FixTag.AllocLinkID: if (_allocLinkID is not null) Duplicate(field); _allocLinkID = (FixField.Text)field; break;
					case FixTag.AllocLinkType: if (_allocLinkType is not null) Duplicate(field); _allocLinkType = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (_noOrders is not null) Duplicate(field); _noOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (noOrdersGroups is null && _noOrders is null) Uncounted(FixTag.NoOrders, field);
						(noOrdersGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.OrderID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._orderID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._orderID = (FixField.Text)field;
						break;
					case FixTag.SecondaryOrderID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._secondaryOrderID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._secondaryOrderID = (FixField.Text)field;
						break;
					case FixTag.ListID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._listID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._listID = (FixField.Text)field;
						break;
					case FixTag.WaveNo:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._waveNo is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._waveNo = (FixField.Text)field;
						break;
					case FixTag.NoExecs: if (_noExecs is not null) Duplicate(field); _noExecs = (FixField.Integer)field; break;
					case FixTag.LastQty:
						if (noExecsGroups is null && _noExecs is null) Uncounted(FixTag.NoExecs, field);
						(noExecsGroups ??= []).Add(new ((FixField.Decimal)field));
						break;
					case FixTag.ExecID:
						if (None(noExecsGroups))
							Unentered(field);
						else if (noExecsGroups![^1]._execID is not null)
							Duplicate(field, FixTag.NoExecs, noExecsGroups!.Count - 1);
						else
							noExecsGroups![^1]._execID = (FixField.Text)field;
						break;
					case FixTag.LastPx:
						if (None(noExecsGroups))
							Unentered(field);
						else if (noExecsGroups![^1]._lastPx is not null)
							Duplicate(field, FixTag.NoExecs, noExecsGroups!.Count - 1);
						else
							noExecsGroups![^1]._lastPx = (FixField.Decimal)field;
						break;
					case FixTag.LastCapacity:
						if (None(noExecsGroups))
							Unentered(field);
						else if (noExecsGroups![^1]._lastCapacity is not null)
							Duplicate(field, FixTag.NoExecs, noExecsGroups!.Count - 1);
						else
							noExecsGroups![^1]._lastCapacity = (FixField.Character)field;
						break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Quantity: if (_shares is not null) Duplicate(field); _shares = (FixField.Decimal)field; break;
					case FixTag.LastMkt: if (_lastMkt is not null) Duplicate(field); _lastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.AvgPx: if (_avgPx is not null) Duplicate(field); _avgPx = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.AvgPxPrecision: if (_avgPrxPrecision is not null) Duplicate(field); _avgPrxPrecision = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.SettlType: if (_settlmntTyp is not null) Duplicate(field); _settlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (_futSettDate is not null) Duplicate(field); _futSettDate = (FixField.Date)field; break;
					case FixTag.GrossTradeAmt: if (_grossTradeAmt is not null) Duplicate(field); _grossTradeAmt = (FixField.Decimal)field; break;
					case FixTag.NetMoney: if (_netMoney is not null) Duplicate(field); _netMoney = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (_openClose is not null) Duplicate(field); _openClose = (FixField.Character)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.NumDaysInterest: if (_numDaysInterest is not null) Duplicate(field); _numDaysInterest = (FixField.Integer)field; break;
					case FixTag.AccruedInterestRate: if (_accruedInterestRate is not null) Duplicate(field); _accruedInterestRate = (FixField.Decimal)field; break;
					case FixTag.NoAllocs: if (_noAllocs is not null) Duplicate(field); _noAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (noAllocsGroups is null && _noAllocs is null) Uncounted(FixTag.NoAllocs, field);
						(noAllocsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.AllocPrice:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocPrice is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocPrice = (FixField.Decimal)field;
						break;
					case FixTag.AllocQty:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocShares is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocShares = (FixField.Decimal)field;
						break;
					case FixTag.ProcessCode:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._processCode is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._processCode = (FixField.Character)field;
						break;
					case FixTag.BrokerOfCredit:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._brokerOfCredit is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._brokerOfCredit = (FixField.Text)field;
						break;
					case FixTag.NotifyBrokerOfCredit:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._notifyBrokerOfCredit is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._notifyBrokerOfCredit = (FixField.Boolean)field;
						break;
					case FixTag.AllocHandlInst:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocHandlInst is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocHandlInst = (FixField.Integer)field;
						break;
					case FixTag.AllocText:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocText is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocText = (FixField.Text)field;
						break;
					case FixTag.EncodedAllocTextLen:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._encodedAllocTextLen is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._encodedAllocTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedAllocText:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._encodedAllocText is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._encodedAllocText = (FixField.Data)field;
						break;
					case FixTag.ExecBroker:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._execBroker is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._execBroker = (FixField.Text)field;
						break;
					case FixTag.ClientID:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._clientID is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._clientID = (FixField.Text)field;
						break;
					case FixTag.Commission:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._commission is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._commType is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._commType = (FixField.Character)field;
						break;
					case FixTag.AllocAvgPx:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocAvgPx is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocAvgPx = (FixField.Decimal)field;
						break;
					case FixTag.AllocNetMoney:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocNetMoney is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocNetMoney = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrAmt:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._settlCurrAmt is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._settlCurrAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrency:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._settlCurrency is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._settlCurrency = (FixField.Text)field;
						break;
					case FixTag.SettlCurrFxRate:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._settlCurrFxRate is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._settlCurrFxRate = (FixField.Decimal)field;
						break;
					case FixTag.SettlCurrFxRateCalc:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._settlCurrFxRateCalc is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._settlCurrFxRateCalc = (FixField.Character)field;
						break;
					case FixTag.AccruedInterestAmt:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._accruedInterestAmt is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._accruedInterestAmt = (FixField.Decimal)field;
						break;
					case FixTag.SettlInstMode:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._settlInstMode is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._settlInstMode = (FixField.Character)field;
						break;
					case FixTag.NoMiscFees:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._noMiscFees is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._noMiscFees = (FixField.Integer)field;
						break;
					case FixTag.MiscFeeAmt:
						if (None(noAllocsGroups))
							Unentered(field);
						else
						{
							if (noAllocsGroups![^1].noMiscFeesGroups is null && noAllocsGroups![^1]._noMiscFees is null) Uncounted(FixTag.NoMiscFees, field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
							(noAllocsGroups![^1].noMiscFeesGroups ??= []).Add(new ((FixField.Decimal)field));
						}
						break;
					case FixTag.MiscFeeCurr:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (None(noAllocsGroups![^1].noMiscFeesGroups))
							Unentered(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else if (noAllocsGroups![^1].noMiscFeesGroups![^1]._miscFeeCurr is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1, FixTag.NoMiscFees, noAllocsGroups![^1].noMiscFeesGroups!.Count - 1);
						else
							noAllocsGroups![^1].noMiscFeesGroups![^1]._miscFeeCurr = (FixField.Text)field;
						break;
					case FixTag.MiscFeeType:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (None(noAllocsGroups![^1].noMiscFeesGroups))
							Unentered(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else if (noAllocsGroups![^1].noMiscFeesGroups![^1]._miscFeeType is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1, FixTag.NoMiscFees, noAllocsGroups![^1].noMiscFeesGroups!.Count - 1);
						else
							noAllocsGroups![^1].noMiscFeesGroups![^1]._miscFeeType = (FixField.Character)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID => _allocID;
		internal FixField.Text? _allocID;

		/// <summary>The FIX AllocTransType, tag 71, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? AllocTransType => _allocTransType;
		internal FixField.Character? _allocTransType;

		/// <summary>The FIX RefAllocID, tag 72, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefAllocID => _refAllocID;
		internal FixField.Text? _refAllocID;

		/// <summary>The FIX AllocLinkID, tag 196, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocLinkID => _allocLinkID;
		internal FixField.Text? _allocLinkID;

		/// <summary>The FIX AllocLinkType, tag 197, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocLinkType => _allocLinkType;
		internal FixField.Integer? _allocLinkType;

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders => _noOrders;
		internal FixField.Integer? _noOrders;

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public List<FixMessage.Allocation.NoOrdersGroup>? NoOrdersGroups => noOrdersGroups;
		internal List<NoOrdersGroup>? noOrdersGroups;

		/// <summary>The FIX NoExecs, tag 124, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoExecs => _noExecs;
		internal FixField.Integer? _noExecs;

		/// <summary>The entries counted by NoExecs, tag 124; null when the group is absent.</summary>
		public List<FixMessage.Allocation.NoExecsGroup>? NoExecsGroups => noExecsGroups;
		internal List<NoExecsGroup>? noExecsGroups;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Shares, tag 53, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? Shares => _shares;
		internal FixField.Decimal? _shares;

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt => _lastMkt;
		internal FixField.Text? _lastMkt;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? AvgPx => _avgPx;
		internal FixField.Decimal? _avgPx;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX AvgPrxPrecision, tag 74, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AvgPrxPrecision => _avgPrxPrecision;
		internal FixField.Integer? _avgPrxPrecision;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp => _settlmntTyp;
		internal FixField.Character? _settlmntTyp;

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate => _futSettDate;
		internal FixField.Date? _futSettDate;

		/// <summary>The FIX GrossTradeAmt, tag 381, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? GrossTradeAmt => _grossTradeAmt;
		internal FixField.Decimal? _grossTradeAmt;

		/// <summary>The FIX NetMoney, tag 118, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? NetMoney => _netMoney;
		internal FixField.Decimal? _netMoney;

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose => _openClose;
		internal FixField.Character? _openClose;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX NumDaysInterest, tag 157, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumDaysInterest => _numDaysInterest;
		internal FixField.Integer? _numDaysInterest;

		/// <summary>The FIX AccruedInterestRate, tag 158, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? AccruedInterestRate => _accruedInterestRate;
		internal FixField.Decimal? _accruedInterestRate;

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs => _noAllocs;
		internal FixField.Integer? _noAllocs;

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public List<FixMessage.Allocation.NoAllocsGroup>? NoAllocsGroups => noAllocsGroups;
		internal List<NoAllocsGroup>? noAllocsGroups;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup : IFixEntry
		{
			internal NoOrdersGroup(FixField.Text delimiter)
			{
				ClOrdID = delimiter;
			}

			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text ClOrdID { get; }

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID => _orderID;
			internal FixField.Text? _orderID;

			/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecondaryOrderID => _secondaryOrderID;
			internal FixField.Text? _secondaryOrderID;

			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ListID => _listID;
			internal FixField.Text? _listID;

			/// <summary>The FIX WaveNo, tag 105, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? WaveNo => _waveNo;
			internal FixField.Text? _waveNo;

			FixField IFixEntry.Opener => ClOrdID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 11 or 37 or 66 or 105 or 198;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoExecs, tag 124.</summary>
		public sealed class NoExecsGroup : IFixEntry
		{
			internal NoExecsGroup(FixField.Decimal delimiter)
			{
				LastShares = delimiter;
			}

			/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Decimal LastShares { get; }

			/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecID => _execID;
			internal FixField.Text? _execID;

			/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? LastPx => _lastPx;
			internal FixField.Decimal? _lastPx;

			/// <summary>The FIX LastCapacity, tag 29, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? LastCapacity => _lastCapacity;
			internal FixField.Character? _lastCapacity;

			FixField IFixEntry.Opener => LastShares;

			bool IFixEntry.Has(int tag)
			{
				return tag is 17 or 29 or 31 or 32;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup : IFixEntry
		{
			internal NoAllocsGroup(FixField.Text delimiter)
			{
				AllocAccount = delimiter;
			}

			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text AllocAccount { get; }

			/// <summary>The FIX AllocPrice, tag 366, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocPrice => _allocPrice;
			internal FixField.Decimal? _allocPrice;

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares => _allocShares;
			internal FixField.Decimal? _allocShares;

			/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? ProcessCode => _processCode;
			internal FixField.Character? _processCode;

			/// <summary>The FIX BrokerOfCredit, tag 92, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? BrokerOfCredit => _brokerOfCredit;
			internal FixField.Text? _brokerOfCredit;

			/// <summary>The FIX NotifyBrokerOfCredit, tag 208, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? NotifyBrokerOfCredit => _notifyBrokerOfCredit;
			internal FixField.Boolean? _notifyBrokerOfCredit;

			/// <summary>The FIX AllocHandlInst, tag 209, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? AllocHandlInst => _allocHandlInst;
			internal FixField.Integer? _allocHandlInst;

			/// <summary>The FIX AllocText, tag 161, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? AllocText => _allocText;
			internal FixField.Text? _allocText;

			/// <summary>The FIX EncodedAllocTextLen, tag 360, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedAllocTextLen => _encodedAllocTextLen;
			internal FixField.Integer? _encodedAllocTextLen;

			/// <summary>The FIX EncodedAllocText, tag 361, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedAllocText => _encodedAllocText;
			internal FixField.Data? _encodedAllocText;

			/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecBroker => _execBroker;
			internal FixField.Text? _execBroker;

			/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClientID => _clientID;
			internal FixField.Text? _clientID;

			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? Commission => _commission;
			internal FixField.Decimal? _commission;

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType => _commType;
			internal FixField.Character? _commType;

			/// <summary>The FIX AllocAvgPx, tag 153, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocAvgPx => _allocAvgPx;
			internal FixField.Decimal? _allocAvgPx;

			/// <summary>The FIX AllocNetMoney, tag 154, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocNetMoney => _allocNetMoney;
			internal FixField.Decimal? _allocNetMoney;

			/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? SettlCurrAmt => _settlCurrAmt;
			internal FixField.Decimal? _settlCurrAmt;

			/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? SettlCurrency => _settlCurrency;
			internal FixField.Text? _settlCurrency;

			/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? SettlCurrFxRate => _settlCurrFxRate;
			internal FixField.Decimal? _settlCurrFxRate;

			/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlCurrFxRateCalc => _settlCurrFxRateCalc;
			internal FixField.Character? _settlCurrFxRateCalc;

			/// <summary>The FIX AccruedInterestAmt, tag 159, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? AccruedInterestAmt => _accruedInterestAmt;
			internal FixField.Decimal? _accruedInterestAmt;

			/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlInstMode => _settlInstMode;
			internal FixField.Character? _settlInstMode;

			/// <summary>The FIX NoMiscFees, tag 136, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoMiscFees => _noMiscFees;
			internal FixField.Integer? _noMiscFees;

			/// <summary>The entries counted by NoMiscFees, tag 136; null when the group is absent.</summary>
			public List<FixMessage.Allocation.NoAllocsGroup.NoMiscFeesGroup>? NoMiscFeesGroups => noMiscFeesGroups;
			internal List<NoMiscFeesGroup>? noMiscFeesGroups;

			/// <summary>One entry of the group counted by NoMiscFees, tag 136.</summary>
			public sealed class NoMiscFeesGroup : IFixEntry
			{
				internal NoMiscFeesGroup(FixField.Decimal delimiter)
				{
					MiscFeeAmt = delimiter;
				}

				/// <summary>The FIX MiscFeeAmt, tag 137, wire type <c>Amt</c>: the entry's delimiter, its first field, which every entry has.</summary>
				public FixField.Decimal MiscFeeAmt { get; }

				/// <summary>The FIX MiscFeeCurr, tag 138, wire type <c>Currency</c>; null when the field is absent.</summary>
				public FixField.Text? MiscFeeCurr => _miscFeeCurr;
				internal FixField.Text? _miscFeeCurr;

				/// <summary>The FIX MiscFeeType, tag 139, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? MiscFeeType => _miscFeeType;
				internal FixField.Character? _miscFeeType;

				FixField IFixEntry.Opener => MiscFeeAmt;

				bool IFixEntry.Has(int tag)
				{
					return tag is 137 or 138 or 139;
				}

				IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
				{
					step = default;

					return null;
				}
			}

			FixField IFixEntry.Opener => AllocAccount;

			bool IFixEntry.Has(int tag)
			{
				return tag is 12 or 13 or 76 or 79 or 80 or 81 or 92 or 109 or 119 or 120 or 136 or 137 or 138 or 139 or 153 or
					154 or 155 or 156 or 159 or 160 or 161 or 208 or 209 or 360 or 361 or 366;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				return FixEntries.Holding(NoMiscFeesGroups, FixTag.NoMiscFees, tag, position, out step);
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoOrdersGroups, FixTag.NoOrders, tag, position, out step) ??
				FixEntries.Holding(NoExecsGroups, FixTag.NoExecs, tag, position, out step) ??
				FixEntries.Holding(NoAllocsGroups, FixTag.NoAllocs, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Allocation(context, check, this);
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
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.AllocID: if (_allocID is not null) Duplicate(field); _allocID = (FixField.Text)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.AllocStatus: if (_allocStatus is not null) Duplicate(field); _allocStatus = (FixField.Integer)field; break;
					case FixTag.AllocRejCode: if (_allocRejCode is not null) Duplicate(field); _allocRejCode = (FixField.Integer)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID => _allocID;
		internal FixField.Text? _allocID;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX AllocStatus, tag 87, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocStatus => _allocStatus;
		internal FixField.Integer? _allocStatus;

		/// <summary>The FIX AllocRejCode, tag 88, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? AllocRejCode => _allocRejCode;
		internal FixField.Integer? _allocRejCode;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.AllocationACK(context, check, this);
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
					case FixTag.BidID: if (_bidID is not null) Duplicate(field); _bidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (_clientBidID is not null) Duplicate(field); _clientBidID = (FixField.Text)field; break;
					case FixTag.BidRequestTransType: if (_bidRequestTransType is not null) Duplicate(field); _bidRequestTransType = (FixField.Character)field; break;
					case FixTag.ListName: if (_listName is not null) Duplicate(field); _listName = (FixField.Text)field; break;
					case FixTag.TotNoRelatedSym: if (_totalNumSecurities is not null) Duplicate(field); _totalNumSecurities = (FixField.Integer)field; break;
					case FixTag.BidType: if (_bidType is not null) Duplicate(field); _bidType = (FixField.Integer)field; break;
					case FixTag.NumTickets: if (_numTickets is not null) Duplicate(field); _numTickets = (FixField.Integer)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.SideValue1: if (_sideValue1 is not null) Duplicate(field); _sideValue1 = (FixField.Decimal)field; break;
					case FixTag.SideValue2: if (_sideValue2 is not null) Duplicate(field); _sideValue2 = (FixField.Decimal)field; break;
					case FixTag.NoBidDescriptors: if (_noBidDescriptors is not null) Duplicate(field); _noBidDescriptors = (FixField.Integer)field; break;
					case FixTag.BidDescriptorType:
						if (noBidDescriptorsGroups is null && _noBidDescriptors is null) Uncounted(FixTag.NoBidDescriptors, field);
						(noBidDescriptorsGroups ??= []).Add(new ((FixField.Integer)field));
						break;
					case FixTag.BidDescriptor:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._bidDescriptor is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._bidDescriptor = (FixField.Text)field;
						break;
					case FixTag.SideValueInd:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._sideValueInd is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._sideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LiquidityValue:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._liquidityValue is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._liquidityValue = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityNumSecurities:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._liquidityNumSecurities is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._liquidityNumSecurities = (FixField.Integer)field;
						break;
					case FixTag.LiquidityPctLow:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._liquidityPctLow is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._liquidityPctLow = (FixField.Decimal)field;
						break;
					case FixTag.LiquidityPctHigh:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._liquidityPctHigh is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._liquidityPctHigh = (FixField.Decimal)field;
						break;
					case FixTag.EFPTrackingError:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._eFPTrackingError is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._eFPTrackingError = (FixField.Decimal)field;
						break;
					case FixTag.FairValue:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._fairValue is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._fairValue = (FixField.Decimal)field;
						break;
					case FixTag.OutsideIndexPct:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._outsideIndexPct is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._outsideIndexPct = (FixField.Decimal)field;
						break;
					case FixTag.ValueOfFutures:
						if (None(noBidDescriptorsGroups))
							Unentered(field);
						else if (noBidDescriptorsGroups![^1]._valueOfFutures is not null)
							Duplicate(field, FixTag.NoBidDescriptors, noBidDescriptorsGroups!.Count - 1);
						else
							noBidDescriptorsGroups![^1]._valueOfFutures = (FixField.Decimal)field;
						break;
					case FixTag.NoBidComponents: if (_noBidComponents is not null) Duplicate(field); _noBidComponents = (FixField.Integer)field; break;
					case FixTag.ListID:
						if (noBidComponentsGroups is null && _noBidComponents is null) Uncounted(FixTag.NoBidComponents, field);
						(noBidComponentsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.Side:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.NetGrossInd:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._netGrossInd is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._netGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._settlmntTyp is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._settlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._futSettDate is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._futSettDate = (FixField.Date)field;
						break;
					case FixTag.Account:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._account is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._account = (FixField.Text)field;
						break;
					case FixTag.LiquidityIndType: if (_liquidityIndType is not null) Duplicate(field); _liquidityIndType = (FixField.Integer)field; break;
					case FixTag.WtAverageLiquidity: if (_wtAverageLiquidity is not null) Duplicate(field); _wtAverageLiquidity = (FixField.Decimal)field; break;
					case FixTag.ExchangeForPhysical: if (_exchangeForPhysical is not null) Duplicate(field); _exchangeForPhysical = (FixField.Boolean)field; break;
					case FixTag.OutMainCntryUIndex: if (_outMainCntryUIndex is not null) Duplicate(field); _outMainCntryUIndex = (FixField.Decimal)field; break;
					case FixTag.CrossPercent: if (_crossPercent is not null) Duplicate(field); _crossPercent = (FixField.Decimal)field; break;
					case FixTag.ProgRptReqs: if (_progRptReqs is not null) Duplicate(field); _progRptReqs = (FixField.Integer)field; break;
					case FixTag.ProgPeriodInterval: if (_progPeriodInterval is not null) Duplicate(field); _progPeriodInterval = (FixField.Integer)field; break;
					case FixTag.IncTaxInd: if (_incTaxInd is not null) Duplicate(field); _incTaxInd = (FixField.Integer)field; break;
					case FixTag.ForexReq: if (_forexReq is not null) Duplicate(field); _forexReq = (FixField.Boolean)field; break;
					case FixTag.NumBidders: if (_numBidders is not null) Duplicate(field); _numBidders = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.BidTradeType: if (_tradeType is not null) Duplicate(field); _tradeType = (FixField.Character)field; break;
					case FixTag.BasisPxType: if (_basisPxType is not null) Duplicate(field); _basisPxType = (FixField.Character)field; break;
					case FixTag.StrikeTime: if (_strikeTime is not null) Duplicate(field); _strikeTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID => _bidID;
		internal FixField.Text? _bidID;

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID => _clientBidID;
		internal FixField.Text? _clientBidID;

		/// <summary>The FIX BidRequestTransType, tag 374, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? BidRequestTransType => _bidRequestTransType;
		internal FixField.Character? _bidRequestTransType;

		/// <summary>The FIX ListName, tag 392, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListName => _listName;
		internal FixField.Text? _listName;

		/// <summary>The FIX TotalNumSecurities, tag 393, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotalNumSecurities => _totalNumSecurities;
		internal FixField.Integer? _totalNumSecurities;

		/// <summary>The FIX BidType, tag 394, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BidType => _bidType;
		internal FixField.Integer? _bidType;

		/// <summary>The FIX NumTickets, tag 395, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumTickets => _numTickets;
		internal FixField.Integer? _numTickets;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX SideValue1, tag 396, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SideValue1 => _sideValue1;
		internal FixField.Decimal? _sideValue1;

		/// <summary>The FIX SideValue2, tag 397, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SideValue2 => _sideValue2;
		internal FixField.Decimal? _sideValue2;

		/// <summary>The FIX NoBidDescriptors, tag 398, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidDescriptors => _noBidDescriptors;
		internal FixField.Integer? _noBidDescriptors;

		/// <summary>The entries counted by NoBidDescriptors, tag 398; null when the group is absent.</summary>
		public List<FixMessage.BidRequest.NoBidDescriptorsGroup>? NoBidDescriptorsGroups => noBidDescriptorsGroups;
		internal List<NoBidDescriptorsGroup>? noBidDescriptorsGroups;

		/// <summary>The FIX NoBidComponents, tag 420, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidComponents => _noBidComponents;
		internal FixField.Integer? _noBidComponents;

		/// <summary>The entries counted by NoBidComponents, tag 420; null when the group is absent.</summary>
		public List<FixMessage.BidRequest.NoBidComponentsGroup>? NoBidComponentsGroups => noBidComponentsGroups;
		internal List<NoBidComponentsGroup>? noBidComponentsGroups;

		/// <summary>The FIX LiquidityIndType, tag 409, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LiquidityIndType => _liquidityIndType;
		internal FixField.Integer? _liquidityIndType;

		/// <summary>The FIX WtAverageLiquidity, tag 410, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? WtAverageLiquidity => _wtAverageLiquidity;
		internal FixField.Decimal? _wtAverageLiquidity;

		/// <summary>The FIX ExchangeForPhysical, tag 411, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ExchangeForPhysical => _exchangeForPhysical;
		internal FixField.Boolean? _exchangeForPhysical;

		/// <summary>The FIX OutMainCntryUIndex, tag 412, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? OutMainCntryUIndex => _outMainCntryUIndex;
		internal FixField.Decimal? _outMainCntryUIndex;

		/// <summary>The FIX CrossPercent, tag 413, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CrossPercent => _crossPercent;
		internal FixField.Decimal? _crossPercent;

		/// <summary>The FIX ProgRptReqs, tag 414, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgRptReqs => _progRptReqs;
		internal FixField.Integer? _progRptReqs;

		/// <summary>The FIX ProgPeriodInterval, tag 415, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgPeriodInterval => _progPeriodInterval;
		internal FixField.Integer? _progPeriodInterval;

		/// <summary>The FIX IncTaxInd, tag 416, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? IncTaxInd => _incTaxInd;
		internal FixField.Integer? _incTaxInd;

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq => _forexReq;
		internal FixField.Boolean? _forexReq;

		/// <summary>The FIX NumBidders, tag 417, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NumBidders => _numBidders;
		internal FixField.Integer? _numBidders;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX TradeType, tag 418, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TradeType => _tradeType;
		internal FixField.Character? _tradeType;

		/// <summary>The FIX BasisPxType, tag 419, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? BasisPxType => _basisPxType;
		internal FixField.Character? _basisPxType;

		/// <summary>The FIX StrikeTime, tag 443, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? StrikeTime => _strikeTime;
		internal FixField.Timestamp? _strikeTime;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>One entry of the group counted by NoBidDescriptors, tag 398.</summary>
		public sealed class NoBidDescriptorsGroup : IFixEntry
		{
			internal NoBidDescriptorsGroup(FixField.Integer delimiter)
			{
				BidDescriptorType = delimiter;
			}

			/// <summary>The FIX BidDescriptorType, tag 399, wire type <c>int</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Integer BidDescriptorType { get; }

			/// <summary>The FIX BidDescriptor, tag 400, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? BidDescriptor => _bidDescriptor;
			internal FixField.Text? _bidDescriptor;

			/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SideValueInd => _sideValueInd;
			internal FixField.Integer? _sideValueInd;

			/// <summary>The FIX LiquidityValue, tag 404, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityValue => _liquidityValue;
			internal FixField.Decimal? _liquidityValue;

			/// <summary>The FIX LiquidityNumSecurities, tag 441, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? LiquidityNumSecurities => _liquidityNumSecurities;
			internal FixField.Integer? _liquidityNumSecurities;

			/// <summary>The FIX LiquidityPctLow, tag 402, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityPctLow => _liquidityPctLow;
			internal FixField.Decimal? _liquidityPctLow;

			/// <summary>The FIX LiquidityPctHigh, tag 403, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? LiquidityPctHigh => _liquidityPctHigh;
			internal FixField.Decimal? _liquidityPctHigh;

			/// <summary>The FIX EFPTrackingError, tag 405, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? EFPTrackingError => _eFPTrackingError;
			internal FixField.Decimal? _eFPTrackingError;

			/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? FairValue => _fairValue;
			internal FixField.Decimal? _fairValue;

			/// <summary>The FIX OutsideIndexPct, tag 407, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? OutsideIndexPct => _outsideIndexPct;
			internal FixField.Decimal? _outsideIndexPct;

			/// <summary>The FIX ValueOfFutures, tag 408, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? ValueOfFutures => _valueOfFutures;
			internal FixField.Decimal? _valueOfFutures;

			FixField IFixEntry.Opener => BidDescriptorType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 399 or 400 or 401 or 402 or 403 or 404 or 405 or 406 or 407 or 408 or 441;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoBidComponents, tag 420.</summary>
		public sealed class NoBidComponentsGroup : IFixEntry
		{
			internal NoBidComponentsGroup(FixField.Text delimiter)
			{
				ListID = delimiter;
			}

			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text ListID { get; }

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NetGrossInd => _netGrossInd;
			internal FixField.Integer? _netGrossInd;

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp => _settlmntTyp;
			internal FixField.Character? _settlmntTyp;

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate => _futSettDate;
			internal FixField.Date? _futSettDate;

			/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Account => _account;
			internal FixField.Text? _account;

			FixField IFixEntry.Opener => ListID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 1 or 54 or 63 or 64 or 66 or 336 or 430;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoBidDescriptorsGroups, FixTag.NoBidDescriptors, tag, position, out step) ??
				FixEntries.Holding(NoBidComponentsGroups, FixTag.NoBidComponents, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.BidRequest(context, check, this);
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
					case FixTag.BidID: if (_bidID is not null) Duplicate(field); _bidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (_clientBidID is not null) Duplicate(field); _clientBidID = (FixField.Text)field; break;
					case FixTag.NoBidComponents: if (_noBidComponents is not null) Duplicate(field); _noBidComponents = (FixField.Integer)field; break;
					case FixTag.Commission:
						if (noBidComponentsGroups is null && _noBidComponents is null) Uncounted(FixTag.NoBidComponents, field);
						(noBidComponentsGroups ??= []).Add(new ((FixField.Decimal)field));
						break;
					case FixTag.CommType:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._commType is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._commType = (FixField.Character)field;
						break;
					case FixTag.ListID:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._listID is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._listID = (FixField.Text)field;
						break;
					case FixTag.Country:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._country is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._country = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._price is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._price = (FixField.Decimal)field;
						break;
					case FixTag.PriceType:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._priceType is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._priceType = (FixField.Integer)field;
						break;
					case FixTag.FairValue:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._fairValue is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._fairValue = (FixField.Decimal)field;
						break;
					case FixTag.NetGrossInd:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._netGrossInd is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._netGrossInd = (FixField.Integer)field;
						break;
					case FixTag.SettlType:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._settlmntTyp is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._settlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._futSettDate is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._futSettDate = (FixField.Date)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noBidComponentsGroups))
							Unentered(field);
						else if (noBidComponentsGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoBidComponents, noBidComponentsGroups!.Count - 1);
						else
							noBidComponentsGroups![^1]._encodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID => _bidID;
		internal FixField.Text? _bidID;

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID => _clientBidID;
		internal FixField.Text? _clientBidID;

		/// <summary>The FIX NoBidComponents, tag 420, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoBidComponents => _noBidComponents;
		internal FixField.Integer? _noBidComponents;

		/// <summary>The entries counted by NoBidComponents, tag 420; null when the group is absent.</summary>
		public List<FixMessage.BidResponse.NoBidComponentsGroup>? NoBidComponentsGroups => noBidComponentsGroups;
		internal List<NoBidComponentsGroup>? noBidComponentsGroups;

		/// <summary>One entry of the group counted by NoBidComponents, tag 420.</summary>
		public sealed class NoBidComponentsGroup : IFixEntry
		{
			internal NoBidComponentsGroup(FixField.Decimal delimiter)
			{
				Commission = delimiter;
			}

			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Decimal Commission { get; }

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType => _commType;
			internal FixField.Character? _commType;

			/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ListID => _listID;
			internal FixField.Text? _listID;

			/// <summary>The FIX Country, tag 421, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Country => _country;
			internal FixField.Text? _country;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price => _price;
			internal FixField.Decimal? _price;

			/// <summary>The FIX PriceType, tag 423, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PriceType => _priceType;
			internal FixField.Integer? _priceType;

			/// <summary>The FIX FairValue, tag 406, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? FairValue => _fairValue;
			internal FixField.Decimal? _fairValue;

			/// <summary>The FIX NetGrossInd, tag 430, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NetGrossInd => _netGrossInd;
			internal FixField.Integer? _netGrossInd;

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp => _settlmntTyp;
			internal FixField.Character? _settlmntTyp;

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate => _futSettDate;
			internal FixField.Date? _futSettDate;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => Commission;

			bool IFixEntry.Has(int tag)
			{
				return tag is 12 or 13 or 44 or 54 or 58 or 63 or 64 or 66 or 336 or 354 or 355 or 406 or 421 or 423 or 430;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoBidComponentsGroups, FixTag.NoBidComponents, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.BidResponse(context, check, this);
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
					case FixTag.RefSeqNum: if (_refSeqNum is not null) Duplicate(field); _refSeqNum = (FixField.Integer)field; break;
					case FixTag.RefMsgType: if (_refMsgType is not null) Duplicate(field); _refMsgType = (FixField.Text)field; break;
					case FixTag.BusinessRejectRefID: if (_businessRejectRefID is not null) Duplicate(field); _businessRejectRefID = (FixField.Text)field; break;
					case FixTag.BusinessRejectReason: if (_businessRejectReason is not null) Duplicate(field); _businessRejectReason = (FixField.Integer)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX RefSeqNum, tag 45, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefSeqNum => _refSeqNum;
		internal FixField.Integer? _refSeqNum;

		/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefMsgType => _refMsgType;
		internal FixField.Text? _refMsgType;

		/// <summary>The FIX BusinessRejectRefID, tag 379, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BusinessRejectRefID => _businessRejectRefID;
		internal FixField.Text? _businessRejectRefID;

		/// <summary>The FIX BusinessRejectReason, tag 380, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BusinessRejectReason => _businessRejectReason;
		internal FixField.Integer? _businessRejectReason;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.BusinessMessageReject(context, check, this);
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
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.ExecID: if (_execID is not null) Duplicate(field); _execID = (FixField.Text)field; break;
					case FixTag.DKReason: if (_dKReason is not null) Duplicate(field); _dKReason = (FixField.Character)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.OrderQty: if (_orderQty is not null) Duplicate(field); _orderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (_cashOrderQty is not null) Duplicate(field); _cashOrderQty = (FixField.Decimal)field; break;
					case FixTag.LastQty: if (_lastShares is not null) Duplicate(field); _lastShares = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (_lastPx is not null) Duplicate(field); _lastPx = (FixField.Decimal)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecID => _execID;
		internal FixField.Text? _execID;

		/// <summary>The FIX DKReason, tag 127, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DKReason => _dKReason;
		internal FixField.Character? _dKReason;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty => _orderQty;
		internal FixField.Decimal? _orderQty;

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty => _cashOrderQty;
		internal FixField.Decimal? _cashOrderQty;

		/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastShares => _lastShares;
		internal FixField.Decimal? _lastShares;

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx => _lastPx;
		internal FixField.Decimal? _lastPx;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.DontKnowTrade(context, check, this);
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
					case FixTag.EmailThreadID: if (_emailThreadID is not null) Duplicate(field); _emailThreadID = (FixField.Text)field; break;
					case FixTag.EmailType: if (_emailType is not null) Duplicate(field); _emailType = (FixField.Character)field; break;
					case FixTag.OrigTime: if (_origTime is not null) Duplicate(field); _origTime = (FixField.Timestamp)field; break;
					case FixTag.Subject: if (_subject is not null) Duplicate(field); _subject = (FixField.Text)field; break;
					case FixTag.EncodedSubjectLen: if (_encodedSubjectLen is not null) Duplicate(field); _encodedSubjectLen = (FixField.Integer)field; break;
					case FixTag.EncodedSubject: if (_encodedSubject is not null) Duplicate(field); _encodedSubject = (FixField.Data)field; break;
					case FixTag.NoRoutingIDs: if (_noRoutingIDs is not null) Duplicate(field); _noRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (noRoutingIDsGroups is null && _noRoutingIDs is null) Uncounted(FixTag.NoRoutingIDs, field);
						(noRoutingIDsGroups ??= []).Add(new ((FixField.Integer)field));
						break;
					case FixTag.RoutingID:
						if (None(noRoutingIDsGroups))
							Unentered(field);
						else if (noRoutingIDsGroups![^1]._routingID is not null)
							Duplicate(field, FixTag.NoRoutingIDs, noRoutingIDsGroups!.Count - 1);
						else
							noRoutingIDsGroups![^1]._routingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.NoLinesOfText: if (_linesOfText is not null) Duplicate(field); _linesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (linesOfTextGroups is null && _linesOfText is null) Uncounted(FixTag.NoLinesOfText, field);
						(linesOfTextGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.EncodedTextLen:
						if (None(linesOfTextGroups))
							Unentered(field);
						else if (linesOfTextGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoLinesOfText, linesOfTextGroups!.Count - 1);
						else
							linesOfTextGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(linesOfTextGroups))
							Unentered(field);
						else if (linesOfTextGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoLinesOfText, linesOfTextGroups!.Count - 1);
						else
							linesOfTextGroups![^1]._encodedText = (FixField.Data)field;
						break;
					case FixTag.RawDataLength: if (_rawDataLength is not null) Duplicate(field); _rawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (_rawData is not null) Duplicate(field); _rawData = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX EmailThreadID, tag 164, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? EmailThreadID => _emailThreadID;
		internal FixField.Text? _emailThreadID;

		/// <summary>The FIX EmailType, tag 94, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? EmailType => _emailType;
		internal FixField.Character? _emailType;

		/// <summary>The FIX OrigTime, tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? OrigTime => _origTime;
		internal FixField.Timestamp? _origTime;

		/// <summary>The FIX Subject, tag 147, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Subject => _subject;
		internal FixField.Text? _subject;

		/// <summary>The FIX EncodedSubjectLen, tag 356, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSubjectLen => _encodedSubjectLen;
		internal FixField.Integer? _encodedSubjectLen;

		/// <summary>The FIX EncodedSubject, tag 357, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSubject => _encodedSubject;
		internal FixField.Data? _encodedSubject;

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs => _noRoutingIDs;
		internal FixField.Integer? _noRoutingIDs;

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public List<FixMessage.Email.NoRoutingIDsGroup>? NoRoutingIDsGroups => noRoutingIDsGroups;
		internal List<NoRoutingIDsGroup>? noRoutingIDsGroups;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.Email.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText => _linesOfText;
		internal FixField.Integer? _linesOfText;

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public List<FixMessage.Email.LinesOfTextGroup>? LinesOfTextGroups => linesOfTextGroups;
		internal List<LinesOfTextGroup>? linesOfTextGroups;

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength => _rawDataLength;
		internal FixField.Integer? _rawDataLength;

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData => _rawData;
		internal FixField.Data? _rawData;

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup : IFixEntry
		{
			internal NoRoutingIDsGroup(FixField.Integer delimiter)
			{
				RoutingType = delimiter;
			}

			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Integer RoutingType { get; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID => _routingID;
			internal FixField.Text? _routingID;

			FixField IFixEntry.Opener => RoutingType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 216 or 217;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				RelatdSym = delimiter;
			}

			/// <summary>The FIX RelatdSym, tag 46, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text RelatdSym { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			FixField IFixEntry.Opener => RelatdSym;

			bool IFixEntry.Has(int tag)
			{
				return tag is 22 or 46 or 48 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
					348 or 349 or 350 or 351;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by LinesOfText, tag 33.</summary>
		public sealed class LinesOfTextGroup : IFixEntry
		{
			internal LinesOfTextGroup(FixField.Text delimiter)
			{
				Text = delimiter;
			}

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Text { get; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => Text;

			bool IFixEntry.Has(int tag)
			{
				return tag is 58 or 354 or 355;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoRoutingIDsGroups, FixTag.NoRoutingIDs, tag, position, out step) ??
				FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step) ??
				FixEntries.Holding(LinesOfTextGroups, FixTag.NoLinesOfText, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Email(context, check, this);
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
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.SecondaryOrderID: if (_secondaryOrderID is not null) Duplicate(field); _secondaryOrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (_origClOrdID is not null) Duplicate(field); _origClOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.NoContraBrokers: if (_noContraBrokers is not null) Duplicate(field); _noContraBrokers = (FixField.Integer)field; break;
					case FixTag.ContraBroker:
						if (noContraBrokersGroups is null && _noContraBrokers is null) Uncounted(FixTag.NoContraBrokers, field);
						(noContraBrokersGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.ContraTrader:
						if (None(noContraBrokersGroups))
							Unentered(field);
						else if (noContraBrokersGroups![^1]._contraTrader is not null)
							Duplicate(field, FixTag.NoContraBrokers, noContraBrokersGroups!.Count - 1);
						else
							noContraBrokersGroups![^1]._contraTrader = (FixField.Text)field;
						break;
					case FixTag.ContraTradeQty:
						if (None(noContraBrokersGroups))
							Unentered(field);
						else if (noContraBrokersGroups![^1]._contraTradeQty is not null)
							Duplicate(field, FixTag.NoContraBrokers, noContraBrokersGroups!.Count - 1);
						else
							noContraBrokersGroups![^1]._contraTradeQty = (FixField.Decimal)field;
						break;
					case FixTag.ContraTradeTime:
						if (None(noContraBrokersGroups))
							Unentered(field);
						else if (noContraBrokersGroups![^1]._contraTradeTime is not null)
							Duplicate(field, FixTag.NoContraBrokers, noContraBrokersGroups!.Count - 1);
						else
							noContraBrokersGroups![^1]._contraTradeTime = (FixField.Timestamp)field;
						break;
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.ExecID: if (_execID is not null) Duplicate(field); _execID = (FixField.Text)field; break;
					case FixTag.ExecTransType: if (_execTransType is not null) Duplicate(field); _execTransType = (FixField.Character)field; break;
					case FixTag.ExecRefID: if (_execRefID is not null) Duplicate(field); _execRefID = (FixField.Text)field; break;
					case FixTag.ExecType: if (_execType is not null) Duplicate(field); _execType = (FixField.Character)field; break;
					case FixTag.OrdStatus: if (_ordStatus is not null) Duplicate(field); _ordStatus = (FixField.Character)field; break;
					case FixTag.OrdRejReason: if (_ordRejReason is not null) Duplicate(field); _ordRejReason = (FixField.Integer)field; break;
					case FixTag.ExecRestatementReason: if (_execRestatementReason is not null) Duplicate(field); _execRestatementReason = (FixField.Integer)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.SettlType: if (_settlmntTyp is not null) Duplicate(field); _settlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (_futSettDate is not null) Duplicate(field); _futSettDate = (FixField.Date)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.OrderQty: if (_orderQty is not null) Duplicate(field); _orderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (_cashOrderQty is not null) Duplicate(field); _cashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (_ordType is not null) Duplicate(field); _ordType = (FixField.Character)field; break;
					case FixTag.Price: if (_price is not null) Duplicate(field); _price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (_stopPx is not null) Duplicate(field); _stopPx = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (_pegDifference is not null) Duplicate(field); _pegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (_discretionInst is not null) Duplicate(field); _discretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (_discretionOffset is not null) Duplicate(field); _discretionOffset = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.ComplianceID: if (_complianceID is not null) Duplicate(field); _complianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (_solicitedFlag is not null) Duplicate(field); _solicitedFlag = (FixField.Boolean)field; break;
					case FixTag.TimeInForce: if (_timeInForce is not null) Duplicate(field); _timeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (_effectiveTime is not null) Duplicate(field); _effectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (_expireDate is not null) Duplicate(field); _expireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (_expireTime is not null) Duplicate(field); _expireTime = (FixField.Timestamp)field; break;
					case FixTag.ExecInst: if (_execInst is not null) Duplicate(field); _execInst = (FixField.Multiple)field; break;
					case FixTag.Rule80A: if (_rule80A is not null) Duplicate(field); _rule80A = (FixField.Character)field; break;
					case FixTag.LastQty: if (_lastShares is not null) Duplicate(field); _lastShares = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (_lastPx is not null) Duplicate(field); _lastPx = (FixField.Decimal)field; break;
					case FixTag.LastSpotRate: if (_lastSpotRate is not null) Duplicate(field); _lastSpotRate = (FixField.Decimal)field; break;
					case FixTag.LastForwardPoints: if (_lastForwardPoints is not null) Duplicate(field); _lastForwardPoints = (FixField.Decimal)field; break;
					case FixTag.LastMkt: if (_lastMkt is not null) Duplicate(field); _lastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.LastCapacity: if (_lastCapacity is not null) Duplicate(field); _lastCapacity = (FixField.Character)field; break;
					case FixTag.LeavesQty: if (_leavesQty is not null) Duplicate(field); _leavesQty = (FixField.Decimal)field; break;
					case FixTag.CumQty: if (_cumQty is not null) Duplicate(field); _cumQty = (FixField.Decimal)field; break;
					case FixTag.AvgPx: if (_avgPx is not null) Duplicate(field); _avgPx = (FixField.Decimal)field; break;
					case FixTag.DayOrderQty: if (_dayOrderQty is not null) Duplicate(field); _dayOrderQty = (FixField.Decimal)field; break;
					case FixTag.DayCumQty: if (_dayCumQty is not null) Duplicate(field); _dayCumQty = (FixField.Decimal)field; break;
					case FixTag.DayAvgPx: if (_dayAvgPx is not null) Duplicate(field); _dayAvgPx = (FixField.Decimal)field; break;
					case FixTag.GTBookingInst: if (_gTBookingInst is not null) Duplicate(field); _gTBookingInst = (FixField.Integer)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.ReportToExch: if (_reportToExch is not null) Duplicate(field); _reportToExch = (FixField.Boolean)field; break;
					case FixTag.Commission: if (_commission is not null) Duplicate(field); _commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (_commType is not null) Duplicate(field); _commType = (FixField.Character)field; break;
					case FixTag.GrossTradeAmt: if (_grossTradeAmt is not null) Duplicate(field); _grossTradeAmt = (FixField.Decimal)field; break;
					case FixTag.SettlCurrAmt: if (_settlCurrAmt is not null) Duplicate(field); _settlCurrAmt = (FixField.Decimal)field; break;
					case FixTag.SettlCurrency: if (_settlCurrency is not null) Duplicate(field); _settlCurrency = (FixField.Text)field; break;
					case FixTag.SettlCurrFxRate: if (_settlCurrFxRate is not null) Duplicate(field); _settlCurrFxRate = (FixField.Decimal)field; break;
					case FixTag.SettlCurrFxRateCalc: if (_settlCurrFxRateCalc is not null) Duplicate(field); _settlCurrFxRateCalc = (FixField.Character)field; break;
					case FixTag.HandlInst: if (_handlInst is not null) Duplicate(field); _handlInst = (FixField.Character)field; break;
					case FixTag.MinQty: if (_minQty is not null) Duplicate(field); _minQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (_maxFloor is not null) Duplicate(field); _maxFloor = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (_openClose is not null) Duplicate(field); _openClose = (FixField.Character)field; break;
					case FixTag.MaxShow: if (_maxShow is not null) Duplicate(field); _maxShow = (FixField.Decimal)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (_futSettDate2 is not null) Duplicate(field); _futSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (_orderQty2 is not null) Duplicate(field); _orderQty2 = (FixField.Decimal)field; break;
					case FixTag.ClearingFirm: if (_clearingFirm is not null) Duplicate(field); _clearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (_clearingAccount is not null) Duplicate(field); _clearingAccount = (FixField.Text)field; break;
					case FixTag.MultiLegReportingType: if (_multiLegReportingType is not null) Duplicate(field); _multiLegReportingType = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecondaryOrderID => _secondaryOrderID;
		internal FixField.Text? _secondaryOrderID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID => _origClOrdID;
		internal FixField.Text? _origClOrdID;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX NoContraBrokers, tag 382, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoContraBrokers => _noContraBrokers;
		internal FixField.Integer? _noContraBrokers;

		/// <summary>The entries counted by NoContraBrokers, tag 382; null when the group is absent.</summary>
		public List<FixMessage.ExecutionReport.NoContraBrokersGroup>? NoContraBrokersGroups => noContraBrokersGroups;
		internal List<NoContraBrokersGroup>? noContraBrokersGroups;

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX ExecID, tag 17, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecID => _execID;
		internal FixField.Text? _execID;

		/// <summary>The FIX ExecTransType, tag 20, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ExecTransType => _execTransType;
		internal FixField.Character? _execTransType;

		/// <summary>The FIX ExecRefID, tag 19, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecRefID => _execRefID;
		internal FixField.Text? _execRefID;

		/// <summary>The FIX ExecType, tag 150, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ExecType => _execType;
		internal FixField.Character? _execType;

		/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdStatus => _ordStatus;
		internal FixField.Character? _ordStatus;

		/// <summary>The FIX OrdRejReason, tag 103, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? OrdRejReason => _ordRejReason;
		internal FixField.Integer? _ordRejReason;

		/// <summary>The FIX ExecRestatementReason, tag 378, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ExecRestatementReason => _execRestatementReason;
		internal FixField.Integer? _execRestatementReason;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp => _settlmntTyp;
		internal FixField.Character? _settlmntTyp;

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate => _futSettDate;
		internal FixField.Date? _futSettDate;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty => _orderQty;
		internal FixField.Decimal? _orderQty;

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty => _cashOrderQty;
		internal FixField.Decimal? _cashOrderQty;

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType => _ordType;
		internal FixField.Character? _ordType;

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price => _price;
		internal FixField.Decimal? _price;

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx => _stopPx;
		internal FixField.Decimal? _stopPx;

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference => _pegDifference;
		internal FixField.Decimal? _pegDifference;

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst => _discretionInst;
		internal FixField.Character? _discretionInst;

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset => _discretionOffset;
		internal FixField.Decimal? _discretionOffset;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID => _complianceID;
		internal FixField.Text? _complianceID;

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag => _solicitedFlag;
		internal FixField.Boolean? _solicitedFlag;

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce => _timeInForce;
		internal FixField.Character? _timeInForce;

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime => _effectiveTime;
		internal FixField.Timestamp? _effectiveTime;

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate => _expireDate;
		internal FixField.Date? _expireDate;

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime => _expireTime;
		internal FixField.Timestamp? _expireTime;

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst => _execInst;
		internal FixField.Multiple? _execInst;

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A => _rule80A;
		internal FixField.Character? _rule80A;

		/// <summary>The FIX LastShares, tag 32, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastShares => _lastShares;
		internal FixField.Decimal? _lastShares;

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx => _lastPx;
		internal FixField.Decimal? _lastPx;

		/// <summary>The FIX LastSpotRate, tag 194, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastSpotRate => _lastSpotRate;
		internal FixField.Decimal? _lastSpotRate;

		/// <summary>The FIX LastForwardPoints, tag 195, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastForwardPoints => _lastForwardPoints;
		internal FixField.Decimal? _lastForwardPoints;

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt => _lastMkt;
		internal FixField.Text? _lastMkt;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX LastCapacity, tag 29, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? LastCapacity => _lastCapacity;
		internal FixField.Character? _lastCapacity;

		/// <summary>The FIX LeavesQty, tag 151, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? LeavesQty => _leavesQty;
		internal FixField.Decimal? _leavesQty;

		/// <summary>The FIX CumQty, tag 14, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CumQty => _cumQty;
		internal FixField.Decimal? _cumQty;

		/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? AvgPx => _avgPx;
		internal FixField.Decimal? _avgPx;

		/// <summary>The FIX DayOrderQty, tag 424, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayOrderQty => _dayOrderQty;
		internal FixField.Decimal? _dayOrderQty;

		/// <summary>The FIX DayCumQty, tag 425, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayCumQty => _dayCumQty;
		internal FixField.Decimal? _dayCumQty;

		/// <summary>The FIX DayAvgPx, tag 426, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? DayAvgPx => _dayAvgPx;
		internal FixField.Decimal? _dayAvgPx;

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst => _gTBookingInst;
		internal FixField.Integer? _gTBookingInst;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX ReportToExch, tag 113, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ReportToExch => _reportToExch;
		internal FixField.Boolean? _reportToExch;

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission => _commission;
		internal FixField.Decimal? _commission;

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType => _commType;
		internal FixField.Character? _commType;

		/// <summary>The FIX GrossTradeAmt, tag 381, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? GrossTradeAmt => _grossTradeAmt;
		internal FixField.Decimal? _grossTradeAmt;

		/// <summary>The FIX SettlCurrAmt, tag 119, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? SettlCurrAmt => _settlCurrAmt;
		internal FixField.Decimal? _settlCurrAmt;

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency => _settlCurrency;
		internal FixField.Text? _settlCurrency;

		/// <summary>The FIX SettlCurrFxRate, tag 155, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? SettlCurrFxRate => _settlCurrFxRate;
		internal FixField.Decimal? _settlCurrFxRate;

		/// <summary>The FIX SettlCurrFxRateCalc, tag 156, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlCurrFxRateCalc => _settlCurrFxRateCalc;
		internal FixField.Character? _settlCurrFxRateCalc;

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst => _handlInst;
		internal FixField.Character? _handlInst;

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty => _minQty;
		internal FixField.Decimal? _minQty;

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor => _maxFloor;
		internal FixField.Decimal? _maxFloor;

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose => _openClose;
		internal FixField.Character? _openClose;

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow => _maxShow;
		internal FixField.Decimal? _maxShow;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 => _futSettDate2;
		internal FixField.Date? _futSettDate2;

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 => _orderQty2;
		internal FixField.Decimal? _orderQty2;

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm => _clearingFirm;
		internal FixField.Text? _clearingFirm;

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount => _clearingAccount;
		internal FixField.Text? _clearingAccount;

		/// <summary>The FIX MultiLegReportingType, tag 442, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? MultiLegReportingType => _multiLegReportingType;
		internal FixField.Character? _multiLegReportingType;

		/// <summary>One entry of the group counted by NoContraBrokers, tag 382.</summary>
		public sealed class NoContraBrokersGroup : IFixEntry
		{
			internal NoContraBrokersGroup(FixField.Text delimiter)
			{
				ContraBroker = delimiter;
			}

			/// <summary>The FIX ContraBroker, tag 375, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text ContraBroker { get; }

			/// <summary>The FIX ContraTrader, tag 337, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ContraTrader => _contraTrader;
			internal FixField.Text? _contraTrader;

			/// <summary>The FIX ContraTradeQty, tag 437, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContraTradeQty => _contraTradeQty;
			internal FixField.Decimal? _contraTradeQty;

			/// <summary>The FIX ContraTradeTime, tag 438, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ContraTradeTime => _contraTradeTime;
			internal FixField.Timestamp? _contraTradeTime;

			FixField IFixEntry.Opener => ContraBroker;

			bool IFixEntry.Has(int tag)
			{
				return tag is 337 or 375 or 437 or 438;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoContraBrokersGroups, FixTag.NoContraBrokers, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ExecutionReport(context, check, this);
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
					case FixTag.TestReqID: if (_testReqID is not null) Duplicate(field); _testReqID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX TestReqID, tag 112, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TestReqID => _testReqID;
		internal FixField.Text? _testReqID;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Heartbeat(context, check, this);
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
					case FixTag.IOIID: if (_iOIid is not null) Duplicate(field); _iOIid = (FixField.Text)field; break;
					case FixTag.IOITransType: if (_iOITransType is not null) Duplicate(field); _iOITransType = (FixField.Character)field; break;
					case FixTag.IOIRefID: if (_iOIRefID is not null) Duplicate(field); _iOIRefID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.IOIQty: if (_iOIShares is not null) Duplicate(field); _iOIShares = (FixField.Text)field; break;
					case FixTag.Price: if (_price is not null) Duplicate(field); _price = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.ValidUntilTime: if (_validUntilTime is not null) Duplicate(field); _validUntilTime = (FixField.Timestamp)field; break;
					case FixTag.IOIQltyInd: if (_iOIQltyInd is not null) Duplicate(field); _iOIQltyInd = (FixField.Character)field; break;
					case FixTag.IOINaturalFlag: if (_iOINaturalFlag is not null) Duplicate(field); _iOINaturalFlag = (FixField.Boolean)field; break;
					case FixTag.NoIOIQualifiers: if (_noIOIQualifiers is not null) Duplicate(field); _noIOIQualifiers = (FixField.Integer)field; break;
					case FixTag.IOIQualifier:
						if (noIOIQualifiersGroups is null && _noIOIQualifiers is null) Uncounted(FixTag.NoIOIQualifiers, field);
						(noIOIQualifiersGroups ??= []).Add(new ((FixField.Character)field));
						break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.URLLink: if (_uRLLink is not null) Duplicate(field); _uRLLink = (FixField.Text)field; break;
					case FixTag.NoRoutingIDs: if (_noRoutingIDs is not null) Duplicate(field); _noRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (noRoutingIDsGroups is null && _noRoutingIDs is null) Uncounted(FixTag.NoRoutingIDs, field);
						(noRoutingIDsGroups ??= []).Add(new ((FixField.Integer)field));
						break;
					case FixTag.RoutingID:
						if (None(noRoutingIDsGroups))
							Unentered(field);
						else if (noRoutingIDsGroups![^1]._routingID is not null)
							Duplicate(field, FixTag.NoRoutingIDs, noRoutingIDsGroups!.Count - 1);
						else
							noRoutingIDsGroups![^1]._routingID = (FixField.Text)field;
						break;
					case FixTag.Spread: if (_spreadToBenchmark is not null) Duplicate(field); _spreadToBenchmark = (FixField.Decimal)field; break;
					case FixTag.Benchmark: if (_benchmark is not null) Duplicate(field); _benchmark = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIid => _iOIid;
		internal FixField.Text? _iOIid;

		/// <summary>The FIX IOITransType, tag 28, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? IOITransType => _iOITransType;
		internal FixField.Character? _iOITransType;

		/// <summary>The FIX IOIRefID, tag 26, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIRefID => _iOIRefID;
		internal FixField.Text? _iOIRefID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX IOIShares, tag 27, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIShares => _iOIShares;
		internal FixField.Text? _iOIShares;

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price => _price;
		internal FixField.Decimal? _price;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ValidUntilTime => _validUntilTime;
		internal FixField.Timestamp? _validUntilTime;

		/// <summary>The FIX IOIQltyInd, tag 25, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? IOIQltyInd => _iOIQltyInd;
		internal FixField.Character? _iOIQltyInd;

		/// <summary>The FIX IOINaturalFlag, tag 130, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? IOINaturalFlag => _iOINaturalFlag;
		internal FixField.Boolean? _iOINaturalFlag;

		/// <summary>The FIX NoIOIQualifiers, tag 199, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoIOIQualifiers => _noIOIQualifiers;
		internal FixField.Integer? _noIOIQualifiers;

		/// <summary>The entries counted by NoIOIQualifiers, tag 199; null when the group is absent.</summary>
		public List<FixMessage.IndicationofInterest.NoIOIQualifiersGroup>? NoIOIQualifiersGroups => noIOIQualifiersGroups;
		internal List<NoIOIQualifiersGroup>? noIOIQualifiersGroups;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink => _uRLLink;
		internal FixField.Text? _uRLLink;

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs => _noRoutingIDs;
		internal FixField.Integer? _noRoutingIDs;

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public List<FixMessage.IndicationofInterest.NoRoutingIDsGroup>? NoRoutingIDsGroups => noRoutingIDsGroups;
		internal List<NoRoutingIDsGroup>? noRoutingIDsGroups;

		/// <summary>The FIX SpreadToBenchmark, tag 218, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? SpreadToBenchmark => _spreadToBenchmark;
		internal FixField.Decimal? _spreadToBenchmark;

		/// <summary>The FIX Benchmark, tag 219, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Benchmark => _benchmark;
		internal FixField.Character? _benchmark;

		/// <summary>One entry of the group counted by NoIOIQualifiers, tag 199.</summary>
		public sealed class NoIOIQualifiersGroup : IFixEntry
		{
			internal NoIOIQualifiersGroup(FixField.Character delimiter)
			{
				IOIQualifier = delimiter;
			}

			/// <summary>The FIX IOIQualifier, tag 104, wire type <c>char</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Character IOIQualifier { get; }

			FixField IFixEntry.Opener => IOIQualifier;

			bool IFixEntry.Has(int tag)
			{
				return tag is 104;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup : IFixEntry
		{
			internal NoRoutingIDsGroup(FixField.Integer delimiter)
			{
				RoutingType = delimiter;
			}

			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Integer RoutingType { get; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID => _routingID;
			internal FixField.Text? _routingID;

			FixField IFixEntry.Opener => RoutingType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 216 or 217;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoIOIQualifiersGroups, FixTag.NoIOIQualifiers, tag, position, out step) ??
				FixEntries.Holding(NoRoutingIDsGroups, FixTag.NoRoutingIDs, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.IndicationofInterest(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ListCancelRequest(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (_clientBidID is not null) Duplicate(field); _clientBidID = (FixField.Text)field; break;
					case FixTag.BidID: if (_bidID is not null) Duplicate(field); _bidID = (FixField.Text)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID => _clientBidID;
		internal FixField.Text? _clientBidID;

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID => _bidID;
		internal FixField.Text? _bidID;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ListExecute(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.ListStatusType: if (_listStatusType is not null) Duplicate(field); _listStatusType = (FixField.Integer)field; break;
					case FixTag.NoRpts: if (_noRpts is not null) Duplicate(field); _noRpts = (FixField.Integer)field; break;
					case FixTag.ListOrderStatus: if (_listOrderStatus is not null) Duplicate(field); _listOrderStatus = (FixField.Integer)field; break;
					case FixTag.RptSeq: if (_rptSeq is not null) Duplicate(field); _rptSeq = (FixField.Integer)field; break;
					case FixTag.ListStatusText: if (_listStatusText is not null) Duplicate(field); _listStatusText = (FixField.Text)field; break;
					case FixTag.EncodedListStatusTextLen: if (_encodedListStatusTextLen is not null) Duplicate(field); _encodedListStatusTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedListStatusText: if (_encodedListStatusText is not null) Duplicate(field); _encodedListStatusText = (FixField.Data)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.TotNoOrders: if (_totNoOrders is not null) Duplicate(field); _totNoOrders = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (_noOrders is not null) Duplicate(field); _noOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (noOrdersGroups is null && _noOrders is null) Uncounted(FixTag.NoOrders, field);
						(noOrdersGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.CumQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._cumQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._cumQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdStatus:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._ordStatus is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._ordStatus = (FixField.Character)field;
						break;
					case FixTag.LeavesQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._leavesQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._leavesQty = (FixField.Decimal)field;
						break;
					case FixTag.CxlQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._cxlQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._cxlQty = (FixField.Decimal)field;
						break;
					case FixTag.AvgPx:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._avgPx is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._avgPx = (FixField.Decimal)field;
						break;
					case FixTag.OrdRejReason:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._ordRejReason is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._ordRejReason = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX ListStatusType, tag 429, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ListStatusType => _listStatusType;
		internal FixField.Integer? _listStatusType;

		/// <summary>The FIX NoRpts, tag 82, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRpts => _noRpts;
		internal FixField.Integer? _noRpts;

		/// <summary>The FIX ListOrderStatus, tag 431, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ListOrderStatus => _listOrderStatus;
		internal FixField.Integer? _listOrderStatus;

		/// <summary>The FIX RptSeq, tag 83, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RptSeq => _rptSeq;
		internal FixField.Integer? _rptSeq;

		/// <summary>The FIX ListStatusText, tag 444, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListStatusText => _listStatusText;
		internal FixField.Text? _listStatusText;

		/// <summary>The FIX EncodedListStatusTextLen, tag 445, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedListStatusTextLen => _encodedListStatusTextLen;
		internal FixField.Integer? _encodedListStatusTextLen;

		/// <summary>The FIX EncodedListStatusText, tag 446, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedListStatusText => _encodedListStatusText;
		internal FixField.Data? _encodedListStatusText;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX TotNoOrders, tag 68, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoOrders => _totNoOrders;
		internal FixField.Integer? _totNoOrders;

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders => _noOrders;
		internal FixField.Integer? _noOrders;

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public List<FixMessage.ListStatus.NoOrdersGroup>? NoOrdersGroups => noOrdersGroups;
		internal List<NoOrdersGroup>? noOrdersGroups;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup : IFixEntry
		{
			internal NoOrdersGroup(FixField.Text delimiter)
			{
				ClOrdID = delimiter;
			}

			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text ClOrdID { get; }

			/// <summary>The FIX CumQty, tag 14, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CumQty => _cumQty;
			internal FixField.Decimal? _cumQty;

			/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdStatus => _ordStatus;
			internal FixField.Character? _ordStatus;

			/// <summary>The FIX LeavesQty, tag 151, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? LeavesQty => _leavesQty;
			internal FixField.Decimal? _leavesQty;

			/// <summary>The FIX CxlQty, tag 84, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CxlQty => _cxlQty;
			internal FixField.Decimal? _cxlQty;

			/// <summary>The FIX AvgPx, tag 6, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? AvgPx => _avgPx;
			internal FixField.Decimal? _avgPx;

			/// <summary>The FIX OrdRejReason, tag 103, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? OrdRejReason => _ordRejReason;
			internal FixField.Integer? _ordRejReason;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => ClOrdID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 6 or 11 or 14 or 39 or 58 or 84 or 103 or 151 or 354 or 355;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoOrdersGroups, FixTag.NoOrders, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ListStatus(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ListStatusRequest(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.TotNoStrikes: if (_totNoStrikes is not null) Duplicate(field); _totNoStrikes = (FixField.Integer)field; break;
					case FixTag.NoStrikes: if (_noStrikes is not null) Duplicate(field); _noStrikes = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (noStrikesGroups is null && _noStrikes is null) Uncounted(FixTag.NoStrikes, field);
						(noStrikesGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._prevClosePx is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._prevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.ClOrdID:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._clOrdID is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._clOrdID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._price is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._price = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._currency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noStrikesGroups))
							Unentered(field);
						else if (noStrikesGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoStrikes, noStrikesGroups!.Count - 1);
						else
							noStrikesGroups![^1]._encodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX TotNoStrikes, tag 422, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoStrikes => _totNoStrikes;
		internal FixField.Integer? _totNoStrikes;

		/// <summary>The FIX NoStrikes, tag 428, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoStrikes => _noStrikes;
		internal FixField.Integer? _noStrikes;

		/// <summary>The entries counted by NoStrikes, tag 428; null when the group is absent.</summary>
		public List<FixMessage.ListStrikePrice.NoStrikesGroup>? NoStrikesGroups => noStrikesGroups;
		internal List<NoStrikesGroup>? noStrikesGroups;

		/// <summary>One entry of the group counted by NoStrikes, tag 428.</summary>
		public sealed class NoStrikesGroup : IFixEntry
		{
			internal NoStrikesGroup(FixField.Text delimiter)
			{
				Symbol = delimiter;
			}

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Symbol { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx => _prevClosePx;
			internal FixField.Decimal? _prevClosePx;

			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClOrdID => _clOrdID;
			internal FixField.Text? _clOrdID;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price => _price;
			internal FixField.Decimal? _price;

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency => _currency;
			internal FixField.Text? _currency;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => Symbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 11 or 15 or 22 or 44 or 48 or 54 or 55 or 58 or 65 or 106 or 107 or 140 or 167 or 200 or 201 or 202 or
					205 or 206 or 207 or 223 or 231 or 348 or 349 or 350 or 351 or 354 or 355;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoStrikesGroups, FixTag.NoStrikes, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ListStrikePrice(context, check, this);
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
					case FixTag.EncryptMethod: if (_encryptMethod is not null) Duplicate(field); _encryptMethod = (FixField.Integer)field; break;
					case FixTag.HeartBtInt: if (_heartBtInt is not null) Duplicate(field); _heartBtInt = (FixField.Integer)field; break;
					case FixTag.RawDataLength: if (_rawDataLength is not null) Duplicate(field); _rawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (_rawData is not null) Duplicate(field); _rawData = (FixField.Data)field; break;
					case FixTag.ResetSeqNumFlag: if (_resetSeqNumFlag is not null) Duplicate(field); _resetSeqNumFlag = (FixField.Boolean)field; break;
					case FixTag.MaxMessageSize: if (_maxMessageSize is not null) Duplicate(field); _maxMessageSize = (FixField.Integer)field; break;
					case FixTag.NoMsgTypes: if (_noMsgTypes is not null) Duplicate(field); _noMsgTypes = (FixField.Integer)field; break;
					case FixTag.RefMsgType:
						if (noMsgTypesGroups is null && _noMsgTypes is null) Uncounted(FixTag.NoMsgTypes, field);
						(noMsgTypesGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.MsgDirection:
						if (None(noMsgTypesGroups))
							Unentered(field);
						else if (noMsgTypesGroups![^1]._msgDirection is not null)
							Duplicate(field, FixTag.NoMsgTypes, noMsgTypesGroups!.Count - 1);
						else
							noMsgTypesGroups![^1]._msgDirection = (FixField.Character)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX EncryptMethod, tag 98, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? EncryptMethod => _encryptMethod;
		internal FixField.Integer? _encryptMethod;

		/// <summary>The FIX HeartBtInt, tag 108, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? HeartBtInt => _heartBtInt;
		internal FixField.Integer? _heartBtInt;

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength => _rawDataLength;
		internal FixField.Integer? _rawDataLength;

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData => _rawData;
		internal FixField.Data? _rawData;

		/// <summary>The FIX ResetSeqNumFlag, tag 141, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ResetSeqNumFlag => _resetSeqNumFlag;
		internal FixField.Boolean? _resetSeqNumFlag;

		/// <summary>The FIX MaxMessageSize, tag 383, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MaxMessageSize => _maxMessageSize;
		internal FixField.Integer? _maxMessageSize;

		/// <summary>The FIX NoMsgTypes, tag 384, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMsgTypes => _noMsgTypes;
		internal FixField.Integer? _noMsgTypes;

		/// <summary>The entries counted by NoMsgTypes, tag 384; null when the group is absent.</summary>
		public List<FixMessage.Logon.NoMsgTypesGroup>? NoMsgTypesGroups => noMsgTypesGroups;
		internal List<NoMsgTypesGroup>? noMsgTypesGroups;

		/// <summary>One entry of the group counted by NoMsgTypes, tag 384.</summary>
		public sealed class NoMsgTypesGroup : IFixEntry
		{
			internal NoMsgTypesGroup(FixField.Text delimiter)
			{
				RefMsgType = delimiter;
			}

			/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text RefMsgType { get; }

			/// <summary>The FIX MsgDirection, tag 385, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? MsgDirection => _msgDirection;
			internal FixField.Character? _msgDirection;

			FixField IFixEntry.Opener => RefMsgType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 372 or 385;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoMsgTypesGroups, FixTag.NoMsgTypes, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Logon(context, check, this);
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
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Logout(context, check, this);
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
					case FixTag.MDReqID: if (_mDReqID is not null) Duplicate(field); _mDReqID = (FixField.Text)field; break;
					case FixTag.NoMDEntries: if (_noMDEntries is not null) Duplicate(field); _noMDEntries = (FixField.Integer)field; break;
					case FixTag.MDUpdateAction:
						if (noMDEntriesGroups is null && _noMDEntries is null) Uncounted(FixTag.NoMDEntries, field);
						(noMDEntriesGroups ??= []).Add(new ((FixField.Character)field));
						break;
					case FixTag.DeleteReason:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._deleteReason is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._deleteReason = (FixField.Character)field;
						break;
					case FixTag.MDEntryType:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryType is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryType = (FixField.Character)field;
						break;
					case FixTag.MDEntryID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryRefID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryRefID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryRefID = (FixField.Text)field;
						break;
					case FixTag.Symbol:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._symbol is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.FinancialStatus:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._financialStatus is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._financialStatus = (FixField.Character)field;
						break;
					case FixTag.CorporateAction:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._corporateAction is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._corporateAction = (FixField.Character)field;
						break;
					case FixTag.MDEntryPx:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryPx is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntrySize is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryDate is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryTime is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tickDirection is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDMkt is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._quoteCondition is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._quoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tradeCondition is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryOriginator is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._locationID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._locationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._deskID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._deskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._openCloseSettleFlag is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._openCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._timeInForce is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._timeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._expireDate is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._expireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._expireTime is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._expireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._minQty is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._minQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._execInst is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._execInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._sellerDays is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._sellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._orderID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._orderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._quoteEntryID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._quoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryBuyer is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntrySeller is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._numberOfOrders is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._numberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryPositionNo is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.TotalVolumeTraded:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._totalVolumeTraded is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._totalVolumeTraded = (FixField.Decimal)field;
						break;
					case FixTag.Text:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID => _mDReqID;
		internal FixField.Text? _mDReqID;

		/// <summary>The FIX NoMDEntries, tag 268, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntries => _noMDEntries;
		internal FixField.Integer? _noMDEntries;

		/// <summary>The entries counted by NoMDEntries, tag 268; null when the group is absent.</summary>
		public List<FixMessage.MarketDataIncrementalRefresh.NoMDEntriesGroup>? NoMDEntriesGroups => noMDEntriesGroups;
		internal List<NoMDEntriesGroup>? noMDEntriesGroups;

		/// <summary>One entry of the group counted by NoMDEntries, tag 268.</summary>
		public sealed class NoMDEntriesGroup : IFixEntry
		{
			internal NoMDEntriesGroup(FixField.Character delimiter)
			{
				MDUpdateAction = delimiter;
			}

			/// <summary>The FIX MDUpdateAction, tag 279, wire type <c>char</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Character MDUpdateAction { get; }

			/// <summary>The FIX DeleteReason, tag 285, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? DeleteReason => _deleteReason;
			internal FixField.Character? _deleteReason;

			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? MDEntryType => _mDEntryType;
			internal FixField.Character? _mDEntryType;

			/// <summary>The FIX MDEntryID, tag 278, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryID => _mDEntryID;
			internal FixField.Text? _mDEntryID;

			/// <summary>The FIX MDEntryRefID, tag 280, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryRefID => _mDEntryRefID;
			internal FixField.Text? _mDEntryRefID;

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Symbol => _symbol;
			internal FixField.Text? _symbol;

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? FinancialStatus => _financialStatus;
			internal FixField.Character? _financialStatus;

			/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CorporateAction => _corporateAction;
			internal FixField.Character? _corporateAction;

			/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntryPx => _mDEntryPx;
			internal FixField.Decimal? _mDEntryPx;

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency => _currency;
			internal FixField.Text? _currency;

			/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntrySize => _mDEntrySize;
			internal FixField.Decimal? _mDEntrySize;

			/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDate</c>; null when the field is absent.</summary>
			public FixField.Date? MDEntryDate => _mDEntryDate;
			internal FixField.Date? _mDEntryDate;

			/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
			public FixField.Time? MDEntryTime => _mDEntryTime;
			internal FixField.Time? _mDEntryTime;

			/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TickDirection => _tickDirection;
			internal FixField.Character? _tickDirection;

			/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? MDMkt => _mDMkt;
			internal FixField.Text? _mDMkt;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? QuoteCondition => _quoteCondition;
			internal FixField.Multiple? _quoteCondition;

			/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? TradeCondition => _tradeCondition;
			internal FixField.Multiple? _tradeCondition;

			/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryOriginator => _mDEntryOriginator;
			internal FixField.Text? _mDEntryOriginator;

			/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? LocationID => _locationID;
			internal FixField.Text? _locationID;

			/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? DeskID => _deskID;
			internal FixField.Text? _deskID;

			/// <summary>The FIX OpenCloseSettleFlag, tag 286, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenCloseSettleFlag => _openCloseSettleFlag;
			internal FixField.Character? _openCloseSettleFlag;

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce => _timeInForce;
			internal FixField.Character? _timeInForce;

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate => _expireDate;
			internal FixField.Date? _expireDate;

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime => _expireTime;
			internal FixField.Timestamp? _expireTime;

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty => _minQty;
			internal FixField.Decimal? _minQty;

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst => _execInst;
			internal FixField.Multiple? _execInst;

			/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SellerDays => _sellerDays;
			internal FixField.Integer? _sellerDays;

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID => _orderID;
			internal FixField.Text? _orderID;

			/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteEntryID => _quoteEntryID;
			internal FixField.Text? _quoteEntryID;

			/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryBuyer => _mDEntryBuyer;
			internal FixField.Text? _mDEntryBuyer;

			/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntrySeller => _mDEntrySeller;
			internal FixField.Text? _mDEntrySeller;

			/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NumberOfOrders => _numberOfOrders;
			internal FixField.Integer? _numberOfOrders;

			/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MDEntryPositionNo => _mDEntryPositionNo;
			internal FixField.Integer? _mDEntryPositionNo;

			/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? TotalVolumeTraded => _totalVolumeTraded;
			internal FixField.Decimal? _totalVolumeTraded;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => MDUpdateAction;

			bool IFixEntry.Has(int tag)
			{
				return tag is 15 or 18 or 22 or 37 or 48 or 55 or 58 or 59 or 65 or 106 or 107 or 110 or 126 or 167 or 200 or 201 or
					202 or 205 or 206 or 207 or 223 or 231 or 269 or 270 or 271 or 272 or 273 or 274 or 275 or 276 or 277 or 278 or
					279 or 280 or 282 or 283 or 284 or 285 or 286 or 287 or 288 or 289 or 290 or 291 or 292 or 299 or 336 or 346 or
					348 or 349 or 350 or 351 or 354 or 355 or 387 or 432;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoMDEntriesGroups, FixTag.NoMDEntries, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.MarketDataIncrementalRefresh(context, check, this);
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
					case FixTag.MDReqID: if (_mDReqID is not null) Duplicate(field); _mDReqID = (FixField.Text)field; break;
					case FixTag.SubscriptionRequestType: if (_subscriptionRequestType is not null) Duplicate(field); _subscriptionRequestType = (FixField.Character)field; break;
					case FixTag.MarketDepth: if (_marketDepth is not null) Duplicate(field); _marketDepth = (FixField.Integer)field; break;
					case FixTag.MDUpdateType: if (_mDUpdateType is not null) Duplicate(field); _mDUpdateType = (FixField.Integer)field; break;
					case FixTag.AggregatedBook: if (_aggregatedBook is not null) Duplicate(field); _aggregatedBook = (FixField.Boolean)field; break;
					case FixTag.NoMDEntryTypes: if (_noMDEntryTypes is not null) Duplicate(field); _noMDEntryTypes = (FixField.Integer)field; break;
					case FixTag.MDEntryType:
						if (noMDEntryTypesGroups is null && _noMDEntryTypes is null) Uncounted(FixTag.NoMDEntryTypes, field);
						(noMDEntryTypesGroups ??= []).Add(new ((FixField.Character)field));
						break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID => _mDReqID;
		internal FixField.Text? _mDReqID;

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType => _subscriptionRequestType;
		internal FixField.Character? _subscriptionRequestType;

		/// <summary>The FIX MarketDepth, tag 264, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MarketDepth => _marketDepth;
		internal FixField.Integer? _marketDepth;

		/// <summary>The FIX MDUpdateType, tag 265, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? MDUpdateType => _mDUpdateType;
		internal FixField.Integer? _mDUpdateType;

		/// <summary>The FIX AggregatedBook, tag 266, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? AggregatedBook => _aggregatedBook;
		internal FixField.Boolean? _aggregatedBook;

		/// <summary>The FIX NoMDEntryTypes, tag 267, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntryTypes => _noMDEntryTypes;
		internal FixField.Integer? _noMDEntryTypes;

		/// <summary>The entries counted by NoMDEntryTypes, tag 267; null when the group is absent.</summary>
		public List<FixMessage.MarketDataRequest.NoMDEntryTypesGroup>? NoMDEntryTypesGroups => noMDEntryTypesGroups;
		internal List<NoMDEntryTypesGroup>? noMDEntryTypesGroups;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.MarketDataRequest.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>One entry of the group counted by NoMDEntryTypes, tag 267.</summary>
		public sealed class NoMDEntryTypesGroup : IFixEntry
		{
			internal NoMDEntryTypesGroup(FixField.Character delimiter)
			{
				MDEntryType = delimiter;
			}

			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Character MDEntryType { get; }

			FixField IFixEntry.Opener => MDEntryType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 269;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				Symbol = delimiter;
			}

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Symbol { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			FixField IFixEntry.Opener => Symbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 22 or 48 or 55 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
					336 or 348 or 349 or 350 or 351;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoMDEntryTypesGroups, FixTag.NoMDEntryTypes, tag, position, out step) ??
				FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.MarketDataRequest(context, check, this);
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
					case FixTag.MDReqID: if (_mDReqID is not null) Duplicate(field); _mDReqID = (FixField.Text)field; break;
					case FixTag.MDReqRejReason: if (_mDReqRejReason is not null) Duplicate(field); _mDReqRejReason = (FixField.Character)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID => _mDReqID;
		internal FixField.Text? _mDReqID;

		/// <summary>The FIX MDReqRejReason, tag 281, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? MDReqRejReason => _mDReqRejReason;
		internal FixField.Character? _mDReqRejReason;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.MarketDataRequestReject(context, check, this);
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
					case FixTag.MDReqID: if (_mDReqID is not null) Duplicate(field); _mDReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.FinancialStatus: if (_financialStatus is not null) Duplicate(field); _financialStatus = (FixField.Character)field; break;
					case FixTag.CorporateAction: if (_corporateAction is not null) Duplicate(field); _corporateAction = (FixField.Character)field; break;
					case FixTag.TotalVolumeTraded: if (_totalVolumeTraded is not null) Duplicate(field); _totalVolumeTraded = (FixField.Decimal)field; break;
					case FixTag.NoMDEntries: if (_noMDEntries is not null) Duplicate(field); _noMDEntries = (FixField.Integer)field; break;
					case FixTag.MDEntryType:
						if (noMDEntriesGroups is null && _noMDEntries is null) Uncounted(FixTag.NoMDEntries, field);
						(noMDEntriesGroups ??= []).Add(new ((FixField.Character)field));
						break;
					case FixTag.MDEntryPx:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryPx is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._currency = (FixField.Text)field;
						break;
					case FixTag.MDEntrySize:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntrySize is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntrySize = (FixField.Decimal)field;
						break;
					case FixTag.MDEntryDate:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryDate is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryDate = (FixField.Date)field;
						break;
					case FixTag.MDEntryTime:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryTime is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryTime = (FixField.Time)field;
						break;
					case FixTag.TickDirection:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tickDirection is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tickDirection = (FixField.Character)field;
						break;
					case FixTag.MDMkt:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDMkt is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDMkt = (FixField.Text)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.QuoteCondition:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._quoteCondition is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._quoteCondition = (FixField.Multiple)field;
						break;
					case FixTag.TradeCondition:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._tradeCondition is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._tradeCondition = (FixField.Multiple)field;
						break;
					case FixTag.MDEntryOriginator:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryOriginator is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryOriginator = (FixField.Text)field;
						break;
					case FixTag.LocationID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._locationID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._locationID = (FixField.Text)field;
						break;
					case FixTag.DeskID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._deskID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._deskID = (FixField.Text)field;
						break;
					case FixTag.OpenCloseSettlFlag:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._openCloseSettleFlag is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._openCloseSettleFlag = (FixField.Character)field;
						break;
					case FixTag.TimeInForce:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._timeInForce is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._timeInForce = (FixField.Character)field;
						break;
					case FixTag.ExpireDate:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._expireDate is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._expireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._expireTime is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._expireTime = (FixField.Timestamp)field;
						break;
					case FixTag.MinQty:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._minQty is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._minQty = (FixField.Decimal)field;
						break;
					case FixTag.ExecInst:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._execInst is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._execInst = (FixField.Multiple)field;
						break;
					case FixTag.SellerDays:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._sellerDays is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._sellerDays = (FixField.Integer)field;
						break;
					case FixTag.OrderID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._orderID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._orderID = (FixField.Text)field;
						break;
					case FixTag.QuoteEntryID:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._quoteEntryID is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._quoteEntryID = (FixField.Text)field;
						break;
					case FixTag.MDEntryBuyer:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryBuyer is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryBuyer = (FixField.Text)field;
						break;
					case FixTag.MDEntrySeller:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntrySeller is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntrySeller = (FixField.Text)field;
						break;
					case FixTag.NumberOfOrders:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._numberOfOrders is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._numberOfOrders = (FixField.Integer)field;
						break;
					case FixTag.MDEntryPositionNo:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._mDEntryPositionNo is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._mDEntryPositionNo = (FixField.Integer)field;
						break;
					case FixTag.Text:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noMDEntriesGroups))
							Unentered(field);
						else if (noMDEntriesGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoMDEntries, noMDEntriesGroups!.Count - 1);
						else
							noMDEntriesGroups![^1]._encodedText = (FixField.Data)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX MDReqID, tag 262, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? MDReqID => _mDReqID;
		internal FixField.Text? _mDReqID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? FinancialStatus => _financialStatus;
		internal FixField.Character? _financialStatus;

		/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CorporateAction => _corporateAction;
		internal FixField.Character? _corporateAction;

		/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? TotalVolumeTraded => _totalVolumeTraded;
		internal FixField.Decimal? _totalVolumeTraded;

		/// <summary>The FIX NoMDEntries, tag 268, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoMDEntries => _noMDEntries;
		internal FixField.Integer? _noMDEntries;

		/// <summary>The entries counted by NoMDEntries, tag 268; null when the group is absent.</summary>
		public List<FixMessage.MarketDataSnapshotFullRefresh.NoMDEntriesGroup>? NoMDEntriesGroups => noMDEntriesGroups;
		internal List<NoMDEntriesGroup>? noMDEntriesGroups;

		/// <summary>One entry of the group counted by NoMDEntries, tag 268.</summary>
		public sealed class NoMDEntriesGroup : IFixEntry
		{
			internal NoMDEntriesGroup(FixField.Character delimiter)
			{
				MDEntryType = delimiter;
			}

			/// <summary>The FIX MDEntryType, tag 269, wire type <c>char</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Character MDEntryType { get; }

			/// <summary>The FIX MDEntryPx, tag 270, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntryPx => _mDEntryPx;
			internal FixField.Decimal? _mDEntryPx;

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency => _currency;
			internal FixField.Text? _currency;

			/// <summary>The FIX MDEntrySize, tag 271, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MDEntrySize => _mDEntrySize;
			internal FixField.Decimal? _mDEntrySize;

			/// <summary>The FIX MDEntryDate, tag 272, wire type <c>UTCDate</c>; null when the field is absent.</summary>
			public FixField.Date? MDEntryDate => _mDEntryDate;
			internal FixField.Date? _mDEntryDate;

			/// <summary>The FIX MDEntryTime, tag 273, wire type <c>UTCTimeOnly</c>; null when the field is absent.</summary>
			public FixField.Time? MDEntryTime => _mDEntryTime;
			internal FixField.Time? _mDEntryTime;

			/// <summary>The FIX TickDirection, tag 274, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TickDirection => _tickDirection;
			internal FixField.Character? _tickDirection;

			/// <summary>The FIX MDMkt, tag 275, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? MDMkt => _mDMkt;
			internal FixField.Text? _mDMkt;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			/// <summary>The FIX QuoteCondition, tag 276, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? QuoteCondition => _quoteCondition;
			internal FixField.Multiple? _quoteCondition;

			/// <summary>The FIX TradeCondition, tag 277, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? TradeCondition => _tradeCondition;
			internal FixField.Multiple? _tradeCondition;

			/// <summary>The FIX MDEntryOriginator, tag 282, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryOriginator => _mDEntryOriginator;
			internal FixField.Text? _mDEntryOriginator;

			/// <summary>The FIX LocationID, tag 283, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? LocationID => _locationID;
			internal FixField.Text? _locationID;

			/// <summary>The FIX DeskID, tag 284, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? DeskID => _deskID;
			internal FixField.Text? _deskID;

			/// <summary>The FIX OpenCloseSettleFlag, tag 286, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenCloseSettleFlag => _openCloseSettleFlag;
			internal FixField.Character? _openCloseSettleFlag;

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce => _timeInForce;
			internal FixField.Character? _timeInForce;

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate => _expireDate;
			internal FixField.Date? _expireDate;

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime => _expireTime;
			internal FixField.Timestamp? _expireTime;

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty => _minQty;
			internal FixField.Decimal? _minQty;

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst => _execInst;
			internal FixField.Multiple? _execInst;

			/// <summary>The FIX SellerDays, tag 287, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SellerDays => _sellerDays;
			internal FixField.Integer? _sellerDays;

			/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? OrderID => _orderID;
			internal FixField.Text? _orderID;

			/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteEntryID => _quoteEntryID;
			internal FixField.Text? _quoteEntryID;

			/// <summary>The FIX MDEntryBuyer, tag 288, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntryBuyer => _mDEntryBuyer;
			internal FixField.Text? _mDEntryBuyer;

			/// <summary>The FIX MDEntrySeller, tag 289, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? MDEntrySeller => _mDEntrySeller;
			internal FixField.Text? _mDEntrySeller;

			/// <summary>The FIX NumberOfOrders, tag 346, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NumberOfOrders => _numberOfOrders;
			internal FixField.Integer? _numberOfOrders;

			/// <summary>The FIX MDEntryPositionNo, tag 290, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? MDEntryPositionNo => _mDEntryPositionNo;
			internal FixField.Integer? _mDEntryPositionNo;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => MDEntryType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 15 or 18 or 37 or 58 or 59 or 110 or 126 or 269 or 270 or 271 or 272 or 273 or 274 or 275 or 276 or
					277 or 282 or 283 or 284 or 286 or 287 or 288 or 289 or 290 or 299 or 336 or 346 or 354 or 355 or 432;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoMDEntriesGroups, FixTag.NoMDEntries, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.MarketDataSnapshotFullRefresh(context, check, this);
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
					case FixTag.QuoteReqID: if (_quoteReqID is not null) Duplicate(field); _quoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.QuoteResponseLevel: if (_quoteResponseLevel is not null) Duplicate(field); _quoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.DefBidSize: if (_defBidSize is not null) Duplicate(field); _defBidSize = (FixField.Decimal)field; break;
					case FixTag.DefOfferSize: if (_defOfferSize is not null) Duplicate(field); _defOfferSize = (FixField.Decimal)field; break;
					case FixTag.NoQuoteSets: if (_noQuoteSets is not null) Duplicate(field); _noQuoteSets = (FixField.Integer)field; break;
					case FixTag.QuoteSetID:
						if (noQuoteSetsGroups is null && _noQuoteSets is null) Uncounted(FixTag.NoQuoteSets, field);
						(noQuoteSetsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.UnderlyingSymbol:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSymbol is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSymbolSfx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityID is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingIDSource is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityType is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingMaturityMonthYear is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingMaturityDay is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingPutOrCall is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingStrikePrice is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingOptAttribute is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingContractMultiplier is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingCouponRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityExchange is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingIssuerLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDescLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteSetValidUntilTime:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._quoteSetValidUntilTime is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._quoteSetValidUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._totQuoteEntries is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._totQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._noQuoteEntries is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._noQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else
						{
							if (noQuoteSetsGroups![^1].noQuoteEntriesGroups is null && noQuoteSetsGroups![^1]._noQuoteEntries is null) Uncounted(FixTag.NoQuoteEntries, field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
							(noQuoteSetsGroups![^1].noQuoteEntriesGroups ??= []).Add(new ((FixField.Text)field));
						}
						break;
					case FixTag.Symbol:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbol is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.BidPx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidPx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidPx = (FixField.Decimal)field;
						break;
					case FixTag.OfferPx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerPx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerPx = (FixField.Decimal)field;
						break;
					case FixTag.BidSize:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidSize is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidSize = (FixField.Decimal)field;
						break;
					case FixTag.OfferSize:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerSize is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerSize = (FixField.Decimal)field;
						break;
					case FixTag.ValidUntilTime:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._validUntilTime is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._validUntilTime = (FixField.Timestamp)field;
						break;
					case FixTag.BidSpotRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidSpotRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.OfferSpotRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerSpotRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerSpotRate = (FixField.Decimal)field;
						break;
					case FixTag.BidForwardPoints:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidForwardPoints is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._bidForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.OfferForwardPoints:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerForwardPoints is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._offerForwardPoints = (FixField.Decimal)field;
						break;
					case FixTag.TransactTime:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._transactTime is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._transactTime = (FixField.Timestamp)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.SettlDate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._futSettDate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._futSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._ordType is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._ordType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._futSettDate2 is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._futSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._orderQty2 is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._orderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._currency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID => _quoteReqID;
		internal FixField.Text? _quoteReqID;

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel => _quoteResponseLevel;
		internal FixField.Integer? _quoteResponseLevel;

		/// <summary>The FIX DefBidSize, tag 293, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DefBidSize => _defBidSize;
		internal FixField.Decimal? _defBidSize;

		/// <summary>The FIX DefOfferSize, tag 294, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? DefOfferSize => _defOfferSize;
		internal FixField.Decimal? _defOfferSize;

		/// <summary>The FIX NoQuoteSets, tag 296, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteSets => _noQuoteSets;
		internal FixField.Integer? _noQuoteSets;

		/// <summary>The entries counted by NoQuoteSets, tag 296; null when the group is absent.</summary>
		public List<FixMessage.MassQuote.NoQuoteSetsGroup>? NoQuoteSetsGroups => noQuoteSetsGroups;
		internal List<NoQuoteSetsGroup>? noQuoteSetsGroups;

		/// <summary>One entry of the group counted by NoQuoteSets, tag 296.</summary>
		public sealed class NoQuoteSetsGroup : IFixEntry
		{
			internal NoQuoteSetsGroup(FixField.Text delimiter)
			{
				QuoteSetID = delimiter;
			}

			/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text QuoteSetID { get; }

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol => _underlyingSymbol;
			internal FixField.Text? _underlyingSymbol;

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx => _underlyingSymbolSfx;
			internal FixField.Text? _underlyingSymbolSfx;

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID => _underlyingSecurityID;
			internal FixField.Text? _underlyingSecurityID;

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource => _underlyingIDSource;
			internal FixField.Text? _underlyingIDSource;

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType => _underlyingSecurityType;
			internal FixField.Text? _underlyingSecurityType;

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear => _underlyingMaturityMonthYear;
			internal FixField.MonthYear? _underlyingMaturityMonthYear;

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay => _underlyingMaturityDay;
			internal FixField.Integer? _underlyingMaturityDay;

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall => _underlyingPutOrCall;
			internal FixField.Integer? _underlyingPutOrCall;

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice => _underlyingStrikePrice;
			internal FixField.Decimal? _underlyingStrikePrice;

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute => _underlyingOptAttribute;
			internal FixField.Character? _underlyingOptAttribute;

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier => _underlyingContractMultiplier;
			internal FixField.Decimal? _underlyingContractMultiplier;

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate => _underlyingCouponRate;
			internal FixField.Decimal? _underlyingCouponRate;

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange => _underlyingSecurityExchange;
			internal FixField.Text? _underlyingSecurityExchange;

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer => _underlyingIssuer;
			internal FixField.Text? _underlyingIssuer;

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen => _encodedUnderlyingIssuerLen;
			internal FixField.Integer? _encodedUnderlyingIssuerLen;

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer => _encodedUnderlyingIssuer;
			internal FixField.Data? _encodedUnderlyingIssuer;

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc => _underlyingSecurityDesc;
			internal FixField.Text? _underlyingSecurityDesc;

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen => _encodedUnderlyingSecurityDescLen;
			internal FixField.Integer? _encodedUnderlyingSecurityDescLen;

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc => _encodedUnderlyingSecurityDesc;
			internal FixField.Data? _encodedUnderlyingSecurityDesc;

			/// <summary>The FIX QuoteSetValidUntilTime, tag 367, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? QuoteSetValidUntilTime => _quoteSetValidUntilTime;
			internal FixField.Timestamp? _quoteSetValidUntilTime;

			/// <summary>The FIX TotQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? TotQuoteEntries => _totQuoteEntries;
			internal FixField.Integer? _totQuoteEntries;

			/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoQuoteEntries => _noQuoteEntries;
			internal FixField.Integer? _noQuoteEntries;

			/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
			public List<FixMessage.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => noQuoteEntriesGroups;
			internal List<NoQuoteEntriesGroup>? noQuoteEntriesGroups;

			/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
			public sealed class NoQuoteEntriesGroup : IFixEntry
			{
				internal NoQuoteEntriesGroup(FixField.Text delimiter)
				{
					QuoteEntryID = delimiter;
				}

				/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
				public FixField.Text QuoteEntryID { get; }

				/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Symbol => _symbol;
				internal FixField.Text? _symbol;

				/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SymbolSfx => _symbolSfx;
				internal FixField.Text? _symbolSfx;

				/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityID => _securityID;
				internal FixField.Text? _securityID;

				/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? IDSource => _iDSource;
				internal FixField.Text? _iDSource;

				/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityType => _securityType;
				internal FixField.Text? _securityType;

				/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
				public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
				internal FixField.MonthYear? _maturityMonthYear;

				/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
				public FixField.Integer? MaturityDay => _maturityDay;
				internal FixField.Integer? _maturityDay;

				/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? PutOrCall => _putOrCall;
				internal FixField.Integer? _putOrCall;

				/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? StrikePrice => _strikePrice;
				internal FixField.Decimal? _strikePrice;

				/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OptAttribute => _optAttribute;
				internal FixField.Character? _optAttribute;

				/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? ContractMultiplier => _contractMultiplier;
				internal FixField.Decimal? _contractMultiplier;

				/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? CouponRate => _couponRate;
				internal FixField.Decimal? _couponRate;

				/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityExchange => _securityExchange;
				internal FixField.Text? _securityExchange;

				/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Issuer => _issuer;
				internal FixField.Text? _issuer;

				/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
				internal FixField.Integer? _encodedIssuerLen;

				/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedIssuer => _encodedIssuer;
				internal FixField.Data? _encodedIssuer;

				/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityDesc => _securityDesc;
				internal FixField.Text? _securityDesc;

				/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
				internal FixField.Integer? _encodedSecurityDescLen;

				/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
				internal FixField.Data? _encodedSecurityDesc;

				/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidPx => _bidPx;
				internal FixField.Decimal? _bidPx;

				/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferPx => _offerPx;
				internal FixField.Decimal? _offerPx;

				/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidSize => _bidSize;
				internal FixField.Decimal? _bidSize;

				/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferSize => _offerSize;
				internal FixField.Decimal? _offerSize;

				/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
				public FixField.Timestamp? ValidUntilTime => _validUntilTime;
				internal FixField.Timestamp? _validUntilTime;

				/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidSpotRate => _bidSpotRate;
				internal FixField.Decimal? _bidSpotRate;

				/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferSpotRate => _offerSpotRate;
				internal FixField.Decimal? _offerSpotRate;

				/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
				public FixField.Decimal? BidForwardPoints => _bidForwardPoints;
				internal FixField.Decimal? _bidForwardPoints;

				/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
				public FixField.Decimal? OfferForwardPoints => _offerForwardPoints;
				internal FixField.Decimal? _offerForwardPoints;

				/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
				public FixField.Timestamp? TransactTime => _transactTime;
				internal FixField.Timestamp? _transactTime;

				/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? TradingSessionID => _tradingSessionID;
				internal FixField.Text? _tradingSessionID;

				/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
				public FixField.Date? FutSettDate => _futSettDate;
				internal FixField.Date? _futSettDate;

				/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OrdType => _ordType;
				internal FixField.Character? _ordType;

				/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
				public FixField.Date? FutSettDate2 => _futSettDate2;
				internal FixField.Date? _futSettDate2;

				/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? OrderQty2 => _orderQty2;
				internal FixField.Decimal? _orderQty2;

				/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
				public FixField.Text? Currency => _currency;
				internal FixField.Text? _currency;

				FixField IFixEntry.Opener => QuoteEntryID;

				bool IFixEntry.Has(int tag)
				{
					return tag is 15 or 22 or 40 or 48 or 55 or 60 or 62 or 64 or 65 or 106 or 107 or 132 or 133 or 134 or 135 or
						167 or 188 or 189 or 190 or 191 or 192 or 193 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
						299 or 336 or 348 or 349 or 350 or 351;
				}

				IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
				{
					step = default;

					return null;
				}
			}

			FixField IFixEntry.Opener => QuoteSetID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 15 or 22 or 40 or 48 or 55 or 60 or 62 or 64 or 65 or 106 or 107 or 132 or 133 or 134 or 135 or 167 or
					188 or 189 or 190 or 191 or 192 or 193 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or 295 or 299 or
					302 or 304 or 305 or 306 or 307 or 308 or 309 or 310 or 311 or 312 or 313 or 314 or 315 or 316 or 317 or 336 or
					348 or 349 or 350 or 351 or 362 or 363 or 364 or 365 or 367 or 435 or 436;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				return FixEntries.Holding(NoQuoteEntriesGroups, FixTag.NoQuoteEntries, tag, position, out step);
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoQuoteSetsGroups, FixTag.NoQuoteSets, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.MassQuote(context, check, this);
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
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.BidID: if (_bidID is not null) Duplicate(field); _bidID = (FixField.Text)field; break;
					case FixTag.ClientBidID: if (_clientBidID is not null) Duplicate(field); _clientBidID = (FixField.Text)field; break;
					case FixTag.ProgRptReqs: if (_progRptReqs is not null) Duplicate(field); _progRptReqs = (FixField.Integer)field; break;
					case FixTag.BidType: if (_bidType is not null) Duplicate(field); _bidType = (FixField.Integer)field; break;
					case FixTag.ProgPeriodInterval: if (_progPeriodInterval is not null) Duplicate(field); _progPeriodInterval = (FixField.Integer)field; break;
					case FixTag.ListExecInstType: if (_listExecInstType is not null) Duplicate(field); _listExecInstType = (FixField.Character)field; break;
					case FixTag.ListExecInst: if (_listExecInst is not null) Duplicate(field); _listExecInst = (FixField.Text)field; break;
					case FixTag.EncodedListExecInstLen: if (_encodedListExecInstLen is not null) Duplicate(field); _encodedListExecInstLen = (FixField.Integer)field; break;
					case FixTag.EncodedListExecInst: if (_encodedListExecInst is not null) Duplicate(field); _encodedListExecInst = (FixField.Data)field; break;
					case FixTag.TotNoOrders: if (_totNoOrders is not null) Duplicate(field); _totNoOrders = (FixField.Integer)field; break;
					case FixTag.NoOrders: if (_noOrders is not null) Duplicate(field); _noOrders = (FixField.Integer)field; break;
					case FixTag.ClOrdID:
						if (noOrdersGroups is null && _noOrders is null) Uncounted(FixTag.NoOrders, field);
						(noOrdersGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.ListSeqNo:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._listSeqNo is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._listSeqNo = (FixField.Integer)field;
						break;
					case FixTag.SettlInstMode:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._settlInstMode is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._settlInstMode = (FixField.Character)field;
						break;
					case FixTag.ClientID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._clientID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._clientID = (FixField.Text)field;
						break;
					case FixTag.ExecBroker:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._execBroker is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._execBroker = (FixField.Text)field;
						break;
					case FixTag.Account:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._account is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._account = (FixField.Text)field;
						break;
					case FixTag.NoAllocs:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._noAllocs is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._noAllocs = (FixField.Integer)field;
						break;
					case FixTag.AllocAccount:
						if (None(noOrdersGroups))
							Unentered(field);
						else
						{
							if (noOrdersGroups![^1].noAllocsGroups is null && noOrdersGroups![^1]._noAllocs is null) Uncounted(FixTag.NoAllocs, field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
							(noOrdersGroups![^1].noAllocsGroups ??= []).Add(new ((FixField.Text)field));
						}
						break;
					case FixTag.AllocQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (None(noOrdersGroups![^1].noAllocsGroups))
							Unentered(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else if (noOrdersGroups![^1].noAllocsGroups![^1]._allocShares is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1, FixTag.NoAllocs, noOrdersGroups![^1].noAllocsGroups!.Count - 1);
						else
							noOrdersGroups![^1].noAllocsGroups![^1]._allocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._settlmntTyp is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._settlmntTyp = (FixField.Character)field;
						break;
					case FixTag.SettlDate:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._futSettDate is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._futSettDate = (FixField.Date)field;
						break;
					case FixTag.HandlInst:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._handlInst is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._handlInst = (FixField.Character)field;
						break;
					case FixTag.ExecInst:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._execInst is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._execInst = (FixField.Multiple)field;
						break;
					case FixTag.MinQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._minQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._minQty = (FixField.Decimal)field;
						break;
					case FixTag.MaxFloor:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._maxFloor is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._maxFloor = (FixField.Decimal)field;
						break;
					case FixTag.ExDestination:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._exDestination is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._exDestination = (FixField.Text)field;
						break;
					case FixTag.NoTradingSessions:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._noTradingSessions is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._noTradingSessions = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noOrdersGroups))
							Unentered(field);
						else
						{
							if (noOrdersGroups![^1].noTradingSessionsGroups is null && noOrdersGroups![^1]._noTradingSessions is null) Uncounted(FixTag.NoTradingSessions, field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
							(noOrdersGroups![^1].noTradingSessionsGroups ??= []).Add(new ((FixField.Text)field));
						}
						break;
					case FixTag.ProcessCode:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._processCode is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._processCode = (FixField.Character)field;
						break;
					case FixTag.Symbol:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._symbol is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._prevClosePx is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._prevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.SideValueInd:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._sideValueInd is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._sideValueInd = (FixField.Integer)field;
						break;
					case FixTag.LocateReqd:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._locateReqd is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._locateReqd = (FixField.Boolean)field;
						break;
					case FixTag.TransactTime:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._transactTime is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._transactTime = (FixField.Timestamp)field;
						break;
					case FixTag.OrderQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._orderQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._orderQty = (FixField.Decimal)field;
						break;
					case FixTag.CashOrderQty:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._cashOrderQty is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._cashOrderQty = (FixField.Decimal)field;
						break;
					case FixTag.OrdType:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._ordType is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._ordType = (FixField.Character)field;
						break;
					case FixTag.Price:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._price is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._price = (FixField.Decimal)field;
						break;
					case FixTag.StopPx:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._stopPx is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._stopPx = (FixField.Decimal)field;
						break;
					case FixTag.Currency:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._currency = (FixField.Text)field;
						break;
					case FixTag.ComplianceID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._complianceID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._complianceID = (FixField.Text)field;
						break;
					case FixTag.SolicitedFlag:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._solicitedFlag is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._solicitedFlag = (FixField.Boolean)field;
						break;
					case FixTag.IOIID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._iOIid is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._iOIid = (FixField.Text)field;
						break;
					case FixTag.QuoteID:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._quoteID is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._quoteID = (FixField.Text)field;
						break;
					case FixTag.TimeInForce:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._timeInForce is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._timeInForce = (FixField.Character)field;
						break;
					case FixTag.EffectiveTime:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._effectiveTime is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._effectiveTime = (FixField.Timestamp)field;
						break;
					case FixTag.ExpireDate:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._expireDate is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._expireDate = (FixField.Date)field;
						break;
					case FixTag.ExpireTime:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._expireTime is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._expireTime = (FixField.Timestamp)field;
						break;
					case FixTag.GTBookingInst:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._gTBookingInst is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._gTBookingInst = (FixField.Integer)field;
						break;
					case FixTag.Commission:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._commission is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._commission = (FixField.Decimal)field;
						break;
					case FixTag.CommType:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._commType is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._commType = (FixField.Character)field;
						break;
					case FixTag.Rule80A:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._rule80A is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._rule80A = (FixField.Character)field;
						break;
					case FixTag.ForexReq:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._forexReq is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._forexReq = (FixField.Boolean)field;
						break;
					case FixTag.SettlCurrency:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._settlCurrency is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._settlCurrency = (FixField.Text)field;
						break;
					case FixTag.Text:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._text is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._text = (FixField.Text)field;
						break;
					case FixTag.EncodedTextLen:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._encodedText = (FixField.Data)field;
						break;
					case FixTag.SettlDate2:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._futSettDate2 is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._futSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._orderQty2 is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._orderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.PositionEffect:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._openClose is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._openClose = (FixField.Character)field;
						break;
					case FixTag.CoveredOrUncovered:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._coveredOrUncovered is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._coveredOrUncovered = (FixField.Integer)field;
						break;
					case FixTag.CustomerOrFirm:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._customerOrFirm is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._customerOrFirm = (FixField.Integer)field;
						break;
					case FixTag.MaxShow:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._maxShow is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._maxShow = (FixField.Decimal)field;
						break;
					case FixTag.PegOffsetValue:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._pegDifference is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._pegDifference = (FixField.Decimal)field;
						break;
					case FixTag.DiscretionInst:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._discretionInst is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._discretionInst = (FixField.Character)field;
						break;
					case FixTag.DiscretionOffsetValue:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._discretionOffset is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._discretionOffset = (FixField.Decimal)field;
						break;
					case FixTag.ClearingFirm:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._clearingFirm is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._clearingFirm = (FixField.Text)field;
						break;
					case FixTag.ClearingAccount:
						if (None(noOrdersGroups))
							Unentered(field);
						else if (noOrdersGroups![^1]._clearingAccount is not null)
							Duplicate(field, FixTag.NoOrders, noOrdersGroups!.Count - 1);
						else
							noOrdersGroups![^1]._clearingAccount = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX BidID, tag 390, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? BidID => _bidID;
		internal FixField.Text? _bidID;

		/// <summary>The FIX ClientBidID, tag 391, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientBidID => _clientBidID;
		internal FixField.Text? _clientBidID;

		/// <summary>The FIX ProgRptReqs, tag 414, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgRptReqs => _progRptReqs;
		internal FixField.Integer? _progRptReqs;

		/// <summary>The FIX BidType, tag 394, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BidType => _bidType;
		internal FixField.Integer? _bidType;

		/// <summary>The FIX ProgPeriodInterval, tag 415, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? ProgPeriodInterval => _progPeriodInterval;
		internal FixField.Integer? _progPeriodInterval;

		/// <summary>The FIX ListExecInstType, tag 433, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ListExecInstType => _listExecInstType;
		internal FixField.Character? _listExecInstType;

		/// <summary>The FIX ListExecInst, tag 69, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListExecInst => _listExecInst;
		internal FixField.Text? _listExecInst;

		/// <summary>The FIX EncodedListExecInstLen, tag 352, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedListExecInstLen => _encodedListExecInstLen;
		internal FixField.Integer? _encodedListExecInstLen;

		/// <summary>The FIX EncodedListExecInst, tag 353, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedListExecInst => _encodedListExecInst;
		internal FixField.Data? _encodedListExecInst;

		/// <summary>The FIX TotNoOrders, tag 68, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotNoOrders => _totNoOrders;
		internal FixField.Integer? _totNoOrders;

		/// <summary>The FIX NoOrders, tag 73, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoOrders => _noOrders;
		internal FixField.Integer? _noOrders;

		/// <summary>The entries counted by NoOrders, tag 73; null when the group is absent.</summary>
		public List<FixMessage.NewOrderList.NoOrdersGroup>? NoOrdersGroups => noOrdersGroups;
		internal List<NoOrdersGroup>? noOrdersGroups;

		/// <summary>One entry of the group counted by NoOrders, tag 73.</summary>
		public sealed class NoOrdersGroup : IFixEntry
		{
			internal NoOrdersGroup(FixField.Text delimiter)
			{
				ClOrdID = delimiter;
			}

			/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text ClOrdID { get; }

			/// <summary>The FIX ListSeqNo, tag 67, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? ListSeqNo => _listSeqNo;
			internal FixField.Integer? _listSeqNo;

			/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlInstMode => _settlInstMode;
			internal FixField.Character? _settlInstMode;

			/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClientID => _clientID;
			internal FixField.Text? _clientID;

			/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ExecBroker => _execBroker;
			internal FixField.Text? _execBroker;

			/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Account => _account;
			internal FixField.Text? _account;

			/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoAllocs => _noAllocs;
			internal FixField.Integer? _noAllocs;

			/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
			public List<FixMessage.NewOrderList.NoOrdersGroup.NoAllocsGroup>? NoAllocsGroups => noAllocsGroups;
			internal List<NoAllocsGroup>? noAllocsGroups;

			/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? SettlmntTyp => _settlmntTyp;
			internal FixField.Character? _settlmntTyp;

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate => _futSettDate;
			internal FixField.Date? _futSettDate;

			/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? HandlInst => _handlInst;
			internal FixField.Character? _handlInst;

			/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
			public FixField.Multiple? ExecInst => _execInst;
			internal FixField.Multiple? _execInst;

			/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MinQty => _minQty;
			internal FixField.Decimal? _minQty;

			/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MaxFloor => _maxFloor;
			internal FixField.Decimal? _maxFloor;

			/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? ExDestination => _exDestination;
			internal FixField.Text? _exDestination;

			/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoTradingSessions => _noTradingSessions;
			internal FixField.Integer? _noTradingSessions;

			/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
			public List<FixMessage.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup>? NoTradingSessionsGroups => noTradingSessionsGroups;
			internal List<NoTradingSessionsGroup>? noTradingSessionsGroups;

			/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? ProcessCode => _processCode;
			internal FixField.Character? _processCode;

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Symbol => _symbol;
			internal FixField.Text? _symbol;

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx => _prevClosePx;
			internal FixField.Decimal? _prevClosePx;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX SideValueInd, tag 401, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? SideValueInd => _sideValueInd;
			internal FixField.Integer? _sideValueInd;

			/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? LocateReqd => _locateReqd;
			internal FixField.Boolean? _locateReqd;

			/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? TransactTime => _transactTime;
			internal FixField.Timestamp? _transactTime;

			/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty => _orderQty;
			internal FixField.Decimal? _orderQty;

			/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? CashOrderQty => _cashOrderQty;
			internal FixField.Decimal? _cashOrderQty;

			/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdType => _ordType;
			internal FixField.Character? _ordType;

			/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? Price => _price;
			internal FixField.Decimal? _price;

			/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StopPx => _stopPx;
			internal FixField.Decimal? _stopPx;

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency => _currency;
			internal FixField.Text? _currency;

			/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ComplianceID => _complianceID;
			internal FixField.Text? _complianceID;

			/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? SolicitedFlag => _solicitedFlag;
			internal FixField.Boolean? _solicitedFlag;

			/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IOIid => _iOIid;
			internal FixField.Text? _iOIid;

			/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? QuoteID => _quoteID;
			internal FixField.Text? _quoteID;

			/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? TimeInForce => _timeInForce;
			internal FixField.Character? _timeInForce;

			/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? EffectiveTime => _effectiveTime;
			internal FixField.Timestamp? _effectiveTime;

			/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? ExpireDate => _expireDate;
			internal FixField.Date? _expireDate;

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime => _expireTime;
			internal FixField.Timestamp? _expireTime;

			/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? GTBookingInst => _gTBookingInst;
			internal FixField.Integer? _gTBookingInst;

			/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
			public FixField.Decimal? Commission => _commission;
			internal FixField.Decimal? _commission;

			/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? CommType => _commType;
			internal FixField.Character? _commType;

			/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Rule80A => _rule80A;
			internal FixField.Character? _rule80A;

			/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
			public FixField.Boolean? ForexReq => _forexReq;
			internal FixField.Boolean? _forexReq;

			/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? SettlCurrency => _settlCurrency;
			internal FixField.Text? _settlCurrency;

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Text => _text;
			internal FixField.Text? _text;

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate2 => _futSettDate2;
			internal FixField.Date? _futSettDate2;

			/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty2 => _orderQty2;
			internal FixField.Decimal? _orderQty2;

			/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OpenClose => _openClose;
			internal FixField.Character? _openClose;

			/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? CoveredOrUncovered => _coveredOrUncovered;
			internal FixField.Integer? _coveredOrUncovered;

			/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? CustomerOrFirm => _customerOrFirm;
			internal FixField.Integer? _customerOrFirm;

			/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? MaxShow => _maxShow;
			internal FixField.Decimal? _maxShow;

			/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
			public FixField.Decimal? PegDifference => _pegDifference;
			internal FixField.Decimal? _pegDifference;

			/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? DiscretionInst => _discretionInst;
			internal FixField.Character? _discretionInst;

			/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
			public FixField.Decimal? DiscretionOffset => _discretionOffset;
			internal FixField.Decimal? _discretionOffset;

			/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClearingFirm => _clearingFirm;
			internal FixField.Text? _clearingFirm;

			/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? ClearingAccount => _clearingAccount;
			internal FixField.Text? _clearingAccount;

			/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
			public sealed class NoAllocsGroup : IFixEntry
			{
				internal NoAllocsGroup(FixField.Text delimiter)
				{
					AllocAccount = delimiter;
				}

				/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
				public FixField.Text AllocAccount { get; }

				/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
				public FixField.Decimal? AllocShares => _allocShares;
				internal FixField.Decimal? _allocShares;

				FixField IFixEntry.Opener => AllocAccount;

				bool IFixEntry.Has(int tag)
				{
					return tag is 79 or 80;
				}

				IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
				{
					step = default;

					return null;
				}
			}

			/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
			public sealed class NoTradingSessionsGroup : IFixEntry
			{
				internal NoTradingSessionsGroup(FixField.Text delimiter)
				{
					TradingSessionID = delimiter;
				}

				/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
				public FixField.Text TradingSessionID { get; }

				FixField IFixEntry.Opener => TradingSessionID;

				bool IFixEntry.Has(int tag)
				{
					return tag is 336;
				}

				IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
				{
					step = default;

					return null;
				}
			}

			FixField IFixEntry.Opener => ClOrdID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 1 or 11 or 12 or 13 or 15 or 18 or 21 or 22 or 23 or 38 or 40 or 44 or 47 or 48 or 54 or 55 or 58 or
					59 or 60 or 63 or 64 or 65 or 67 or 76 or 77 or 78 or 79 or 80 or 81 or 99 or 100 or 106 or 107 or 109 or 110 or
					111 or 114 or 117 or 120 or 121 or 126 or 140 or 152 or 160 or 167 or 168 or 192 or 193 or 200 or 201 or 202 or
					203 or 204 or 205 or 206 or 207 or 210 or 211 or 223 or 231 or 336 or 348 or 349 or 350 or 351 or 354 or 355 or
					376 or 377 or 386 or 388 or 389 or 401 or 427 or 432 or 439 or 440;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				return FixEntries.Holding(NoAllocsGroups, FixTag.NoAllocs, tag, position, out step) ??
					FixEntries.Holding(NoTradingSessionsGroups, FixTag.NoTradingSessions, tag, position, out step);
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoOrdersGroups, FixTag.NoOrders, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.NewOrderList(context, check, this);
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
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.NoAllocs: if (_noAllocs is not null) Duplicate(field); _noAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (noAllocsGroups is null && _noAllocs is null) Uncounted(FixTag.NoAllocs, field);
						(noAllocsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.AllocQty:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocShares is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType: if (_settlmntTyp is not null) Duplicate(field); _settlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (_futSettDate is not null) Duplicate(field); _futSettDate = (FixField.Date)field; break;
					case FixTag.HandlInst: if (_handlInst is not null) Duplicate(field); _handlInst = (FixField.Character)field; break;
					case FixTag.ExecInst: if (_execInst is not null) Duplicate(field); _execInst = (FixField.Multiple)field; break;
					case FixTag.MinQty: if (_minQty is not null) Duplicate(field); _minQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (_maxFloor is not null) Duplicate(field); _maxFloor = (FixField.Decimal)field; break;
					case FixTag.ExDestination: if (_exDestination is not null) Duplicate(field); _exDestination = (FixField.Text)field; break;
					case FixTag.NoTradingSessions: if (_noTradingSessions is not null) Duplicate(field); _noTradingSessions = (FixField.Integer)field; break;
					case FixTag.TradingSessionID:
						if (noTradingSessionsGroups is null && _noTradingSessions is null) Uncounted(FixTag.NoTradingSessions, field);
						(noTradingSessionsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.ProcessCode: if (_processCode is not null) Duplicate(field); _processCode = (FixField.Character)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.PrevClosePx: if (_prevClosePx is not null) Duplicate(field); _prevClosePx = (FixField.Decimal)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.LocateReqd: if (_locateReqd is not null) Duplicate(field); _locateReqd = (FixField.Boolean)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (_orderQty is not null) Duplicate(field); _orderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (_cashOrderQty is not null) Duplicate(field); _cashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (_ordType is not null) Duplicate(field); _ordType = (FixField.Character)field; break;
					case FixTag.Price: if (_price is not null) Duplicate(field); _price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (_stopPx is not null) Duplicate(field); _stopPx = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.ComplianceID: if (_complianceID is not null) Duplicate(field); _complianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (_solicitedFlag is not null) Duplicate(field); _solicitedFlag = (FixField.Boolean)field; break;
					case FixTag.IOIID: if (_iOIid is not null) Duplicate(field); _iOIid = (FixField.Text)field; break;
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.TimeInForce: if (_timeInForce is not null) Duplicate(field); _timeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (_effectiveTime is not null) Duplicate(field); _effectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (_expireDate is not null) Duplicate(field); _expireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (_expireTime is not null) Duplicate(field); _expireTime = (FixField.Timestamp)field; break;
					case FixTag.GTBookingInst: if (_gTBookingInst is not null) Duplicate(field); _gTBookingInst = (FixField.Integer)field; break;
					case FixTag.Commission: if (_commission is not null) Duplicate(field); _commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (_commType is not null) Duplicate(field); _commType = (FixField.Character)field; break;
					case FixTag.Rule80A: if (_rule80A is not null) Duplicate(field); _rule80A = (FixField.Character)field; break;
					case FixTag.ForexReq: if (_forexReq is not null) Duplicate(field); _forexReq = (FixField.Boolean)field; break;
					case FixTag.SettlCurrency: if (_settlCurrency is not null) Duplicate(field); _settlCurrency = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (_futSettDate2 is not null) Duplicate(field); _futSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (_orderQty2 is not null) Duplicate(field); _orderQty2 = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (_openClose is not null) Duplicate(field); _openClose = (FixField.Character)field; break;
					case FixTag.CoveredOrUncovered: if (_coveredOrUncovered is not null) Duplicate(field); _coveredOrUncovered = (FixField.Integer)field; break;
					case FixTag.CustomerOrFirm: if (_customerOrFirm is not null) Duplicate(field); _customerOrFirm = (FixField.Integer)field; break;
					case FixTag.MaxShow: if (_maxShow is not null) Duplicate(field); _maxShow = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (_pegDifference is not null) Duplicate(field); _pegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (_discretionInst is not null) Duplicate(field); _discretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (_discretionOffset is not null) Duplicate(field); _discretionOffset = (FixField.Decimal)field; break;
					case FixTag.ClearingFirm: if (_clearingFirm is not null) Duplicate(field); _clearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (_clearingAccount is not null) Duplicate(field); _clearingAccount = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs => _noAllocs;
		internal FixField.Integer? _noAllocs;

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public List<FixMessage.NewOrderSingle.NoAllocsGroup>? NoAllocsGroups => noAllocsGroups;
		internal List<NoAllocsGroup>? noAllocsGroups;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp => _settlmntTyp;
		internal FixField.Character? _settlmntTyp;

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate => _futSettDate;
		internal FixField.Date? _futSettDate;

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst => _handlInst;
		internal FixField.Character? _handlInst;

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst => _execInst;
		internal FixField.Multiple? _execInst;

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty => _minQty;
		internal FixField.Decimal? _minQty;

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor => _maxFloor;
		internal FixField.Decimal? _maxFloor;

		/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? ExDestination => _exDestination;
		internal FixField.Text? _exDestination;

		/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoTradingSessions => _noTradingSessions;
		internal FixField.Integer? _noTradingSessions;

		/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
		public List<FixMessage.NewOrderSingle.NoTradingSessionsGroup>? NoTradingSessionsGroups => noTradingSessionsGroups;
		internal List<NoTradingSessionsGroup>? noTradingSessionsGroups;

		/// <summary>The FIX ProcessCode, tag 81, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? ProcessCode => _processCode;
		internal FixField.Character? _processCode;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? PrevClosePx => _prevClosePx;
		internal FixField.Decimal? _prevClosePx;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? LocateReqd => _locateReqd;
		internal FixField.Boolean? _locateReqd;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty => _orderQty;
		internal FixField.Decimal? _orderQty;

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty => _cashOrderQty;
		internal FixField.Decimal? _cashOrderQty;

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType => _ordType;
		internal FixField.Character? _ordType;

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price => _price;
		internal FixField.Decimal? _price;

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx => _stopPx;
		internal FixField.Decimal? _stopPx;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID => _complianceID;
		internal FixField.Text? _complianceID;

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag => _solicitedFlag;
		internal FixField.Boolean? _solicitedFlag;

		/// <summary>The FIX IOIid, tag 23, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IOIid => _iOIid;
		internal FixField.Text? _iOIid;

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce => _timeInForce;
		internal FixField.Character? _timeInForce;

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime => _effectiveTime;
		internal FixField.Timestamp? _effectiveTime;

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate => _expireDate;
		internal FixField.Date? _expireDate;

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime => _expireTime;
		internal FixField.Timestamp? _expireTime;

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst => _gTBookingInst;
		internal FixField.Integer? _gTBookingInst;

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission => _commission;
		internal FixField.Decimal? _commission;

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType => _commType;
		internal FixField.Character? _commType;

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A => _rule80A;
		internal FixField.Character? _rule80A;

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq => _forexReq;
		internal FixField.Boolean? _forexReq;

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency => _settlCurrency;
		internal FixField.Text? _settlCurrency;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 => _futSettDate2;
		internal FixField.Date? _futSettDate2;

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 => _orderQty2;
		internal FixField.Decimal? _orderQty2;

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose => _openClose;
		internal FixField.Character? _openClose;

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CoveredOrUncovered => _coveredOrUncovered;
		internal FixField.Integer? _coveredOrUncovered;

		/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CustomerOrFirm => _customerOrFirm;
		internal FixField.Integer? _customerOrFirm;

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow => _maxShow;
		internal FixField.Decimal? _maxShow;

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference => _pegDifference;
		internal FixField.Decimal? _pegDifference;

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst => _discretionInst;
		internal FixField.Character? _discretionInst;

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset => _discretionOffset;
		internal FixField.Decimal? _discretionOffset;

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm => _clearingFirm;
		internal FixField.Text? _clearingFirm;

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount => _clearingAccount;
		internal FixField.Text? _clearingAccount;

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup : IFixEntry
		{
			internal NoAllocsGroup(FixField.Text delimiter)
			{
				AllocAccount = delimiter;
			}

			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text AllocAccount { get; }

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares => _allocShares;
			internal FixField.Decimal? _allocShares;

			FixField IFixEntry.Opener => AllocAccount;

			bool IFixEntry.Has(int tag)
			{
				return tag is 79 or 80;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
		public sealed class NoTradingSessionsGroup : IFixEntry
		{
			internal NoTradingSessionsGroup(FixField.Text delimiter)
			{
				TradingSessionID = delimiter;
			}

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text TradingSessionID { get; }

			FixField IFixEntry.Opener => TradingSessionID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 336;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoAllocsGroups, FixTag.NoAllocs, tag, position, out step) ??
				FixEntries.Holding(NoTradingSessionsGroups, FixTag.NoTradingSessions, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.NewOrderSingle(context, check, this);
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
					case FixTag.OrigTime: if (_origTime is not null) Duplicate(field); _origTime = (FixField.Timestamp)field; break;
					case FixTag.Urgency: if (_urgency is not null) Duplicate(field); _urgency = (FixField.Character)field; break;
					case FixTag.Headline: if (_headline is not null) Duplicate(field); _headline = (FixField.Text)field; break;
					case FixTag.EncodedHeadlineLen: if (_encodedHeadlineLen is not null) Duplicate(field); _encodedHeadlineLen = (FixField.Integer)field; break;
					case FixTag.EncodedHeadline: if (_encodedHeadline is not null) Duplicate(field); _encodedHeadline = (FixField.Data)field; break;
					case FixTag.NoRoutingIDs: if (_noRoutingIDs is not null) Duplicate(field); _noRoutingIDs = (FixField.Integer)field; break;
					case FixTag.RoutingType:
						if (noRoutingIDsGroups is null && _noRoutingIDs is null) Uncounted(FixTag.NoRoutingIDs, field);
						(noRoutingIDsGroups ??= []).Add(new ((FixField.Integer)field));
						break;
					case FixTag.RoutingID:
						if (None(noRoutingIDsGroups))
							Unentered(field);
						else if (noRoutingIDsGroups![^1]._routingID is not null)
							Duplicate(field, FixTag.NoRoutingIDs, noRoutingIDsGroups!.Count - 1);
						else
							noRoutingIDsGroups![^1]._routingID = (FixField.Text)field;
						break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.RelatdSym:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.NoLinesOfText: if (_linesOfText is not null) Duplicate(field); _linesOfText = (FixField.Integer)field; break;
					case FixTag.Text:
						if (linesOfTextGroups is null && _linesOfText is null) Uncounted(FixTag.NoLinesOfText, field);
						(linesOfTextGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.EncodedTextLen:
						if (None(linesOfTextGroups))
							Unentered(field);
						else if (linesOfTextGroups![^1]._encodedTextLen is not null)
							Duplicate(field, FixTag.NoLinesOfText, linesOfTextGroups!.Count - 1);
						else
							linesOfTextGroups![^1]._encodedTextLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedText:
						if (None(linesOfTextGroups))
							Unentered(field);
						else if (linesOfTextGroups![^1]._encodedText is not null)
							Duplicate(field, FixTag.NoLinesOfText, linesOfTextGroups!.Count - 1);
						else
							linesOfTextGroups![^1]._encodedText = (FixField.Data)field;
						break;
					case FixTag.URLLink: if (_uRLLink is not null) Duplicate(field); _uRLLink = (FixField.Text)field; break;
					case FixTag.RawDataLength: if (_rawDataLength is not null) Duplicate(field); _rawDataLength = (FixField.Integer)field; break;
					case FixTag.RawData: if (_rawData is not null) Duplicate(field); _rawData = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrigTime, tag 42, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? OrigTime => _origTime;
		internal FixField.Timestamp? _origTime;

		/// <summary>The FIX Urgency, tag 61, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Urgency => _urgency;
		internal FixField.Character? _urgency;

		/// <summary>The FIX Headline, tag 148, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Headline => _headline;
		internal FixField.Text? _headline;

		/// <summary>The FIX EncodedHeadlineLen, tag 358, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedHeadlineLen => _encodedHeadlineLen;
		internal FixField.Integer? _encodedHeadlineLen;

		/// <summary>The FIX EncodedHeadline, tag 359, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedHeadline => _encodedHeadline;
		internal FixField.Data? _encodedHeadline;

		/// <summary>The FIX NoRoutingIDs, tag 215, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRoutingIDs => _noRoutingIDs;
		internal FixField.Integer? _noRoutingIDs;

		/// <summary>The entries counted by NoRoutingIDs, tag 215; null when the group is absent.</summary>
		public List<FixMessage.News.NoRoutingIDsGroup>? NoRoutingIDsGroups => noRoutingIDsGroups;
		internal List<NoRoutingIDsGroup>? noRoutingIDsGroups;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.News.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>The FIX LinesOfText, tag 33, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? LinesOfText => _linesOfText;
		internal FixField.Integer? _linesOfText;

		/// <summary>The entries counted by LinesOfText, tag 33; null when the group is absent.</summary>
		public List<FixMessage.News.LinesOfTextGroup>? LinesOfTextGroups => linesOfTextGroups;
		internal List<LinesOfTextGroup>? linesOfTextGroups;

		/// <summary>The FIX URLLink, tag 149, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? URLLink => _uRLLink;
		internal FixField.Text? _uRLLink;

		/// <summary>The FIX RawDataLength, tag 95, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? RawDataLength => _rawDataLength;
		internal FixField.Integer? _rawDataLength;

		/// <summary>The FIX RawData, tag 96, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? RawData => _rawData;
		internal FixField.Data? _rawData;

		/// <summary>One entry of the group counted by NoRoutingIDs, tag 215.</summary>
		public sealed class NoRoutingIDsGroup : IFixEntry
		{
			internal NoRoutingIDsGroup(FixField.Integer delimiter)
			{
				RoutingType = delimiter;
			}

			/// <summary>The FIX RoutingType, tag 216, wire type <c>int</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Integer RoutingType { get; }

			/// <summary>The FIX RoutingID, tag 217, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? RoutingID => _routingID;
			internal FixField.Text? _routingID;

			FixField IFixEntry.Opener => RoutingType;

			bool IFixEntry.Has(int tag)
			{
				return tag is 216 or 217;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				RelatdSym = delimiter;
			}

			/// <summary>The FIX RelatdSym, tag 46, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text RelatdSym { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			FixField IFixEntry.Opener => RelatdSym;

			bool IFixEntry.Has(int tag)
			{
				return tag is 22 or 46 or 48 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
					348 or 349 or 350 or 351;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by LinesOfText, tag 33.</summary>
		public sealed class LinesOfTextGroup : IFixEntry
		{
			internal LinesOfTextGroup(FixField.Text delimiter)
			{
				Text = delimiter;
			}

			/// <summary>The FIX Text, tag 58, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Text { get; }

			/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedTextLen => _encodedTextLen;
			internal FixField.Integer? _encodedTextLen;

			/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedText => _encodedText;
			internal FixField.Data? _encodedText;

			FixField IFixEntry.Opener => Text;

			bool IFixEntry.Has(int tag)
			{
				return tag is 58 or 354 or 355;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoRoutingIDsGroups, FixTag.NoRoutingIDs, tag, position, out step) ??
				FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step) ??
				FixEntries.Holding(LinesOfTextGroups, FixTag.NoLinesOfText, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.News(context, check, this);
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
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.SecondaryOrderID: if (_secondaryOrderID is not null) Duplicate(field); _secondaryOrderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (_origClOrdID is not null) Duplicate(field); _origClOrdID = (FixField.Text)field; break;
					case FixTag.OrdStatus: if (_ordStatus is not null) Duplicate(field); _ordStatus = (FixField.Character)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.CxlRejResponseTo: if (_cxlRejResponseTo is not null) Duplicate(field); _cxlRejResponseTo = (FixField.Character)field; break;
					case FixTag.CxlRejReason: if (_cxlRejReason is not null) Duplicate(field); _cxlRejReason = (FixField.Integer)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX SecondaryOrderID, tag 198, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecondaryOrderID => _secondaryOrderID;
		internal FixField.Text? _secondaryOrderID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID => _origClOrdID;
		internal FixField.Text? _origClOrdID;

		/// <summary>The FIX OrdStatus, tag 39, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdStatus => _ordStatus;
		internal FixField.Character? _ordStatus;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX CxlRejResponseTo, tag 434, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CxlRejResponseTo => _cxlRejResponseTo;
		internal FixField.Character? _cxlRejResponseTo;

		/// <summary>The FIX CxlRejReason, tag 102, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CxlRejReason => _cxlRejReason;
		internal FixField.Integer? _cxlRejReason;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.OrderCancelReject(context, check, this);
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
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.OrigClOrdID: if (_origClOrdID is not null) Duplicate(field); _origClOrdID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.NoAllocs: if (_noAllocs is not null) Duplicate(field); _noAllocs = (FixField.Integer)field; break;
					case FixTag.AllocAccount:
						if (noAllocsGroups is null && _noAllocs is null) Uncounted(FixTag.NoAllocs, field);
						(noAllocsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.AllocQty:
						if (None(noAllocsGroups))
							Unentered(field);
						else if (noAllocsGroups![^1]._allocShares is not null)
							Duplicate(field, FixTag.NoAllocs, noAllocsGroups!.Count - 1);
						else
							noAllocsGroups![^1]._allocShares = (FixField.Decimal)field;
						break;
					case FixTag.SettlType: if (_settlmntTyp is not null) Duplicate(field); _settlmntTyp = (FixField.Character)field; break;
					case FixTag.SettlDate: if (_futSettDate is not null) Duplicate(field); _futSettDate = (FixField.Date)field; break;
					case FixTag.HandlInst: if (_handlInst is not null) Duplicate(field); _handlInst = (FixField.Character)field; break;
					case FixTag.ExecInst: if (_execInst is not null) Duplicate(field); _execInst = (FixField.Multiple)field; break;
					case FixTag.MinQty: if (_minQty is not null) Duplicate(field); _minQty = (FixField.Decimal)field; break;
					case FixTag.MaxFloor: if (_maxFloor is not null) Duplicate(field); _maxFloor = (FixField.Decimal)field; break;
					case FixTag.ExDestination: if (_exDestination is not null) Duplicate(field); _exDestination = (FixField.Text)field; break;
					case FixTag.NoTradingSessions: if (_noTradingSessions is not null) Duplicate(field); _noTradingSessions = (FixField.Integer)field; break;
					case FixTag.TradingSessionID:
						if (noTradingSessionsGroups is null && _noTradingSessions is null) Uncounted(FixTag.NoTradingSessions, field);
						(noTradingSessionsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (_orderQty is not null) Duplicate(field); _orderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (_cashOrderQty is not null) Duplicate(field); _cashOrderQty = (FixField.Decimal)field; break;
					case FixTag.OrdType: if (_ordType is not null) Duplicate(field); _ordType = (FixField.Character)field; break;
					case FixTag.Price: if (_price is not null) Duplicate(field); _price = (FixField.Decimal)field; break;
					case FixTag.StopPx: if (_stopPx is not null) Duplicate(field); _stopPx = (FixField.Decimal)field; break;
					case FixTag.PegOffsetValue: if (_pegDifference is not null) Duplicate(field); _pegDifference = (FixField.Decimal)field; break;
					case FixTag.DiscretionInst: if (_discretionInst is not null) Duplicate(field); _discretionInst = (FixField.Character)field; break;
					case FixTag.DiscretionOffsetValue: if (_discretionOffset is not null) Duplicate(field); _discretionOffset = (FixField.Decimal)field; break;
					case FixTag.ComplianceID: if (_complianceID is not null) Duplicate(field); _complianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (_solicitedFlag is not null) Duplicate(field); _solicitedFlag = (FixField.Boolean)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.TimeInForce: if (_timeInForce is not null) Duplicate(field); _timeInForce = (FixField.Character)field; break;
					case FixTag.EffectiveTime: if (_effectiveTime is not null) Duplicate(field); _effectiveTime = (FixField.Timestamp)field; break;
					case FixTag.ExpireDate: if (_expireDate is not null) Duplicate(field); _expireDate = (FixField.Date)field; break;
					case FixTag.ExpireTime: if (_expireTime is not null) Duplicate(field); _expireTime = (FixField.Timestamp)field; break;
					case FixTag.GTBookingInst: if (_gTBookingInst is not null) Duplicate(field); _gTBookingInst = (FixField.Integer)field; break;
					case FixTag.Commission: if (_commission is not null) Duplicate(field); _commission = (FixField.Decimal)field; break;
					case FixTag.CommType: if (_commType is not null) Duplicate(field); _commType = (FixField.Character)field; break;
					case FixTag.Rule80A: if (_rule80A is not null) Duplicate(field); _rule80A = (FixField.Character)field; break;
					case FixTag.ForexReq: if (_forexReq is not null) Duplicate(field); _forexReq = (FixField.Boolean)field; break;
					case FixTag.SettlCurrency: if (_settlCurrency is not null) Duplicate(field); _settlCurrency = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.SettlDate2: if (_futSettDate2 is not null) Duplicate(field); _futSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (_orderQty2 is not null) Duplicate(field); _orderQty2 = (FixField.Decimal)field; break;
					case FixTag.PositionEffect: if (_openClose is not null) Duplicate(field); _openClose = (FixField.Character)field; break;
					case FixTag.CoveredOrUncovered: if (_coveredOrUncovered is not null) Duplicate(field); _coveredOrUncovered = (FixField.Integer)field; break;
					case FixTag.CustomerOrFirm: if (_customerOrFirm is not null) Duplicate(field); _customerOrFirm = (FixField.Integer)field; break;
					case FixTag.MaxShow: if (_maxShow is not null) Duplicate(field); _maxShow = (FixField.Decimal)field; break;
					case FixTag.LocateReqd: if (_locateReqd is not null) Duplicate(field); _locateReqd = (FixField.Boolean)field; break;
					case FixTag.ClearingFirm: if (_clearingFirm is not null) Duplicate(field); _clearingFirm = (FixField.Text)field; break;
					case FixTag.ClearingAccount: if (_clearingAccount is not null) Duplicate(field); _clearingAccount = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID => _origClOrdID;
		internal FixField.Text? _origClOrdID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX NoAllocs, tag 78, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoAllocs => _noAllocs;
		internal FixField.Integer? _noAllocs;

		/// <summary>The entries counted by NoAllocs, tag 78; null when the group is absent.</summary>
		public List<FixMessage.OrderCancelReplaceRequest.NoAllocsGroup>? NoAllocsGroups => noAllocsGroups;
		internal List<NoAllocsGroup>? noAllocsGroups;

		/// <summary>The FIX SettlmntTyp, tag 63, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlmntTyp => _settlmntTyp;
		internal FixField.Character? _settlmntTyp;

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate => _futSettDate;
		internal FixField.Date? _futSettDate;

		/// <summary>The FIX HandlInst, tag 21, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HandlInst => _handlInst;
		internal FixField.Character? _handlInst;

		/// <summary>The FIX ExecInst, tag 18, wire type <c>MultipleValueString</c>; null when the field is absent.</summary>
		public FixField.Multiple? ExecInst => _execInst;
		internal FixField.Multiple? _execInst;

		/// <summary>The FIX MinQty, tag 110, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MinQty => _minQty;
		internal FixField.Decimal? _minQty;

		/// <summary>The FIX MaxFloor, tag 111, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxFloor => _maxFloor;
		internal FixField.Decimal? _maxFloor;

		/// <summary>The FIX ExDestination, tag 100, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? ExDestination => _exDestination;
		internal FixField.Text? _exDestination;

		/// <summary>The FIX NoTradingSessions, tag 386, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoTradingSessions => _noTradingSessions;
		internal FixField.Integer? _noTradingSessions;

		/// <summary>The entries counted by NoTradingSessions, tag 386; null when the group is absent.</summary>
		public List<FixMessage.OrderCancelReplaceRequest.NoTradingSessionsGroup>? NoTradingSessionsGroups => noTradingSessionsGroups;
		internal List<NoTradingSessionsGroup>? noTradingSessionsGroups;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty => _orderQty;
		internal FixField.Decimal? _orderQty;

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty => _cashOrderQty;
		internal FixField.Decimal? _cashOrderQty;

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType => _ordType;
		internal FixField.Character? _ordType;

		/// <summary>The FIX Price, tag 44, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? Price => _price;
		internal FixField.Decimal? _price;

		/// <summary>The FIX StopPx, tag 99, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StopPx => _stopPx;
		internal FixField.Decimal? _stopPx;

		/// <summary>The FIX PegDifference, tag 211, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? PegDifference => _pegDifference;
		internal FixField.Decimal? _pegDifference;

		/// <summary>The FIX DiscretionInst, tag 388, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? DiscretionInst => _discretionInst;
		internal FixField.Character? _discretionInst;

		/// <summary>The FIX DiscretionOffset, tag 389, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? DiscretionOffset => _discretionOffset;
		internal FixField.Decimal? _discretionOffset;

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID => _complianceID;
		internal FixField.Text? _complianceID;

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag => _solicitedFlag;
		internal FixField.Boolean? _solicitedFlag;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX TimeInForce, tag 59, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? TimeInForce => _timeInForce;
		internal FixField.Character? _timeInForce;

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime => _effectiveTime;
		internal FixField.Timestamp? _effectiveTime;

		/// <summary>The FIX ExpireDate, tag 432, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? ExpireDate => _expireDate;
		internal FixField.Date? _expireDate;

		/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ExpireTime => _expireTime;
		internal FixField.Timestamp? _expireTime;

		/// <summary>The FIX GTBookingInst, tag 427, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? GTBookingInst => _gTBookingInst;
		internal FixField.Integer? _gTBookingInst;

		/// <summary>The FIX Commission, tag 12, wire type <c>Amt</c>; null when the field is absent.</summary>
		public FixField.Decimal? Commission => _commission;
		internal FixField.Decimal? _commission;

		/// <summary>The FIX CommType, tag 13, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CommType => _commType;
		internal FixField.Character? _commType;

		/// <summary>The FIX Rule80A, tag 47, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Rule80A => _rule80A;
		internal FixField.Character? _rule80A;

		/// <summary>The FIX ForexReq, tag 121, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? ForexReq => _forexReq;
		internal FixField.Boolean? _forexReq;

		/// <summary>The FIX SettlCurrency, tag 120, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? SettlCurrency => _settlCurrency;
		internal FixField.Text? _settlCurrency;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 => _futSettDate2;
		internal FixField.Date? _futSettDate2;

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 => _orderQty2;
		internal FixField.Decimal? _orderQty2;

		/// <summary>The FIX OpenClose, tag 77, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OpenClose => _openClose;
		internal FixField.Character? _openClose;

		/// <summary>The FIX CoveredOrUncovered, tag 203, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CoveredOrUncovered => _coveredOrUncovered;
		internal FixField.Integer? _coveredOrUncovered;

		/// <summary>The FIX CustomerOrFirm, tag 204, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? CustomerOrFirm => _customerOrFirm;
		internal FixField.Integer? _customerOrFirm;

		/// <summary>The FIX MaxShow, tag 210, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? MaxShow => _maxShow;
		internal FixField.Decimal? _maxShow;

		/// <summary>The FIX LocateReqd, tag 114, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? LocateReqd => _locateReqd;
		internal FixField.Boolean? _locateReqd;

		/// <summary>The FIX ClearingFirm, tag 439, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingFirm => _clearingFirm;
		internal FixField.Text? _clearingFirm;

		/// <summary>The FIX ClearingAccount, tag 440, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClearingAccount => _clearingAccount;
		internal FixField.Text? _clearingAccount;

		/// <summary>One entry of the group counted by NoAllocs, tag 78.</summary>
		public sealed class NoAllocsGroup : IFixEntry
		{
			internal NoAllocsGroup(FixField.Text delimiter)
			{
				AllocAccount = delimiter;
			}

			/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text AllocAccount { get; }

			/// <summary>The FIX AllocShares, tag 80, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? AllocShares => _allocShares;
			internal FixField.Decimal? _allocShares;

			FixField IFixEntry.Opener => AllocAccount;

			bool IFixEntry.Has(int tag)
			{
				return tag is 79 or 80;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		/// <summary>One entry of the group counted by NoTradingSessions, tag 386.</summary>
		public sealed class NoTradingSessionsGroup : IFixEntry
		{
			internal NoTradingSessionsGroup(FixField.Text delimiter)
			{
				TradingSessionID = delimiter;
			}

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text TradingSessionID { get; }

			FixField IFixEntry.Opener => TradingSessionID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 336;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoAllocsGroups, FixTag.NoAllocs, tag, position, out step) ??
				FixEntries.Holding(NoTradingSessionsGroups, FixTag.NoTradingSessions, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.OrderCancelReplaceRequest(context, check, this);
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
					case FixTag.OrigClOrdID: if (_origClOrdID is not null) Duplicate(field); _origClOrdID = (FixField.Text)field; break;
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.ListID: if (_listID is not null) Duplicate(field); _listID = (FixField.Text)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.OrderQty: if (_orderQty is not null) Duplicate(field); _orderQty = (FixField.Decimal)field; break;
					case FixTag.CashOrderQty: if (_cashOrderQty is not null) Duplicate(field); _cashOrderQty = (FixField.Decimal)field; break;
					case FixTag.ComplianceID: if (_complianceID is not null) Duplicate(field); _complianceID = (FixField.Text)field; break;
					case FixTag.SolicitedFlag: if (_solicitedFlag is not null) Duplicate(field); _solicitedFlag = (FixField.Boolean)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrigClOrdID, tag 41, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrigClOrdID => _origClOrdID;
		internal FixField.Text? _origClOrdID;

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX ListID, tag 66, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ListID => _listID;
		internal FixField.Text? _listID;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty => _orderQty;
		internal FixField.Decimal? _orderQty;

		/// <summary>The FIX CashOrderQty, tag 152, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? CashOrderQty => _cashOrderQty;
		internal FixField.Decimal? _cashOrderQty;

		/// <summary>The FIX ComplianceID, tag 376, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ComplianceID => _complianceID;
		internal FixField.Text? _complianceID;

		/// <summary>The FIX SolicitedFlag, tag 377, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? SolicitedFlag => _solicitedFlag;
		internal FixField.Boolean? _solicitedFlag;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.OrderCancelRequest(context, check, this);
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
					case FixTag.OrderID: if (_orderID is not null) Duplicate(field); _orderID = (FixField.Text)field; break;
					case FixTag.ClOrdID: if (_clOrdID is not null) Duplicate(field); _clOrdID = (FixField.Text)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.Account: if (_account is not null) Duplicate(field); _account = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX OrderID, tag 37, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? OrderID => _orderID;
		internal FixField.Text? _orderID;

		/// <summary>The FIX ClOrdID, tag 11, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClOrdID => _clOrdID;
		internal FixField.Text? _clOrdID;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX Account, tag 1, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Account => _account;
		internal FixField.Text? _account;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.OrderStatusRequest(context, check, this);
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
					case FixTag.QuoteReqID: if (_quoteReqID is not null) Duplicate(field); _quoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.QuoteResponseLevel: if (_quoteResponseLevel is not null) Duplicate(field); _quoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.BidPx: if (_bidPx is not null) Duplicate(field); _bidPx = (FixField.Decimal)field; break;
					case FixTag.OfferPx: if (_offerPx is not null) Duplicate(field); _offerPx = (FixField.Decimal)field; break;
					case FixTag.BidSize: if (_bidSize is not null) Duplicate(field); _bidSize = (FixField.Decimal)field; break;
					case FixTag.OfferSize: if (_offerSize is not null) Duplicate(field); _offerSize = (FixField.Decimal)field; break;
					case FixTag.ValidUntilTime: if (_validUntilTime is not null) Duplicate(field); _validUntilTime = (FixField.Timestamp)field; break;
					case FixTag.BidSpotRate: if (_bidSpotRate is not null) Duplicate(field); _bidSpotRate = (FixField.Decimal)field; break;
					case FixTag.OfferSpotRate: if (_offerSpotRate is not null) Duplicate(field); _offerSpotRate = (FixField.Decimal)field; break;
					case FixTag.BidForwardPoints: if (_bidForwardPoints is not null) Duplicate(field); _bidForwardPoints = (FixField.Decimal)field; break;
					case FixTag.OfferForwardPoints: if (_offerForwardPoints is not null) Duplicate(field); _offerForwardPoints = (FixField.Decimal)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.SettlDate: if (_futSettDate is not null) Duplicate(field); _futSettDate = (FixField.Date)field; break;
					case FixTag.OrdType: if (_ordType is not null) Duplicate(field); _ordType = (FixField.Character)field; break;
					case FixTag.SettlDate2: if (_futSettDate2 is not null) Duplicate(field); _futSettDate2 = (FixField.Date)field; break;
					case FixTag.OrderQty2: if (_orderQty2 is not null) Duplicate(field); _orderQty2 = (FixField.Decimal)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID => _quoteReqID;
		internal FixField.Text? _quoteReqID;

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel => _quoteResponseLevel;
		internal FixField.Integer? _quoteResponseLevel;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX BidPx, tag 132, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidPx => _bidPx;
		internal FixField.Decimal? _bidPx;

		/// <summary>The FIX OfferPx, tag 133, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferPx => _offerPx;
		internal FixField.Decimal? _offerPx;

		/// <summary>The FIX BidSize, tag 134, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidSize => _bidSize;
		internal FixField.Decimal? _bidSize;

		/// <summary>The FIX OfferSize, tag 135, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferSize => _offerSize;
		internal FixField.Decimal? _offerSize;

		/// <summary>The FIX ValidUntilTime, tag 62, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? ValidUntilTime => _validUntilTime;
		internal FixField.Timestamp? _validUntilTime;

		/// <summary>The FIX BidSpotRate, tag 188, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidSpotRate => _bidSpotRate;
		internal FixField.Decimal? _bidSpotRate;

		/// <summary>The FIX OfferSpotRate, tag 190, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferSpotRate => _offerSpotRate;
		internal FixField.Decimal? _offerSpotRate;

		/// <summary>The FIX BidForwardPoints, tag 189, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? BidForwardPoints => _bidForwardPoints;
		internal FixField.Decimal? _bidForwardPoints;

		/// <summary>The FIX OfferForwardPoints, tag 191, wire type <c>PriceOffset</c>; null when the field is absent.</summary>
		public FixField.Decimal? OfferForwardPoints => _offerForwardPoints;
		internal FixField.Decimal? _offerForwardPoints;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate => _futSettDate;
		internal FixField.Date? _futSettDate;

		/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OrdType => _ordType;
		internal FixField.Character? _ordType;

		/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? FutSettDate2 => _futSettDate2;
		internal FixField.Date? _futSettDate2;

		/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? OrderQty2 => _orderQty2;
		internal FixField.Decimal? _orderQty2;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Quote(context, check, this);
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
					case FixTag.QuoteReqID: if (_quoteReqID is not null) Duplicate(field); _quoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.QuoteStatus: if (_quoteAckStatus is not null) Duplicate(field); _quoteAckStatus = (FixField.Integer)field; break;
					case FixTag.QuoteRejectReason: if (_quoteRejectReason is not null) Duplicate(field); _quoteRejectReason = (FixField.Integer)field; break;
					case FixTag.QuoteResponseLevel: if (_quoteResponseLevel is not null) Duplicate(field); _quoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.NoQuoteSets: if (_noQuoteSets is not null) Duplicate(field); _noQuoteSets = (FixField.Integer)field; break;
					case FixTag.QuoteSetID:
						if (noQuoteSetsGroups is null && _noQuoteSets is null) Uncounted(FixTag.NoQuoteSets, field);
						(noQuoteSetsGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.UnderlyingSymbol:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSymbol is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSymbol = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSymbolSfx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityID is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingIDSource is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityType is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingMaturityMonthYear is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingMaturityDay is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingPutOrCall is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingStrikePrice is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingOptAttribute is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingContractMultiplier is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingCouponRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityExchange is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingIssuerLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._underlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._underlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDescLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._encodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.TotNoQuoteEntries:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._totQuoteEntries is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._totQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.NoQuoteEntries:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (noQuoteSetsGroups![^1]._noQuoteEntries is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1]._noQuoteEntries = (FixField.Integer)field;
						break;
					case FixTag.QuoteEntryID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else
						{
							if (noQuoteSetsGroups![^1].noQuoteEntriesGroups is null && noQuoteSetsGroups![^1]._noQuoteEntries is null) Uncounted(FixTag.NoQuoteEntries, field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
							(noQuoteSetsGroups![^1].noQuoteEntriesGroups ??= []).Add(new ((FixField.Text)field));
						}
						break;
					case FixTag.Symbol:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbol is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbol = (FixField.Text)field;
						break;
					case FixTag.SymbolSfx:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.QuoteEntryRejectReason:
						if (None(noQuoteSetsGroups))
							Unentered(field);
						else if (None(noQuoteSetsGroups![^1].noQuoteEntriesGroups))
							Unentered(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1);
						else if (noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._quoteEntryRejectReason is not null)
							Duplicate(field, FixTag.NoQuoteSets, noQuoteSetsGroups!.Count - 1, FixTag.NoQuoteEntries, noQuoteSetsGroups![^1].noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteSetsGroups![^1].noQuoteEntriesGroups![^1]._quoteEntryRejectReason = (FixField.Integer)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID => _quoteReqID;
		internal FixField.Text? _quoteReqID;

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX QuoteAckStatus, tag 297, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteAckStatus => _quoteAckStatus;
		internal FixField.Integer? _quoteAckStatus;

		/// <summary>The FIX QuoteRejectReason, tag 300, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteRejectReason => _quoteRejectReason;
		internal FixField.Integer? _quoteRejectReason;

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel => _quoteResponseLevel;
		internal FixField.Integer? _quoteResponseLevel;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX NoQuoteSets, tag 296, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteSets => _noQuoteSets;
		internal FixField.Integer? _noQuoteSets;

		/// <summary>The entries counted by NoQuoteSets, tag 296; null when the group is absent.</summary>
		public List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup>? NoQuoteSetsGroups => noQuoteSetsGroups;
		internal List<NoQuoteSetsGroup>? noQuoteSetsGroups;

		/// <summary>One entry of the group counted by NoQuoteSets, tag 296.</summary>
		public sealed class NoQuoteSetsGroup : IFixEntry
		{
			internal NoQuoteSetsGroup(FixField.Text delimiter)
			{
				QuoteSetID = delimiter;
			}

			/// <summary>The FIX QuoteSetID, tag 302, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text QuoteSetID { get; }

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol => _underlyingSymbol;
			internal FixField.Text? _underlyingSymbol;

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx => _underlyingSymbolSfx;
			internal FixField.Text? _underlyingSymbolSfx;

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID => _underlyingSecurityID;
			internal FixField.Text? _underlyingSecurityID;

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource => _underlyingIDSource;
			internal FixField.Text? _underlyingIDSource;

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType => _underlyingSecurityType;
			internal FixField.Text? _underlyingSecurityType;

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear => _underlyingMaturityMonthYear;
			internal FixField.MonthYear? _underlyingMaturityMonthYear;

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay => _underlyingMaturityDay;
			internal FixField.Integer? _underlyingMaturityDay;

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall => _underlyingPutOrCall;
			internal FixField.Integer? _underlyingPutOrCall;

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice => _underlyingStrikePrice;
			internal FixField.Decimal? _underlyingStrikePrice;

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute => _underlyingOptAttribute;
			internal FixField.Character? _underlyingOptAttribute;

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier => _underlyingContractMultiplier;
			internal FixField.Decimal? _underlyingContractMultiplier;

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate => _underlyingCouponRate;
			internal FixField.Decimal? _underlyingCouponRate;

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange => _underlyingSecurityExchange;
			internal FixField.Text? _underlyingSecurityExchange;

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer => _underlyingIssuer;
			internal FixField.Text? _underlyingIssuer;

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen => _encodedUnderlyingIssuerLen;
			internal FixField.Integer? _encodedUnderlyingIssuerLen;

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer => _encodedUnderlyingIssuer;
			internal FixField.Data? _encodedUnderlyingIssuer;

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc => _underlyingSecurityDesc;
			internal FixField.Text? _underlyingSecurityDesc;

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen => _encodedUnderlyingSecurityDescLen;
			internal FixField.Integer? _encodedUnderlyingSecurityDescLen;

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc => _encodedUnderlyingSecurityDesc;
			internal FixField.Data? _encodedUnderlyingSecurityDesc;

			/// <summary>The FIX TotQuoteEntries, tag 304, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? TotQuoteEntries => _totQuoteEntries;
			internal FixField.Integer? _totQuoteEntries;

			/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? NoQuoteEntries => _noQuoteEntries;
			internal FixField.Integer? _noQuoteEntries;

			/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
			public List<FixMessage.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => noQuoteEntriesGroups;
			internal List<NoQuoteEntriesGroup>? noQuoteEntriesGroups;

			/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
			public sealed class NoQuoteEntriesGroup : IFixEntry
			{
				internal NoQuoteEntriesGroup(FixField.Text delimiter)
				{
					QuoteEntryID = delimiter;
				}

				/// <summary>The FIX QuoteEntryID, tag 299, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
				public FixField.Text QuoteEntryID { get; }

				/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Symbol => _symbol;
				internal FixField.Text? _symbol;

				/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SymbolSfx => _symbolSfx;
				internal FixField.Text? _symbolSfx;

				/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityID => _securityID;
				internal FixField.Text? _securityID;

				/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? IDSource => _iDSource;
				internal FixField.Text? _iDSource;

				/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityType => _securityType;
				internal FixField.Text? _securityType;

				/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
				public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
				internal FixField.MonthYear? _maturityMonthYear;

				/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
				public FixField.Integer? MaturityDay => _maturityDay;
				internal FixField.Integer? _maturityDay;

				/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? PutOrCall => _putOrCall;
				internal FixField.Integer? _putOrCall;

				/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
				public FixField.Decimal? StrikePrice => _strikePrice;
				internal FixField.Decimal? _strikePrice;

				/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
				public FixField.Character? OptAttribute => _optAttribute;
				internal FixField.Character? _optAttribute;

				/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? ContractMultiplier => _contractMultiplier;
				internal FixField.Decimal? _contractMultiplier;

				/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
				public FixField.Decimal? CouponRate => _couponRate;
				internal FixField.Decimal? _couponRate;

				/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityExchange => _securityExchange;
				internal FixField.Text? _securityExchange;

				/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? Issuer => _issuer;
				internal FixField.Text? _issuer;

				/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
				internal FixField.Integer? _encodedIssuerLen;

				/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedIssuer => _encodedIssuer;
				internal FixField.Data? _encodedIssuer;

				/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
				public FixField.Text? SecurityDesc => _securityDesc;
				internal FixField.Text? _securityDesc;

				/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
				public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
				internal FixField.Integer? _encodedSecurityDescLen;

				/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
				public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
				internal FixField.Data? _encodedSecurityDesc;

				/// <summary>The FIX QuoteEntryRejectReason, tag 368, wire type <c>int</c>; null when the field is absent.</summary>
				public FixField.Integer? QuoteEntryRejectReason => _quoteEntryRejectReason;
				internal FixField.Integer? _quoteEntryRejectReason;

				FixField IFixEntry.Opener => QuoteEntryID;

				bool IFixEntry.Has(int tag)
				{
					return tag is 22 or 48 or 55 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or
						231 or 299 or 348 or 349 or 350 or 351 or 368;
				}

				IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
				{
					step = default;

					return null;
				}
			}

			FixField IFixEntry.Opener => QuoteSetID;

			bool IFixEntry.Has(int tag)
			{
				return tag is 22 or 48 or 55 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
					295 or 299 or 302 or 304 or 305 or 306 or 307 or 308 or 309 or 310 or 311 or 312 or 313 or 314 or 315 or 316 or
					317 or 348 or 349 or 350 or 351 or 362 or 363 or 364 or 365 or 368 or 435 or 436;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				return FixEntries.Holding(NoQuoteEntriesGroups, FixTag.NoQuoteEntries, tag, position, out step);
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoQuoteSetsGroups, FixTag.NoQuoteSets, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.QuoteAcknowledgement(context, check, this);
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
					case FixTag.QuoteReqID: if (_quoteReqID is not null) Duplicate(field); _quoteReqID = (FixField.Text)field; break;
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.QuoteCancelType: if (_quoteCancelType is not null) Duplicate(field); _quoteCancelType = (FixField.Integer)field; break;
					case FixTag.QuoteResponseLevel: if (_quoteResponseLevel is not null) Duplicate(field); _quoteResponseLevel = (FixField.Integer)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.NoQuoteEntries: if (_noQuoteEntries is not null) Duplicate(field); _noQuoteEntries = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (noQuoteEntriesGroups is null && _noQuoteEntries is null) Uncounted(FixTag.NoQuoteEntries, field);
						(noQuoteEntriesGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSymbol:
						if (None(noQuoteEntriesGroups))
							Unentered(field);
						else if (noQuoteEntriesGroups![^1]._underlyingSymbol is not null)
							Duplicate(field, FixTag.NoQuoteEntries, noQuoteEntriesGroups!.Count - 1);
						else
							noQuoteEntriesGroups![^1]._underlyingSymbol = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID => _quoteReqID;
		internal FixField.Text? _quoteReqID;

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX QuoteCancelType, tag 298, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteCancelType => _quoteCancelType;
		internal FixField.Integer? _quoteCancelType;

		/// <summary>The FIX QuoteResponseLevel, tag 301, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? QuoteResponseLevel => _quoteResponseLevel;
		internal FixField.Integer? _quoteResponseLevel;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX NoQuoteEntries, tag 295, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoQuoteEntries => _noQuoteEntries;
		internal FixField.Integer? _noQuoteEntries;

		/// <summary>The entries counted by NoQuoteEntries, tag 295; null when the group is absent.</summary>
		public List<FixMessage.QuoteCancel.NoQuoteEntriesGroup>? NoQuoteEntriesGroups => noQuoteEntriesGroups;
		internal List<NoQuoteEntriesGroup>? noQuoteEntriesGroups;

		/// <summary>One entry of the group counted by NoQuoteEntries, tag 295.</summary>
		public sealed class NoQuoteEntriesGroup : IFixEntry
		{
			internal NoQuoteEntriesGroup(FixField.Text delimiter)
			{
				Symbol = delimiter;
			}

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Symbol { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbol => _underlyingSymbol;
			internal FixField.Text? _underlyingSymbol;

			FixField IFixEntry.Opener => Symbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 22 or 48 or 55 or 65 or 106 or 107 or 167 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or
					311 or 348 or 349 or 350 or 351;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoQuoteEntriesGroups, FixTag.NoQuoteEntries, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.QuoteCancel(context, check, this);
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
					case FixTag.QuoteReqID: if (_quoteReqID is not null) Duplicate(field); _quoteReqID = (FixField.Text)field; break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.Symbol:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.SymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._symbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._symbolSfx = (FixField.Text)field;
						break;
					case FixTag.SecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityID = (FixField.Text)field;
						break;
					case FixTag.SecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._iDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._iDSource = (FixField.Text)field;
						break;
					case FixTag.SecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityType = (FixField.Text)field;
						break;
					case FixTag.MaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.MaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._maturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._maturityDay = (FixField.Integer)field;
						break;
					case FixTag.PutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._putOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._putOrCall = (FixField.Integer)field;
						break;
					case FixTag.StrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._strikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._strikePrice = (FixField.Decimal)field;
						break;
					case FixTag.OptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._optAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._optAttribute = (FixField.Character)field;
						break;
					case FixTag.ContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._contractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._contractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.CouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._couponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._couponRate = (FixField.Decimal)field;
						break;
					case FixTag.SecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityExchange = (FixField.Text)field;
						break;
					case FixTag.Issuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._issuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._issuer = (FixField.Text)field;
						break;
					case FixTag.EncodedIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedIssuer = (FixField.Data)field;
						break;
					case FixTag.SecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._securityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._securityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.PrevClosePx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._prevClosePx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._prevClosePx = (FixField.Decimal)field;
						break;
					case FixTag.QuoteRequestType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._quoteRequestType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._quoteRequestType = (FixField.Integer)field;
						break;
					case FixTag.TradingSessionID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._tradingSessionID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._tradingSessionID = (FixField.Text)field;
						break;
					case FixTag.Side:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.OrderQty:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._orderQty is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._orderQty = (FixField.Decimal)field;
						break;
					case FixTag.SettlDate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._futSettDate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._futSettDate = (FixField.Date)field;
						break;
					case FixTag.OrdType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._ordType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._ordType = (FixField.Character)field;
						break;
					case FixTag.SettlDate2:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._futSettDate2 is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._futSettDate2 = (FixField.Date)field;
						break;
					case FixTag.OrderQty2:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._orderQty2 is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._orderQty2 = (FixField.Decimal)field;
						break;
					case FixTag.ExpireTime:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._expireTime is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._expireTime = (FixField.Timestamp)field;
						break;
					case FixTag.TransactTime:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._transactTime is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._transactTime = (FixField.Timestamp)field;
						break;
					case FixTag.Currency:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._currency is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._currency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteReqID, tag 131, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteReqID => _quoteReqID;
		internal FixField.Text? _quoteReqID;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.QuoteRequest.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				Symbol = delimiter;
			}

			/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text Symbol { get; }

			/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SymbolSfx => _symbolSfx;
			internal FixField.Text? _symbolSfx;

			/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityID => _securityID;
			internal FixField.Text? _securityID;

			/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? IDSource => _iDSource;
			internal FixField.Text? _iDSource;

			/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityType => _securityType;
			internal FixField.Text? _securityType;

			/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
			internal FixField.MonthYear? _maturityMonthYear;

			/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? MaturityDay => _maturityDay;
			internal FixField.Integer? _maturityDay;

			/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? PutOrCall => _putOrCall;
			internal FixField.Integer? _putOrCall;

			/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? StrikePrice => _strikePrice;
			internal FixField.Decimal? _strikePrice;

			/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OptAttribute => _optAttribute;
			internal FixField.Character? _optAttribute;

			/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? ContractMultiplier => _contractMultiplier;
			internal FixField.Decimal? _contractMultiplier;

			/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? CouponRate => _couponRate;
			internal FixField.Decimal? _couponRate;

			/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityExchange => _securityExchange;
			internal FixField.Text? _securityExchange;

			/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? Issuer => _issuer;
			internal FixField.Text? _issuer;

			/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
			internal FixField.Integer? _encodedIssuerLen;

			/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedIssuer => _encodedIssuer;
			internal FixField.Data? _encodedIssuer;

			/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? SecurityDesc => _securityDesc;
			internal FixField.Text? _securityDesc;

			/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
			internal FixField.Integer? _encodedSecurityDescLen;

			/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
			internal FixField.Data? _encodedSecurityDesc;

			/// <summary>The FIX PrevClosePx, tag 140, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? PrevClosePx => _prevClosePx;
			internal FixField.Decimal? _prevClosePx;

			/// <summary>The FIX QuoteRequestType, tag 303, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? QuoteRequestType => _quoteRequestType;
			internal FixField.Integer? _quoteRequestType;

			/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? TradingSessionID => _tradingSessionID;
			internal FixField.Text? _tradingSessionID;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX OrderQty, tag 38, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty => _orderQty;
			internal FixField.Decimal? _orderQty;

			/// <summary>The FIX FutSettDate, tag 64, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate => _futSettDate;
			internal FixField.Date? _futSettDate;

			/// <summary>The FIX OrdType, tag 40, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? OrdType => _ordType;
			internal FixField.Character? _ordType;

			/// <summary>The FIX FutSettDate2, tag 193, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
			public FixField.Date? FutSettDate2 => _futSettDate2;
			internal FixField.Date? _futSettDate2;

			/// <summary>The FIX OrderQty2, tag 192, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? OrderQty2 => _orderQty2;
			internal FixField.Decimal? _orderQty2;

			/// <summary>The FIX ExpireTime, tag 126, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? ExpireTime => _expireTime;
			internal FixField.Timestamp? _expireTime;

			/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
			public FixField.Timestamp? TransactTime => _transactTime;
			internal FixField.Timestamp? _transactTime;

			/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? Currency => _currency;
			internal FixField.Text? _currency;

			FixField IFixEntry.Opener => Symbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 15 or 22 or 38 or 40 or 48 or 54 or 55 or 60 or 64 or 65 or 106 or 107 or 126 or 140 or 167 or 192 or
					193 or 200 or 201 or 202 or 205 or 206 or 207 or 223 or 231 or 303 or 336 or 348 or 349 or 350 or 351;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.QuoteRequest(context, check, this);
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
					case FixTag.QuoteID: if (_quoteID is not null) Duplicate(field); _quoteID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX QuoteID, tag 117, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? QuoteID => _quoteID;
		internal FixField.Text? _quoteID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.QuoteStatusRequest(context, check, this);
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
					case FixTag.RefSeqNum: if (_refSeqNum is not null) Duplicate(field); _refSeqNum = (FixField.Integer)field; break;
					case FixTag.RefTagID: if (_refTagID is not null) Duplicate(field); _refTagID = (FixField.Integer)field; break;
					case FixTag.RefMsgType: if (_refMsgType is not null) Duplicate(field); _refMsgType = (FixField.Text)field; break;
					case FixTag.SessionRejectReason: if (_sessionRejectReason is not null) Duplicate(field); _sessionRejectReason = (FixField.Integer)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX RefSeqNum, tag 45, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefSeqNum => _refSeqNum;
		internal FixField.Integer? _refSeqNum;

		/// <summary>The FIX RefTagID, tag 371, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? RefTagID => _refTagID;
		internal FixField.Integer? _refTagID;

		/// <summary>The FIX RefMsgType, tag 372, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? RefMsgType => _refMsgType;
		internal FixField.Text? _refMsgType;

		/// <summary>The FIX SessionRejectReason, tag 373, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SessionRejectReason => _sessionRejectReason;
		internal FixField.Integer? _sessionRejectReason;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.Reject(context, check, this);
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
					case FixTag.BeginSeqNo: if (_beginSeqNo is not null) Duplicate(field); _beginSeqNo = (FixField.Integer)field; break;
					case FixTag.EndSeqNo: if (_endSeqNo is not null) Duplicate(field); _endSeqNo = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX BeginSeqNo, tag 7, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? BeginSeqNo => _beginSeqNo;
		internal FixField.Integer? _beginSeqNo;

		/// <summary>The FIX EndSeqNo, tag 16, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? EndSeqNo => _endSeqNo;
		internal FixField.Integer? _endSeqNo;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.ResendRequest(context, check, this);
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
					case FixTag.SecurityReqID: if (_securityReqID is not null) Duplicate(field); _securityReqID = (FixField.Text)field; break;
					case FixTag.SecurityResponseID: if (_securityResponseID is not null) Duplicate(field); _securityResponseID = (FixField.Text)field; break;
					case FixTag.SecurityResponseType: if (_securityResponseType is not null) Duplicate(field); _securityResponseType = (FixField.Integer)field; break;
					case FixTag.TotNoRelatedSym: if (_totalNumSecurities is not null) Duplicate(field); _totalNumSecurities = (FixField.Integer)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.UnderlyingSymbol:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSymbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingIDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingMaturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingMaturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingPutOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingStrikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingOptAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingContractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingCouponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._ratioQty is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._ratioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingCurrency is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingCurrency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX SecurityReqID, tag 320, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityReqID => _securityReqID;
		internal FixField.Text? _securityReqID;

		/// <summary>The FIX SecurityResponseID, tag 322, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityResponseID => _securityResponseID;
		internal FixField.Text? _securityResponseID;

		/// <summary>The FIX SecurityResponseType, tag 323, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityResponseType => _securityResponseType;
		internal FixField.Integer? _securityResponseType;

		/// <summary>The FIX TotalNumSecurities, tag 393, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TotalNumSecurities => _totalNumSecurities;
		internal FixField.Integer? _totalNumSecurities;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.SecurityDefinition.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				UnderlyingSymbol = delimiter;
			}

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text UnderlyingSymbol { get; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx => _underlyingSymbolSfx;
			internal FixField.Text? _underlyingSymbolSfx;

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID => _underlyingSecurityID;
			internal FixField.Text? _underlyingSecurityID;

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource => _underlyingIDSource;
			internal FixField.Text? _underlyingIDSource;

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType => _underlyingSecurityType;
			internal FixField.Text? _underlyingSecurityType;

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear => _underlyingMaturityMonthYear;
			internal FixField.MonthYear? _underlyingMaturityMonthYear;

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay => _underlyingMaturityDay;
			internal FixField.Integer? _underlyingMaturityDay;

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall => _underlyingPutOrCall;
			internal FixField.Integer? _underlyingPutOrCall;

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice => _underlyingStrikePrice;
			internal FixField.Decimal? _underlyingStrikePrice;

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute => _underlyingOptAttribute;
			internal FixField.Character? _underlyingOptAttribute;

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier => _underlyingContractMultiplier;
			internal FixField.Decimal? _underlyingContractMultiplier;

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate => _underlyingCouponRate;
			internal FixField.Decimal? _underlyingCouponRate;

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange => _underlyingSecurityExchange;
			internal FixField.Text? _underlyingSecurityExchange;

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer => _underlyingIssuer;
			internal FixField.Text? _underlyingIssuer;

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen => _encodedUnderlyingIssuerLen;
			internal FixField.Integer? _encodedUnderlyingIssuerLen;

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer => _encodedUnderlyingIssuer;
			internal FixField.Data? _encodedUnderlyingIssuer;

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc => _underlyingSecurityDesc;
			internal FixField.Text? _underlyingSecurityDesc;

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen => _encodedUnderlyingSecurityDescLen;
			internal FixField.Integer? _encodedUnderlyingSecurityDescLen;

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc => _encodedUnderlyingSecurityDesc;
			internal FixField.Data? _encodedUnderlyingSecurityDesc;

			/// <summary>The FIX RatioQty, tag 319, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? RatioQty => _ratioQty;
			internal FixField.Decimal? _ratioQty;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingCurrency => _underlyingCurrency;
			internal FixField.Text? _underlyingCurrency;

			FixField IFixEntry.Opener => UnderlyingSymbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 54 or 305 or 306 or 307 or 308 or 309 or 310 or 311 or 312 or 313 or 314 or 315 or 316 or 317 or
					318 or 319 or 362 or 363 or 364 or 365 or 435 or 436;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SecurityDefinition(context, check, this);
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
					case FixTag.SecurityReqID: if (_securityReqID is not null) Duplicate(field); _securityReqID = (FixField.Text)field; break;
					case FixTag.SecurityRequestType: if (_securityRequestType is not null) Duplicate(field); _securityRequestType = (FixField.Integer)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.NoRelatedSym: if (_noRelatedSym is not null) Duplicate(field); _noRelatedSym = (FixField.Integer)field; break;
					case FixTag.UnderlyingSymbol:
						if (noRelatedSymGroups is null && _noRelatedSym is null) Uncounted(FixTag.NoRelatedSym, field);
						(noRelatedSymGroups ??= []).Add(new ((FixField.Text)field));
						break;
					case FixTag.UnderlyingSymbolSfx:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSymbolSfx is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSymbolSfx = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityID:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityID is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityID = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityIDSource:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingIDSource is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingIDSource = (FixField.Text)field;
						break;
					case FixTag.UnderlyingSecurityType:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityType is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityType = (FixField.Text)field;
						break;
					case FixTag.UnderlyingMaturityMonthYear:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingMaturityMonthYear is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingMaturityMonthYear = (FixField.MonthYear)field;
						break;
					case FixTag.UnderlyingMaturityDay:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingMaturityDay is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingMaturityDay = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingPutOrCall:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingPutOrCall is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingPutOrCall = (FixField.Integer)field;
						break;
					case FixTag.UnderlyingStrikePrice:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingStrikePrice is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingStrikePrice = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingOptAttribute:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingOptAttribute is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingOptAttribute = (FixField.Character)field;
						break;
					case FixTag.UnderlyingContractMultiplier:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingContractMultiplier is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingContractMultiplier = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingCouponRate:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingCouponRate is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingCouponRate = (FixField.Decimal)field;
						break;
					case FixTag.UnderlyingSecurityExchange:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityExchange is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityExchange = (FixField.Text)field;
						break;
					case FixTag.UnderlyingIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingIssuer = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingIssuerLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingIssuerLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingIssuerLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingIssuer:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingIssuer is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingIssuer = (FixField.Data)field;
						break;
					case FixTag.UnderlyingSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingSecurityDesc = (FixField.Text)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDescLen:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingSecurityDescLen is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingSecurityDescLen = (FixField.Integer)field;
						break;
					case FixTag.EncodedUnderlyingSecurityDesc:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._encodedUnderlyingSecurityDesc is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._encodedUnderlyingSecurityDesc = (FixField.Data)field;
						break;
					case FixTag.RatioQty:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._ratioQty is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._ratioQty = (FixField.Decimal)field;
						break;
					case FixTag.Side:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._side is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._side = (FixField.Character)field;
						break;
					case FixTag.UnderlyingCurrency:
						if (None(noRelatedSymGroups))
							Unentered(field);
						else if (noRelatedSymGroups![^1]._underlyingCurrency is not null)
							Duplicate(field, FixTag.NoRelatedSym, noRelatedSymGroups!.Count - 1);
						else
							noRelatedSymGroups![^1]._underlyingCurrency = (FixField.Text)field;
						break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX SecurityReqID, tag 320, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityReqID => _securityReqID;
		internal FixField.Text? _securityReqID;

		/// <summary>The FIX SecurityRequestType, tag 321, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityRequestType => _securityRequestType;
		internal FixField.Integer? _securityRequestType;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX NoRelatedSym, tag 146, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NoRelatedSym => _noRelatedSym;
		internal FixField.Integer? _noRelatedSym;

		/// <summary>The entries counted by NoRelatedSym, tag 146; null when the group is absent.</summary>
		public List<FixMessage.SecurityDefinitionRequest.NoRelatedSymGroup>? NoRelatedSymGroups => noRelatedSymGroups;
		internal List<NoRelatedSymGroup>? noRelatedSymGroups;

		/// <summary>One entry of the group counted by NoRelatedSym, tag 146.</summary>
		public sealed class NoRelatedSymGroup : IFixEntry
		{
			internal NoRelatedSymGroup(FixField.Text delimiter)
			{
				UnderlyingSymbol = delimiter;
			}

			/// <summary>The FIX UnderlyingSymbol, tag 311, wire type <c>String</c>: the entry's delimiter, its first field, which every entry has.</summary>
			public FixField.Text UnderlyingSymbol { get; }

			/// <summary>The FIX UnderlyingSymbolSfx, tag 312, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSymbolSfx => _underlyingSymbolSfx;
			internal FixField.Text? _underlyingSymbolSfx;

			/// <summary>The FIX UnderlyingSecurityID, tag 309, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityID => _underlyingSecurityID;
			internal FixField.Text? _underlyingSecurityID;

			/// <summary>The FIX UnderlyingIDSource, tag 305, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIDSource => _underlyingIDSource;
			internal FixField.Text? _underlyingIDSource;

			/// <summary>The FIX UnderlyingSecurityType, tag 310, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityType => _underlyingSecurityType;
			internal FixField.Text? _underlyingSecurityType;

			/// <summary>The FIX UnderlyingMaturityMonthYear, tag 313, wire type <c>MonthYear</c>; null when the field is absent.</summary>
			public FixField.MonthYear? UnderlyingMaturityMonthYear => _underlyingMaturityMonthYear;
			internal FixField.MonthYear? _underlyingMaturityMonthYear;

			/// <summary>The FIX UnderlyingMaturityDay, tag 314, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingMaturityDay => _underlyingMaturityDay;
			internal FixField.Integer? _underlyingMaturityDay;

			/// <summary>The FIX UnderlyingPutOrCall, tag 315, wire type <c>int</c>; null when the field is absent.</summary>
			public FixField.Integer? UnderlyingPutOrCall => _underlyingPutOrCall;
			internal FixField.Integer? _underlyingPutOrCall;

			/// <summary>The FIX UnderlyingStrikePrice, tag 316, wire type <c>Price</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingStrikePrice => _underlyingStrikePrice;
			internal FixField.Decimal? _underlyingStrikePrice;

			/// <summary>The FIX UnderlyingOptAttribute, tag 317, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? UnderlyingOptAttribute => _underlyingOptAttribute;
			internal FixField.Character? _underlyingOptAttribute;

			/// <summary>The FIX UnderlyingContractMultiplier, tag 436, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingContractMultiplier => _underlyingContractMultiplier;
			internal FixField.Decimal? _underlyingContractMultiplier;

			/// <summary>The FIX UnderlyingCouponRate, tag 435, wire type <c>float</c>; null when the field is absent.</summary>
			public FixField.Decimal? UnderlyingCouponRate => _underlyingCouponRate;
			internal FixField.Decimal? _underlyingCouponRate;

			/// <summary>The FIX UnderlyingSecurityExchange, tag 308, wire type <c>Exchange</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityExchange => _underlyingSecurityExchange;
			internal FixField.Text? _underlyingSecurityExchange;

			/// <summary>The FIX UnderlyingIssuer, tag 306, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingIssuer => _underlyingIssuer;
			internal FixField.Text? _underlyingIssuer;

			/// <summary>The FIX EncodedUnderlyingIssuerLen, tag 362, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingIssuerLen => _encodedUnderlyingIssuerLen;
			internal FixField.Integer? _encodedUnderlyingIssuerLen;

			/// <summary>The FIX EncodedUnderlyingIssuer, tag 363, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingIssuer => _encodedUnderlyingIssuer;
			internal FixField.Data? _encodedUnderlyingIssuer;

			/// <summary>The FIX UnderlyingSecurityDesc, tag 307, wire type <c>String</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingSecurityDesc => _underlyingSecurityDesc;
			internal FixField.Text? _underlyingSecurityDesc;

			/// <summary>The FIX EncodedUnderlyingSecurityDescLen, tag 364, wire type <c>Length</c>; null when the field is absent.</summary>
			public FixField.Integer? EncodedUnderlyingSecurityDescLen => _encodedUnderlyingSecurityDescLen;
			internal FixField.Integer? _encodedUnderlyingSecurityDescLen;

			/// <summary>The FIX EncodedUnderlyingSecurityDesc, tag 365, wire type <c>data</c>; null when the field is absent.</summary>
			public FixField.Data? EncodedUnderlyingSecurityDesc => _encodedUnderlyingSecurityDesc;
			internal FixField.Data? _encodedUnderlyingSecurityDesc;

			/// <summary>The FIX RatioQty, tag 319, wire type <c>Qty</c>; null when the field is absent.</summary>
			public FixField.Decimal? RatioQty => _ratioQty;
			internal FixField.Decimal? _ratioQty;

			/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
			public FixField.Character? Side => _side;
			internal FixField.Character? _side;

			/// <summary>The FIX UnderlyingCurrency, tag 318, wire type <c>Currency</c>; null when the field is absent.</summary>
			public FixField.Text? UnderlyingCurrency => _underlyingCurrency;
			internal FixField.Text? _underlyingCurrency;

			FixField IFixEntry.Opener => UnderlyingSymbol;

			bool IFixEntry.Has(int tag)
			{
				return tag is 54 or 305 or 306 or 307 or 308 or 309 or 310 or 311 or 312 or 313 or 314 or 315 or 316 or 317 or
					318 or 319 or 362 or 363 or 364 or 365 or 435 or 436;
			}

			IFixEntry? IFixCarrier.Holder(int tag, int position, out FixGroupStep step)
			{
				step = default;

				return null;
			}
		}

		// The entry of this type's groups the reader put a field of the tag and position in.
		private protected override IFixEntry? Holder(int tag, int position, out FixGroupStep step)
		{
			return FixEntries.Holding(NoRelatedSymGroups, FixTag.NoRelatedSym, tag, position, out step);
		}

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SecurityDefinitionRequest(context, check, this);
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
					case FixTag.SecurityStatusReqID: if (_securityStatusReqID is not null) Duplicate(field); _securityStatusReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.UnsolicitedIndicator: if (_unsolicitedIndicator is not null) Duplicate(field); _unsolicitedIndicator = (FixField.Boolean)field; break;
					case FixTag.SecurityTradingStatus: if (_securityTradingStatus is not null) Duplicate(field); _securityTradingStatus = (FixField.Integer)field; break;
					case FixTag.FinancialStatus: if (_financialStatus is not null) Duplicate(field); _financialStatus = (FixField.Character)field; break;
					case FixTag.CorporateAction: if (_corporateAction is not null) Duplicate(field); _corporateAction = (FixField.Character)field; break;
					case FixTag.HaltReason: if (_haltReason is not null) Duplicate(field); _haltReason = (FixField.Character)field; break;
					case FixTag.InViewOfCommon: if (_inViewOfCommon is not null) Duplicate(field); _inViewOfCommon = (FixField.Boolean)field; break;
					case FixTag.DueToRelated: if (_dueToRelated is not null) Duplicate(field); _dueToRelated = (FixField.Boolean)field; break;
					case FixTag.BuyVolume: if (_buyVolume is not null) Duplicate(field); _buyVolume = (FixField.Decimal)field; break;
					case FixTag.SellVolume: if (_sellVolume is not null) Duplicate(field); _sellVolume = (FixField.Decimal)field; break;
					case FixTag.HighPx: if (_highPx is not null) Duplicate(field); _highPx = (FixField.Decimal)field; break;
					case FixTag.LowPx: if (_lowPx is not null) Duplicate(field); _lowPx = (FixField.Decimal)field; break;
					case FixTag.LastPx: if (_lastPx is not null) Duplicate(field); _lastPx = (FixField.Decimal)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.Adjustment: if (_adjustment is not null) Duplicate(field); _adjustment = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX SecurityStatusReqID, tag 324, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityStatusReqID => _securityStatusReqID;
		internal FixField.Text? _securityStatusReqID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX UnsolicitedIndicator, tag 325, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? UnsolicitedIndicator => _unsolicitedIndicator;
		internal FixField.Boolean? _unsolicitedIndicator;

		/// <summary>The FIX SecurityTradingStatus, tag 326, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SecurityTradingStatus => _securityTradingStatus;
		internal FixField.Integer? _securityTradingStatus;

		/// <summary>The FIX FinancialStatus, tag 291, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? FinancialStatus => _financialStatus;
		internal FixField.Character? _financialStatus;

		/// <summary>The FIX CorporateAction, tag 292, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? CorporateAction => _corporateAction;
		internal FixField.Character? _corporateAction;

		/// <summary>The FIX HaltReason, tag 327, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? HaltReason => _haltReason;
		internal FixField.Character? _haltReason;

		/// <summary>The FIX InViewOfCommon, tag 328, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? InViewOfCommon => _inViewOfCommon;
		internal FixField.Boolean? _inViewOfCommon;

		/// <summary>The FIX DueToRelated, tag 329, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? DueToRelated => _dueToRelated;
		internal FixField.Boolean? _dueToRelated;

		/// <summary>The FIX BuyVolume, tag 330, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? BuyVolume => _buyVolume;
		internal FixField.Decimal? _buyVolume;

		/// <summary>The FIX SellVolume, tag 331, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? SellVolume => _sellVolume;
		internal FixField.Decimal? _sellVolume;

		/// <summary>The FIX HighPx, tag 332, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? HighPx => _highPx;
		internal FixField.Decimal? _highPx;

		/// <summary>The FIX LowPx, tag 333, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LowPx => _lowPx;
		internal FixField.Decimal? _lowPx;

		/// <summary>The FIX LastPx, tag 31, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? LastPx => _lastPx;
		internal FixField.Decimal? _lastPx;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX Adjustment, tag 334, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? Adjustment => _adjustment;
		internal FixField.Integer? _adjustment;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SecurityStatus(context, check, this);
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
					case FixTag.SecurityStatusReqID: if (_securityStatusReqID is not null) Duplicate(field); _securityStatusReqID = (FixField.Text)field; break;
					case FixTag.Symbol: if (_symbol is not null) Duplicate(field); _symbol = (FixField.Text)field; break;
					case FixTag.SymbolSfx: if (_symbolSfx is not null) Duplicate(field); _symbolSfx = (FixField.Text)field; break;
					case FixTag.SecurityID: if (_securityID is not null) Duplicate(field); _securityID = (FixField.Text)field; break;
					case FixTag.SecurityIDSource: if (_iDSource is not null) Duplicate(field); _iDSource = (FixField.Text)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.MaturityMonthYear: if (_maturityMonthYear is not null) Duplicate(field); _maturityMonthYear = (FixField.MonthYear)field; break;
					case FixTag.MaturityDay: if (_maturityDay is not null) Duplicate(field); _maturityDay = (FixField.Integer)field; break;
					case FixTag.PutOrCall: if (_putOrCall is not null) Duplicate(field); _putOrCall = (FixField.Integer)field; break;
					case FixTag.StrikePrice: if (_strikePrice is not null) Duplicate(field); _strikePrice = (FixField.Decimal)field; break;
					case FixTag.OptAttribute: if (_optAttribute is not null) Duplicate(field); _optAttribute = (FixField.Character)field; break;
					case FixTag.ContractMultiplier: if (_contractMultiplier is not null) Duplicate(field); _contractMultiplier = (FixField.Decimal)field; break;
					case FixTag.CouponRate: if (_couponRate is not null) Duplicate(field); _couponRate = (FixField.Decimal)field; break;
					case FixTag.SecurityExchange: if (_securityExchange is not null) Duplicate(field); _securityExchange = (FixField.Text)field; break;
					case FixTag.Issuer: if (_issuer is not null) Duplicate(field); _issuer = (FixField.Text)field; break;
					case FixTag.EncodedIssuerLen: if (_encodedIssuerLen is not null) Duplicate(field); _encodedIssuerLen = (FixField.Integer)field; break;
					case FixTag.EncodedIssuer: if (_encodedIssuer is not null) Duplicate(field); _encodedIssuer = (FixField.Data)field; break;
					case FixTag.SecurityDesc: if (_securityDesc is not null) Duplicate(field); _securityDesc = (FixField.Text)field; break;
					case FixTag.EncodedSecurityDescLen: if (_encodedSecurityDescLen is not null) Duplicate(field); _encodedSecurityDescLen = (FixField.Integer)field; break;
					case FixTag.EncodedSecurityDesc: if (_encodedSecurityDesc is not null) Duplicate(field); _encodedSecurityDesc = (FixField.Data)field; break;
					case FixTag.Currency: if (_currency is not null) Duplicate(field); _currency = (FixField.Text)field; break;
					case FixTag.SubscriptionRequestType: if (_subscriptionRequestType is not null) Duplicate(field); _subscriptionRequestType = (FixField.Character)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX SecurityStatusReqID, tag 324, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityStatusReqID => _securityStatusReqID;
		internal FixField.Text? _securityStatusReqID;

		/// <summary>The FIX Symbol, tag 55, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Symbol => _symbol;
		internal FixField.Text? _symbol;

		/// <summary>The FIX SymbolSfx, tag 65, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SymbolSfx => _symbolSfx;
		internal FixField.Text? _symbolSfx;

		/// <summary>The FIX SecurityID, tag 48, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityID => _securityID;
		internal FixField.Text? _securityID;

		/// <summary>The FIX IDSource, tag 22, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? IDSource => _iDSource;
		internal FixField.Text? _iDSource;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX MaturityMonthYear, tag 200, wire type <c>MonthYear</c>; null when the field is absent.</summary>
		public FixField.MonthYear? MaturityMonthYear => _maturityMonthYear;
		internal FixField.MonthYear? _maturityMonthYear;

		/// <summary>The FIX MaturityDay, tag 205, wire type <c>DayOfMonth</c>; null when the field is absent.</summary>
		public FixField.Integer? MaturityDay => _maturityDay;
		internal FixField.Integer? _maturityDay;

		/// <summary>The FIX PutOrCall, tag 201, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? PutOrCall => _putOrCall;
		internal FixField.Integer? _putOrCall;

		/// <summary>The FIX StrikePrice, tag 202, wire type <c>Price</c>; null when the field is absent.</summary>
		public FixField.Decimal? StrikePrice => _strikePrice;
		internal FixField.Decimal? _strikePrice;

		/// <summary>The FIX OptAttribute, tag 206, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? OptAttribute => _optAttribute;
		internal FixField.Character? _optAttribute;

		/// <summary>The FIX ContractMultiplier, tag 231, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? ContractMultiplier => _contractMultiplier;
		internal FixField.Decimal? _contractMultiplier;

		/// <summary>The FIX CouponRate, tag 223, wire type <c>float</c>; null when the field is absent.</summary>
		public FixField.Decimal? CouponRate => _couponRate;
		internal FixField.Decimal? _couponRate;

		/// <summary>The FIX SecurityExchange, tag 207, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityExchange => _securityExchange;
		internal FixField.Text? _securityExchange;

		/// <summary>The FIX Issuer, tag 106, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Issuer => _issuer;
		internal FixField.Text? _issuer;

		/// <summary>The FIX EncodedIssuerLen, tag 348, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedIssuerLen => _encodedIssuerLen;
		internal FixField.Integer? _encodedIssuerLen;

		/// <summary>The FIX EncodedIssuer, tag 349, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedIssuer => _encodedIssuer;
		internal FixField.Data? _encodedIssuer;

		/// <summary>The FIX SecurityDesc, tag 107, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityDesc => _securityDesc;
		internal FixField.Text? _securityDesc;

		/// <summary>The FIX EncodedSecurityDescLen, tag 350, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedSecurityDescLen => _encodedSecurityDescLen;
		internal FixField.Integer? _encodedSecurityDescLen;

		/// <summary>The FIX EncodedSecurityDesc, tag 351, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedSecurityDesc => _encodedSecurityDesc;
		internal FixField.Data? _encodedSecurityDesc;

		/// <summary>The FIX Currency, tag 15, wire type <c>Currency</c>; null when the field is absent.</summary>
		public FixField.Text? Currency => _currency;
		internal FixField.Text? _currency;

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType => _subscriptionRequestType;
		internal FixField.Character? _subscriptionRequestType;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SecurityStatusRequest(context, check, this);
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
					case FixTag.GapFillFlag: if (_gapFillFlag is not null) Duplicate(field); _gapFillFlag = (FixField.Boolean)field; break;
					case FixTag.NewSeqNo: if (_newSeqNo is not null) Duplicate(field); _newSeqNo = (FixField.Integer)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX GapFillFlag, tag 123, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? GapFillFlag => _gapFillFlag;
		internal FixField.Boolean? _gapFillFlag;

		/// <summary>The FIX NewSeqNo, tag 36, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? NewSeqNo => _newSeqNo;
		internal FixField.Integer? _newSeqNo;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SequenceReset(context, check, this);
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
					case FixTag.SettlInstID: if (_settlInstID is not null) Duplicate(field); _settlInstID = (FixField.Text)field; break;
					case FixTag.SettlInstTransType: if (_settlInstTransType is not null) Duplicate(field); _settlInstTransType = (FixField.Character)field; break;
					case FixTag.SettlInstRefID: if (_settlInstRefID is not null) Duplicate(field); _settlInstRefID = (FixField.Text)field; break;
					case FixTag.SettlInstMode: if (_settlInstMode is not null) Duplicate(field); _settlInstMode = (FixField.Character)field; break;
					case FixTag.SettlInstSource: if (_settlInstSource is not null) Duplicate(field); _settlInstSource = (FixField.Character)field; break;
					case FixTag.AllocAccount: if (_allocAccount is not null) Duplicate(field); _allocAccount = (FixField.Text)field; break;
					case FixTag.SettlLocation: if (_settlLocation is not null) Duplicate(field); _settlLocation = (FixField.Text)field; break;
					case FixTag.TradeDate: if (_tradeDate is not null) Duplicate(field); _tradeDate = (FixField.Date)field; break;
					case FixTag.AllocID: if (_allocID is not null) Duplicate(field); _allocID = (FixField.Text)field; break;
					case FixTag.LastMkt: if (_lastMkt is not null) Duplicate(field); _lastMkt = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.Side: if (_side is not null) Duplicate(field); _side = (FixField.Character)field; break;
					case FixTag.SecurityType: if (_securityType is not null) Duplicate(field); _securityType = (FixField.Text)field; break;
					case FixTag.EffectiveTime: if (_effectiveTime is not null) Duplicate(field); _effectiveTime = (FixField.Timestamp)field; break;
					case FixTag.TransactTime: if (_transactTime is not null) Duplicate(field); _transactTime = (FixField.Timestamp)field; break;
					case FixTag.ClientID: if (_clientID is not null) Duplicate(field); _clientID = (FixField.Text)field; break;
					case FixTag.ExecBroker: if (_execBroker is not null) Duplicate(field); _execBroker = (FixField.Text)field; break;
					case FixTag.StandInstDbType: if (_standInstDbType is not null) Duplicate(field); _standInstDbType = (FixField.Integer)field; break;
					case FixTag.StandInstDbName: if (_standInstDbName is not null) Duplicate(field); _standInstDbName = (FixField.Text)field; break;
					case FixTag.StandInstDbID: if (_standInstDbID is not null) Duplicate(field); _standInstDbID = (FixField.Text)field; break;
					case FixTag.SettlDeliveryType: if (_settlDeliveryType is not null) Duplicate(field); _settlDeliveryType = (FixField.Integer)field; break;
					case FixTag.SettlDepositoryCode: if (_settlDepositoryCode is not null) Duplicate(field); _settlDepositoryCode = (FixField.Text)field; break;
					case FixTag.SettlBrkrCode: if (_settlBrkrCode is not null) Duplicate(field); _settlBrkrCode = (FixField.Text)field; break;
					case FixTag.SettlInstCode: if (_settlInstCode is not null) Duplicate(field); _settlInstCode = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentName: if (_securitySettlAgentName is not null) Duplicate(field); _securitySettlAgentName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentCode: if (_securitySettlAgentCode is not null) Duplicate(field); _securitySettlAgentCode = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentAcctNum: if (_securitySettlAgentAcctNum is not null) Duplicate(field); _securitySettlAgentAcctNum = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentAcctName: if (_securitySettlAgentAcctName is not null) Duplicate(field); _securitySettlAgentAcctName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentContactName: if (_securitySettlAgentContactName is not null) Duplicate(field); _securitySettlAgentContactName = (FixField.Text)field; break;
					case FixTag.SecuritySettlAgentContactPhone: if (_securitySettlAgentContactPhone is not null) Duplicate(field); _securitySettlAgentContactPhone = (FixField.Text)field; break;
					case FixTag.CashSettlAgentName: if (_cashSettlAgentName is not null) Duplicate(field); _cashSettlAgentName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentCode: if (_cashSettlAgentCode is not null) Duplicate(field); _cashSettlAgentCode = (FixField.Text)field; break;
					case FixTag.CashSettlAgentAcctNum: if (_cashSettlAgentAcctNum is not null) Duplicate(field); _cashSettlAgentAcctNum = (FixField.Text)field; break;
					case FixTag.CashSettlAgentAcctName: if (_cashSettlAgentAcctName is not null) Duplicate(field); _cashSettlAgentAcctName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentContactName: if (_cashSettlAgentContactName is not null) Duplicate(field); _cashSettlAgentContactName = (FixField.Text)field; break;
					case FixTag.CashSettlAgentContactPhone: if (_cashSettlAgentContactPhone is not null) Duplicate(field); _cashSettlAgentContactPhone = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX SettlInstID, tag 162, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstID => _settlInstID;
		internal FixField.Text? _settlInstID;

		/// <summary>The FIX SettlInstTransType, tag 163, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstTransType => _settlInstTransType;
		internal FixField.Character? _settlInstTransType;

		/// <summary>The FIX SettlInstRefID, tag 214, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstRefID => _settlInstRefID;
		internal FixField.Text? _settlInstRefID;

		/// <summary>The FIX SettlInstMode, tag 160, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstMode => _settlInstMode;
		internal FixField.Character? _settlInstMode;

		/// <summary>The FIX SettlInstSource, tag 165, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SettlInstSource => _settlInstSource;
		internal FixField.Character? _settlInstSource;

		/// <summary>The FIX AllocAccount, tag 79, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocAccount => _allocAccount;
		internal FixField.Text? _allocAccount;

		/// <summary>The FIX SettlLocation, tag 166, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlLocation => _settlLocation;
		internal FixField.Text? _settlLocation;

		/// <summary>The FIX TradeDate, tag 75, wire type <c>LocalMktDate</c>; null when the field is absent.</summary>
		public FixField.Date? TradeDate => _tradeDate;
		internal FixField.Date? _tradeDate;

		/// <summary>The FIX AllocID, tag 70, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? AllocID => _allocID;
		internal FixField.Text? _allocID;

		/// <summary>The FIX LastMkt, tag 30, wire type <c>Exchange</c>; null when the field is absent.</summary>
		public FixField.Text? LastMkt => _lastMkt;
		internal FixField.Text? _lastMkt;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX Side, tag 54, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? Side => _side;
		internal FixField.Character? _side;

		/// <summary>The FIX SecurityType, tag 167, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecurityType => _securityType;
		internal FixField.Text? _securityType;

		/// <summary>The FIX EffectiveTime, tag 168, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? EffectiveTime => _effectiveTime;
		internal FixField.Timestamp? _effectiveTime;

		/// <summary>The FIX TransactTime, tag 60, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TransactTime => _transactTime;
		internal FixField.Timestamp? _transactTime;

		/// <summary>The FIX ClientID, tag 109, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ClientID => _clientID;
		internal FixField.Text? _clientID;

		/// <summary>The FIX ExecBroker, tag 76, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? ExecBroker => _execBroker;
		internal FixField.Text? _execBroker;

		/// <summary>The FIX StandInstDbType, tag 169, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? StandInstDbType => _standInstDbType;
		internal FixField.Integer? _standInstDbType;

		/// <summary>The FIX StandInstDbName, tag 170, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? StandInstDbName => _standInstDbName;
		internal FixField.Text? _standInstDbName;

		/// <summary>The FIX StandInstDbID, tag 171, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? StandInstDbID => _standInstDbID;
		internal FixField.Text? _standInstDbID;

		/// <summary>The FIX SettlDeliveryType, tag 172, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? SettlDeliveryType => _settlDeliveryType;
		internal FixField.Integer? _settlDeliveryType;

		/// <summary>The FIX SettlDepositoryCode, tag 173, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlDepositoryCode => _settlDepositoryCode;
		internal FixField.Text? _settlDepositoryCode;

		/// <summary>The FIX SettlBrkrCode, tag 174, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlBrkrCode => _settlBrkrCode;
		internal FixField.Text? _settlBrkrCode;

		/// <summary>The FIX SettlInstCode, tag 175, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SettlInstCode => _settlInstCode;
		internal FixField.Text? _settlInstCode;

		/// <summary>The FIX SecuritySettlAgentName, tag 176, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentName => _securitySettlAgentName;
		internal FixField.Text? _securitySettlAgentName;

		/// <summary>The FIX SecuritySettlAgentCode, tag 177, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentCode => _securitySettlAgentCode;
		internal FixField.Text? _securitySettlAgentCode;

		/// <summary>The FIX SecuritySettlAgentAcctNum, tag 178, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentAcctNum => _securitySettlAgentAcctNum;
		internal FixField.Text? _securitySettlAgentAcctNum;

		/// <summary>The FIX SecuritySettlAgentAcctName, tag 179, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentAcctName => _securitySettlAgentAcctName;
		internal FixField.Text? _securitySettlAgentAcctName;

		/// <summary>The FIX SecuritySettlAgentContactName, tag 180, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentContactName => _securitySettlAgentContactName;
		internal FixField.Text? _securitySettlAgentContactName;

		/// <summary>The FIX SecuritySettlAgentContactPhone, tag 181, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? SecuritySettlAgentContactPhone => _securitySettlAgentContactPhone;
		internal FixField.Text? _securitySettlAgentContactPhone;

		/// <summary>The FIX CashSettlAgentName, tag 182, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentName => _cashSettlAgentName;
		internal FixField.Text? _cashSettlAgentName;

		/// <summary>The FIX CashSettlAgentCode, tag 183, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentCode => _cashSettlAgentCode;
		internal FixField.Text? _cashSettlAgentCode;

		/// <summary>The FIX CashSettlAgentAcctNum, tag 184, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentAcctNum => _cashSettlAgentAcctNum;
		internal FixField.Text? _cashSettlAgentAcctNum;

		/// <summary>The FIX CashSettlAgentAcctName, tag 185, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentAcctName => _cashSettlAgentAcctName;
		internal FixField.Text? _cashSettlAgentAcctName;

		/// <summary>The FIX CashSettlAgentContactName, tag 186, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentContactName => _cashSettlAgentContactName;
		internal FixField.Text? _cashSettlAgentContactName;

		/// <summary>The FIX CashSettlAgentContactPhone, tag 187, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? CashSettlAgentContactPhone => _cashSettlAgentContactPhone;
		internal FixField.Text? _cashSettlAgentContactPhone;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.SettlementInstructions(context, check, this);
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
					case FixTag.TestReqID: if (_testReqID is not null) Duplicate(field); _testReqID = (FixField.Text)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX TestReqID, tag 112, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TestReqID => _testReqID;
		internal FixField.Text? _testReqID;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.TestRequest(context, check, this);
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
					case FixTag.TradSesReqID: if (_tradSesReqID is not null) Duplicate(field); _tradSesReqID = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.TradSesMethod: if (_tradSesMethod is not null) Duplicate(field); _tradSesMethod = (FixField.Integer)field; break;
					case FixTag.TradSesMode: if (_tradSesMode is not null) Duplicate(field); _tradSesMode = (FixField.Integer)field; break;
					case FixTag.UnsolicitedIndicator: if (_unsolicitedIndicator is not null) Duplicate(field); _unsolicitedIndicator = (FixField.Boolean)field; break;
					case FixTag.TradSesStatus: if (_tradSesStatus is not null) Duplicate(field); _tradSesStatus = (FixField.Integer)field; break;
					case FixTag.TradSesStartTime: if (_tradSesStartTime is not null) Duplicate(field); _tradSesStartTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesOpenTime: if (_tradSesOpenTime is not null) Duplicate(field); _tradSesOpenTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesPreCloseTime: if (_tradSesPreCloseTime is not null) Duplicate(field); _tradSesPreCloseTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesCloseTime: if (_tradSesCloseTime is not null) Duplicate(field); _tradSesCloseTime = (FixField.Timestamp)field; break;
					case FixTag.TradSesEndTime: if (_tradSesEndTime is not null) Duplicate(field); _tradSesEndTime = (FixField.Timestamp)field; break;
					case FixTag.TotalVolumeTraded: if (_totalVolumeTraded is not null) Duplicate(field); _totalVolumeTraded = (FixField.Decimal)field; break;
					case FixTag.Text: if (_text is not null) Duplicate(field); _text = (FixField.Text)field; break;
					case FixTag.EncodedTextLen: if (_encodedTextLen is not null) Duplicate(field); _encodedTextLen = (FixField.Integer)field; break;
					case FixTag.EncodedText: if (_encodedText is not null) Duplicate(field); _encodedText = (FixField.Data)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX TradSesReqID, tag 335, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradSesReqID => _tradSesReqID;
		internal FixField.Text? _tradSesReqID;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX TradSesMethod, tag 338, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMethod => _tradSesMethod;
		internal FixField.Integer? _tradSesMethod;

		/// <summary>The FIX TradSesMode, tag 339, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMode => _tradSesMode;
		internal FixField.Integer? _tradSesMode;

		/// <summary>The FIX UnsolicitedIndicator, tag 325, wire type <c>Boolean</c>; null when the field is absent.</summary>
		public FixField.Boolean? UnsolicitedIndicator => _unsolicitedIndicator;
		internal FixField.Boolean? _unsolicitedIndicator;

		/// <summary>The FIX TradSesStatus, tag 340, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesStatus => _tradSesStatus;
		internal FixField.Integer? _tradSesStatus;

		/// <summary>The FIX TradSesStartTime, tag 341, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesStartTime => _tradSesStartTime;
		internal FixField.Timestamp? _tradSesStartTime;

		/// <summary>The FIX TradSesOpenTime, tag 342, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesOpenTime => _tradSesOpenTime;
		internal FixField.Timestamp? _tradSesOpenTime;

		/// <summary>The FIX TradSesPreCloseTime, tag 343, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesPreCloseTime => _tradSesPreCloseTime;
		internal FixField.Timestamp? _tradSesPreCloseTime;

		/// <summary>The FIX TradSesCloseTime, tag 344, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesCloseTime => _tradSesCloseTime;
		internal FixField.Timestamp? _tradSesCloseTime;

		/// <summary>The FIX TradSesEndTime, tag 345, wire type <c>UTCTimestamp</c>; null when the field is absent.</summary>
		public FixField.Timestamp? TradSesEndTime => _tradSesEndTime;
		internal FixField.Timestamp? _tradSesEndTime;

		/// <summary>The FIX TotalVolumeTraded, tag 387, wire type <c>Qty</c>; null when the field is absent.</summary>
		public FixField.Decimal? TotalVolumeTraded => _totalVolumeTraded;
		internal FixField.Decimal? _totalVolumeTraded;

		/// <summary>The FIX Text, tag 58, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? Text => _text;
		internal FixField.Text? _text;

		/// <summary>The FIX EncodedTextLen, tag 354, wire type <c>Length</c>; null when the field is absent.</summary>
		public FixField.Integer? EncodedTextLen => _encodedTextLen;
		internal FixField.Integer? _encodedTextLen;

		/// <summary>The FIX EncodedText, tag 355, wire type <c>data</c>; null when the field is absent.</summary>
		public FixField.Data? EncodedText => _encodedText;
		internal FixField.Data? _encodedText;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.TradingSessionStatus(context, check, this);
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
					case FixTag.TradSesReqID: if (_tradSesReqID is not null) Duplicate(field); _tradSesReqID = (FixField.Text)field; break;
					case FixTag.TradingSessionID: if (_tradingSessionID is not null) Duplicate(field); _tradingSessionID = (FixField.Text)field; break;
					case FixTag.TradSesMethod: if (_tradSesMethod is not null) Duplicate(field); _tradSesMethod = (FixField.Integer)field; break;
					case FixTag.TradSesMode: if (_tradSesMode is not null) Duplicate(field); _tradSesMode = (FixField.Integer)field; break;
					case FixTag.SubscriptionRequestType: if (_subscriptionRequestType is not null) Duplicate(field); _subscriptionRequestType = (FixField.Character)field; break;

					default:
						if (!SetStandardField(field))
							OutOfScope(field);

						break;
				}
			}
		}

		/// <summary>The FIX TradSesReqID, tag 335, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradSesReqID => _tradSesReqID;
		internal FixField.Text? _tradSesReqID;

		/// <summary>The FIX TradingSessionID, tag 336, wire type <c>String</c>; null when the field is absent.</summary>
		public FixField.Text? TradingSessionID => _tradingSessionID;
		internal FixField.Text? _tradingSessionID;

		/// <summary>The FIX TradSesMethod, tag 338, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMethod => _tradSesMethod;
		internal FixField.Integer? _tradSesMethod;

		/// <summary>The FIX TradSesMode, tag 339, wire type <c>int</c>; null when the field is absent.</summary>
		public FixField.Integer? TradSesMode => _tradSesMode;
		internal FixField.Integer? _tradSesMode;

		/// <summary>The FIX SubscriptionRequestType, tag 263, wire type <c>char</c>; null when the field is absent.</summary>
		public FixField.Character? SubscriptionRequestType => _subscriptionRequestType;
		internal FixField.Character? _subscriptionRequestType;

		/// <summary>Asks the context for the check of this type and runs it.</summary>
		private protected override void Check(Fix42Context context, FixCheck check)
		{
			context.Validators.TradingSessionStatusRequest(context, check, this);
		}
	}
}
