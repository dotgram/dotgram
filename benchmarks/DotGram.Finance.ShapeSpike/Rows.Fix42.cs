// SPIKE (not for merging): the rows of Fix42, written as a consumer of each column writes them. Written by
// a script from one template; the three versions differ in their types and in the group of an Order.

using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using M42 = DotGram.Finance.Fix.Fix42.FixMessage;
using P42 = DotGram.Finance.Fix.Fix42.FixParser;
using C42 = DotGram.Finance.Fix.Fix42.Fix42Context;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

public partial class FixCollectionShapeBenchmarks
{
	byte[] order42Bytes = [];
	byte[] snap42Bytes = [];
	M42.NewOrderSingle[] orders42 = [];
	M42.NewOrderSingle[] bare42 = [];
	M42.MarketDataSnapshotFullRefresh snap42 = null!;

	void Setup42()
	{
		order42Bytes = Latin1(Wire("42", "D", OrderBody("42", true)));
		snap42Bytes  = Latin1(Wire("42", "W", SnapshotBody()));
		orders42     = new M42.NewOrderSingle[Batch];
		bare42       = new M42.NewOrderSingle[Batch];

		var bareBytes = Latin1(Wire("42", "D", OrderBody("42", false)));

		for (var i = 0; i < Batch; i++)
		{
			orders42[i] = (M42.NewOrderSingle)P42.ParseMessage(order42Bytes);
			bare42[i]   = (M42.NewOrderSingle)P42.ParseMessage(bareBytes);
		}

		snap42 = (M42.MarketDataSnapshotFullRefresh)P42.ParseMessage(snap42Bytes);
	}

	static long Digest42(M42 message)
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

		if (message is M42.NewOrderSingle order)
		{
#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var e in order.NoAllocsGroups)
#else
			foreach (var e in order.NoAllocsGroups ?? [])
#endif
				digest += 7;
		}

		if (message is M42.MarketDataSnapshotFullRefresh snapshot)
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
	static long Entries42(M42.MarketDataSnapshotFullRefresh message)
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
	static long Fields42(M42 message)
	{
		long sum = 0;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	// The nested walk of 4.2 has no second level: the entries of the allocation group.
	static long Nested42(M42.NewOrderSingle message)
	{
		long sum = 0;

#if FIX_SHAPE_C || FIX_SHAPE_D
		foreach (var alloc in message.NoAllocsGroups)
#else
		foreach (var alloc in message.NoAllocsGroups ?? [])
#endif
			sum += alloc.AllocAccount.Position + alloc.AllocShares!.Position;

		return sum;
	}

	// I6: the values of ExecInst.
	static long Multiple42(M42.NewOrderSingle message)
	{
		long sum = 0;

		foreach (var value in message.ExecInst!.Value)
			sum += value.Length;

		return sum;
	}

	[Benchmark, BenchmarkCategory("W2")]
	public object Fix42_P2_ParseSnapshot()
	{
		return P42.ParseMessage(snap42Bytes);
	}

	[Benchmark, BenchmarkCategory("W2")]
	public long Fix42_E1_ParseIterateSnapshot()
	{
		var message = (M42.MarketDataSnapshotFullRefresh)P42.ParseMessage(snap42Bytes);

		return Entries42(message) + Fields42(message);
	}

	[Benchmark, BenchmarkCategory("W2")]
	public long Fix42_E2_ParseIterateOrder()
	{
		var message = (M42.NewOrderSingle)P42.ParseMessage(order42Bytes);

		return Nested42(message) + Multiple42(message);
	}

	// What each row of Fix42 answers once, to be held to the constants in the source.
	void Check42(Action<string, long> check)
	{
		check("Fix42_P2_ParseSnapshot", Digest42((M42)Fix42_P2_ParseSnapshot()));
		check("Fix42_E1_ParseIterateSnapshot", Fix42_E1_ParseIterateSnapshot());
		check("Fix42_E2_ParseIterateOrder", Fix42_E2_ParseIterateOrder());
	}
}
