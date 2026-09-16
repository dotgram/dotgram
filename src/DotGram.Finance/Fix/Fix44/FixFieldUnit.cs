using System;

namespace DotGram.Finance.Fix;

// A streaming parse commits one ordinary field or one complete length/data pair.
readonly struct FixFieldUnit
{
	public FixFieldUnit(FixField first, FixField? second = null) { First = first; Second = second; }
	public FixField First { get; }
	public FixField? Second { get; }

	public static FixField[] Flatten(FixFieldUnit[] units)
	{
		var count = units.Length;
		foreach (var unit in units) if (unit.Second != null) count++;
		var fields = new FixField[count];
		var index = 0;
		foreach (var unit in units)
		{
			fields[index++] = unit.First;
			if (unit.Second != null) fields[index++] = unit.Second;
		}
		return fields;
	}
}
