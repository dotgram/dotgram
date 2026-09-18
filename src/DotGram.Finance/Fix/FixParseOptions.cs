using System;

namespace DotGram.Finance.Fix;

/// <summary>An immutable parsing policy, reusable concurrently across messages.</summary>
public sealed class FixParseOptions
{
	/// <summary>
	/// Reads wire framing under the given validation policy.
	/// </summary>
	public FixParseOptions(FixParseMode mode = FixParseMode.Strict) : this(FixFraming.Wire, mode) { }

	/// <summary>
	/// Reads the given framing under the given validation policy.
	/// </summary>
	/// <param name="framing">Wire framing for SOH-separated input, log framing for a lossless pipe rendering.</param>
	/// <param name="mode">The validation policy applied after wire recognition.</param>
	/// <param name="fieldOptions">Null uses the standard length/data dictionary.</param>
	public FixParseOptions(FixFraming framing, FixParseMode mode = FixParseMode.Strict, FixFieldOptions? fieldOptions = null)
	{
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) throw new ArgumentOutOfRangeException(nameof(mode));
		if (framing != FixFraming.Wire && framing != FixFraming.Log)
			throw new ArgumentOutOfRangeException(nameof(framing));

		Framing      = framing;
		FieldOptions = fieldOptions ?? FixFieldOptions.Default;
		Mode         = mode;
	}

	public FixFieldOptions   FieldOptions { get; }
	public FixFraming   Framing      { get; }
	public FixParseMode Mode         { get; }
}
