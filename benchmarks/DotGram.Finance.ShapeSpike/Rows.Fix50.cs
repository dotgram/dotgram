// SPIKE (not for merging): the rows of Fix50, written as a consumer of each column writes them. Written by
// a script from one template; the three versions differ in their types and in the group of an Order.

using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using M50 = DotGram.Finance.Fix.Fix50.FixMessage;
using P50 = DotGram.Finance.Fix.Fix50.FixParser;
using C50 = DotGram.Finance.Fix.Fix50.Fix50Context;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

public partial class FixCollectionShapeBenchmarks
{
	byte[] order50Bytes = [];
	byte[] snap50Bytes = [];
	M50.NewOrderSingle[] orders50 = [];
	M50.NewOrderSingle[] bare50 = [];
	M50.MarketDataSnapshotFullRefresh snap50 = null!;

	void Setup50()
	{
		order50Bytes = Latin1(Wire("50", "D", OrderBody("50", true)));
		snap50Bytes  = Latin1(Wire("50", "W", SnapshotBody()));
		orders50     = new M50.NewOrderSingle[Batch];
		bare50       = new M50.NewOrderSingle[Batch];

		var bareBytes = Latin1(Wire("50", "D", OrderBody("50", false)));

		for (var i = 0; i < Batch; i++)
		{
			orders50[i] = (M50.NewOrderSingle)P50.ParseMessage(order50Bytes);
			bare50[i]   = (M50.NewOrderSingle)P50.ParseMessage(bareBytes);
		}

		snap50 = (M50.MarketDataSnapshotFullRefresh)P50.ParseMessage(snap50Bytes);
	}

	static long Digest50(M50 message)
	{
		long digest = message.IsValid ? 0 : 1;
		long fields;

		foreach (var field in message.Fields)
			digest += field.Tag;

#if FIX_SHAPE_D
		fields = message.Fields.Length;
#else
		fields = message.Fields.Count;
#endif
		digest += fields * 1000003;

		if (message is M50.NewOrderSingle order)
		{
#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var e in order.NoPartyIDsGroups)
#else
			foreach (var e in order.NoPartyIDsGroups ?? [])
#endif
				digest += 7;
		}

		if (message is M50.MarketDataSnapshotFullRefresh snapshot)
		{
#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var e in snapshot.NoMDEntriesGroups)
#else
			foreach (var e in snapshot.NoMDEntriesGroups ?? [])
#endif
				digest += e.MDEntryPx!.Position;
		}

		return digest;
	}

	// I1: foreach over the entries of a snapshot.
	static long Entries50(M50.MarketDataSnapshotFullRefresh message)
	{
		long sum = 0;

#if FIX_SHAPE_C || FIX_SHAPE_D
		foreach (var e in message.NoMDEntriesGroups)
#else
		foreach (var e in message.NoMDEntriesGroups ?? [])
#endif
			sum += e.MDEntryPx!.Position;

		return sum;
	}

	// I4: foreach over the Fields of a message.
	static long Fields50(M50 message)
	{
		long sum = 0;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	// I3: parties, then the sub-IDs of each.
	static long Nested50(M50.NewOrderSingle message)
	{
		long sum = 0;

#if FIX_SHAPE_C || FIX_SHAPE_D
		foreach (var party in message.NoPartyIDsGroups)
#else
		foreach (var party in message.NoPartyIDsGroups ?? [])
#endif
		{
			sum += party.PartyID.Position;

#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var sub in party.NoPartySubIDsGroups)
#else
			foreach (var sub in party.NoPartySubIDsGroups ?? [])
#endif
				sum += sub.PartySubID.Position;
		}

		return sum;
	}

	// I6: the values of ExecInst.
	static long Multiple50(M50.NewOrderSingle message)
	{
		long sum = 0;

		foreach (var value in message.ExecInst!.Value)
			sum += value.Length;

		return sum;
	}

	[Benchmark, BenchmarkCategory("W2")]
	public object Fix50_P2_ParseSnapshot()
	{
		return P50.ParseMessage(snap50Bytes);
	}

	[Benchmark, BenchmarkCategory("W2")]
	public long Fix50_E1_ParseIterateSnapshot()
	{
		var message = (M50.MarketDataSnapshotFullRefresh)P50.ParseMessage(snap50Bytes);

		return Entries50(message) + Fields50(message);
	}

	[Benchmark, BenchmarkCategory("W2")]
	public long Fix50_E2_ParseIterateOrder()
	{
		var message = (M50.NewOrderSingle)P50.ParseMessage(order50Bytes);

		return Nested50(message) + Multiple50(message);
	}

	// What each row of Fix50 answers once, to be held to the constants in the source.
	void Check50(Action<string, long> check)
	{
		check("Fix50_P2_ParseSnapshot", Digest50((M50)Fix50_P2_ParseSnapshot()));
		check("Fix50_E1_ParseIterateSnapshot", Fix50_E1_ParseIterateSnapshot());
		check("Fix50_E2_ParseIterateOrder", Fix50_E2_ParseIterateOrder());
	}
}
