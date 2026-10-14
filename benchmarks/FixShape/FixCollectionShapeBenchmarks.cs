using System;
using System.Globalization;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;

using DotGram.Finance.Fix;

using F42 = DotGram.Finance.Fix.Fix42;
using F44 = DotGram.Finance.Fix.Fix44;
using F50 = DotGram.Finance.Fix.Fix50;

namespace DotGram.Finance.FixShape;

// The public collection shape of a parse result, measured as a consumer reads it. Every column runs the
// same methods; what differs is the DotGram.Finance the job was built against (FixShape) and, where a
// consumer would write it differently, the code under #if. Each row's checksum is held to a constant in
// the setup, so every column is proven to do the same work. The rows that return a message return it as
// object: the project BenchmarkDotNet generates references this one, not the DotGram.Finance under it.
[MemoryDiagnoser]
public class FixCollectionShapeBenchmarks
{
	const int Orders    = 100;
	const int Snapshots = 10;

	byte[] _order42    = [];
	byte[] _order44    = [];
	byte[] _order50    = [];
	byte[] _snapshot42 = [];
	byte[] _snapshot44 = [];
	byte[] _snapshot50 = [];

	F44.FixMessage.NewOrderSingle[]                _orders    = [];
	F44.FixMessage.NewOrderSingle[]                _bare      = [];
	F44.FixMessage.MarketDataSnapshotFullRefresh[] _snapshots = [];

	internal static readonly (string Row, long Sum)[] Expected =
	[
		("Fix44_P1_ParseOrder",             81),
		("Fix44_P2_ParseSnapshot",          3011),
		("Fix44_V1_ParseValidateOrder",     81000),
		("Fix44_V2_ParseValidateSnapshot",  3011000),
		("Fix44_I1_GroupsForeach",          120770000),
		("Fix44_I2_GroupsFor",              120770000),
		("Fix44_I3_NestedForeach",          463200),
		("Fix44_I4_FieldsForeach",          8108380),
		("Fix44_I5_LinqCount",              5000),
		("Fix44_I6_MultipleForeach",        24500),
		("Fix44_I7_AbsentForeach",          100),
		("Fix44_E1_ParseIterateSnapshot",   12887838),
		("Fix44_E2_ParseIterateOrder",      4877),
		("Fix42_P2_ParseSnapshot",          3011),
		("Fix42_E1_ParseIterateSnapshot",   12887838),
		("Fix42_E2_ParseIterateOrder",      1245),
		("Fix50_P2_ParseSnapshot",          3012),
		("Fix50_E1_ParseIterateSnapshot",   12896966),
		("Fix50_E2_ParseIterateOrder",      5005),
	];

	[GlobalSetup]
	public void Setup()
	{
		var parties = new StringBuilder("453=8|");

		for (var i = 0; i < 8; i++)
			parties.Append(CultureInfo.InvariantCulture, $"448=P{i}|447=D|452=1|802=2|523=S{i}A|803=1|523=S{i}B|803=2|");

		var allocs = new StringBuilder("78=8|");

		for (var i = 0; i < 8; i++)
			allocs.Append(CultureInfo.InvariantCulture, $"79=ACC{i}|80=10|");

		var entries = new StringBuilder("268=1000|");

		for (var i = 0; i < 1000; i++)
			entries.Append(i % 2 == 0 ? "269=0|270=12.50|271=100|" : "269=1|270=12.75|271=200|");

		const string Tail = "18=1 5 G H|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|";

		_order42    = Wire("FIX.4.2", "D", "11=ORDER|21=1|" + allocs + Tail);
		_order44    = Wire("FIX.4.4", "D", "11=ORDER|" + parties + Tail);
		_order50    = Wire("FIXT.1.1", "D", "11=ORDER|" + parties + Tail, "9");
		_snapshot42 = Wire("FIX.4.2", "W", "262=REQ|55=ABC|" + entries);
		_snapshot44 = Wire("FIX.4.4", "W", "262=REQ|55=ABC|" + entries);
		_snapshot50 = Wire("FIXT.1.1", "W", "262=REQ|55=ABC|" + entries, "9");

		var bare = Wire("FIX.4.4", "D", "11=ORDER|" + Tail);

		_orders    = [.. Enumerable.Range(0, Orders).Select(_ => (F44.FixMessage.NewOrderSingle)F44.FixParser.ParseMessage(_order44))];
		_bare      = [.. Enumerable.Range(0, Orders).Select(_ => (F44.FixMessage.NewOrderSingle)F44.FixParser.ParseMessage(bare))];
		_snapshots = [.. Enumerable.Range(0, Snapshots).Select(_ => (F44.FixMessage.MarketDataSnapshotFullRefresh)F44.FixParser.ParseMessage(_snapshot44))];

		if (Environment.GetEnvironmentVariable("FIXH1_PRINT") is null)
		{
			foreach (var (row, sum) in Expected)
				if (Run(row) != sum)
					throw new InvalidOperationException($"{row}: checksum {Run(row)}, expected {sum}.");
		}
	}

	internal long Run(string row)
	{
		return row switch
		{
			"Fix44_P1_ParseOrder"            => Fields(Fix44_P1_ParseOrder()),
			"Fix44_P2_ParseSnapshot"         => Fields(Fix44_P2_ParseSnapshot()),
			"Fix44_V1_ParseValidateOrder"    => Fields(Fix44_V1_ParseValidateOrder()) * 1000 + Findings((F44.FixMessage)Fix44_V1_ParseValidateOrder()),
			"Fix44_V2_ParseValidateSnapshot" => Fields(Fix44_V2_ParseValidateSnapshot()) * 1000 + Findings((F44.FixMessage)Fix44_V2_ParseValidateSnapshot()),
			"Fix44_I1_GroupsForeach"         => Fix44_I1_GroupsForeach(),
			"Fix44_I2_GroupsFor"             => Fix44_I2_GroupsFor(),
			"Fix44_I3_NestedForeach"         => Fix44_I3_NestedForeach(),
			"Fix44_I4_FieldsForeach"         => Fix44_I4_FieldsForeach(),
			"Fix44_I5_LinqCount"             => Fix44_I5_LinqCount(),
			"Fix44_I6_MultipleForeach"       => Fix44_I6_MultipleForeach(),
			"Fix44_I7_AbsentForeach"         => Fix44_I7_AbsentForeach(),
			"Fix44_E1_ParseIterateSnapshot"  => Fix44_E1_ParseIterateSnapshot(),
			"Fix44_E2_ParseIterateOrder"     => Fix44_E2_ParseIterateOrder(),
			"Fix42_P2_ParseSnapshot"         => Fields(Fix42_P2_ParseSnapshot()),
			"Fix42_E1_ParseIterateSnapshot"  => Fix42_E1_ParseIterateSnapshot(),
			"Fix42_E2_ParseIterateOrder"     => Fix42_E2_ParseIterateOrder(),
			"Fix50_P2_ParseSnapshot"         => Fields(Fix50_P2_ParseSnapshot()),
			"Fix50_E1_ParseIterateSnapshot"  => Fix50_E1_ParseIterateSnapshot(),
			"Fix50_E2_ParseIterateOrder"     => Fix50_E2_ParseIterateOrder(),
			_                                => throw new ArgumentException(row),
		};
	}

	internal string Report(string row)
	{
		var message = row switch
		{
			"Fix44_V1_ParseValidateOrder"    => (F44.FixMessage)Fix44_V1_ParseValidateOrder(),
			"Fix44_V2_ParseValidateSnapshot" => (F44.FixMessage)Fix44_V2_ParseValidateSnapshot(),
			_                                => null,
		};

		if (message is null)
			return "";

#if FIXSHAPE_C
		return string.Join("; ", message.InvalidFindings.Select(f => f.ToString()));
#else
		return string.Join("; ", (message.InvalidFindings ?? []).Select(f => f.ToString()));
#endif
	}

	static long Fields(object message)
	{
		return message switch
		{
			F42.FixMessage m => m.Fields.Count,
			F44.FixMessage m => m.Fields.Count,
			F50.FixMessage m => m.Fields.Count,
			_                => throw new ArgumentException(nameof(message)),
		};
	}

	static long Findings(F44.FixMessage message)
	{
#if FIXSHAPE_C
		return message.InvalidFindings.Count;
#else
		return message.InvalidFindings?.Count ?? 0;
#endif
	}

	// ── FIX 4.4: parse, parse and validate ──────────────────────────────────────────────────────

	[Benchmark]
	public object Fix44_P1_ParseOrder()
	{
		return F44.FixParser.ParseMessage(_order44);
	}

	[Benchmark]
	public object Fix44_P2_ParseSnapshot()
	{
		return F44.FixParser.ParseMessage(_snapshot44);
	}

	[Benchmark]
	public object Fix44_V1_ParseValidateOrder()
	{
		var message = F44.FixParser.ParseMessage(_order44);

		message.Validate(F44.Fix44Context.Default);

		return message;
	}

	[Benchmark]
	public object Fix44_V2_ParseValidateSnapshot()
	{
		var message = F44.FixParser.ParseMessage(_snapshot44);

		message.Validate(F44.Fix44Context.Default);

		return message;
	}

	// ── FIX 4.4: iterate what was parsed, a batch of messages a call ────────────────────────────

	[Benchmark]
	public long Fix44_I1_GroupsForeach()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
			sum += Entries44(message);

		return sum;
	}

	[Benchmark]
	public long Fix44_I2_GroupsFor()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
		{
			var entries = message.NoMDEntriesGroups;

#if FIXSHAPE_C
			for (var i = 0; i < entries.Count; i++)
				sum += entries[i].MDEntryPx!.Position;
#else
			if (entries is not null)
				for (var i = 0; i < entries.Count; i++)
					sum += entries[i].MDEntryPx!.Position;
#endif
		}

		return sum;
	}

	[Benchmark]
	public long Fix44_I3_NestedForeach()
	{
		var sum = 0L;

		foreach (var order in _orders)
			sum += Parties44(order);

		return sum;
	}

	[Benchmark]
	public long Fix44_I4_FieldsForeach()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
			sum += Tags44(message);

		return sum;
	}

	[Benchmark]
	public long Fix44_I5_LinqCount()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
#if FIXSHAPE_C
			sum += message.NoMDEntriesGroups.Count(e => e.MDEntryType.Value == '0');
#else
			sum += (message.NoMDEntriesGroups ?? []).Count(e => e.MDEntryType.Value == '0');
#endif

		return sum;
	}

	[Benchmark]
	public long Fix44_I6_MultipleForeach()
	{
		var sum = 0L;

		foreach (var order in _orders)
			sum += Values44(order);

		return sum;
	}

	[Benchmark]
	public long Fix44_I7_AbsentForeach()
	{
		var sum = 0L;

		foreach (var order in _bare)
		{
			sum++;

#if FIXSHAPE_C
			foreach (var party in order.NoPartyIDsGroups)
#else
			foreach (var party in order.NoPartyIDsGroups ?? [])
#endif
				sum += party.PartyID.Position;
		}

		return sum;
	}

	// ── FIX 4.4: parse and iterate ──────────────────────────────────────────────────────────────

	[Benchmark]
	public long Fix44_E1_ParseIterateSnapshot()
	{
		var message = (F44.FixMessage.MarketDataSnapshotFullRefresh)F44.FixParser.ParseMessage(_snapshot44);

		return Entries44(message) + Tags44(message);
	}

	[Benchmark]
	public long Fix44_E2_ParseIterateOrder()
	{
		var order = (F44.FixMessage.NewOrderSingle)F44.FixParser.ParseMessage(_order44);

		return Parties44(order) + Values44(order);
	}

	static long Entries44(F44.FixMessage.MarketDataSnapshotFullRefresh message)
	{
		var sum = 0L;

#if FIXSHAPE_C
		foreach (var entry in message.NoMDEntriesGroups)
#else
		foreach (var entry in message.NoMDEntriesGroups ?? [])
#endif
			sum += entry.MDEntryPx!.Position;

		return sum;
	}

	static long Tags44(F44.FixMessage message)
	{
		var sum = 0L;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	static long Parties44(F44.FixMessage.NewOrderSingle order)
	{
		var sum = 0L;

#if FIXSHAPE_C
		foreach (var party in order.NoPartyIDsGroups)
			foreach (var sub in party.NoPartySubIDsGroups)
#else
		foreach (var party in order.NoPartyIDsGroups ?? [])
			foreach (var sub in party.NoPartySubIDsGroups ?? [])
#endif
				sum += sub.PartySubID.Position;

		return sum;
	}

	static long Values44(F44.FixMessage.NewOrderSingle order)
	{
		var sum = 0L;

		foreach (var value in order.ExecInst!.Value)
			sum += value[0];

		return sum;
	}

	// ── FIX 4.2 ─────────────────────────────────────────────────────────────────────────────────

	[Benchmark]
	public object Fix42_P2_ParseSnapshot()
	{
		return F42.FixParser.ParseMessage(_snapshot42);
	}

	[Benchmark]
	public long Fix42_E1_ParseIterateSnapshot()
	{
		var message = (F42.FixMessage.MarketDataSnapshotFullRefresh)F42.FixParser.ParseMessage(_snapshot42);
		var sum     = 0L;

#if FIXSHAPE_C
		foreach (var entry in message.NoMDEntriesGroups)
#else
		foreach (var entry in message.NoMDEntriesGroups ?? [])
#endif
			sum += entry.MDEntryPx!.Position;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	[Benchmark]
	public long Fix42_E2_ParseIterateOrder()
	{
		var order = (F42.FixMessage.NewOrderSingle)F42.FixParser.ParseMessage(_order42);
		var sum   = 0L;

#if FIXSHAPE_C
		foreach (var alloc in order.NoAllocsGroups)
#else
		foreach (var alloc in order.NoAllocsGroups ?? [])
#endif
			sum += alloc.AllocAccount.Position;

		foreach (var value in order.ExecInst!.Value)
			sum += value[0];

		return sum;
	}

	// ── FIX 5.0 SP2 ─────────────────────────────────────────────────────────────────────────────

	[Benchmark]
	public object Fix50_P2_ParseSnapshot()
	{
		return F50.FixParser.ParseMessage(_snapshot50);
	}

	[Benchmark]
	public long Fix50_E1_ParseIterateSnapshot()
	{
		var message = (F50.FixMessage.MarketDataSnapshotFullRefresh)F50.FixParser.ParseMessage(_snapshot50);
		var sum     = 0L;

#if FIXSHAPE_C
		foreach (var entry in message.NoMDEntriesGroups)
#else
		foreach (var entry in message.NoMDEntriesGroups ?? [])
#endif
			sum += entry.MDEntryPx!.Position;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	[Benchmark]
	public long Fix50_E2_ParseIterateOrder()
	{
		var order = (F50.FixMessage.NewOrderSingle)F50.FixParser.ParseMessage(_order50);
		var sum   = 0L;

#if FIXSHAPE_C
		foreach (var party in order.NoPartyIDsGroups)
			foreach (var sub in party.NoPartySubIDsGroups)
#else
		foreach (var party in order.NoPartyIDsGroups ?? [])
			foreach (var sub in party.NoPartySubIDsGroups ?? [])
#endif
				sum += sub.PartySubID.Position;

		foreach (var value in order.ExecInst!.Value)
			sum += value[0];

		return sum;
	}

	static byte[] Wire(string begin, string type, string fields, string? applVerId = null)
	{
		var body = "35=" + type + "|" + (applVerId is null ? "" : "1128=" + applVerId + "|") + "49=S|56=T|34=1|52=20260915-12:00:00|" + fields;

		body = body.Replace('|', '\u0001');

		var wire = "8=" + begin + "\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum  = 0;

		foreach (var c in wire)
			sum = (sum + c) & 255;

		return Encoding.Latin1.GetBytes(wire + "10=" + sum.ToString("000", CultureInfo.InvariantCulture) + "\u0001");
	}
}
