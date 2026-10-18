using System.Linq;

namespace DotGram.Finance.FixShape;

// Two rows for the cost of the operators that reach a list through its interfaces: Select and a copy into a List<T>.
public partial class FixCollectionShapeBenchmarks
{
	[BenchmarkDotNet.Attributes.Benchmark]
	public long Fix44_I9_SelectForeach()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
			foreach (var value in (message.NoMDEntriesGroups ?? []).Select(e => (long)e.MDEntryType.Value))
				sum += value;

		return sum;
	}

	[BenchmarkDotNet.Attributes.Benchmark]
	public long Fix44_I10_NewList()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
			sum += Copy(message.NoMDEntriesGroups ?? []);

		return sum;
	}

	static int Copy<T>(System.Collections.Generic.IEnumerable<T> entries)
	{
		return new System.Collections.Generic.List<T>(entries).Count;
	}
}
