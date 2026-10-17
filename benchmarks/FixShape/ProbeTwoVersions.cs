using DotGram.Finance.Fix.Fix42;
using DotGram.Finance.Fix.Fix44;
using System.Linq;
using F42 = DotGram.Finance.Fix.Fix42;
using F44 = DotGram.Finance.Fix.Fix44;

namespace DotGram.Finance.FixShape;

// A file that imports two FIX versions' namespaces and uses the operators on groups of both.
public partial class FixCollectionShapeBenchmarks
{
	long ProbeTwoVersions()
	{
		var a = _snapshots[0].NoMDEntriesGroups;
		var b = ((F42.FixMessage.MarketDataSnapshotFullRefresh)F42.FixParser.ParseMessage(_snapshot42)).NoMDEntriesGroups;

		if (a is null || b is null)
			return 0;

		return a.Count(x => x.MDEntryType.Value == '0') + b.Count(x => x.MDEntryType.Value == '0') + (a.Any(x => x.MDEntryType.Value == '1') ? 1 : 0) + (b.Any() ? 1 : 0);
	}

}
