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
	public FixParseOptions(FixFraming framing, FixParseMode mode = FixParseMode.Strict)
	{
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) throw new ArgumentOutOfRangeException(nameof(mode));
		if (framing != FixFraming.Wire && framing != FixFraming.Log)
			throw new ArgumentOutOfRangeException(nameof(framing));

		Framing      = framing;
		FieldOptions = new FixOptions();
		Mode         = mode;
	}

	public FixOptions   FieldOptions { get; }
	public FixFraming   Framing      { get; }
	public FixParseMode Mode         { get; }

	/// <summary>
	/// The character the chosen framing ends a field with.
	/// </summary>
	internal char Separator => Framing == FixFraming.Log ? '|' : '\u0001';
}
