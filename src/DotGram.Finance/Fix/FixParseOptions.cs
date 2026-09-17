using System;

namespace DotGram.Finance.Fix;

/// <summary>An immutable parsing policy, reusable concurrently across messages.</summary>
public sealed class FixParseOptions
{
	public FixParseOptions(FixParseMode mode = FixParseMode.Strict) : this('\u0001', mode) { }

	/// <summary>Use SOH for wire input or pipe for a lossless log rendering.</summary>
	public FixParseOptions(char separator, FixParseMode mode = FixParseMode.Strict)
	{
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) throw new ArgumentOutOfRangeException(nameof(mode));
		FieldOptions = new FixOptions(separator);
		Mode = mode;
	}

	public FixOptions FieldOptions { get; }
	public char Separator => FieldOptions.Separator;
	public FixParseMode Mode { get; }
}
