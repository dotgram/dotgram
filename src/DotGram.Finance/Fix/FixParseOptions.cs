using System;

namespace DotGram.Finance.Fix;

/// <summary>An immutable parsing policy, reusable concurrently across messages.</summary>
public sealed class FixParseOptions
{
	/// <summary>
	/// Reads wire framing with the standard length/data dictionary.
	/// </summary>
	public FixParseOptions() : this(FixFraming.Wire) { }

	/// <summary>
	/// Reads the given framing.
	/// </summary>
	/// <param name="framing">Wire framing for SOH-separated input, log framing for a lossless pipe rendering.</param>
	/// <param name="fieldOptions">Null uses the standard length/data dictionary.</param>
	public FixParseOptions(FixFraming framing, FixFieldOptions? fieldOptions = null)
	{
		if (framing != FixFraming.Wire && framing != FixFraming.Log)
			throw new ArgumentOutOfRangeException(nameof(framing));

		Framing      = framing;
		FieldOptions = fieldOptions ?? FixFieldOptions.Default;
	}

	/// <summary>
	/// The length/data dictionary used when reading fields.
	/// </summary>
	public FixFieldOptions FieldOptions { get; }
	/// <summary>
	/// How the input separates one field from the next.
	/// </summary>
	public FixFraming      Framing      { get; }
}
