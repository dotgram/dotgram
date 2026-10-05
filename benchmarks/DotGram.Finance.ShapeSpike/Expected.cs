namespace DotGram.Finance.Benchmarks.ShapeSpike;

public partial class FixCollectionShapeBenchmarks
{
	// What every row answers once, the same in every column and on both runtimes: the proof that the columns do
	// the same work. Printed by running a column with FIX_SPIKE_PRINT=1.
	static readonly Dictionary<string, long> Expected = new()
	{
		["Fix42_P2_ParseSnapshot"] = 3023896871,
		["Fix42_E1_ParseIterateSnapshot"] = 12887838,
		["Fix42_E2_ParseIterateOrder"] = 3284,
		["Fix44_P1_ParseOrder"] = 82039757,
		["Fix44_P2_ParseSnapshot"] = 3023896871,
		["Fix44_V1_ParseValidateOrder"] = 82039757,
		["Fix44_V2_ParseValidateSnapshot"] = 3023896871,
		["Fix44_I1_GroupsForeach"] = 12077000,
		["Fix44_I2_GroupsFor"] = 12077000,
		["Fix44_I3_NestedForeach"] = 837200,
		["Fix44_I4_FieldsForeach"] = 810838,
		["Fix44_I5_LinqCount"] = 500,
		["Fix44_I6_MultipleForeach"] = 400,
		["Fix44_I7_AbsentForeach"] = 100,
		["Fix44_E1_ParseIterateSnapshot"] = 12887838,
		["Fix44_E2_ParseIterateOrder"] = 8376,
		["Fix50_P2_ParseSnapshot"] = 3024906002,
		["Fix50_E1_ParseIterateSnapshot"] = 12896966,
		["Fix50_E2_ParseIterateOrder"] = 8568,
	};
}
