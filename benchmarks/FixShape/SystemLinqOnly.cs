using System.Linq;

namespace DotGram.Finance.FixShape;

// A consumer that imports System.Linq and nothing of the package's namespaces: the query operators it reaches are
// Enumerable's whatever the package adds beside its list type.
public partial class FixCollectionShapeBenchmarks
{
	[BenchmarkDotNet.Attributes.Benchmark]
	public long Fix44_I8_LinqCountSystemLinq()
	{
		var sum = 0L;

		foreach (var message in _snapshots)
		{
			var entries = message.NoMDEntriesGroups;

			if (entries is not null)
				sum += entries.Count(e => e.MDEntryType.Value == '0');
		}

		return sum;
	}
}
