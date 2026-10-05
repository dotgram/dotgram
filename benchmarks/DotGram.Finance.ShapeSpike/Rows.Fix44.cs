// SPIKE (not for merging): the rows of Fix44, written as a consumer of each column writes them. Written by
// a script from one template; the three versions differ in their types and in the group of an Order.

using System.Linq;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

using M44 = DotGram.Finance.Fix.Fix44.FixMessage;
using P44 = DotGram.Finance.Fix.Fix44.FixParser;
using C44 = DotGram.Finance.Fix.Fix44.Fix44Context;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

public partial class FixCollectionShapeBenchmarks
{
	byte[] order44Bytes = [];
	byte[] snap44Bytes = [];
	M44.NewOrderSingle[] orders44 = [];
	M44.NewOrderSingle[] bare44 = [];
	M44.MarketDataSnapshotFullRefresh snap44 = null!;

	void Setup44()
	{
		order44Bytes = Latin1(Wire("44", "D", OrderBody("44", true)));
		snap44Bytes  = Latin1(Wire("44", "W", SnapshotBody()));
		orders44     = new M44.NewOrderSingle[Batch];
		bare44       = new M44.NewOrderSingle[Batch];

		var bareBytes = Latin1(Wire("44", "D", OrderBody("44", false)));

		for (var i = 0; i < Batch; i++)
		{
			orders44[i] = (M44.NewOrderSingle)P44.ParseMessage(order44Bytes);
			bare44[i]   = (M44.NewOrderSingle)P44.ParseMessage(bareBytes);
		}

		snap44 = (M44.MarketDataSnapshotFullRefresh)P44.ParseMessage(snap44Bytes);
	}

	static long Digest44(M44 message)
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

		if (message is M44.NewOrderSingle order)
		{
#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var e in order.NoPartyIDsGroups)
#else
			foreach (var e in order.NoPartyIDsGroups ?? [])
#endif
				digest += 7;
		}

		if (message is M44.MarketDataSnapshotFullRefresh snapshot)
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
	static long Entries44(M44.MarketDataSnapshotFullRefresh message)
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
	static long Fields44(M44 message)
	{
		long sum = 0;

		foreach (var field in message.Fields)
			sum += field.Tag;

		return sum;
	}

	// I3: parties, then the sub-IDs of each.
	static long Nested44(M44.NewOrderSingle message)
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
	static long Multiple44(M44.NewOrderSingle message)
	{
		long sum = 0;

		foreach (var value in message.ExecInst!.Value)
			sum += value.Length;

		return sum;
	}

	[Benchmark, BenchmarkCategory("W1")]
	public object Fix44_P1_ParseOrder()
	{
		return P44.ParseMessage(order44Bytes);
	}

	[Benchmark, BenchmarkCategory("W1")]
	public object Fix44_P2_ParseSnapshot()
	{
		return P44.ParseMessage(snap44Bytes);
	}

	[Benchmark, BenchmarkCategory("W1")]
	public object Fix44_V1_ParseValidateOrder()
	{
		var message = P44.ParseMessage(order44Bytes);

		message.Validate(C44.Default);

		return message;
	}

	[Benchmark, BenchmarkCategory("W1")]
	public object Fix44_V2_ParseValidateSnapshot()
	{
		var message = P44.ParseMessage(snap44Bytes);

		message.Validate(C44.Default);

		return message;
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I1_GroupsForeach()
	{
		return Entries44(snap44);
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I2_GroupsFor()
	{
		long sum = 0;

#if FIX_SHAPE_D
		var list = snap44.NoMDEntriesGroups;

		for (var i = 0; i < list.Length; i++)
#elif FIX_SHAPE_C
		var list = snap44.NoMDEntriesGroups;

		for (var i = 0; i < list.Count; i++)
#else
		var list = snap44.NoMDEntriesGroups ?? [];

		for (var i = 0; i < list.Count; i++)
#endif
			sum += list[i].MDEntryPx!.Position;

		return sum;
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I3_NestedForeach()
	{
		long sum = 0;

		foreach (var order in orders44)
			sum += Nested44(order);

		return sum;
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I4_FieldsForeach()
	{
		return Fields44(snap44);
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I5_LinqCount()
	{
#if FIX_SHAPE_C || FIX_SHAPE_D
		return snap44.NoMDEntriesGroups.Count(static e => e.MDEntryType?.Value == '0');
#else
		return (snap44.NoMDEntriesGroups ?? []).Count(static e => e.MDEntryType?.Value == '0');
#endif
	}

	[Benchmark, BenchmarkCategory("W1", "W3")]
	public long Fix44_I6_MultipleForeach()
	{
		long sum = 0;

		foreach (var order in orders44)
			sum += Multiple44(order);

		return sum;
	}

	[Benchmark, BenchmarkCategory("W1", "W3", "W4")]
	public long Fix44_I7_AbsentForeach()
	{
		long sum = 0;

		foreach (var order in bare44)
		{
#if FIX_SHAPE_C || FIX_SHAPE_D
			foreach (var party in order.NoPartyIDsGroups)
#else
			foreach (var party in order.NoPartyIDsGroups ?? [])
#endif
				sum += party.PartyID.Position;

			sum++;
		}

		return sum;
	}

	[Benchmark, BenchmarkCategory("W1", "W3")]
	public long Fix44_E1_ParseIterateSnapshot()
	{
		var message = (M44.MarketDataSnapshotFullRefresh)P44.ParseMessage(snap44Bytes);

		return Entries44(message) + Fields44(message);
	}

	[Benchmark, BenchmarkCategory("W1", "W3")]
	public long Fix44_E2_ParseIterateOrder()
	{
		var message = (M44.NewOrderSingle)P44.ParseMessage(order44Bytes);

		return Nested44(message) + Multiple44(message);
	}

	// What each row of Fix44 answers once, to be held to the constants in the source.
	void Check44(Action<string, long> check)
	{
		check("Fix44_P1_ParseOrder", Digest44((M44)Fix44_P1_ParseOrder()));
		check("Fix44_P2_ParseSnapshot", Digest44((M44)Fix44_P2_ParseSnapshot()));
		check("Fix44_V1_ParseValidateOrder", Digest44((M44)Fix44_V1_ParseValidateOrder()));
		check("Fix44_V2_ParseValidateSnapshot", Digest44((M44)Fix44_V2_ParseValidateSnapshot()));
		check("Fix44_I1_GroupsForeach", Fix44_I1_GroupsForeach());
		check("Fix44_I2_GroupsFor", Fix44_I2_GroupsFor());
		check("Fix44_I3_NestedForeach", Fix44_I3_NestedForeach());
		check("Fix44_I4_FieldsForeach", Fix44_I4_FieldsForeach());
		check("Fix44_I5_LinqCount", Fix44_I5_LinqCount());
		check("Fix44_I6_MultipleForeach", Fix44_I6_MultipleForeach());
		check("Fix44_I7_AbsentForeach", Fix44_I7_AbsentForeach());
		check("Fix44_E1_ParseIterateSnapshot", Fix44_E1_ParseIterateSnapshot());
		check("Fix44_E2_ParseIterateOrder", Fix44_E2_ParseIterateOrder());
	}
}
