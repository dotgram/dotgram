using System;

namespace DotGram.Finance.Fix;

/// <summary>Selects the field delimiter.</summary>
public sealed class FixFieldOptions
{
	public FixFieldOptions(char separator = '\u0001')
	{
		if (separator != '\u0001' && separator != '|') throw new ArgumentOutOfRangeException(nameof(separator));
		Separator = separator;
	}

	public char Separator { get; }
}
