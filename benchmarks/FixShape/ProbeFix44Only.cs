using DotGram.Finance.Fix.Fix44;

namespace DotGram.Finance.FixShape;

// Only the version namespace imported (System.Linq is implicit).
public partial class FixCollectionShapeBenchmarks
{
	long ProbeFix44Only()
	{
		var a = _snapshots[0].NoMDEntriesGroups;

		return a is null ? 0 : a.Count(x => x.MDEntryType.Value == '0') + (a.Any(x => x.MDEntryType.Value == '1') ? 1 : 0);
	}

}
